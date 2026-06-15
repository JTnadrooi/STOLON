using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace STOLON
{
    [Dependency(ServiceLifetime.Singleton)]
    public abstract class EntityDefinition : IDialogueProvider, IMipmapped, IEquatable<EntityDefinition>
    {
        /// <summary>
        /// Gets the full/display name of this <see cref="Entity"/>.
        /// </summary>
        public string FullName { get; }

        public EntityProfile Profile { get; }

        public IReadOnlyDictionary<int, Texture2D> Mipmaps => Profile.Mipmaps;

        public string Description { get; }

        /// <summary>
        /// Gets the unique Id of this <see cref="Entity"/>, no capitals.
        /// </summary>
        public string Id { get; }

        public string Name { get; }

        public string SymbolNotation { get; }

        public ConditionalNote[] AbilityNotes { get; }

        public ConditionalNote[] AllocationNotes { get; }

        public EntityDefinition(string id, string name, string symbolNotation, ITexture2DCollection textures, string? description = null, string? fullName = null)
        {
            Id = id;
            Name = name;
            SymbolNotation = symbolNotation;
            FullName = fullName ?? name;
            Description = description ?? string.Empty;

            Profile = ResolveProfile(textures);
            (AllocationNotes, AbilityNotes) = ResolveNotes();
        }

        protected virtual EntityProfile ResolveProfile(ITexture2DCollection textures)
            => new EntityProfile(Id, textures);

        protected virtual (ConditionalNote[] allocationNotes, ConditionalNote[] abilityNotes) ResolveNotes()
            => (Array.Empty<ConditionalNote>(), Array.Empty<ConditionalNote>());

        public bool Equals(EntityDefinition? other) => other is not null && other.Id == Id;

        public virtual int GetVirtualAllocation(EntitySelection info)
            => info.GetAllocation(Id);
    }

    /// <summary>
    /// Represent the character/other that can interact with the board. Be it as part of a group or solo.
    /// </summary>
    public abstract class Entity
    {
        public EntityDefinition Definition { get; }

        private IMoveProvider _moveProvider;
        public IMoveProvider MoveProvider => _moveProvider ?? throw new InvalidOperationException(this.GetType().Name + " instance has no MoveProvider.");

        public ImmutableArray<ConditionalNote> ActiveNotes { get; }

        public Entity(EntityDefinition definition, IMoveProvider moveProvider)
        {
            _moveProvider = moveProvider;

            Definition = definition;
        }

        public virtual bool HasWon(BoardState state)
        {
            throw new NotImplementedException();
        }

        public virtual ReadOnlySpan<IMove> GetAvailableMoves(BoardState state)
        {
            throw new NotImplementedException();
        }

        public virtual Entity NodeCopy()
        {
            return this;
        }
    }

    public static class EntityDrawingExtensions
    {
        public static void DrawEntity(this DrawingContext context, EntityDefinition entity, int res, Vector2 position, Vector2 scale, float rotation = 0f, Vector2? origin = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f, EntityDrawMode drawMode = EntityDrawMode.None)
            => context.DrawEntity(entity.Profile, res, position, scale, rotation, origin, effects, layerDepth, drawMode);
        public static void DrawEntity(this DrawingContext context, EntityDefinition entity, int res, Vector2 position, float scale = 1f, float rotation = 0f, Vector2? origin = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f, EntityDrawMode drawMode = EntityDrawMode.None)
            => context.DrawEntity(entity.Profile, res, position, scale, rotation, origin, effects, layerDepth, drawMode);
        public static void DrawEntity(this DrawingContext context, EntityProfile entityProfile, int res, Vector2 position, float scale = 1f, float rotation = 0f, Vector2? origin = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f, EntityDrawMode drawMode = EntityDrawMode.None)
            => context.DrawEntity(entityProfile, res, position, new Vector2(scale), rotation, origin, effects, layerDepth, drawMode);
        public static void DrawEntity(this DrawingContext context, EntityProfile entityProfile, int res, Vector2 position, Vector2 scale, float rotation = 0f, Vector2? origin = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f, EntityDrawMode drawMode = EntityDrawMode.None)
        {
            void DrawEntity(Texture2D texture, Vector2 position, Vector2 scale, float rotation = 0f, Vector2? origin = null, Rectangle? sourceRectangle = null, Color? color = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
            {
                if (drawMode == EntityDrawMode.WithBackground) context.DrawArea(new RectangleF(position, new Vector2(res) * scale).ToRectangle(), Color.Black);
                context.Draw(texture, position, scale, rotation, origin, sourceRectangle, color, effects, layerDepth);
            }
            Rectangle sourceRec;
            Texture2D? texture;
            if (entityProfile.TryGetMipmap(res, out texture))
                DrawEntity(texture!, position, scale, rotation, origin, null, null, effects, layerDepth);
            else
            {
                switch (res)
                {
                    case 128:
                        texture = entityProfile.Mipmaps[512];
                        sourceRec = new Rectangle(entityProfile.Focus - new Point(128), new Size(256, 256));
                        scale *= 0.25f;
                        DrawEntity(texture, position + (drawMode == EntityDrawMode.Menu ? entityProfile.MenuOffset : Point.Zero).ToVector2(), scale, rotation, origin, sourceRec, null, effects, layerDepth);
                        break;
                    default: throw new Exception();
                }
            }
        }

        public static void DrawSymbolNotation(this DrawingContext context, Font2D font, string symbolNotationStr, Rectangle bounds)
        {
            Vector2 dimensions = font.FastMeasure(symbolNotationStr);
            Vector2 scale = Vector2.One;

            context.DrawArea(bounds, Color.Black);
            context.DrawRectangle(bounds, Color.White, Interface.LineWidth);

            if (dimensions.X > bounds.Width - 10) scale = new Vector2(0.8f, 1);
            dimensions *= scale;

            Vector2 strPos = Centering.Center(dimensions.ToPoint(), bounds);
            NumberHelper.OnPixel(ref strPos);

            context.DrawString(font, symbolNotationStr, strPos, scale: scale);
        }
    }
}
