using System.Diagnostics.CodeAnalysis;

namespace STOLON
{
    public class SiloEntityDefinition : EntityDefinition
    {
        public SiloEntityDefinition(ITexture2DCollection textures) : base("silo", "Silo", "Sl", textures, "Silo desc", "Silo 28SHA")
        {

        }

        protected override EntityProfile ResolveProfile(ITexture2DCollection textures)
            => new EntityProfile("silo", textures, new Point(245, 180));

        protected override (ConditionalNote[] allocationNotes, ConditionalNote[] abilityNotes) ResolveNotes()
            => (
            [
                new ConditionalNote(this, "Gains 20% valloc when Deceit is selected.", i => i.Contains("deceit"), ConditionalNotePolarity.Positive),
                new ConditionalNote(this, "Loses 50% valloc when more than 3 entities are selected.", i => i.Count > 3, ConditionalNotePolarity.Negative),
            ],
            [
                new ConditionalNote(this, "If valloc is above 50, gain the abilty to decide where the opponent places their marker. You will not be able to win in one of the 3 moves after.", i => i.GetVirtualAllocation("silo") > 50, ConditionalNotePolarity.Positive),
                new ConditionalNote(this, "If valloc is above 20, gain the abilty to remove oppenent markers from a row, must be normal gravity.", i => i.GetVirtualAllocation("silo") > 20, ConditionalNotePolarity.Positive),
            ]);

        public override int GetVirtualAllocation(EntitySelection info)
            => new AllocationBuilder(info, this)
                .ApplyMultiplierWhen(info.Contains("deceit"), 1.2f)
                .ApplyMultiplierWhen(info.Entries.Count > 3, 0.5f)
                .End();
    }

    public class SiloEntity : Entity
    {
        public SiloEntity(EntityDefinition definition, ITexture2DCollection textures, IMoveProvider moveProvider) : base(definition, moveProvider)
        {

        }

        public override bool HasWon(BoardState state)
        {
            return state.SearchFor(SearchTarget.GetDefaultTargets(), state.GetEntityIndex(this));
        }

        public override IMove[] GetAvailableMoves(BoardState state)
        {
            List<(IMove move, int length, Point? landingPos)> moveStore = new List<(IMove move, int length, Point? landingPos)>();

            for (int x = 0; x < state.Tiles.GetLength(0); x++)
                for (int y = 0; y < state.Tiles.GetLength(1); y++)
                {
                    Tile? tile;
                    Point currentPos = new Point(x, y);
                    Point? landingPos = null;
                    int length = 0;
                    while (true)
                    {
                        if (state.TryGetTileAt(currentPos, out tile))
                        {
                            if (tile.Value.IsSolid())
                            {
                                break;
                            }

                            landingPos = currentPos;

                            if (tile.Value.HasAttribute(TileAttributes.GravDown))
                            {
                                currentPos = new Point(currentPos.X, currentPos.Y - 1);
                                length++;
                            }
                            else if (tile.Value.HasAttribute(TileAttributes.GravUp))
                            {
                                currentPos = new Point(currentPos.X, currentPos.Y + 1);
                                length++;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (length > 0)
                        moveStore.Add((new GravityAffectedMove(x, y), length, landingPos));
                }

            //Console.WriteLine(moveStore.ToJoinedString(",\n"));
            //throw new Exception();

            IMove[] moves = moveStore
                .GroupBy(t => t.landingPos) // group moves with same landing pos
                .Select(g => g.OrderByDescending(t => t.length).First()) // pick the longest move from the same-landing-pos group
                .Select(t => t.move) // extract move
                .ToArray();
            //Console.WriteLine(moves.ToJoinedString(", "));

            return moves;
        }
    }
}
