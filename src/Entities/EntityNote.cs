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
    public abstract class EntityNoteBase
    {
        public string Text { get; }
        public EntityNoteBase(string text)
        {
            Text = text;
        }
        public abstract bool IsEffective(SelectionInfo info);
    }

    public sealed class EntityNote : EntityNoteBase
    {
        private Func<SelectionInfo, bool> _isEffective;

        public EntityNote(string text, Func<SelectionInfo, bool> isEffective) : base(text)
        {
            _isEffective = isEffective;
        }
        public override bool IsEffective(SelectionInfo info) => _isEffective(info);
    }

    public sealed class DependentEntityNote<TOtherEntity> : EntityNoteBase where TOtherEntity : Entity
    {
        private string _entityId;
        public DependentEntityNote(string text) : base("When " + STOLON.Environment.GetEntityInstance<TOtherEntity>().Id + " is selected: " + text)
        {
            _entityId = STOLON.Environment.GetEntityInstance<TOtherEntity>().Id;
        }

        public override bool IsEffective(SelectionInfo info) => info.IsSelected(_entityId);
    }
}
