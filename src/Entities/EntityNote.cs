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
        public bool IsNegative { get; }
        public EntityNoteBase(string text, bool isNegative)
        {
            Text = text;
            IsNegative = isNegative;
        }
        public abstract bool IsActive(SelectionInfo info);
    }

    public sealed class EntityNote : EntityNoteBase
    {
        private Func<SelectionInfo, bool> _isActive;

        public EntityNote(string text, Func<SelectionInfo, bool> isActive, bool isNegative) : base(text, isNegative)
        {
            _isActive = isActive;
        }
        public override bool IsActive(SelectionInfo info) => _isActive(info);
    }

    public sealed class DependentEntityNote<TOtherEntity> : EntityNoteBase where TOtherEntity : Entity
    {
        private string _entityId;
        public DependentEntityNote(string text, bool isNegative) : base("When " + STOLON.Environment.GetEntityInstance<TOtherEntity>().Id + " is selected: " + text, isNegative)
        {
            _entityId = STOLON.Environment.GetEntityInstance<TOtherEntity>().Id;
        }

        public override bool IsActive(SelectionInfo info) => info.IsSelected(_entityId);
    }
}
