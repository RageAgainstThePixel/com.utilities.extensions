# com.utilities.extensions

[![Discord](https://img.shields.io/discord/855294214065487932.svg?label=&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2)](https://discord.gg/xQgMW9ufN4) [![openupm](https://img.shields.io/npm/v/com.utilities.extensions?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.utilities.extensions/) [![openupm](https://img.shields.io/badge/dynamic/json?color=brightgreen&label=downloads&query=%24.downloads&suffix=%2Fmonth&url=https%3A%2F%2Fpackage.openupm.com%2Fdownloads%2Fpoint%2Flast-month%2Fcom.utilities.extensions)](https://openupm.com/packages/com.utilities.extensions/)

Common extensions for for the [Unity](https://unity.com/) Game Engine types.

## Installing

Requires Unity 2021.3 LTS or higher.

The recommended installation method is though the unity package manager and [OpenUPM](https://openupm.com/packages/com.utilities.extensions).

### Via Unity Package Manager and OpenUPM

#### Terminal

```terminal
openupm add com.utilities.extensions
```

#### Manual

- Open your Unity project settings
- Select the `Package Manager`
![scoped-registries](Utilities.Extensions/Packages/com.utilities.extensions/Documentation~/images/package-manager-scopes.png)
- Add the OpenUPM package registry:
  - Name: `OpenUPM`
  - URL: `https://package.openupm.com`
  - Scope(s):
    - `com.utilities`
- Open the Unity Package Manager window
- Change the Registry from Unity to `My Registries`
- Add the `Utilities.Extensions` package

### Via Unity Package Manager and Git url

- Open your Unity Package Manager
- Add package from git url: `https://github.com/RageAgainstThePixel/com.utilities.extensions.git#upm`

## Documentation

### Runtime Extensions

- Addressables Extensions
- Component Extensions
- GameObject Extensions
- Transform Extensions
- Unity.Object Extensions
- NativeArray Extensions
  - AsSpan: Converts a NativeArray to a Span for versions of Unity that do not support `NativeArray.AsSpan()`.
  - MemoryStream.ToNativeArray: Converts a MemoryStream to a NativeArray with minimal allocations.
  - CopyFrom: Copies data from one NativeArray to another while being able to specify the start index and length of data to copy.
  - ToBase64String: Converts a NativeArray to a Base64 string with minimal allocations.
  - FromBase64String: Converts a Base64 string to a NativeArray with minimal allocations.
  - WriteAsync: Writes to a Stream from a NativeArray asynchronously with minimal allocations.

### Runtime Utilities

- SerializedDictionary: A generic dictionary that can be serialized by Unity's serialization system, allowing you to use dictionaries in your MonoBehaviour and ScriptableObject classes.

### Editor Extensions

- EditorGUILayout Extensions
- SerializedProperty Extensions
- ScriptableObject Extensions
- Unity.Object Extensions

### Editor Utilities

- AbstractDashboardWindow
- Regenerate Asset Guids: A utility to regenerate the GUIDs of all assets in the project or a specified directory.
- Script Icon Utility: A utility to set a custom icon for all scripts inside of a specified directory.
- A component context utility to upgrade and downgrade components from derived, non-abstract types.

### Attributes

- SceneReferenceAttribute: When added to a serialized string field, it will render the scene picker in the inspector, allowing you to select a scene from the project. If the scene is not in the build settings, it will be added automatically.

### Behaviours

- LoadSceneButton: A simple uGUI button that loads a scene when clicked.
