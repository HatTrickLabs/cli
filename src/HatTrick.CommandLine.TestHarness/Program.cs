using System;
using System.CodeDom.Compiler;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using HatTrick.CommandLine;

namespace HatTrick.CommandLine.TestHarness
{
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            RegisterCommands();

            Command cmd = Parser.Parse(args);
            DefinitionRegistry.GetInstance().ExecuteCommand(cmd);
        }

        static Person[] BuildPeople(int length)
        {
            Person[] people = new Person[length];
            for (int i = 0; i < length; i++)
            {
                people[i] = new() { Id = i, FirstName = "xxxxx", LastName = "yyyyyy" };
            }
            return people;
        }

        static void RegisterCommands()
        {
            CommandDefinition cmd = new("guid");
            cmd.Help = "Generates new globaly unique identifiers.";
            cmd.Handler = (c) => { Console.WriteLine(Guid.NewGuid().ToString()); };
            DefinitionRegistry.GetInstance().Add(cmd);
        }
    }
}