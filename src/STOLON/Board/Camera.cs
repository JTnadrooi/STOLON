namespace STOLON
{
    public class Camera2D
    {
        private float _zoom;

        public Point Dimensions;
        public Vector2 Position;
        public float MaxZoom;
        public float MinZoom;
        public float Rotation;

        public Vector2 AntiScale => new Vector2(1f / Zoom);

        public Matrix Projection => Matrix.CreateOrthographicOffCenter(0, ScreenSize.X, ScreenSize.Y, 0, -1, 1);

        public Rectangle ScreenRectangle => new Rectangle(0, 0, Dimensions.X, Dimensions.Y);

        public Vector2 ScreenSize => new Vector2(Dimensions.X, Dimensions.Y);

        public float Zoom
        {
            get => _zoom;
            set
            {
                _zoom = value;
                if (_zoom < MinZoom) _zoom = MinZoom;
                if (_zoom > MaxZoom) _zoom = MaxZoom;
            }
        }

        public Matrix View => Matrix.CreateTranslation(-Position.X, -Position.Y, 0) *
                                Matrix.CreateScale(Zoom) *
                                Matrix.CreateRotationZ(Rotation) *
                                Matrix.CreateTranslation(ScreenSize.X / 2, ScreenSize.Y / 2, 0);

        public Camera2D(Point? dimensions = null)
        {
            Dimensions = dimensions ?? STOLON.Instance.GetVirtualDimensions();

            MinZoom = 0.1f;
            MaxZoom = 100f;
            Rotation = 0;

            Zoom = 1;
        }

        public Vector2 Project(Vector2 worldPosition)
        {
            return (worldPosition - Position) * Zoom + ScreenSize / 2;
        }

        public Vector2 Unproject(Vector2 screenPosition)
        {
            return Position + (screenPosition - ScreenSize / 2) / Zoom;
        }

        public void OnPixel(ref Vector2 worldPosition)
        {
            Vector2 screenPosition = Project(worldPosition);
            NumberHelper.OnPixel(ref screenPosition);
            worldPosition = Unproject(screenPosition);
        }

        public override string ToString() => string.Format("Camera, pos: {0} area: {1}", Position, GetVisibleArea());

        public Rectangle GetVisibleArea()
        {
            Vector2 r = Unproject(Vector2.Zero);
            return new Rectangle((int)r.X, (int)r.Y, (int)(ScreenSize.X / Zoom), (int)(ScreenSize.Y / Zoom));
        }
    }
}
