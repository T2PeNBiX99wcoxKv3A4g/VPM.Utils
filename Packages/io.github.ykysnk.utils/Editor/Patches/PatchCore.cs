using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEditor;

namespace io.github.ykysnk.utils.Editor.Patches
{
    [InitializeOnLoad]
    internal static class PatchCore
    {
        static PatchCore()
        {
            foreach (var loader in PatchLoaders)
            {
                if (!loader.Enabled) continue;

                var harmony = new Harmony(loader.QualifiedName);
                loader.Load();
                harmony.PatchAll(loader.GetType().Assembly);
                AssemblyReloadEvents.beforeAssemblyReload += () =>
                {
                    loader.Unload();
                    harmony.UnpatchAll(loader.QualifiedName);
                };
                Utils.Log(nameof(PatchCore), $"Loaded patch loader ({loader.DisplayName})");
            }
        }

        private static IEnumerable<Type> PatchLoaderTypes => AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetCustomAttributes(typeof(ExportsPatchLoader), false)).Select(x =>
                ((ExportsPatchLoader)x).Type).Where(x => x.IsAssignableFrom(typeof(IPatchLoader)));

        private static IEnumerable<IPatchLoader> PatchLoaders =>
            PatchLoaderTypes.Select(InstantiateLoader).Where(x => x != null)!;

        private static IPatchLoader? InstantiateLoader(Type loaderType)
        {
            var loader = (IPatchLoader?)Activator.CreateInstance(loaderType);
            if (loader == null)
                Utils.LogWarning(nameof(PatchCore), $"Failed to instantiate loader of type {loaderType}");

            return loader;
        }
    }
}