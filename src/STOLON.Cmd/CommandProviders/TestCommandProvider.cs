using STOLON.Installer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.Cmd
{
    public class TestCommandProvider : CommandProvider
    {
        public TestCommandProvider() : base("_STOLON_") { }

        [Command("Adds two values.", aliases: ["plus"])]
        public void Add(int a, int b) => Console.WriteLine(a + b);
        //[Command]
        //public int Add(int a, int b, int c) => a + b + c;
        [Command("Greets someone with the specified name.")]
        public void Greet(string name) => Console.WriteLine($"Hello, {name}!");

    }
}
