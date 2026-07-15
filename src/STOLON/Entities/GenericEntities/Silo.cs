using System.Diagnostics.CodeAnalysis;

namespace STOLON
{
    public class SiloEntityDefinition : EntityDefinition
    {
        private readonly ITexture2DCollection _textures;

        public SiloEntityDefinition(ITexture2DCollection textures) : base("silo", "Silo", "Sl", textures, "Silo desc", "Silo 28SHA")
        {
            _textures = textures;
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

        public override Entity GetDefaultEntity(IMoveProvider moveProvider)
        {
            return new SiloEntity(this, _textures, moveProvider);
        }
    }

    public class SiloEntity : Entity
    {
        internal int LastAbilityUse;

        public SiloEntity(EntityDefinition definition, ITexture2DCollection textures, IMoveProvider moveProvider) : base(definition, moveProvider)
        {
            LastAbilityUse = int.MinValue;
        }

        public override bool HasWon(BoardState state)
        {
            return state.SearchFor(SearchTarget.GetDefaultTargets(), state.GetEntityIndex(this));
        }

        public override IMove[] GetAvailableMoves(BoardState state)
        {
            List<IMove> availableMoves = GravityAffectedMove.GetUniqueMoves(state).ToList();

            //if (state._currentPlayerIndex == 0)
            //{
            //    Console.WriteLine(LastAbilityUse + " " + state.CurrentMoveIndex);
            //}

            if (state.CurrentMoveIndex > LastAbilityUse + 1 * state.PlayerCount)
            {
                for (int i = 0; i < state.Dimensions.X; i++)
                {
                    availableMoves.Add(new ColumnDisableMove(i));
                }
            }

            return availableMoves.ToArray();
        }
    }
}
