# GlobN
[![NuGet](https://img.shields.io/nuget/v/Acklann.GlobN.svg)](https://www.nuget.org/packages/Acklann.GlobN/)
[![NuGet](https://img.shields.io/nuget/dt/Acklann.GlobN.svg)](https://www.nuget.org/packages/Acklann.GlobN/)
[![Gitter](https://img.shields.io/gitter/room/nwjs/nw.js.svg)](https://gitter.im/Ackara/Lobby?utm_source=share-link&utm_medium=link&utm_campaign=share-link)
---

## What is GlobN
**GlobN** is a file pattern matching library for .NET. Grabbing files from the command-line can never be easier.

### How it works
Let say your current directory is as follows.

```txt
C:\projects\coolapp

index.html
js/
-- site.ts
-- site.js
-- viewModel/
   -- view.ts
   -- view.js
```

Most of the functions you'll be utilizing are extension methods. So lets say you want to grab all the files within the current directory.

```csharp
string cd = System.Environment.CurrentDirectory;
IEnumerable<string> all5Files = cd.GetFiles();
```

Lets say your in the **css folder** and you want to grab all of the `.js` files.

```csharp
var siteJs = cd.GetFiles("../js/*.js");
/* return: js\site.js */

var allJsFiles cd.GetFiles(@"..\**\*.js");
/* return: js\site.js; js\viewModel\view.js */
```

Finally, here are some other straight forward examples.

```csharp
var filteredList = listOfPaths.Filter("*.js");

IEnumerable<string> files = @"..\js\*.ts".ResolvePath();

Glob glob = "*.png";
glob.IsMatch("/site/content/bg.png");
```

**Supported Expressions**

| Pattern | Description                                                                                      |
|---------|--------------------------------------------------------------------------------------------------|
| ..\     | Moves the current directory up one folder. **Only applicable at the beginning of the pattern**.  |
| *       | Match zero or more characters excluding the directory separator.                                 |
| **      | Match zero or more directories.                                                                  |
| ?       | Match a single character.                                                                        |
| !       | Negates the matching pattern.                                                                    |

### Where can I get it
**GlobN** is available at [nuget.org](https://www.nuget.org/packages/Acklann.GlobN). `PM> Install-Package Acklann.GlobN`


