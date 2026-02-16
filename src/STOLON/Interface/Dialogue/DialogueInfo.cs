using System.Diagnostics.CodeAnalysis;

namespace STOLON
{
    /// <summary>
    /// Represent a pushable dialogue prompt.
    /// </summary>
    public readonly struct DialogueInfo
    {
        /// <summary>
        /// The text this <see cref="DialogueInfo"/> holds.
        /// </summary>
        public string Text { get; }
        /// <summary>
        /// The milliseconds the <see cref="DialogueInfo"/> stagnates after all the <see cref="Text"/> is displayed.
        /// </summary>
        public int PostMilliseconds { get; }
        /// <summary>
        /// The initial <see cref="IDialogueProvider"/>.
        /// </summary>
        public IDialogueProvider Provider { get; }
        /// <summary>
        /// Create a new <see cref="DialogueInfo"/> with a set <see cref="IDialogueProvider"/> and <see cref="Text"/>.
        /// </summary>
        /// <param name="provider">The initial <see cref="IDialogueProvider"/>.</param>
        /// <param name="text">The text this <see cref="DialogueInfo"/> holds.</param>
        public DialogueInfo(IDialogueProvider provider, string text, int postMs = Textframe.PostReadMilliseconds)
        {
            Provider = provider;
            Text = text;
            PostMilliseconds = postMs;
        }
        public override bool Equals([NotNullWhen(true)] object? obj) => ToString() == (obj == null ? string.Empty : obj).ToString();
        public static bool operator ==(DialogueInfo left, DialogueInfo right) => left.Equals(right);
        public static bool operator !=(DialogueInfo left, DialogueInfo right) => !(left == right);
        public override int GetHashCode() => HashCode.Combine(Text.GetHashCode(), Provider.GetHashCode());
    }
}
