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
    public sealed class EntityNote
    {
        public string Text { get; }
        public bool IsNegative { get; }

        private Func<SelectionInfo, bool> _isActive;

        public EntityNote(string text, Func<SelectionInfo, bool> isActive, bool isNegative)
        {
            _isActive = isActive;
            Text = text;
            IsNegative = isNegative;
        }
        public bool IsActive(SelectionInfo info) => _isActive(info);

        public static EntityNote GetDependentEntityNote<TOtherEntity>(string text, bool isNegative) where TOtherEntity : Entity
        {
            string _entityId = STOLON.Environment.GetEntityInstance<TOtherEntity>().Id;
            return new EntityNote($"When {_entityId} is selected: {text}", i => i.IsSelected(_entityId), isNegative);
        }
    }
}
