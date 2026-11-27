# Utils for VPM and Unity

Shared runtime and editor helpers for the VPM packages: logging, reflection accessors, an inspector base with a
consistent error UI, GUI and menu helpers, a UPM client and a Harmony patch loader.

[Documentation](https://t2penbix99wcoxkv3a4g.github.io/VPM-Docs/#/utils/Overview) ·
[日本語](https://t2penbix99wcoxkv3a4g.github.io/VPM-Docs/?lang=ja#/utils/Overview) ·
[简体中文](https://t2penbix99wcoxkv3a4g.github.io/VPM-Docs/?lang=zh-CN#/utils/Overview) ·
[繁體中文](https://t2penbix99wcoxkv3a4g.github.io/VPM-Docs/?lang=zh-TW#/utils/Overview)

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

## Documentation

The documentation site covers setup, the editor helpers, the patch lifecycle and the full API reference:

- [Overview](https://t2penbix99wcoxkv3a4g.github.io/VPM-Docs/#/utils/Overview) - what the package contains and when to use it
- [Installation](https://t2penbix99wcoxkv3a4g.github.io/VPM-Docs/#/utils/Installation) - the listing, namespaces and updates
- [Editor helpers](https://t2penbix99wcoxkv3a4g.github.io/VPM-Docs/#/utils/Editor-Helpers) - `BasicEditor`, the GUI helpers, dialogs and the UPM client
- [Patches](https://t2penbix99wcoxkv3a4g.github.io/VPM-Docs/#/utils/Patches) - the patch lifecycle, method conventions and reverse patches
- [API](https://t2penbix99wcoxkv3a4g.github.io/VPM-Docs/#/utils/API) - every public type and member

The site is available in English, Japanese, Simplified Chinese and Traditional Chinese; the links above open the
English text and the language picker in the corner switches the rest.

## License

MIT, see [LICENSE](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.Utils/blob/main/LICENSE).
