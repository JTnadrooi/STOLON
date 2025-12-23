
using Betwixt;

namespace STOLON
{
    public struct DialogueDrawArgs
    {
        public DialogueInfo Source { get; }
        public int[] TimeMap { get; }
        public int PostTime { get; }
        public DialogueDrawArgs(DialogueInfo source, int[] timeMap, int postTime)
        {
            Source = source;
            TimeMap = timeMap;
            PostTime = postTime;
        }
        public static DialogueDrawArgs FromInfo(DialogueInfo info)
        {
            return new DialogueDrawArgs(info, new int[info.Text.Length].Select((item, i) => info.Text[i] switch
            {
                '.' => Textframe.CHAR_READ_MILLISECONDS * 3,
                '?' => Textframe.CHAR_READ_MILLISECONDS * 3,
                _ => Textframe.CHAR_READ_MILLISECONDS,
            }).ToArray(), info.PostMilliseconds);
        }
    }
    public class Textframe : Service, ITextframe, ISingletonDependency
    {
        private Queue<DialogueInfo> _dialogueQueue;
        private Rectangle _dialoguebounds;
        private DialogueInfo? _currentDialogue;
        private DialogueDrawArgs? _currentDialogueDrawArgs;
        private Point _dialogueTextPos;
        private string _toDrawDialogueText;
        private Font2D _font;

        private Point _providerTextPos;
        private float _providerTextScaleCoefficient;
        private Tweener<float>? _providerTextSizeTweener;

        private float _dialogueShowCoefficient;
        private bool _awaitingMouseDialogueHover;
        private bool _hide;
        public bool Hide
        {
            get => _hide;
            set => _hide = value;
        }

        public Rectangle DialogueBounds => _dialoguebounds;
        public const int CHAR_READ_MILLISECONDS = 75; // per char
        public const int POST_READ_MILLISECONDS = CHAR_READ_MILLISECONDS * 10; // how long the dialogue stagnates after its finished.

        private int _msSinceLastChar;
        private int _charsRead;
        private int _postTimeRead;

        public const int BOX_W = 384;
        public const int BOX_H = 96;
        public const int BOX_OFFSET_Y = 10;

        private readonly IRichLogger _logger;
        private readonly IFont2DCollection _fonts;
        private readonly IInputManager _input;

        public Textframe(IRichLogger logger, IFont2DCollection fonts, IInputManager input) : base(null)
        {
            _logger = logger;
            _fonts = fonts;

            _dialogueQueue = new Queue<DialogueInfo>();
            _dialogueTextPos = Point.Zero;
            _currentDialogue = null;
            _currentDialogueDrawArgs = null;
            _dialogueShowCoefficient = 0f;
            _msSinceLastChar = 0;
            _charsRead = 0;
            _toDrawDialogueText = string.Empty;
            _font = _fonts.Medium;
            _input = input;
        }
        public void Queue(DialogueInfo[] dialogue)
        {
            for (int i = 0; i < dialogue.Length; i++)
                Queue(dialogue[i]);
        }
        public void Queue(DialogueInfo dialogue)
        {
            _dialogueQueue.Enqueue(dialogue);
            _logger.Log("dialogue queued with text: " + dialogue.Text);
        }
        public void Next()
        {
            if (_dialogueQueue.Count == 0) throw new Exception();

            _logger.Log(">attempting dequeuing of dialogue with text: " + _dialogueQueue.Peek().Text);
            bool providerDiffers = _currentDialogue.HasValue && _currentDialogue.Value.Provider.Name != _dialogueQueue.Peek().Provider.Name;
            if (providerDiffers) _logger.Log("dialogue has new provider of name: " + _dialogueQueue.Peek().Provider.Name);

            _currentDialogue = _dialogueQueue.Dequeue();
            _currentDialogueDrawArgs = DialogueDrawArgs.FromInfo(_currentDialogue.Value);

            _toDrawDialogueText = string.Empty;
            _msSinceLastChar = 0;
            _charsRead = 0;
            _postTimeRead = 0;

            //initialDialogueMilliseconds = GetMillisecondsFromText(currentDialogue.Value.Text) + currentDialogue.Value.ExtraMS;
            //dialogueMillisecondsRemaining = initialDialogueMilliseconds;

            _awaitingMouseDialogueHover = true;
            if (providerDiffers || _providerTextSizeTweener == null) // initialize or refresh.
            {
                _providerTextSizeTweener = new Tweener<float>(0.0001f, 1f, 1, Ease.Sine.Out); // 0 does not work with size calculations..
                _providerTextSizeTweener.Start();
            }

            _logger.Success();
        }

