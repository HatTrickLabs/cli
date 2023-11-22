using Xunit;


namespace HatTrick.CommandLine.Test
{
    public class CommandBuilderTests
    {
        #region build (string input)
        [Fact]
        public void Build_WhenInput_IsEmpty_ShouldBuild_DefaultCommand()
        {
            string input = string.Empty;
            Command cmd = CommandBuilder.Build(input);
            Assert.Equal(CommandDefinition.DefaultCommandName, cmd.Name);
            Assert.Empty(cmd.Options);
        }

        [Fact]
        public void Build_WhenInput_Contains_CommandOnly_ShouldBuild_OptionlessCommand()
        {
            string input = "go";
            Command cmd = CommandBuilder.Build(input);
            Assert.Equal("go", cmd.Name);
            Assert.Empty(cmd.Options);
        }

        [Fact]
        public void Build_WhenInput_ContainsExplicitAssignPatterns_WithColon_ShouldBuild_ExplicitAssignedOptions()
        {
            string input = "go -a : true -b:false -c: true -d :false -e:true";
            Command cmd = CommandBuilder.Build(input);

            Assert.Equal("go", cmd.Name);
            Assert.Collection<Option>(cmd.Options,
                (x) => { Assert.Equal("-a", x.Flag); Assert.Equal("true", x.Argument); },
                (x) => { Assert.Equal("-b", x.Flag); Assert.Equal("false", x.Argument); },
                (x) => { Assert.Equal("-c", x.Flag); Assert.Equal("true", x.Argument); },
                (x) => { Assert.Equal("-d", x.Flag); Assert.Equal("false", x.Argument); },
                (x) => { Assert.Equal("-e", x.Flag); Assert.Equal("true", x.Argument); }
            );
        }

        [Fact]
        public void Build_WhenInput_ContainsExplicitAssignPatterns_WithEqual_ShouldBuild_ExplicitAssignedOptions()
        {
            string input = "go -a = true -b=false -c= true -d =false -e=true";
            Command cmd = CommandBuilder.Build(input);

            Assert.Equal("go", cmd.Name);
            Assert.Collection<Option>(cmd.Options,
                (x) => { Assert.Equal("-a", x.Flag); Assert.Equal("true", x.Argument); },
                (x) => { Assert.Equal("-b", x.Flag); Assert.Equal("false", x.Argument); },
                (x) => { Assert.Equal("-c", x.Flag); Assert.Equal("true", x.Argument); },
                (x) => { Assert.Equal("-d", x.Flag); Assert.Equal("false", x.Argument); },
                (x) => { Assert.Equal("-e", x.Flag); Assert.Equal("true", x.Argument); }
            );
        }

        [Fact]
        public void Build_WhenInput_ContainsCompoundTerseFlags_ShouldUnrollFlags_IntoIndividual_TerseFlags()
        {
            string input = "go --abc -xyz";
            Command cmd = CommandBuilder.Build(input);

            Assert.Equal("go", cmd.Name);
            Assert.Equal(4, cmd.Options.Length);
            Assert.Collection<Option>(cmd.Options,
                (x) => Assert.Equal("--abc", x.Flag),
                (x) => Assert.Equal("-x", x.Flag),
                (x) => Assert.Equal("-y", x.Flag),
                (x) => Assert.Equal("-z", x.Flag)
            );
        }

        [Fact]
        public void Build_WhenInput_ContainsWhiteSpace_ShouldRetainWhitespace()
        {
            string input = "run --from \"first base\" --to \"second base\"";
            Command cmd = CommandBuilder.Build(input);

            Assert.Equal("run", cmd.Name);
            Assert.Equal(2, cmd.Options.Length);
            Assert.Collection<Option>(cmd.Options,
                (x) => { Assert.Equal("--from", x.Flag); Assert.Equal("first base", x.Argument); },
                (x) => { Assert.Equal("--to", x.Flag); Assert.Equal("second base", x.Argument); }
            );
        }

        [Fact]
        public void Build_WhenInput_Represent_Undelimited_And_DoubleQuoted_Tokens_ShouldSplit_OnCloseQuote()
        {
            //"abc""--xyz""aaa"
            //Passing the above into exe via command line will result in a single arg as follows:
            //abc\"--xyz\"aaa
            //we WANT to tokenize that as 3 different tokens abc xyz aaa
            string input = "\"abc\"\"--xyz\"\"aaa\"";
            Command cmd = CommandBuilder.Build(input);

            Assert.Equal("abc", cmd.Name);
            Assert.Collection<Option>(cmd.Options,
                (x) => { Assert.Equal("--xyz", x.Flag); Assert.Equal("aaa", x.Argument); }
            );
        }

