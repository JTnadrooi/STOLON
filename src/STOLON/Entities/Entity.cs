using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public enum EntityDrawMode
    {
        None = 0,
        Menu = 1,
        WithBackground = 2,
    }
    public class EntityProfile : IMipmapped
    {
        private Dictionary<int, Texture2D> mipmaps;
        private Point _focus;
        private Point _menuOffset;

        public Texture2D Texture512 => Mipmaps[512];
        public Texture2D Texture256 => this.TryGetMipmap(256, out Texture2D? t) ? t! : throw new Exception();
        public Texture2D Texture128 => this.TryGetMipmap(128, out Texture2D? t) ? t! : throw new Exception();

        public IReadOnlyDictionary<int, Texture2D> Mipmaps => mipmaps;
        public Point Focus { get => _focus; set => _focus = value; }
        public Point MenuOffset { get => _menuOffset; set => _menuOffset = value; }

        public EntityProfile(string entityName, Point? focus = null, Point? menuOffset = null) : this(
            STOLON.Textures.TryGetValue($"Entities\\{entityName}\\{entityName}-512", out Texture2D? val512) ? val512 : throw new Exception(),
            STOLON.Textures.TryGetValue($"Entities\\{entityName}\\{entityName}-256", out Texture2D? val256) ? val256 : null,
            STOLON.Textures.TryGetValue($"Entities\\{entityName}\\{entityName}-128", out Texture2D? val128) ? val128 : null,
            focus)
        { }
        public EntityProfile(Texture2D t512, Texture2D? t256 = null, Texture2D? t128 = null, Point? focus = null, Point? menuOffset = null)
        {
            mipmaps = new Dictionary<int, Texture2D>();
            mipmaps[512] = t512;
            if (t256 != null) mipmaps[256] = t256;
            if (t128 != null) mipmaps[128] = t128;
            _focus = focus ?? Centering.Get(t512).ToPoint();
            _menuOffset = menuOffset ?? Point.Zero;
        }

        public static EntityProfile Debug => new EntityProfile(
            STOLON.Textures[$"Debug\\temp-512"],
            null,
            STOLON.Textures[$"Debug\\profile-128"]);
        public static EntityProfile GetDebug(Texture2D? t512, Texture2D? t256 = null, Texture2D? t128 = null, Point? focus = null, Point? menuOffset = null)
            => new EntityProfile(t512 ?? STOLON.Textures[$"Debug\\temp-512"], t256, t128, focus, menuOffset);
        public static EntityProfile GetDebug(string entityName, Point? focus = null, Point? menuOffset = null)
            => new EntityProfile(
                STOLON.Textures.TryGetValue($"Entities\\{entityName}\\{entityName}-512", out Texture2D? val512) ? val512 : STOLON.Textures[$"Debug\\temp-512"],
                STOLON.Textures.TryGetValue($"Entities\\{entityName}\\{entityName}-256", out Texture2D? val256) ? val256 : null,
                STOLON.Textures.TryGetValue($"Entities\\{entityName}\\{entityName}-128", out Texture2D? val128) ? val128 : null,
            focus);
    }
    /// <summary>
    /// Represent the character/other that can interact with the board. Be it as part of a group or solo.
    /// </summary>
    public abstract class Entity : IDialogueProvider, IMipmapped, IEquatable<Entity>
    {

        public string FullName { get; }
        public EntityProfile Profile { get; }
        public IReadOnlyDictionary<int, Texture2D> Mipmaps => Profile.Mipmaps;
        public abstract Computer? Computer { get; }
        public string Description { get; }
        /// <summary>
        /// The unique ID of this <see cref="Entity"/>, no capital letters.
        /// </summary>
        public string Id { get; }
        public string Name { get; }
        public string SymbolNotation { get; }

        public ConditionalNote[] AbilityNotes { get; }
        public ConditionalNote[] AllocationNotes { get; }

        public Entity(string id, string name, string symbolNotation, string? description = null, string? fullName = null)
        {
            Id = id;
            Name = name;
            SymbolNotation = symbolNotation;
            FullName = fullName ?? name;
            Description = description ?? string.Empty;

            Profile = ResolveProfile();
            (AllocationNotes, AbilityNotes) = ResolveNotes();
        }

        protected virtual EntityProfile ResolveProfile()
            => new EntityProfile(Id);

        protected virtual (ConditionalNote[] allocationNotes, ConditionalNote[] abilityNotes) ResolveNotes()
            => (Array.Empty<ConditionalNote>(), Array.Empty<ConditionalNote>());

        /// <summary>
        /// Get the <see cref="Player"/> of this <see cref="Entity"/>.
        /// </summary>
        /// <returns>A new <see cref="Player"/> created from this <see cref="Entity"/>.</returns>
        public Player GetPlayer() => new Player(Name, Computer ?? throw new InvalidOperationException($"Entity '{Name}' has no associated computer."));
        public virtual int GetVirtualAllocation(EntitySelection info)
        {
            return info.GetAllocation(this.Id);
        }
        public bool Equals(Entity? other) => other != null && other.Id == Id;
    }
    public static class EntityDrawingExtensions
    {
        public static void DrawEntity(this DrawingContext context, Entity entity, int res, Vector2 position, Vector2 scale, float rotation = 0f, Vector2? origin = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f, EntityDrawMode drawMode = EntityDrawMode.None)
            => context.DrawEntity(entity.Profile, res, position, scale, rotation, origin, effects, layerDepth, drawMode);
        public static void DrawEntity(this DrawingContext context, Entity entity, int res, Vector2 position, float scale = 1f, float rotation = 0f, Vector2? origin = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f, EntityDrawMode drawMode = EntityDrawMode.None)
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
        public static void DrawSymbolNotation(this DrawingContext context, string symbolNotationStr, Rectangle bounds)
        {
            context.DrawArea(bounds, Color.Black);
            context.DrawRectangle(bounds, Color.White, Interface.LINE_WIDTH);
            Vector2 dimensions = STOLON.Fonts.Medium.FastMeasure(symbolNotationStr);
            Vector2 scale = Vector2.One;
            if (dimensions.X > bounds.Width - 10) scale = new Vector2(0.8f, 1);
            dimensions *= scale;
            context.DrawString(STOLON.Fonts.Medium, symbolNotationStr, Centering.Center(dimensions.ToPoint(), bounds).PixelLock(), scale: scale);
        }

    }
    /// <summary>
    /// A class that can interact with a <see cref="Board"/>.
    /// </summary>
    public abstract class Computer
    {
        /// <summary>
        /// The source <see cref="Entity"/>.
        /// </summary>
        public Entity? Source { get; }
        public Computer(Entity? source)
        {
            Source = source;
        }
        /// <summary>
        /// Do a move best for the <see cref="Source"/> <see cref="Entity"/> on the <paramref name="board"/>.
        /// </summary>
        /// <param name="board">The <see cref="Board"/> to do a move on.</param>
        public abstract void DoMove(Board board);

        /// <summary>
        /// Gets the <see cref="Player"/> this <see cref="Computer"/> plays for.
        /// </summary>
        /// <param name="state">The current state of the <see cref="Board"/>.</param>
        /// <returns>The <see cref="Player"/> this <see cref="Computer"/> plays for.</returns>
        public Player GetPlayer(BoardState state)
        {
            Player[] players = state.Players.ToArray();
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].Computer == this)
                {
                    return players[i];
                }
            }
            throw new Exception();
        }
    }
}
