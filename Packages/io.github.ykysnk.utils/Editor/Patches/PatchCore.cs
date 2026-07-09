using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using io.github.ykysnk.utils.NonUdon;
using UnityEditor;

namespace io.github.ykysnk.utils.Editor.Patches
{
    [InitializeOnLoad]
    internal static class PatchCore
    {
        internal static readonly Dictionary<string, MethodInfo> MethodCache = new();

        static PatchCore()
        {
            var patchLoaders = PatchLoaders.ToArray();
            Utils.Log(nameof(PatchCore), $"Initializing patch core ({patchLoaders.Length} loaders)");

            foreach (var loader in patchLoaders)
            {
                if (!loader.Enabled) continue;
                var harmony = new Harmony(loader.QualifiedName);
                loader.ThisHarmony = harmony;
                loader.Load(harmony);
                harmony.PatchAll(loader.GetType().Assembly);
                AssemblyReloadEvents.beforeAssemblyReload += () =>
                {
                    loader.Unload(harmony);
                    harmony.UnpatchAll(loader.QualifiedName);
                };
                Utils.Log(nameof(PatchCore), $"Loaded patch loader ({loader.DisplayName})");
            }
        }

        private static IEnumerable<Type> PatchLoaderTypes => AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetCustomAttributes(typeof(ExportsPatchLoader), false)).Select(x =>
                ((ExportsPatchLoader)x).Type).Where(x => typeof(IPatchLoader).IsAssignableFrom(x));

        private static IEnumerable<IPatchLoader> PatchLoaders =>
            PatchLoaderTypes.Select(ReflectionUtils.Instantiate<IPatchLoader>).Where(x => x != null)!;
    }
}