        //public void Queue(int count, Func<string, int, string>? selector = null)
        //{
        //    STOLON.Debug.Log(">mass queueing a stream of size: " + count);
        //    selector ??= new Func<string, int, string>((s, i) => s);
        //    //for (int i = 0; i < count; i++) Queue(new DialogueInfo(StolonEnvironment.Instance, selector.Invoke((()StolonGame.Instance.Environment.GameStateManager.Current).GetRandomSplashText(), i)));
        //    throw new NotImplementedException();
        //    STOLON.Debug.Success();
        //}

        public override void Update(int elapsedMilliseconds)
        {
            bool textFrameGoUp = false;

            _msSinceLastChar += elapsedMilliseconds;

            if (_dialogueQueue.Count > 0 && !_currentDialogue.HasValue) Next();

            if (_currentDialogue.HasValue) // dialogue is here!
            {
                if (_currentDialogue.Value.Text.Length == 0) throw new Exception("Text size zero.");

                if (_toDrawDialogueText == _currentDialogue.Value.Text) // if no text is left to add..
                {
                    _postTimeRead += elapsedMilliseconds; // only add postread if text is full.
                    if (_dialogueQueue.Count > 0 && _postTimeRead > _currentDialogueDrawArgs!.Value.PostTime) Next(); // ..and queue is full, go next.
                }
                else if (_msSinceLastChar > _currentDialogueDrawArgs!.Value.TimeMap[_charsRead]) // else if its time for a new char..
                {
                    _toDrawDialogueText += _currentDialogue.Value.Text[_charsRead]; // ..add said char.
                    _charsRead++;
                    _msSinceLastChar = 0;
                }

                _providerTextSizeTweener!.Update(elapsedMilliseconds / 1000f);
                _providerTextScaleCoefficient = MathF.Min(_providerTextSizeTweener.Value, _dialogueShowCoefficient > 0.9f ? 1f : _dialogueShowCoefficient);

                _dialogueTextPos = _dialoguebounds.Location
                    + new Point((int)(_dialoguebounds.Width / 2f - _font.FastMeasure(_toDrawDialogueText).X / 2f),
                    (int)(_dialoguebounds.Height / 2f));

                _providerTextPos = _dialoguebounds.Location
                    + new Point((int)(_dialoguebounds.Width / 2f - _font.FastMeasure(_currentDialogue.Value.Provider.Name).X * _providerTextScaleCoefficient / 2f), (int)(BOX_H - _font.Dimensions.Y - 5));
            }

            if (_awaitingMouseDialogueHover) textFrameGoUp = true;
            if (DialogueBounds.Contains(_input.VirtualMousePos) && !_hide)
            {
                _awaitingMouseDialogueHover = false;
                textFrameGoUp = true;
            }
            else if (!_awaitingMouseDialogueHover)
            {
                textFrameGoUp = false;
            }

            DynamicTweening.PushSubunitary(ref _dialogueShowCoefficient, textFrameGoUp, elapsedMilliseconds, smoothness: 2);
            _dialogueShowCoefficient = Math.Clamp(_dialogueShowCoefficient, 0.1f, 1f);

            _dialoguebounds = new Rectangle(
                (int)Centering.CenterX(BOX_W, 0, STOLON.V_WIDTH).X,
                (int)((BOX_H * _dialogueShowCoefficient - BOX_H) + BOX_OFFSET_Y) - (_hide ? 100 : 0),
                BOX_W, BOX_H
            );
        }
        public int GetMillisecondsFromText(string text)
        {
            return text.Length * CHAR_READ_MILLISECONDS;
        }
        public override void Draw(DrawingContext drawingContext)
        {
            drawingContext.DrawArea(_dialoguebounds, Color.Black);
            if (_currentDialogue.HasValue)
            {
                drawingContext.DrawString(_font, _toDrawDialogueText, _dialogueTextPos.ToVector2());
                drawingContext.DrawString(_font, _currentDialogue.Value.Provider.Name.ToUpper(), _providerTextPos.ToVector2(), _providerTextScaleCoefficient);
            }
            drawingContext.DrawRectangle(_dialoguebounds, Color.White, Interface.LINE_WIDTH);

            base.Draw(drawingContext);
        }
    }
}