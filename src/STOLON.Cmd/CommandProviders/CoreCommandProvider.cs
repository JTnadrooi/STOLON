using STOLON.Installer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.Cmd
{
    public class CoreCommandProvider : CommandProvider
    {
        public CoreCommandProvider() : base("_STOLON_") { }

        [Command("Adds two values.", aliases: ["plus"])]
        public void Add(int a, int b = 0) => Console.WriteLine(a + b);
        //[Command]
        //public int Add(int a, int b, int c) => a + b + c;
        [Command("Greets someone with the specified name.")]
        public void Greet(string name, bool loud = false) => Console.WriteLine($"Hello, {(loud ? name.ToUpper() : name)}!");
    }
}
