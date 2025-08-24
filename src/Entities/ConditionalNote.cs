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
    [Flags]
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
        public bool IsPositiveOrNeutral => (Polarity & (ConditionalNotePolarity.Neutral | ConditionalNotePolarity.Positive)) != 0;
        public bool IsNegative => Polarity == ConditionalNotePolarity.Negative;

        private Func<EntitySelection, bool> _isActive;

        public ConditionalNote(string text, Func<EntitySelection, bool> isActive, ConditionalNotePolarity polarity = ConditionalNotePolarity.Neutral)
        {
            _isActive = isActive;
            Text = text;
            Polarity = polarity;
        }
        public bool IsActive(EntitySelection info) => _isActive(info);

        //public static ConditionalNote GetEntityDependent<TOtherEntity>(string text, ConditionalNotePolarity polarity = ConditionalNotePolarity.Neutral) where TOtherEntity : Entity
        //{
        //    string _entityId = STOLON.Environment.GetEntityInstance<TOtherEntity>().Id;
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
