using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public sealed class SelectionWindow : Window
    {
        public SelectionWindow(WindowDependencies deps) : base(deps, 160, 120)
        {
            IsResizable = false;
        }

        protected override void DrawContents(DrawingContext drawingContext)
        {

        }
    }
}
