using System;
using System.Threading.Tasks;
using Crypto.Services.DataService;
using Crypto.Services.dboData;
using Crypto.Services.dboDataService;

namespace Crypto.CommandLine
{
    #region person
    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
    }
    #endregion

    #region person service
    public class PersonService
    {
        public static void Add(Person person)
        {
            Console.WriteLine($"Person: {person.LastName}, {person.FirstName} born on {person.BirthDate.Date.ToString("yyyy-MM-dd")}");
        }

        public static async Task AddAsync(Person person)
        {
            var logs = await db.SelectMany<Log>().From(dbo.Log).Where(dbo.Log.Id == 3).ExecuteAsync();
        }

        public static void AddPerson(string firstName, string lastName, DateTime birthDate)
        {
            Console.WriteLine($"Person: {lastName}, {firstName} born on {birthDate.Date.ToString("yyyy-MM-dd")}");
        }

        public static async Task AddPersonAsync(string firstName, string lastName, DateTime birthDate)
        {
            var logs = await db.SelectMany<Log>().From(dbo.Log).Where(dbo.Log.Id == 3).ExecuteAsync();
        }

        public static void AddPesonTest(string firstName, string lastName, DateTime? birthDate)
        {
            Console.WriteLine($"Person: {lastName}, {firstName} born on {birthDate?.Date.ToString("yyyy-MM-dd")}");
        }
    }
    #endregion
}
