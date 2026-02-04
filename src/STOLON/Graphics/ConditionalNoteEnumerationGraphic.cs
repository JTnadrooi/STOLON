namespace STOLON
{
    public class ConditionalNoteEnumerationGraphic : IUpdatable, IGraphic
    {
        public ConditionalNote[] Notes { get; set; }
        public Vector2 Pos { get; }
        public int TextWidth { get; }

        private CachedNoteData[] _cachedNotes;

        private string _counterStr;
        private Vector2 _counterPos;

        private readonly record struct CachedNoteData(string WrappedText, int LineCount, bool IsActive, Texture2D NoteSign);

        private const int NoteClearance = 12;
        private const int NoteBorderXClearance = 10;
        public const int TileSize = 128;

        public EntitySelection? Selection { get; set; }

        private readonly ITexture2DCollection _textures;
        private readonly IFont2DCollection _fonts;

        public ConditionalNoteEnumerationGraphic(Vector2 pos, int textWidth, ITexture2DCollection textures, IFont2DCollection fonts)
        {
            _textures = textures;
            _fonts = fonts;

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
                    noteSign = _textures[
                        isPosOrNeutral ? "UI\\note_sign_pos_enabled" : "UI\\note_sign_neg_enabled"
                    ];
                else noteSign = _textures["UI\\note_sign_disabled"];

                if (isPosOrNeutral) Count(ref activePosCount, ref totalPosCount);
                else Count(ref activeNegCount, ref totalNegCount);

                _cachedNotes[i] = new CachedNoteData(_fonts.Small.Wrap(Notes[i].Text, TextWidth - NoteBorderXClearance * 2 - NoteClearance, int.MaxValue, out var lc).ToUpper(),
                    lc,
                    isActive,
                    noteSign
                );
            }

            _counterStr = $"[{activePosCount}/{totalPosCount}] / [{activeNegCount}/{totalNegCount}]";
            _counterPos = Centering.CenterX((int)_fonts.Small.FastMeasure(_counterStr).X, 10, TileSize) + new Vector2(Pos.X, 0);
        }

        public void Draw(DrawingContext drawingContext)
        {
            int notesClearingUp = 7;
            int noteSpacing = _fonts.Small.CoreFont.LineHeight / 2;

            for (int i = 0; i < _cachedNotes.Length; i++)
            {
                CachedNoteData note = _cachedNotes[i];
                drawingContext.DrawString(_fonts.Small, note.WrappedText, new Vector2((int)Pos.X + NoteBorderXClearance + NoteClearance, (int)Pos.Y - notesClearingUp - note.LineCount * _fonts.Small.CoreFont.LineHeight));
                //drawingContext.DrawString(_fonts.Small, "-", new Vector2((int)Pos.X + NOTE_BORDER_X_CLEARANCE, (int)Pos.Y - notesClearingUp - _fonts.Small.CoreFont.LineHeight));
                drawingContext.Draw(note.NoteSign, new Vector2((int)Pos.X + NoteBorderXClearance, (int)Pos.Y - notesClearingUp - _fonts.Small.CoreFont.LineHeight));

                if (note.IsActive)
                    drawingContext.DrawRectangle(new Rectangle((int)Pos.X + 4, (int)Pos.Y - notesClearingUp - note.LineCount * _fonts.Small.CoreFont.LineHeight - 3, TextWidth - 8, note.LineCount * _fonts.Small.CoreFont.LineHeight + 6), thickness: 1);

                notesClearingUp += note.LineCount * _fonts.Small.CoreFont.LineHeight + noteSpacing;
            }

            drawingContext.Draw(_textures["UI\\dotted_line-128"], new Vector2(Pos.X, _fonts.Small.CoreFont.LineHeight + 20 - 1));

            drawingContext.DrawString(_fonts.Small, _counterStr, _counterPos);
        }
    }
}
