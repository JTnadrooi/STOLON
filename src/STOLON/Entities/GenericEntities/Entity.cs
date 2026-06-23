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

        public virtual IMove[] GetAvailableMoves(BoardState state)
        {
            throw new NotImplementedException();
        }

        public virtual Entity NodeCopy()
        {
            return this;
        }

        public static IMove[] GetUniqueGravityAffectedMoves(BoardState state)
        {
            List<(IMove move, int length, Point? landingPos)> moveStore = new List<(IMove move, int length, Point? landingPos)>();

            for (int x = 0; x < state.Tiles.GetLength(0); x++)
                for (int y = 0; y < state.Tiles.GetLength(1); y++)
                {
                    Tile? tile;
                    Point currentPos = new Point(x, y);
                    Point? landingPos = null;
                    int length = 0;
                    while (true)
                    {
                        if (state.TryGetTileAt(currentPos, out tile))
                        {
                            if (tile.Value.IsSolid() || tile.Value.HasAttribute(TileAttributes.Disabled))
                            {
                                break;
                            }

                            landingPos = currentPos;

                            if (tile.Value.HasAttribute(TileAttributes.GravDown))
                            {
                                currentPos = new Point(currentPos.X, currentPos.Y - 1);
                                length++;
                            }
                            else if (tile.Value.HasAttribute(TileAttributes.GravUp))
                            {
                                currentPos = new Point(currentPos.X, currentPos.Y + 1);
                                length++;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (length > 0)
                        moveStore.Add((new GravityAffectedMove(x, y), length, landingPos));
                }

            //Console.WriteLine(moveStore.ToJoinedString(",\n"));
            //throw new Exception();

            IMove[] moves = moveStore
                .GroupBy(t => t.landingPos) // group moves with same landing pos
                .Select(g => g.OrderByDescending(t => t.length).First()) // pick the longest move from the same-landing-pos group
                .Select(t => t.move) // extract move
                .ToArray();
            //Console.WriteLine(moves.ToJoinedString(", "));

            return moves;
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
