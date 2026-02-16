namespace STOLON
{
    /// <summary>
    /// Provides a way to make a object responsible for dialogue. 
    /// </summary>
    public interface IDialogueProvider
    {
        /// <summary>
        /// The symbolnotation of this <see cref="Entity"/>, example: DL for Deadline.
        /// </summary>
        public string SymbolNotation { get; }
        /// <summary>
        /// The name of this <see cref="Entity"/>, example: Deadline.
        /// </summary>
        public string Name { get; }
    }
}
