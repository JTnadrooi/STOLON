using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public enum BorderScalingStyle
    {
        Repeat,
        Stretch,
    }

    public sealed class Border
    {
        public Texture2D Source { get; }

        public int AddedHeight => PaddingTop + PaddingBottom;
        public int AddedWidth => PaddingLeft + PaddingRight;

        public int PaddingTop { get; }
        public int PaddingLeft { get; }
        public int PaddingBottom { get; }
        public int PaddingRight { get; }

        public Texture2DRegion TopRight { get; } // clockwise. \/
        public Texture2DRegion TopLeft { get; }
        public Texture2DRegion BottomRight { get; }
        public Texture2DRegion BottomLeft { get; }

        public Texture2DRegion Top { get; } // clockwise. \/
        public Texture2DRegion Right { get; }
        public Texture2DRegion Bottom { get; }
        public Texture2DRegion Left { get; }

        public Texture2DRegion Center { get; } // unused.

        public BorderScalingStyle ScalingStyle { get; }

        public const int RegionSize = 16;

        /// <summary>
        /// Initializes a new instance of the <see cref="Border"/> class using the same border thickness on all sides.
        /// </summary>
        /// <param name="source">The <see cref="Texture2D"/> containing the graphics for the border.</param>
        /// <param name="borderThickness">The thickness to apply to all four sides of the border. Border thickness is the difference between the outer edge of the border texture and the inside.</param>
        /// <param name="scalingStyle">The scaling style to use when rendering the border.</param>
        public Border(Texture2D source, int borderThickness, BorderScalingStyle scalingStyle = BorderScalingStyle.Stretch)
            : this(source, borderThickness, borderThickness, borderThickness, borderThickness, scalingStyle) { }

        public Border(Texture2D source, int borderThicknessUp, int borderThicknessLeft, int borderThicknessDown, int borderThicknessRight, BorderScalingStyle scalingStyle = BorderScalingStyle.Stretch)
        {
            Source = source;

            if (source.Width != 48) throw new InvalidOperationException($"Cannot create border from texture with invalid width; '{source.Width}' is not equal to 48.");
            if (source.Height != 48) throw new InvalidOperationException($"Cannot create border from texture with invalid height; '{source.Height}' is not equal to 48.");

            Texture2DAtlas atlas = Texture2DAtlas.Create(source.Name + "_border", source, RegionSize, RegionSize);

            TopLeft = atlas[0];
            Top = atlas[1];
            TopRight = atlas[2];

            Left = atlas[3];
            Center = atlas[4];
            Right = atlas[5];

            BottomLeft = atlas[6];
            Bottom = atlas[7];
            BottomRight = atlas[8];

            PaddingTop = borderThicknessUp;
            PaddingRight = borderThicknessRight;
            PaddingBottom = borderThicknessDown;
            PaddingLeft = borderThicknessLeft;

            ScalingStyle = scalingStyle;
        }

        public Vector2 GetCompensatingOffset()
        {
            return new Vector2(PaddingBottom, PaddingLeft);
        }
    }

    public static class BorderDrawingExtensions
    {
        public static void DrawBorderAround(this DrawingContext drawingContext, Border border, Rectangle innerBounds)
        {
            if (border.ScalingStyle == BorderScalingStyle.Repeat) throw new NotImplementedException();

            int cornerTopY = innerBounds.Y + innerBounds.Height - Border.RegionSize + border.PaddingTop;
            int cornerBottomY = innerBounds.Y - border.PaddingBottom;

            int borderOffsetX = innerBounds.X + Border.RegionSize - border.PaddingLeft;
            int borderOffsetY = innerBounds.Y + Border.RegionSize - border.PaddingBottom;

            float borderXStretch = (innerBounds.Width - Border.RegionSize * 2 + border.PaddingLeft + border.PaddingRight) / (float)Border.RegionSize;
            float borderYStretch = (innerBounds.Height - Border.RegionSize * 2 + border.PaddingTop + border.PaddingBottom) / (float)Border.RegionSize;

            drawingContext.Draw(border.TopLeft,
                new Vector2(innerBounds.X - border.PaddingLeft, cornerTopY));

            drawingContext.Draw(border.TopRight,
                new Vector2(innerBounds.X + innerBounds.Width - Border.RegionSize + border.PaddingRight, cornerTopY));

            drawingContext.Draw(border.BottomLeft,
                new Vector2(innerBounds.X - border.PaddingLeft, cornerBottomY));

            drawingContext.Draw(border.BottomRight,
                new Vector2(innerBounds.X + innerBounds.Width - Border.RegionSize + border.PaddingRight, cornerBottomY));

            // border drawing ensues.

            drawingContext.Draw(border.Top,
                new Vector2(borderOffsetX, cornerTopY), scale: new Vector2(borderXStretch, 1));

            drawingContext.Draw(border.Bottom,
                new Vector2(borderOffsetX, cornerBottomY), scale: new Vector2(borderXStretch, 1));

            drawingContext.Draw(border.Left,
                new Vector2(innerBounds.X - border.PaddingLeft, borderOffsetY), scale: new Vector2(1, borderYStretch));

            drawingContext.Draw(border.Right,
                new Vector2(innerBounds.X + innerBounds.Width - Border.RegionSize + border.PaddingRight, borderOffsetY), scale: new Vector2(1, borderYStretch));
        }

        public static void DrawBorderInside(this DrawingContext drawingContext, Border border, Rectangle outerBounds)
        {
            throw new NotImplementedException();
        }
    }
}
