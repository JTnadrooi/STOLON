using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.Tests.Tests
{
    [TestClass]
    public class BoardTests
    {
        [TestMethod]
        [DataRow(0, 0, "a1")]
        [DataRow(3, 3, "d4")]
        [DataRow(2, 4, "c5")]
        public void GetCoordsFromPoint(int x, int y, string expected)
        {
            Board.GetCoordsFromPoint(new Point(x, y)).Should().Be(expected);
        }

        [TestMethod]
        [DataRow("a1", 0, 0)]
        [DataRow("d4", 3, 3)]
        [DataRow("c5", 2, 4)]
        public void GetPointFromCoords(string coords, int expectedX, int expectedY)
        {
            Board.GetPointFromCoords(coords).Should().Be(new Point(expectedX, expectedY));
        }
    }
}
