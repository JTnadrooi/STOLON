using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace STOLON
{
    /// <summary>
    /// Represent the main component of a <see cref="Entity"/>.
    /// </summary>
    public abstract class Entity : IDialogueProvider, IMipmapped
    {
        public abstract IReadOnlyDictionary<int, GameTexture> Mipmaps { get; }
        /// <summary>
        /// Create a new <see cref="Entity"/> with set values.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="symbolNotation"></param>
        public Entity(string id, string name, string symbolNotation)
        {
            Id = id;
            Name = name;
            SymbolNotation = symbolNotation;
        }
        /// <summary>
        /// Get the <see cref="Player"/> of this <see cref="Entity"/>.
        /// </summary>
        /// <returns>A new <see cref="Player"/> created from this <see cref="Entity"/>.</returns>
        public Player GetPlayer()
        {
            return new Player(Name, Computer);
        }

        /// <summary>
        /// Get the <see cref="Computer"/> of this <see cref="Entity"/>.
        /// </summary>
        public abstract Computer Computer { get; }
        /// <summary>
        /// A short description of this <see cref="Entity"/>.
        /// </summary>
        public virtual string? Description { get; }
        /// <summary>
        /// The unique ID of this <see cref="Entity"/>, no capital letters.
        /// </summary>
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string SymbolNotation { get; protected set; }
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
        /// <summary>
        /// Create a new <see cref="Computer"/> with a set <see cref="Source"/> <see cref="Entity"/>.
        /// </summary>
        /// <param name="source">The source <see cref="Entity"/>.</param>
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
