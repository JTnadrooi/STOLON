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
    public enum EntityNotePolarity
    {
        Neutral,
        Positive,
        Negative,
    }

    public sealed class EntityNote
    {

        public string Text { get; }
        public EntityNotePolarity Polarity { get; }

        private Func<SelectionInfo, bool> _isActive;

        public EntityNote(string text, Func<SelectionInfo, bool> isActive, EntityNotePolarity polarity = EntityNotePolarity.Neutral)
        {
            _isActive = isActive;
            Text = text;
            Polarity = polarity;
        }
        public bool IsActive(SelectionInfo info) => _isActive(info);

        public static EntityNote GetDependentEntityNote<TOtherEntity>(string text, EntityNotePolarity polarity = EntityNotePolarity.Neutral) where TOtherEntity : Entity
        {
            string _entityId = STOLON.Environment.GetEntityInstance<TOtherEntity>().Id;
            return new EntityNote($"{text} when {_entityId} is selected.", i => i.IsSelected(_entityId), polarity);
        }
    }
}
