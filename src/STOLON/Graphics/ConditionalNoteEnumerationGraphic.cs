using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public class ConditionalNoteEnumerationGraphic : IGraphic
    {
        public ConditionalNote[] Notes { get; set; }
        public Vector2 Pos { get; }
        public int TextWidth { get; }

        private CachedNoteData[] _cachedNotes;

        private string _counterStr;
        private Vector2 _counterPos;

        private readonly record struct CachedNoteData(string WrappedText, int LineCount, bool IsActive, Texture2D NoteSign);

        private const int NOTE_CLEARANCE = 12;
        private const int NOTE_BORDER_X_CLEARANCE = 10;
        public const int TILE_SIZE = 128;

        public EntitySelection? Selection { get; set; }

        public ConditionalNoteEnumerationGraphic(Vector2 pos, int textWidth)
        {
            Notes = Array.Empty<ConditionalNote>();
            Pos = pos;
            TextWidth = textWidth;
            _counterStr = string.Empty;
            _cachedNotes = Array.Empty<CachedNoteData>();
        }

        public void Update(int elapsedMilliseconds)
        {
            int activePosCount = 0;
            int activeNegCount = 0;
            int totalPosCount = 0;
            int totalNegCount = 0;

            if (Notes.Length != _cachedNotes.Length) _cachedNotes = new CachedNoteData[Notes.Length];
            for (int i = 0; i < Notes.Length; i++)
            {
                bool isActive = Selection is null ? false : Notes[i].IsActive(Selection);
                void Count(ref int active, ref int total)
                {
                    if (isActive) active++;
                    total++;
                }
                Texture2D noteSign;
                var note = Notes[i];
                bool isPosOrNeutral = note.IsPositiveOrNeutral;

                if (isActive)
                    noteSign = STOLON.Textures[
                        isPosOrNeutral ? "UI\\note_sign_pos_enabled" : "UI\\note_sign_neg_enabled"
                    ];
                else noteSign = STOLON.Textures["UI\\note_sign_disabled"];

                if (isPosOrNeutral) Count(ref activePosCount, ref totalPosCount);
                else Count(ref activeNegCount, ref totalNegCount);

                _cachedNotes[i] = new CachedNoteData(STOLON.Fonts.Small.Wrap(Notes[i].Text, TextWidth - NOTE_BORDER_X_CLEARANCE * 2 - NOTE_CLEARANCE, int.MaxValue, out var lc).ToUpper(),
                    lc,
                    isActive,
                    noteSign
                );
            }

            _counterStr = $"[{activePosCount}/{totalPosCount}] / [{activeNegCount}/{totalNegCount}]";
            _counterPos = Centering.CenterX((int)STOLON.Fonts.Small.FastMeasure(_counterStr).X, 10, TILE_SIZE) + new Vector2(Pos.X, 0);
        }

        public void Draw(DrawingContext drawingContext)
        {
            int notesClearingUp = 7;
            int noteSpacing = STOLON.Fonts.Small.CoreFont.LineHeight / 2;

            for (int i = 0; i < _cachedNotes.Length; i++)
            {
                CachedNoteData note = _cachedNotes[i];
                drawingContext.DrawString(STOLON.Fonts.Small, note.WrappedText, new Vector2((int)Pos.X + NOTE_BORDER_X_CLEARANCE + NOTE_CLEARANCE, (int)Pos.Y - notesClearingUp - note.LineCount * STOLON.Fonts.Small.CoreFont.LineHeight));
                //drawingContext.DrawString(STOLON.Fonts.Small, "-", new Vector2((int)Pos.X + NOTE_BORDER_X_CLEARANCE, (int)Pos.Y - notesClearingUp - STOLON.Fonts.Small.CoreFont.LineHeight));
                drawingContext.Draw(note.NoteSign, new Vector2((int)Pos.X + NOTE_BORDER_X_CLEARANCE, (int)Pos.Y - notesClearingUp - STOLON.Fonts.Small.CoreFont.LineHeight));

                if (note.IsActive)
                    drawingContext.DrawRectangle(new Rectangle((int)Pos.X + 4, (int)Pos.Y - notesClearingUp - note.LineCount * STOLON.Fonts.Small.CoreFont.LineHeight - 3, TextWidth - 8, note.LineCount * STOLON.Fonts.Small.CoreFont.LineHeight + 6), thickness: 1);

                notesClearingUp += note.LineCount * STOLON.Fonts.Small.CoreFont.LineHeight + noteSpacing;
            }

            drawingContext.Draw(STOLON.Textures["UI\\dotted_line-128"], new Vector2(Pos.X, STOLON.Fonts.Small.CoreFont.LineHeight + 20 - 1));

            drawingContext.DrawString(STOLON.Fonts.Small, _counterStr, _counterPos);
        }
    }
}
