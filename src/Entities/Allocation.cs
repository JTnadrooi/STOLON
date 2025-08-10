using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public static class AllocationHelpers
    {
        public struct AllocationHelperChain
        {
            private int _initialAlloc;
            private List<int> _deltas;

            public AllocationHelperChain(int initialAlloc)
            {
                _initialAlloc = initialAlloc;
                _deltas = new List<int>();
            }

            public AllocationHelperChain ApplyMultiplier(bool shouldApply, float multiplier) => Apply(shouldApply, valloc => (int)(valloc * (multiplier - 1f)));
            public AllocationHelperChain ApplyFlat(bool shouldApply, int addition) => Apply(shouldApply, valloc => valloc + addition);
            public AllocationHelperChain Apply(bool shouldApply, Func<int, int> eval)
            {
                if (shouldApply) _deltas.Add(eval(_initialAlloc));
                return this;
            }

            public int End()
            {
                return _deltas.Sum() + _initialAlloc;
            }
        }

        public static AllocationHelperChain Start(SelectionInfo info, Entity entity) => new AllocationHelperChain(info.GetAllocation(entity.Id));
    }
}
