# GlobN

---

## What is GlobN
**GlobN** is a file pattern matching library for .NET. Grabbing files as if your on the command-line can never be easier.

### How it works
Let say your current directory is as follows.
```txt
C:\projects\coolapp

index.html
js/
-- site.ts
-- site.js
-- viewModel/
---- view.ts
---- view.js
```

Most of the functions you'll be utilizing are extension methods. So lets say you want to grab all the files within the current directory.

```csharp
string cd = System.Environment.CurrentDirectory;
IEnumerable<string> all5Files = cd.GlobFiles();
```

Lets say your in the **css folder** and you want to grab all of the `.js` files.

```csharp
var oneJsFile = cd.GlobFiles("../js/*.js");
// return: js\site.js

var allJsFiles cd.GlobFiles(@"..\**\*.js");
// return: js\site.js; js\viewModel\view.js
```

Finally, here are some other straight forward examples. 
```csharp
var filteredList = listOfPaths.Filter("*.js");

var glob = new Acklann.GlobN.Glob("file.txt");
bool success = glob.IsMatch(@"%TEMP%\file.txt"); // supports environment variables.
```

**Supported Expressions**

| Pattern | Description                                                                                      |
|---------|--------------------------------------------------------------------------------------------------|
| ..      | Moves the selected directory up one folder. **Only applicable at the beginning of the pattern**. |
| *       | Match zero or more characters excluding the directory separator.                                 |
| **      | Match zero or more directories.                                                                  |

### Where can I get it
**GlobN** is available at [nuget.org](https://www.nuget.org/packages/Acklann.Glob).

`PM> Install-Package Acklann.Glob`

`PS> dotnet add Acklann.Glob`

