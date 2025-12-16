using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public class Shell : Service
    {
        private string _text;

        private Font2D _font;

        public Shell() : base(STOLON.Environment)
        {
            _text = string.Empty;
            _font = STOLON.Fonts.Small;
        }

        public void WriteLine(string text)
        {
            _text = text; // ADD FONT SYSTEM THAT ALWAYS DRAWS IT NORMALLY
        }

        public override void Update(int elapsedMilliseconds)
        {

        }

        public override void Draw(DrawingContext drawingContext)
        {
            int lines = _text.Count(c => c == '\n') + 1;
            drawingContext.DrawString(_font, _text, new Vector2(10, STOLON.V_HEIGHT - _font.Dimensions.Y - _font.Dimensions.Y * lines));
        }
    }
}
