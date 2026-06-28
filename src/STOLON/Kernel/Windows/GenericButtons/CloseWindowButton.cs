namespace STOLON
{
    public sealed class CloseWindowButton : WindowButton
    {
        private Texture2D _hoverTexture;

        public CloseWindowButton(ITexture2DCollection textures) : base(textures["UI\\Window\\window_button_close"], 0)
        {
            _hoverTexture = textures["UI\\Window\\window_button_close-inverted"];
        }

        protected override void OnDefault(Window source)
        {
            Texture = InitialTexture;
        }

        protected override void OnHover(Window source)
        {
            Texture = _hoverTexture;
        }

        protected override void OnClick(Window source)
        {
            source.Close();
        }
    }
}