        [Fact]
        public void Build_WhenInput_Represent_Undelimited_And_DoubleQuoted_Tokens_ShouldSplit_OnCloseQuote_And_RetainProperWhitespace()
        {
            //"abc""--xyz""aa a"
            //Passing the above into exe via command line will result in a single arg as follows:
            //abc\"--xyz\"aa a
            //we WANT to tokenize that as 3 different tokens abc --xyz aa a  ... aa a being a single token
            string input = "\"abc\"\"--xyz\"\"aa a\"";
            Command cmd = CommandBuilder.Build(input);

            Assert.Equal("abc", cmd.Name);
            Assert.Collection<Option>(cmd.Options,
                (x) => { Assert.Equal("--xyz", x.Flag); Assert.Equal("aa a", x.Argument); }
            );
        }
        #endregion

        #region build (string[] args)
        [Fact]
        public void Build_WhenInputArgs_IsEmpty_ShouldBuild_DefaultCommand()
        {
            string[] args = Array.Empty<string>();
            Command cmd = CommandBuilder.Build(args);
            Assert.Equal(CommandDefinition.DefaultCommandName, cmd.Name);
            Assert.Empty(cmd.Options);
        }

        [Fact]
        public void Build_WhenInputArgs_Contains_CommandOnly_ShouldBuild_OptionlessCommand()
        {
            string[] args = new string[] { "go" };
            Command cmd = CommandBuilder.Build(args);
            Assert.Equal("go", cmd.Name);
            Assert.Empty(cmd.Options);
        }

        [Fact]
        public void Build_WhenInputArgs_ContainsExplicitAssignPatterns_WithColon_ShouldBuild_ExplicitAssignedOptions()
        {
            string[] args = new string[] { "go", "-a", ":", "true", "-b:false", "-c:", "true", "-d", ":false", "-e:true" };
            Command cmd = CommandBuilder.Build(args);

            Assert.Equal("go", cmd.Name);
            Assert.Collection<Option>(cmd.Options,
                (x) => { Assert.Equal("-a", x.Flag); Assert.Equal("true", x.Argument); },
                (x) => { Assert.Equal("-b", x.Flag); Assert.Equal("false", x.Argument); },
                (x) => { Assert.Equal("-c", x.Flag); Assert.Equal("true", x.Argument); },
                (x) => { Assert.Equal("-d", x.Flag); Assert.Equal("false", x.Argument); },
                (x) => { Assert.Equal("-e", x.Flag); Assert.Equal("true", x.Argument); }
            );
        }

        [Fact]
        public void Build_WhenInput_ContainsCompoundTerseFlags_FollowedByArgument_ShouldUnrollFlags_IntoIndividual_TerseFlags_And_AssignArgToLast()
        {
            string args = "go --abc yes -xyz no";
            Command cmd = CommandBuilder.Build(args);

            Assert.Equal("go", cmd.Name);
            Assert.Equal(4, cmd.Options.Length);
            Assert.Collection<Option>(cmd.Options,
                (x) => { Assert.Equal("--abc", x.Flag); Assert.Equal("yes", x.Argument); },
                (x) => { Assert.Equal("-x", x.Flag); Assert.Null(x.Argument); },
                (x) => { Assert.Equal("-y", x.Flag); Assert.Null(x.Argument); },
                (x) => { Assert.Equal("-z", x.Flag); Assert.Equal("no", x.Argument); }
            );
        }

        [Fact]
        public void Build_WhenInputArgs_ContainsExplicitAssignPatterns_WithEqual_ShouldBuild_ExplicitAssignedOptions()
        {
            string[] args = new string[] { "go", "-a", "=", "true", "-b=false", "-c=", "true", "-d", "=false", "-e=true" };
            Command cmd = CommandBuilder.Build(args);

            Assert.Equal("go", cmd.Name);
            Assert.Collection<Option>(cmd.Options,
                (x) => { Assert.Equal("-a", x.Flag); Assert.Equal("true", x.Argument); },
                (x) => { Assert.Equal("-b", x.Flag); Assert.Equal("false", x.Argument); },
                (x) => { Assert.Equal("-c", x.Flag); Assert.Equal("true", x.Argument); },
                (x) => { Assert.Equal("-d", x.Flag); Assert.Equal("false", x.Argument); },
                (x) => { Assert.Equal("-e", x.Flag); Assert.Equal("true", x.Argument); }
            );
        }

