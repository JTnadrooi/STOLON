using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public class Autocompletion
    {
        public IEnumerable<string> Options { get; }

        public string? BestOption { get; }

        public Autocompletion(IEnumerable<string> options)
        {
            Options = options;
            BestOption = Options.OrderBy(s => s.Length).FirstOrDefault();
        }
    }

    public static class Autocomplete
    {
        public static Autocompletion Complete(string str, IEnumerable<string> options)
        {
            //IEnumerable<string> transformedOptions = options.Select(x => x.ToLower());
            //string transformedStr = str.ToLower();

            return new Autocompletion(options.Where(s => s.StartsWith(str) && s != str).OrderBy(s => s));
        }
    }
}
