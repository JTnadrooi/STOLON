using System.Collections.Immutable;

namespace STOLON
{
    public readonly struct SearchTarget
    {
        public readonly ImmutableArray<Point> Nodes { get; }

        public SearchTarget(ReadOnlySpan<Point> nodes)
        {
            Nodes = nodes.ToImmutableArray();
        }

        public readonly SearchTarget AsInverted()
        {
            List<Point> invertedNodes = new List<Point>();

            foreach (Point node in Nodes)
                invertedNodes.Add(node * new Point(-1, -1));

            return new SearchTarget(invertedNodes.ToArray());
        }

        public static ImmutableArray<SearchTarget> GetDefaultTargets()
        {
            return new SearchTarget[] {
                    new SearchTarget(new Point[]
                    {
                        Point.Zero,
                        new Point(0, 1),
                        new Point(0, 2),
                        new Point(0, 3),
                    }),
                    new SearchTarget(new Point[]
                    {
                        Point.Zero,
                        new Point(1, 0),
                        new Point(2, 0),
                        new Point(3, 0),
                    }),
                    new SearchTarget(new Point[]
                    {
                        Point.Zero,
                        new Point(1, 1),
                        new Point(2, 2),
                        new Point(3, 3),
                    }),
                    new SearchTarget(new Point[]
                    {
                        Point.Zero,
                        new Point(1, -1),
                        new Point(2, -2),
                        new Point(3, -3),
                    }),
            }.ToImmutableArray();
        }

        // uncomment code below for type load error, very funny.

        //public static ImmutableArray<SearchTarget> DefaultTargets { get; }  // here is error

        //static SearchTarget()
        //{
        //    DefaultTargets = new SearchTarget[] {
        //            new SearchTarget(new Point[]
        //            {
        //                Point.Zero,
        //                new Point(0, 1),
        //                new Point(0, 2),
        //                new Point(0, 3),
        //            }),
        //            new SearchTarget(new Point[]
        //            {
        //                Point.Zero,
        //                new Point(1, 0),
        //                new Point(2, 0),
        //                new Point(3, 0),
        //            }),
        //            new SearchTarget(new Point[]
        //            {
        //                Point.Zero,
        //                new Point(1, 1),
        //                new Point(2, 2),
        //                new Point(3, 3),
        //            }),
        //            new SearchTarget(new Point[]
        //            {
        //                Point.Zero,
        //                new Point(1, -1),
        //                new Point(2, -2),
        //                new Point(3, -3),
        //            }),
        //    }.ToImmutableArray();
        //}
    }
}
