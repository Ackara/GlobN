# GlobN
[![NuGet](https://img.shields.io/nuget/v/Acklann.GlobN.svg)](https://www.nuget.org/packages/Acklann.GlobN/)

**GlobN** is a fast file pattern matching library for that outperforms Regex ([see benchmarks](tests/GlobN.Benchmark/BenchmarkDotNet.Artifacts/results/vbench.html)).

## Usage

**GlobN** is available at [nuget](https://www.nuget.org/packages/Acklann.GlobN). `PM> Install-Package Acklann.GlobN`

```csharp
bool success = Glob.IsMatch("index.html", "index.*");
/* returns: true */

Glob pattern = "**/*.js";
IEnumerable<string> allJsFilePaths = pattern.ResolvePath(@"C:\app\scripts\");
/* returns: The paths of all .js files within the current directory and its sub-directories. */

Glob pattern = "../../index.html";
string fullPath = pattern.ExpandPath();
/* returns: The full path of the specified file. */

Glob pattern = "scripts/**/auth/*.ts";
IEnumerable<string> filteredList = pattern.Filter(new string[] { ... }).
/* returns: Only the strings that match the pattern */
```

**Supported Expressions**

| Pattern | Description                                                                                      |
|---------|--------------------------------------------------------------------------------------------------|
| ..\     | Moves the current directory up one folder. **Only applicable at the beginning of the pattern**.
| *       | Match zero or more characters excluding the directory separator.
| **      | Match zero or more directories.
| ?       | Match a single character.
| !       | Negates the matching pattern. **Only applicable at the beginning of the pattern**.

**NOTE: matches are case-insensitive.**
