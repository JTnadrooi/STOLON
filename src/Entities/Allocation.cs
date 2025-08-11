using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AsitLib;

namespace STOLON
{
    public readonly struct AllocationHelperChain
    {
        private readonly int _initialAlloc;
        private readonly List<int> _deltas;

        public IReadOnlyList<int> Deltas => _deltas;

        public AllocationHelperChain(SelectionInfo info, Entity entity) : this(info.GetAllocation(entity.Id)) { }
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

        public int End() => End(ia => ia.Sum());
        public int End(Func<int[], int> deltaEval) => deltaEval(_deltas.ToArray()) + _initialAlloc;
        public override string ToString() => $"[{(_deltas.Count == 0 ? _initialAlloc.ToString() : _deltas.ToJoinedString(", "))}]";
    }
}