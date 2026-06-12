using System.Diagnostics.CodeAnalysis;

namespace STOLON
{
    public class SiloEntity : Entity
    {
        public SiloEntity(ITexture2DCollection textures, IMoveProvider? moveProvider = null) : base("silo", "Silo", "Sl", textures, "Silo desc", "Silo 28SHA", moveProvider: moveProvider)
        { }

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

        public override bool HasWon(BoardState state)
        {
            return false;
        }

        public override IMove[] GetAvailableMoves(BoardState state)
        {
            return [];
        }
    }
}
