using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gamanet.C4.Client.Panels.DemoPanel.Entities;
using static System.Collections.Specialized.NameObjectCollectionBase;


namespace Gamanet.C4.Client.Panels.DemoPanel.Entities
{
    public class TestDataProvider
    {
        public static IEnumerable<PersonEntity> GetRandomPeople(int count)
        {
            List<PersonEntity> randomPeople = new List<PersonEntity>(count);
            Random r = new();

            for (int i = 0; i < 10; i++)
            {
                randomPeople.Add(new PersonEntity { Name = $"Name {r.Next(100):000}" });
            }

            return randomPeople;
        }

        public static IEnumerable<PersonEntity> GetTestPeople(int count)
        {
            List<PersonEntity> testPeople = new List<PersonEntity>(count);

            testPeople.AddRange([
                        new() { Name = "Test Person 1", Country = "Poland", Phone= "+48 123 456 789", Email = "testperson1@poczta.pl"},
                            new() { Name = "Test Person 2", Country = "Poland", Phone= "+48 456 789 123", Email = "testperson2@poczta.pl"},
                            new() { Name = "Test Person 3", Country = "Germany", Phone= "+49 789 123 456", 
                                Email = "Test: very_very_very_very_" +
                                        "very_very_very_very_" +
                                        "very_very_very_very_" +
                                        "very_very_very_very_" +
                                        "very_very_very_very_" +
                                        "long_email_address_testperson3@mail.de"},
                ]);

            Random r = new();

            // Now for testing many items (scroll bar visibilities, limiting size of items control etc.)
            for (int i = 4; i < count; i++)
            {
                testPeople.Add(new()
                {
                    // Random: Only to see a change in view when clicking "fill data"
                    Name = $"Test Person {r.Next(100):000}",
                    Country = $"Poland",
                    Phone = $"012 345 {i:000}",
                    Email = $"email.address{i:00}@mail.com"
                });
            }

            return testPeople;
        }

    }
}
