namespace STOLON
{
    /// <summary>
    /// Represents a <see cref="Func{TResult}"/> or <see cref="Action"/> with no parameters.
    /// </summary>
    public class DynamicTask
    {
        private Func<object?> func;

        /// <summary>
        /// Create a new <see cref="DynamicTask"/> from an <see cref="Action"/> <see langword="delegate"/>.
        /// </summary>
        /// <param name="action">The <see cref="Action"/> to create this <see cref="DynamicTask"/> from.</param>
        public DynamicTask(Action action) : this(() =>
        {
            action.Invoke();
            return null;
        })
        { }

        /// <summary>
        /// Create a new <see cref="DynamicTask"/> from an <see cref="Func{TResult}"/> <see langword="delegate"/>.
        /// </summary>
        /// <param name="function">The <see cref="Func{TResult}"/> to create this <see cref="DynamicTask"/> from.</param>
        public DynamicTask(Func<object?> function) => func = function;

        /// <summary>
        /// Run this <see cref="DynamicTask"/> and optionally get its return value.
        /// </summary>
        /// <returns>The return value of the <see cref="DynamicTask"/>, null if the task ran was an <see cref="Action"/>.</returns>
        public object? Run() => func.Invoke();

        /// <summary>
        /// Get this <see cref="DynamicTask"/> as a <see cref="Function{T}"/>.
        /// </summary>
        /// <returns>This <see cref="DynamicTask"/> as a <see cref="Function{T}"/>.</returns>
        public Func<object?> AsFunction() => func;

        public static explicit operator Func<object?>(DynamicTask t) => t.AsFunction();
    }
}
