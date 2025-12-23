namespace STOLON
{
    public enum ConditionalNotePolarity
    {
        Neutral = 0,
        Positive = 1,
        Negative = 2,
    }

    public sealed class ConditionalNote
    {
        public string Text { get; }
        public ConditionalNotePolarity Polarity { get; }
        public bool IsPositiveOrNeutral => Polarity == ConditionalNotePolarity.Neutral || Polarity == ConditionalNotePolarity.Positive;
        public bool IsNegative => Polarity == ConditionalNotePolarity.Negative;
        public Entity Source { get; }

        private Func<EntitySelection, bool> _isActive;

        public ConditionalNote(Entity source, string text, Func<EntitySelection, bool> isActive, ConditionalNotePolarity polarity = ConditionalNotePolarity.Neutral)
        {
            _isActive = isActive;
            Text = text;
            Polarity = polarity;
            Source = source;
        }
        public bool IsActive(EntitySelection info) => _isActive(info) && info.Contains(Source.Id);

        //public static ConditionalNote GetEntityDependent<TOtherEntity>(string text, ConditionalNotePolarity polarity = ConditionalNotePolarity.Neutral) where TOtherEntity : Entity
        //{
        //    string _entityId = _environment.GetEntityInstance<TOtherEntity>().Id;
        //    return new ConditionalNote($"{text} when {_entityId} is selected.", i => i.IsSelected(_entityId), domain, polarity);
        //}
        public static ConditionalNoteBuilder Build() => new ConditionalNoteBuilder();
    }

    public class ConditionalNoteBuilder
    {
        public ConditionalNoteBuilder()
        {
            throw new NotImplementedException();
        }
    }
}