        [Fact]
        public void Build_WhenInputArgs_ContainsCompoundTerseFlags_ShouldUnrollFlags_IntoIndividual_TerseFlags()
        {
            string[] args = new string[] { "go", "--abc", "-xyz" };
            Command cmd = CommandBuilder.Build(args);

            Assert.Equal("go", cmd.Name);
            Assert.Equal(4, cmd.Options.Length);
            Assert.Collection<Option>(cmd.Options,
                (x) => Assert.Equal("--abc", x.Flag),
                (x) => Assert.Equal("-x", x.Flag),
                (x) => Assert.Equal("-y", x.Flag),
                (x) => Assert.Equal("-z", x.Flag)
            );
        }

        [Fact]
        public void Build_WhenInputArgs_ContainsCompoundTerseFlags_FollowedByArgument_ShouldUnrollFlags_IntoIndividual_TerseFlags_And_AssignArgToLast()
        {
            string[] args = new string[] { "go", "--abc", "yes", "-xyz", "no" };
            Command cmd = CommandBuilder.Build(args);

            Assert.Equal("go", cmd.Name);
            Assert.Equal(4, cmd.Options.Length);
            Assert.Collection<Option>(cmd.Options,
                (x) => { Assert.Equal("--abc", x.Flag); Assert.Equal("yes", x.Argument); },
                (x) => { Assert.Equal("-x", x.Flag); Assert.Null(x.Argument); },
                (x) => { Assert.Equal("-y", x.Flag); Assert.Null(x.Argument); },
                (x) => { Assert.Equal("-z", x.Flag); Assert.Equal("no", x.Argument); }
            );
        }

        [Fact]
        public void Build_WhenInputArg_ContainsWhiteSpace_ShouldRetainWhitespace()
        {
            string[] args = new string[] { "run", "--from", "first base", "--to", "second base" };
            Command cmd = CommandBuilder.Build(args);

            Assert.Equal("run", cmd.Name);
            Assert.Equal(2, cmd.Options.Length);
            Assert.Collection<Option>(cmd.Options,
                (x) => { Assert.Equal("--from", x.Flag); Assert.Equal("first base", x.Argument); },
                (x) => { Assert.Equal("--to", x.Flag); Assert.Equal("second base", x.Argument); }
            );
        }

        [Fact]
        public void Build_WhenInputArgs_Represent_Undelimited_And_DoubleQuoted_Tokens_ShouldSplit_OnCloseQuote()
        {
            //"abc""--xyz""aaa"
            //Passing the above into exe via command line will result in a single arg as follows:
            //abc\"--xyz\"aaa
            //we WANT to tokenize that as 3 different tokens abc --xyz aaa
            string[] args = new string[] { "abc\"--xyz\"aaa" };
            Command cmd = CommandBuilder.Build(args);

            Assert.Equal("abc", cmd.Name);
            Assert.Collection<Option>(cmd.Options,
                (x) => { Assert.Equal("--xyz", x.Flag); Assert.Equal("aaa", x.Argument); }
            );
        }

        [Fact]
        public void Build_WhenInputArgs_Represent_Undelimited_And_DoubleQuoted_Tokens_ShouldSplit_OnCloseQuote_And_RetainProperWhitespace()
        {
            //"abc""--xyz""aa a"
            //Passing the above into exe via command line will result in a single arg as follows:
            //abc\"--xyz\"aa a
            //we WANT to tokenize that as 3 different tokens abc --xyz aa a  ... aa a being a single token
            string[] args = new string[] { "abc\"--xyz\"aa a" };
            Command cmd = CommandBuilder.Build(args);

            Assert.Equal("abc", cmd.Name);
            Assert.Collection<Option>(cmd.Options,
                (x) => { Assert.Equal("--xyz", x.Flag); Assert.Equal("aa a", x.Argument); }
            );
        }
        #endregion
    }
}
