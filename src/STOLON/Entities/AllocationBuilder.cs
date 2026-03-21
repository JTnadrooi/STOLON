namespace STOLON
{
    public readonly struct AllocationBuilder
    {
        private readonly int _initialAllocation;
        private readonly List<int> _deltas;

        /// <summary>
        /// Initializes a new instance of the <see cref="AllocationBuilder"/> struct using the initial allocation from the specified <paramref name="entity"/>.
        /// </summary>
        public AllocationBuilder(EntitySelection selection, Entity entity) : this(selection.GetAllocation(entity.Id)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AllocationBuilder"/> struct with the specified initial allocation value.
        /// </summary>
        public AllocationBuilder(int initialAllocation)
        {
            _initialAllocation = initialAllocation;
            _deltas = new List<int>();
        }

        /// <summary>
        /// Applies a multiplier if the specified condition is <see langword="true"/>.
        /// </summary>
        /// <param name="condition">If <see langword="true"/>, the multiplier will be applied; otherwise, no change occurs.</param>
        /// <param name="multiplier">The multiplier value to apply (e.g., 1.5f increases by 50%, 0.5f decreases by 50%).</param>
        /// <returns>The current <see cref="AllocationBuilder"/> instance for method chaining.</returns>
        public AllocationBuilder ApplyMultiplierWhen(bool condition, float multiplier) => ApplyWhen(condition, valloc => (int)(valloc * (multiplier - 1f)));

        /// <summary>
        /// Applies a flat additive adjustment to the allocation if the specified condition is <see langword="true"/>.
        /// </summary>
        /// <param name="condition">If <see langword="true"/>, the addition will be applied; otherwise, no change occurs.</param>
        /// <param name="addition">The value to add to the allocation.</param>
        /// <returns>The current <see cref="AllocationBuilder"/> instance for method chaining.</returns>
        public AllocationBuilder ApplyFlatWhen(bool condition, int addition) => ApplyWhen(condition, valloc => valloc + addition);

        /// <summary>
        /// Applies a custom adjustment to the allocation if the specified condition is <see langword="true"/>.
        /// </summary>
        /// <param name="condition">If <see langword="true"/>, the evaluation function will be applied; otherwise, no change occurs.</param>
        /// <param name="evaluator">A function that takes the initial allocation and returns an adjusted delta value.</param>
        /// <returns>The current <see cref="AllocationBuilder"/> instance for method chaining.</returns>
        public AllocationBuilder ApplyWhen(bool condition, Func<int, int> evaluator)
        {
            if (condition) _deltas.Add(evaluator.Invoke(_initialAllocation));
            return this;
        }

        /// <summary>
        /// Calculates the final allocation by summing all accumulated deltas and adding them to the initial allocation.
        /// </summary>
        /// <returns>The final allocation value after applying all adjustments.</returns>
        public int End() => End(deltas => deltas.Sum());

        /// <summary>
        /// Calculates the final allocation value using a custom evaluation function to combine the deltas.
        /// </summary>
        /// <param name="deltaEvaluator">A function that takes an array of delta values and returns a combined result.</param>
        /// <returns>The final allocation value after applying the custom delta evaluation and adding the initial allocation.</returns>
        public int End(Func<int[], int> deltaEvaluator) => deltaEvaluator.Invoke(_deltas.ToArray()) + _initialAllocation;

        public override string ToString() => $"[{(_deltas.Count == 0 ? _initialAllocation.ToString() : _deltas.ToJoinedString(", "))}]";
    }
}