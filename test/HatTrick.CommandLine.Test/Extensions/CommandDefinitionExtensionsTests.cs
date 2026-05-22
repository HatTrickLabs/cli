using Xunit;
using HatTrick.CommandLine.Extensions;

namespace HatTrick.CommandLine.Test
{
    [Collection("Sequential")]
    public class CommandDefinitionExtensionsTests
    {
        #region map to [on validate]
        [Fact]
        public void MapTo_OnValidate_WhenOptionKey_DoesNotMatch_WithAPropertyName_ShouldThrow_CommandMappingException()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            var cmdDef = new CommandDefinition("go");
            cmdDef.Help = "help!";
            cmdDef.AddOption<string>(key: "first-name", help: "First name.", (terse: "-f", verbose: "--fn"));
            cmdDef.AddOption<string>(key: "last-name", help: "Last name.", (terse: "-l", verbose: "--ln"));
            cmdDef.AddOption<int>(key: "age", help: "Age.", (terse: "-a", verbose: "--age"));
            cmdDef.MapTo<Person>().Then((person) => { });

            //none of the option keys match directly with propery names from the Person class,
            //should fail validation when added to the registry
            Action action = () => registry.Add(cmdDef);
            Assert.Contains("No property found", Assert.Throws<CommandMappingException>(action).Message);
        }

