using System;
using System.Threading.Tasks;

namespace HatTrick.CommandLine.TestHarness
{
    #region person
    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateOnly? BirthDate { get; set; }
        public Guid? Code { get; set; }
    }
    #endregion

    #region person service
    public class PersonService
    {
        public static void Add(Person person)
        {
            Console.WriteLine($"Person: {person.LastName}, {person.FirstName} born on {person.BirthDate?.ToString("yyyy-MM-dd")}");
        }

        public static async Task AddAsync(Person person)
        {
            await Task.Delay(5);
            Console.WriteLine($"Person: {person.LastName}, {person.FirstName} born on {person.BirthDate?.ToString("yyyy-MM-dd")}");
        }

        public static void AddPerson(string firstName, string lastName, DateOnly? birthDate, Guid? code = null)
        {
            Console.WriteLine($"Person: {lastName}, {firstName} born on {birthDate?.ToString("yyyy-MM-dd")}");
        }

        public static async Task AddPersonAsync(string firstName, string lastName, DateOnly? birthDate, Guid? code)
        {
            Console.WriteLine($"Person: {lastName}, {firstName} born on {birthDate?.ToString("yyyy-MM-dd")}");
            await Task.Delay(5);
        }

        public static void AddPesonTest(string firstName, string lastName, DateOnly? birthDate, Guid? code)
        {
            Console.WriteLine($"Person: {lastName}, {firstName} born on {birthDate?.ToString("yyyy-MM-dd")}");
        }
    }
    #endregion
}
