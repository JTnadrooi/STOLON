using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.CLI.Build
{
    public static class Builder
    {
        /// <summary>
        /// Builds assets using the specified <paramref name="builder"/>.
        /// </summary>
        /// <typeparam name="TBuilder">The <see cref="IBuilder"/> implementing type.</typeparam>
        /// <param name="builder">The asset builder.</param>
        /// <param name="force">If <see langword="true"/>, all assets will be rebuild regardless if they are up to date or not. See <see cref="BuildHelper.NeedsBuild(string, string, bool)"/>.</param>
        public static void Build<TBuilder>(TBuilder builder, bool force) where TBuilder : IBuilder
        {
            BuildItemInfo[] buildItems = builder.GetBuildItems();

            CLI.Debug.Log($">{(force ? "force-" : string.Empty)}building for Builder of type '{typeof(TBuilder).Name}'.");
            CLI.Debug.Log($"found {buildItems.Length} files.");

            builder.PreBuild();

            foreach (BuildItemInfo buildItem in buildItems)
            {
                if (!BuildHelper.NeedsBuild(buildItem.Source, buildItem.Destination, force)) continue;

                CLI.Debug.Log($">building '{buildItem.Source}' as '{buildItem.Destination}'.");

                builder.Build(buildItem.Source, buildItem.Destination);

                CLI.Debug.Success();
            }

            builder.PostBuild();

            CLI.Debug.Success();
        }
    }
}
