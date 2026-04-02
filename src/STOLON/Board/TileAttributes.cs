using Autofac;
using System.Collections.Frozen;
using System.Runtime.CompilerServices;

namespace STOLON
{
    public abstract class TileAttribute
    {
        /// <summary>
        /// The ID of the <see cref="TileAttribute"/>
        /// </summary>
        public string Id;
        /// <summary>
        /// Create a new <see cref="TileAttribute"/> with a set <see cref="Id"/>.
        /// </summary>
        /// <param name="id"></param>
        public TileAttribute(string id)
        {
            Id = id;
        }

        public override int GetHashCode() => Id.GetHashCode();

        public override bool Equals(object? obj) => Id.Equals(Id);

        public override string ToString() => Id;

        /// <summary>
        /// A <see cref="FrozenDictionary{TKey, TValue}"/> holding all registered <see cref="TileAttribute"/> objects.
        /// </summary>
        public static FrozenDictionary<string, TileAttribute> Attributes { get; }
        /// <summary>
        /// A <see cref="FrozenSet"/>
        /// </summary>
        public static FrozenSet<TileAttribute> DefaultAttributes { get; }

        static TileAttribute()
        {
            Dictionary<string, TileAttribute> tileAttributes = new Dictionary<string, TileAttribute>();

            static void Register<T>(T attribute, Dictionary<string, TileAttribute> dic) where T : TileAttribute => dic.Add(attribute.Id, attribute);

            Register(new Player0OccupiedTileAttribute(), tileAttributes);
            Register(new Player1OccupiedTileAttribute(), tileAttributes);
            Register(new GravDownTileAttribute(), tileAttributes);
            Register(new GravUpTileAttribute(), tileAttributes);
            Register(new SolidTileAttribute(), tileAttributes);

            Attributes = tileAttributes.ToFrozenDictionary();

            DefaultAttributes = new HashSet<TileAttribute>()
            {
                Get<GravDownTileAttribute>(),
            }.ToFrozenSet();
        }

        public static HashSet<TileAttribute> GetOccupiedTileAttributeFor(int playerIndex) => new HashSet<TileAttribute>()
            {
                (TileAttribute)Attributes[$"Player{playerIndex}Occupied"],
                TileAttribute.Get<SolidTileAttribute>(),
            };

        /// <summary>
        /// Replace a <see cref="TileAttribute"/> in a <see cref="HashSet{T}"/> of <see cref="TileAttribute"/> objects.
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTo"></typeparam>
        /// <param name="attributes">The <see cref="HashSet{T}"/> of attributes to alter.</param>
        /// <returns>The <paramref name="attributes"/> <see cref="HashSet{T}"/>.</returns>
        public static HashSet<TileAttribute> ReplaceAttribute<TFrom, TTo>(HashSet<TileAttribute> attributes) where TFrom : TileAttribute where TTo : TileAttribute
        {
            if (!attributes.Remove(Get<TFrom>())) throw new Exception();
            if (!attributes.Add(Get<TTo>())) throw new Exception();
            return attributes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetName<TTileAttribute>() where TTileAttribute : TileAttribute
            => typeof(TTileAttribute).Name.Replace("TileAttribute", string.Empty);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TileAttribute Get<TTileAttribute>() where TTileAttribute : TileAttribute
            => (TileAttribute)Attributes[GetName<TTileAttribute>()];
    }

    /// <summary>
    /// Tile is occupied by player 0.
    /// </summary>
    public sealed class Player0OccupiedTileAttribute : TileAttribute
    {
        public Player0OccupiedTileAttribute() : base("Player0Occupied") { }
    }

    /// <summary>
    /// Tile is occupied by player 1.
    /// </summary>
    public sealed class Player1OccupiedTileAttribute : TileAttribute
    {
        public Player1OccupiedTileAttribute() : base("Player1Occupied") { }
    }

    /// <summary>
    /// Tile applies gravity DOWN.
    /// </summary>
    public sealed class GravDownTileAttribute : TileAttribute
    {
        public GravDownTileAttribute() : base("GravDown") { }
    }

    /// <summary>
    /// Tile applies gravity UP.
    /// </summary>
    public sealed class GravUpTileAttribute : TileAttribute
    {
        public GravUpTileAttribute() : base("GravUp") { }
    }

    /// <summary>
    /// Tile ends the fall of a tile.
    /// </summary>
    public sealed class SolidTileAttribute : TileAttribute
    {
        public SolidTileAttribute() : base("Solid") { }
    }
}
