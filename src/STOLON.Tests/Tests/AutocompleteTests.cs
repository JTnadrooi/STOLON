using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.Tests
{
    [TestClass]
    public class AutocompleteTests
    {
        [TestMethod]
        public void Complete_ReturnsWordsStartingWithString()
        {
            Autocomplete.Complete("w", ["write", "when", "hello", "azure", "north", "upend", "witch"]).Options.Should().BeEquivalentTo(["write", "when", "witch"]);
            Autocomplete.Complete("wr", ["write", "when", "hello", "azure", "north", "upend", "witch"]).Options.Should().BeEquivalentTo(["write"]);
        }

        [TestMethod]
        public void Complete_EmptyString_ReturnsAllOptions()
        {
            string[] input = ["write", "when", "hello", "azure", "north", "upend", "witch"];

            Autocomplete.Complete("", input).Should().BeEquivalentTo(input);
        }

        [TestMethod]
        public void Complete_ExactMatch_ReturnsExcludesExactMatch()
        {
            Autocomplete.Complete("write", ["write", "writer"]).Options.Should().BeEquivalentTo(["writer"]);
        }
    }
}