        [Fact]
        public void MapTo_OnValidate_WhenCorrelationKey_DoesNotMatch_WithAPropertyName_ShouldThrow_CommandMappingException()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            var cmdDef = new CommandDefinition("go");
            cmdDef.Help = "help!";
            cmdDef.AddOption<string>(key: "first-name", help: "First name.", (terse: "-f", verbose: "--fn"));
            cmdDef.AddOption<string>(key: "last-name", help: "Last name.", (terse: "-l", verbose: "--ln"));
            cmdDef.AddOption<int>(key: "age", help: "Age.", (terse: "-a", verbose: "--age"));
            cmdDef.MapTo<Person>(
                (optionKey: "first-name", propertyName: "FirstName"),
                (optionKey: "last-name", propertyName: "LastNm"),//LastNm does not exist on Person..
                (optionKey: "age", propertyName: "Age")
            ).Then((person) => { });
            Action action = () => registry.Add(cmdDef);
            Assert.Contains("via correlation", Assert.Throws<CommandMappingException>(action).Message);
        }

        [Fact]
        public void MapTo_OnValidate_WhenCorrelationKeys_Match_WithAPropertyNames_ShouldPass()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            var cmdDef = new CommandDefinition("go");
            cmdDef.Help = "help!";
            cmdDef.AddOption<string>(key: "first-name", help: "First name.", (terse: "-f", verbose: "--fn"));
            cmdDef.AddOption<string>(key: "last-name", help: "Last name.", (terse: "-l", verbose: "--ln"));
            cmdDef.AddOption<int>(key: "age", help: "Age.", (terse: "-a", verbose: "--age"));
            cmdDef.MapTo<Person>(
                (optionKey: "first-name", propertyName: "FirstName"),
                (optionKey: "last-name", propertyName: "LastName"),
                (optionKey: "age", propertyName: "Age")
            ).Then((person) => { });

            //correlation maps should all align, should pass validation
            registry.Add(cmdDef);
        }

        [Fact]
        public void MapTo_OnValidate_WhenOptionKeys_MatchDirectly_WithProperyNames_ShouldPass()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            var cmdDef = new CommandDefinition("go");
            cmdDef.Help = "help!";
            cmdDef.AddOption<string>(key: "FirstName", help: "First name.", (terse: "-f", verbose: "--fn"));
            cmdDef.AddOption<string>(key: "LastName", help: "Last name.", (terse: "-l", verbose: "--ln"));
            cmdDef.AddOption<int>(key: "Age", help: "Age.", (terse: "-a", verbose: "--age"));

            cmdDef.MapTo<Person>().Then((person) => { 
            
            });

            //all of the option keys match directly with Person propery values.  Add should pass validation.
            registry.Add(cmdDef);
        }

        [Fact]
        public void MapTo_OnValidate_WhenProperty_IsNotAssignableFrom_OptionType_ShouldThrow_CommandMappingException()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            var cmdDef = new CommandDefinition("go");
            cmdDef.Help = "help!";
            cmdDef.AddOption<string>(key: "FirstName", help: "First name.", (terse: "-f", verbose: "--fn"));
            cmdDef.AddOption<string>(key: "LastName", help: "Last name.", (terse: "-l", verbose: "--ln"));
            //typeing the age op as string to force validation exception
            cmdDef.AddOption<string>(key: "Age", help: "Age.", (terse: "-a", verbose: "--age"));

            cmdDef.MapTo<Person>().Then((person) => {

            });

            //Age property of int is not assignable from Age option type of string
            Action action = () => registry.Add(cmdDef);
            Assert.Contains("Type mismatch while mapping option value to target", 
                Assert.Throws<CommandMappingException>(action).Message);
        }

        [Fact]
        public void MapTo_OnValidate_WithCorrelationMap_WhenProperty_IsNotAssignableFrom_OptionType_ShouldThrow_CommandMappingException()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            var cmdDef = new CommandDefinition("go");
            cmdDef.Help = "help!";
            cmdDef.AddOption<string>(key: "FirstName", help: "First name.", (terse: "-f", verbose: "--fn"));
            cmdDef.AddOption<string>(key: "LastName", help: "Last name.", (terse: "-l", verbose: "--ln"));
            //typeing the age op as string to force validation exception
            cmdDef.AddOption<string>(key: "age", help: "Age.", (terse: "-a", verbose: "--age"));

            cmdDef.MapTo<Person>(("age","Age")).Then((person) => {

            });

            //Age property of int is not assignable from Age option type of string
            Action action = () => registry.Add(cmdDef);
            Assert.Contains("Type mismatch while mapping option value to target",
                Assert.Throws<CommandMappingException>(action).Message);
        }

        [Fact]
        public void MapTo_OnValidate_WithoutCorrelationMap_IfCommandDefinition_ContainsUnmappableProperty_ShouldThrow_CommandMappingException()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            var cmdDef = new CommandDefinition("go");
            cmdDef.Help = "help!";
            cmdDef.AddOption<string>(key: "FirstName", help: "First name.", (terse: "-f", verbose: "--fn"));
            cmdDef.AddOption<string>(key: "LastName", help: "Last name.", (terse: "-l", verbose: "--ln"));
            cmdDef.AddOption<int>(key: "Age", help: "Age.", (terse: "-a", verbose: "--age"));
            //add an unmappable option
            cmdDef.AddOption<string>(key: "Gender", help: "Person's gender.", (terse: "-g", verbose: "--gender"));

            cmdDef.MapTo<Person>().Then((person) => { });

            //Age property of int is not assignable from Age option type of string
            Action action = () => registry.Add(cmdDef);
            Assert.Contains("No property found on",Assert.Throws<CommandMappingException>(action).Message);
        }

        [Fact]
        public void MapTo_OnValidate_WithCorrelationMap_IncludingTheIgnoreToken_IfCommandDefinition_ContainsUnmappableProperty_ShouldPass()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            var cmdDef = new CommandDefinition("go");
            cmdDef.Help = "help!";
            cmdDef.AddOption<string>(key: "FirstName", help: "First name.", (terse: "-f", verbose: "--fn"));
            cmdDef.AddOption<string>(key: "LastName", help: "Last name.", (terse: "-l", verbose: "--ln"));
            cmdDef.AddOption<int>(key: "Age", help: "Age.", (terse: "-a", verbose: "--age"));
            //add an unmappable option
            cmdDef.AddOption<string>(key: "Gender", help: "Person's gender.", (terse: "-g", verbose: "--gender"));

            //inform the mapper to ignore the Gender option...~ marks it as un-mappable...
            cmdDef.MapTo<Person>((optionKey:"Gender", propertyName: "~")).Then((person) => { });

            //should pass validation
            registry.Add(cmdDef);
        }
        #endregion

        #region map to [usage]
        [Fact]
        public void MapTo_ShouldMap_AllOptions_ToProperties_AndSuccessfullyCall_MappedDelegate()
        {
            DefinitionRegistry.Clear();
            var regiistry = DefinitionRegistry.GetInstance();
            var cmdDef = new CommandDefinition("create-person");
            cmdDef.Help = "help!";
            cmdDef.AddOption<string>(key: "first-name", help: "Person's first name.", (terse: "-f", verbose: "--fn"));
            cmdDef.AddOption<string>(key: "last-name", help: "Person's last name.", (terse: "-l", verbose: "--ln"));
            cmdDef.AddOption<int>(key: "age", help: "Person's age.", (terse: "-a", verbose: "--age"));
            cmdDef.MapTo<Person>(
                (optionKey: "first-name", propertyName: "FirstName"),
                (optionKey: "last-name", propertyName: "LastName"),
                (optionKey: "age", propertyName: "Age")
            ).Then((person) =>
            {
                Assert.Equal("Charlie", person.FirstName);
                Assert.Equal("Brown", person.LastName);
                Assert.Equal(8, person.Age);
            });
            regiistry.Add(cmdDef);

            string input = "create-person -l Brown -f Charlie -a 8";
            Command command = CommandBuilder.Build(input);
            CommandExecutor exe = regiistry.GetCommandExecutor(command);
            exe.Execute();
        }
        #endregion

        #region map to signature [on validate]
        [Fact]
        public void MapToSignature_OnValidate_WhenOptionKey_DoesNotMatch_WithAParameter_ShouldThrow_CommandMappingException()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            var cmdDef = new CommandDefinition("go");
            cmdDef.Help = "help!";
            cmdDef.AddOption<string>(key: "firstName", help: "First name.", (terse: "-f", verbose: "--fn"));
            cmdDef.AddOption<string>(key: "lastName", help: "Last name.", (terse: "-l", verbose: "--ln"));
            //misspell age to cause mapping conflict
            cmdDef.AddOption<int>(key: "agee", help: "Age.", (terse: "-a", verbose: "--age"));
            cmdDef.MapToSignature(Person.Create).Go();

            Action action = () => registry.Add(cmdDef);
            Assert.Contains("No parameter found on signature for option", Assert.Throws<CommandMappingException>(action).Message);
        }

        [Fact]
        public void MapToSignature_OnValidate_WhenCorrelationKey_DoesNotMatch_WithAParameterName_ShouldThrow_CommandMappingException()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            var cmdDef = new CommandDefinition("go");
            cmdDef.Help = "help!";
            cmdDef.AddOption<string>(key: "first-name", help: "First name.", (terse: "-f", verbose: "--fn"));
            cmdDef.AddOption<string>(key: "last-name", help: "Last name.", (terse: "-l", verbose: "--ln"));
            cmdDef.AddOption<int>(key: "age", help: "Age.", (terse: "-a", verbose: "--age"));
            cmdDef.MapToSignature(Person.Create,
                (optionKey: "first-name", parameterName: "firstName"),
                (optionKey: "last-name", parameterName: "lastNm"),//LastNm does not exist on Person..
                (optionKey: "age", parameterName: "Age")
            ).Go();
            Action action = () => registry.Add(cmdDef);
            Assert.Contains("via correlation", Assert.Throws<CommandMappingException>(action).Message);
        }

        [Fact]
        public void MapTSignature_OnValidate_WhenCorrelationKeys_Match_WithAParameterNames_ShouldPass()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            var cmdDef = new CommandDefinition("go");
            cmdDef.Help = "help!";
            cmdDef.AddOption<string>(key: "first-name", help: "First name.", (terse: "-f", verbose: "--fn"));
            cmdDef.AddOption<string>(key: "last-name", help: "Last name.", (terse: "-l", verbose: "--ln"));
            cmdDef.AddOption<int>(key: "age", help: "Age.", (terse: "-a", verbose: "--age"));
            cmdDef.MapToSignature(Person.Create,
                (optionKey: "first-name", parameterName: "firstName"),
                (optionKey: "last-name", parameterName: "lastName"),
                (optionKey: "age", parameterName: "age")
            ).Go();

            //correlation maps should all align, should pass validation
            registry.Add(cmdDef);
        }

        [Fact]
        public void MapToSignature_OnValidate_WhenOptionKeys_MatchDirectly_WithParameterNames_ShouldPass()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            var cmdDef = new CommandDefinition("go");
            cmdDef.Help = "help!";
            cmdDef.AddOption<string>(key: "firstName", help: "First name.", (terse: "-f", verbose: "--fn"));
            cmdDef.AddOption<string>(key: "lastName", help: "Last name.", (terse: "-l", verbose: "--ln"));
            cmdDef.AddOption<int>(key: "age", help: "Age.", (terse: "-a", verbose: "--age"));

            cmdDef.MapToSignature(Person.Create).Go();

            //all of the option keys match directly with signature parameters.  Add should pass validation.
            registry.Add(cmdDef);
        }

        [Fact]
        public void MapToSignature_OnValidate_WhenParameter_IsNotAssignableFrom_OptionType_ShouldThrow_CommandMappingException()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            var cmdDef = new CommandDefinition("go");
            cmdDef.Help = "help!";
            cmdDef.AddOption<string>(key: "firstName", help: "First name.", (terse: "-f", verbose: "--fn"));
            cmdDef.AddOption<string>(key: "lastName", help: "Last name.", (terse: "-l", verbose: "--ln"));
            //typeing the age op as string to force validation exception
            cmdDef.AddOption<string>(key: "age", help: "Age.", (terse: "-a", verbose: "--age"));

            cmdDef.MapToSignature(Person.Create).Go();

            //Age parameter of int is not assignable from age option type of string
            Action action = () => registry.Add(cmdDef);
            Assert.Contains("Type mismatch while mapping option value to parameter",
                Assert.Throws<CommandMappingException>(action).Message);
        }

        [Fact]
        public void MapToSignature_OnValidate_WithCorrelationMap_WhenParameter_IsNotAssignableFrom_OptionType_ShouldThrow_CommandMappingException()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            var cmdDef = new CommandDefinition("go");
            cmdDef.Help = "help!";
            cmdDef.AddOption<string>(key: "firstName", help: "First name.", (terse: "-f", verbose: "--fn"));
            cmdDef.AddOption<string>(key: "lastName", help: "Last name.", (terse: "-l", verbose: "--ln"));
            //typeing the age op as string to force validation exception
            cmdDef.AddOption<string>(key: "Age", help: "Age.", (terse: "-a", verbose: "--age"));

            cmdDef.MapToSignature(Person.Create, ("Age", "age")).Go();

            //Age property of int is not assignable from Age option type of string
            Action action = () => registry.Add(cmdDef);
            Assert.Contains("Type mismatch while mapping option value to parameter",
                Assert.Throws<CommandMappingException>(action).Message);
        }

        [Fact]
        public void MapToSignature_OnValidate_WithoutCorrelationMap_IfCommandDefinition_ContainsUnmappableParameter_ShouldThrow_CommandMappingException()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            var cmdDef = new CommandDefinition("go");
            cmdDef.Help = "help!";
            cmdDef.AddOption<string>(key: "firstName", help: "First name.", (terse: "-f", verbose: "--fn"));
            cmdDef.AddOption<string>(key: "lastName", help: "Last name.", (terse: "-l", verbose: "--ln"));
            cmdDef.AddOption<int>(key: "age", help: "Age.", (terse: "-a", verbose: "--age"));
            //add an unmappable option
            cmdDef.AddOption<string>(key: "gender", help: "Person's gender.", (terse: "-g", verbose: "--gender"));

            cmdDef.MapToSignature(Person.Create).Go();

            //Age property of int is not assignable from Age option type of string
            Action action = () => registry.Add(cmdDef);
            Assert.Contains("No parameter found on signature for option", Assert.Throws<CommandMappingException>(action).Message);
        }

        [Fact]
        public void MapToSignature_OnValidate_WithCorrelationMap_IncludingTheIgnoreToken_IfCommandDefinition_ContainsUnmappableParameter_ShouldPass()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            var cmdDef = new CommandDefinition("go");
            cmdDef.Help = "help!";
            cmdDef.AddOption<string>(key: "firstName", help: "First name.", (terse: "-f", verbose: "--fn"));
            cmdDef.AddOption<string>(key: "lastName", help: "Last name.", (terse: "-l", verbose: "--ln"));
            cmdDef.AddOption<int>(key: "age", help: "Age.", (terse: "-a", verbose: "--age"));
            //add an unmappable option
            cmdDef.AddOption<string>(key: "gender", help: "Person's gender.", (terse: "-g", verbose: "--gender"));

            //inform the mapper to ignore the Gender option...~ marks it as un-mappable...
            cmdDef.MapToSignature(Person.Create, (optionKey: "gender", parameterName: "~")).Go();

            //should pass validation
            registry.Add(cmdDef);
        }

        [Fact]
        public void MapToSignature_OnValidate_WhenSignature_ContainsParameter_WithNoMatchingOption_ShouldThrow_CommandMappingException()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            var cmdDef = new CommandDefinition("go");
            cmdDef.Help = "help!";
            cmdDef.AddOption<string>(key: "firstName", help: "First name.", (terse: "-f", verbose: "--fn"));
            cmdDef.AddOption<string>(key: "lastName", help: "Last name.", (terse: "-l", verbose: "--ln"));
            cmdDef.AddOption<int>(key: "age", help: "Age.", (terse: "-a", verbose: "--age"));

            cmdDef.MapToSignature((string fn, string ln, string mn, int age) => { }).Go();

            //Age property of int is not assignable from Age option type of string
            Action action = () => registry.Add(cmdDef);
            Assert.Contains("No parameter found on signature for option", Assert.Throws<CommandMappingException>(action).Message);
        }
        #endregion

        #region map to signature [usage]

        #endregion
    }

    #region person [class]
    internal class Person
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }

        public static void Create(string firstName, string lastName, int age)
        { }
    }
    #endregion
}
