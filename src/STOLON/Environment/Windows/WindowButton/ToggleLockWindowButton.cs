namespace STOLON
{
    public sealed class ToggleLockWindowButton : WindowButton
    {
        private Texture2D _hoverTexture;
        private Texture2D _hoverTextureToggled;

        private Texture2D _initialTextureToggled;

        public ToggleLockWindowButton(ITexture2DCollection textures) : base(textures["UI\\Window\\window_button_lock"], 1)
        {
            _hoverTexture = textures["UI\\Window\\window_button_lock-inverted"];
            _hoverTextureToggled = textures["UI\\Window\\window_button_unlock-inverted"];

            _initialTextureToggled = textures["UI\\Window\\window_button_unlock"];
        }

        protected override void OnDefault(Window source)
        {
            if (Window.IsLocked) Texture = _initialTextureToggled;
            else Texture = InitialTexture;
        }

        protected override void OnHover(Window source)
        {
            if (Window.IsLocked) Texture = _hoverTextureToggled;
            else Texture = _hoverTexture;
        }

        protected override void OnClick(Window source)
        {
            if (Window.IsLocked)
            {
                Window.Unlock();
                Window.Position += new Vector2(10);
            }
            else Window.Lock();
        }
    }
}
