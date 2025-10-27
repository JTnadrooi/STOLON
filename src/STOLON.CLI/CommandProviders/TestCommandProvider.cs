using AsitLib;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STOLON.CLI.Command;

namespace STOLON.CLI
{
    public class TestCommandProvider : CommandProvider
    {
        public TestCommandProvider() : base("test") { }
        [Command("Add two values.", aliases: ["plus"], inheritNamespace: false, isReadOnly: true)]
        public void Add(int a, int b = 0) => Console.WriteLine(a + b);
        [Command("Count to a number.", inheritNamespace: false, isReadOnly: true)]
        public void Count([Range(0, 10)] int target) => Console.WriteLine(Enumerable.Range(1, target).ToJoinedString(", "));
        //[Command]
        //public int Add(int a, int b, int c) => a + b + c;
        [Command("Greet someone with the specified name.", inheritNamespace: false, isReadOnly: true)]
        public void Greet(string? name = null, bool loud = false, bool ahoy = false)
        {
            string greeting = string.IsNullOrEmpty(name) ? "Greeting" : (ahoy ? "Ahoy" : "Hello");
            string finalName = name ?? string.Empty;

            if (loud)
            {
                greeting = greeting.ToUpper();
                finalName = finalName.ToUpper();
            }

            string namePart = string.IsNullOrEmpty(finalName) ? string.Empty : $" {finalName}";
            Console.WriteLine($"{greeting}{namePart}!");
        }
    }
}
