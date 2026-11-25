# Utils for VPM and Unity

Shared helpers for the VPM packages: runtime utilities, reflection helpers, an inspector base with a consistent
error UI, GUI and menu helpers, a UPM client, and a Harmony patch loader.

## Installation

The package is `io.github.ykysnk.utils`, distributed through the repository at
<https://t2penbix99wcoxkv3a4g.github.io/VPM/>. Add that listing to ALCOM or the VPM CLI, then install the package.

It has no VPM dependencies of its own. Most projects get it indirectly: [Localization](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.Localization)
and [YKYToolKit](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit) declare it as a dependency.

Unity 2022.3 or newer. The VRChat SDK is optional: the SDK-specific helpers (`IUtilsEditorOnly`, the player distance
checks) compile only when `com.vrchat.base` is installed, and nothing else references SDK types. The editor patch
framework needs Harmony, which ships with the VRChat SDK.

## What is in the package

| Assembly | Namespace | Contents |
| --- | --- | --- |
| `io.github.ykysnk.utils` | `io.github.ykysnk.utils` | `Utils` logging and assertions, `IsInPrefab` / `IsPlaying`, `ToLayer`, `BasicEditorComponent` |
| `io.github.ykysnk.utils.non-udon` | `io.github.ykysnk.utils.NonUdon` | `BooleanVector3`, `ColorHex`, `Reflect`, `ReflectionWrapper`, `ReflectionUtils`, `Result` |
| `io.github.ykysnk.utils.Editor` | `io.github.ykysnk.utils.Editor` | `BasicEditor`, the GUI and menu helpers, `UpmInstaller`, the property drawers, the patch framework |

## Reflection helpers

`ReflectionUtils` logs instead of throwing: a missing member yields `null` or `default`, which is what an optional
dependency needs.

```csharp
// Probe an editor extension that may or may not be installed.
var type = ReflectionUtils.FindType("Some.Plugin.EditorWindow");
if (type == null) return; // not installed, skip the integration

var property = ReflectionUtils.FindProperty(type, "IsEnabled");
var method = ReflectionUtils.FindMethod(type, "Refresh");
```

Lookups are cached per type and name, including confirmed misses, so repeated calls cost a dictionary lookup instead
of a reflection walk. `FindType` caches only names that resolved, because an assembly can be loaded later.

`Reflect` goes further and caches compiled accessors for a member that is used repeatedly:

```csharp
var health = Reflect.For(component).Field("health");
var value = health.Get<int>();
```

## Patches

The editor assembly ships a small Harmony layer that applies patch groups, removes them before every assembly
reload and skips a group whose target is missing. A loader is declared per assembly:

```csharp
[assembly: ExportsPatchLoader(typeof(MyLoader))]

internal sealed class MyLoader : PatchLoader<MyLoader>
{
    public override void Load(Harmony harmony) => Run(MyGroup.Instance);
}
```

See [Patches](https://t2penbix99wcoxkv3a4g.github.io/VPM-Docs/#/utils/Patches) for the full lifecycle, the patch
method conventions and reverse patches.

## Documentation

The full documentation is at <https://t2penbix99wcoxkv3a4g.github.io/VPM-Docs/#/utils/Overview>, with a complete API
reference on the [API page](https://t2penbix99wcoxkv3a4g.github.io/VPM-Docs/#/utils/API). The site is available in
English, Japanese, Simplified Chinese and Traditional Chinese.

## License

MIT, see [LICENSE](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.Utils/blob/main/LICENSE).
