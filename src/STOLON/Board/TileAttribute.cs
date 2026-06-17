using Autofac;
using System.Collections.Frozen;
using System.Runtime.CompilerServices;

namespace STOLON
{
    public static class TileAttribute
    {
        public static TileAttributes GetOccupiedTileAttributeFor(int playerIndex) => playerIndex == 0 ? TileAttributes.Player0Occupied : TileAttributes.Player1Occupied;
    }
}
