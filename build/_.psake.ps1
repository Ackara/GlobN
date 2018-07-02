# SYNOPSIS: This is a psake task file.

$localTasks = "$PSScriptRoot\tasks.psake.ps1";
if (Test-Path $localTasks) { Include $localTasks; }

Properties {
	# Constants
	$RootDir = "$(Split-Path $PSScriptRoot -Parent)";
	$ManifestJson = "$PSScriptRoot\manifest.json";
	$ArtifactsDir = "$RootDir\artifacts";
	$PoshModulesDir = "";
    $SolutionName = "";

	# Args
	$SkipCompilation = $false;
	$Configuration = "";
	$Secrets = @{ };
	$Major = $false;
	$Minor = $false;
	$Branch = "";
}

Task "Default" -depends @("restore", "compile", "test", "pack");

Task "Deploy" -alias "publish" -description "This task compiles, test then publishes the solution." `
-depends @("restore", "version", "compile", "test", "pack", "push", "tag");

#region ----- COMPILATION -----

Task "Import-Dependencies" -alias "restore" -description "This task imports all build dependencies." `
-action {
	#  Importing all required powershell modules.
	foreach ($moduleId in @("Ncrement", "Pester"))
	{
		$modulePath = "$PoshModulesDir\$moduleId\*\*.psd1";
		if (-not (Test-Path $modulePath))
		{
			Save-Module $moduleId -Path $PoshModulesDir;
		}
		Import-Module $modulePath -Force;
		Write-Host "  * imported the '$moduleId.$(Split-Path (Get-Item $modulePath).DirectoryName -Leaf)' powershell module.";
	}

    # Creating the 'manifest.json' file.
    if (-not (Test-Path $ManifestJson))
    {
        New-NcrementManifest $ManifestJson -Author "Ackara";
    }
}

Task "Increment-VersionNumber" -alias "version" -description "This task increments the project's version numbers" `
-depends @("restore") -action {
    $manifest = Get-NcrementManifest $ManifestJson;
	$manifest.ReleaseNotes = Get-Content "$RootDir\releaseNotes.txt" | Out-String;
    $oldVersion = $manifest | Convert-NcrementVersionNumberToString;
	$result = $manifest | Step-NcrementVersionNumber $Branch -Break:$Major -Feature:$Minor -Patch | Update-NcrementProjectFile "$RootDir\src" -Commit;
    $newVersion = $manifest | Convert-NcrementVersionNumberToString;
	
	Write-Host "  * Incremented version number from '$oldVersion' to '$newVersion'.";
	foreach ($file in $result.ModifiedFiles)
	{
		Write-Host "    * Updated $(Split-Path $file -Leaf).";
	}
}

Task "Build-Solution" -alias "compile" -description "This task compiles the solution." `
-depends @("restore") -precondition { return (-not $SkipCompilation); } -action {
	Write-Header "dotnet: msbuild";
	Exec { &dotnet msbuild $((Get-Item "$RootDir\*.sln").FullName) "/p:Configuration=$Configuration" "/verbosity:minimal"; }
}

Task "Run-Tests" -alias "test" -description "This task invoke all tests within the 'tests' folder." `
-depends @("restore") -action {
	try
	{
        # Running all MSTest assemblies.
        Push-Location $RootDir;
		foreach ($testFile in (Get-ChildItem "$RootDir\tests\*\bin\$Configuration" -Recurse -Filter "*$SolutionName*test*.dll"))
		{
			Write-Header "dotnet: vstest '$($testFile.BaseName)'";
			Exec { &dotnet vstest $testFile.FullName; }
		}

		# Running all Pester scripts.
		$testsFailed = 0;
		foreach ($testFile in (Get-ChildItem "$RootDir\tests\*\" -Recurse -Filter "*tests.ps1"))
		{
			Write-Header "Pester '$($testFile.BaseName)'";
			$results = Invoke-Pester -Script $testFile.FullName -PassThru;
			$testsFailed += $results.FailedCount;
            if ($results.FailedCount -gt 0) { throw "'$($testFile.BaseName)' failed '$($results.FailedCount)' test(s)."; }
		}
	}
	finally { Pop-Location; }
}

Task "Run-Benchmarks" -alias "benchmark" -description "This task runs all project benchmarks." `
-depends @("restore") -action {
	$benchmarkProject = Get-ChildItem $RootDir -Recurse -Filter "*Benchmark.csproj" | Select-Object -First 1;
	
	if (Test-Path $benchmarkProject.FullName)
	{
		Write-Header "dotnet: rebuild";
		Exec { &dotnet clean $((Get-Item "$RootDir\*.sln").FullName); }
		Exec { &dotnet build $((Get-Item "$RootDir\*.sln").FullName) --configuration Release; }

		try
		{
			$exe = Get-ChildItem "$($benchmarkProject.DirectoryName)\bin\Release" -Recurse -Filter "*Benchmark.dll" | Select-Object -First 1;
			Push-Location $exe.DirectoryName;
			Write-Header "dotnet: run benchmarks";
			Exec { &dotnet $exe.FullName; }

			# Copying benchmark results to report.
			$reportFile = Get-Item "$($benchmarkProject.DirectoryName)\*.md";
			if (Test-Path $reportFile)
			{
				$summary = Get-Item "$($exe.DirectoryName)\*artifacts*\*\*.md" | Get-Content | Out-String;
				$report = $reportFile | Get-Content | Out-String;
				$match = [Regex]::Match($report, '(?i)#+\s+(Summary|Results?|Report)');
				$report = $report.Substring(0, ($match.Index + $match.Length));
				"$report`r`n`r`n$summary" | Out-File $reportFile -Encoding utf8;
				Get-Item "$($exe.DirectoryName)\*artifacts*\*\*.html" | Invoke-Item;
			}
		}
		finally { Pop-Location; }
	}
	else { Write-Host "no benchmarks projects found." -ForegroundColor Yellow; }
}

#endregion

#region ----- PUBLISHING -----

Task "Publish-Solution" -alias "push" -description "This task publish all packages to their respective host." `
-depends @("pack", "push-nuget");

Task "Publish-NuGetPackages" -alias "push-nuget" -description "This task publish all nuget packages to nuget.org." `
-depends @("restore") -action {
	$apiKey = Get-Secret "nugetKey";
	Assert (Test-Path $ArtifactsDir) "No nuget packages were found. Try running the 'pack' task then try again.";

	foreach ($nupkg in (Get-ChildItem $ArtifactsDir -Recurse -Filter "*.nupkg"))
	{
		Write-Header "dotnet: nuget push '$($nupkg.Name)'";
		Exec { &dotnet nuget push $nupkg.FullName --source "https://api.nuget.org/v3/index.json" --api-key $apiKey; }
	}
}

Task "Tag-Release" -alias "tag" -description "This task tags the last commit with the version number." `
-depends @("restore") -precondition { return ($Branch -ieq "master"); } -action {
    $version = Get-NcrementManifest $ManifestJson | Convert-NcrementVersionNumberToString;
    Exec { &git tag v$version | Out-Null; }
    Exec { &git push "origin" --tags | Out-Null; }
}

#endregion

#region ----- HELPER FUNCTIONS -----

function Get-Secret([Parameter(ValueFromPipeline)][string]$key)
{
	$value = $Secrets.$key;
    $secretsJson = "$PSScriptRoot\secrets.json";


	if ([string]::IsNullOrEmpty($value) -and (Test-Path $secretsJson))
	{
		$value = Get-Content $secretsJson | Out-String | ConvertFrom-Json | Select-Object -ExpandProperty $key;
	}
    elseif ((-not (Test-Path $secretsJson)) -and ($Secrets.Count -gt 0))
    {
        $Secrets | ConvertTo-Json | Out-File $secretsJson -Encoding utf8;
        Write-Host "  * Added '$(Split-Path $secretsJson -Leaf)' to project.";
    }

    Assert (-not [string]::IsNullOrEmpty($value)) "Your '$key' was not specified. Provided a value via the `$Secrets parameter eg. `$Secrets=@{'$key'='your_sercret_value'}";
	return $value;
}

function Write-Header([string]$Title = "", [int]$length = 70, [switch]$AsValue)
{
	$header = [string]::Join('', [System.Linq.Enumerable]::Repeat('-', $length));
	if (-not [String]::IsNullOrEmpty($Title))
	{
		$header = $header.Insert(4, " $Title ");
		if ($header.Length -gt $length) { $header = $header.Substring(0, $length); }
	}

	if ($AsValue) { return $header; } else { Write-Host ''; Write-Host $header -ForegroundColor DarkGray; Write-Host ''; }
}

function Get-IfNull([Parameter(Mandatory, ValueFromPipeline)][string]$Value, [Parameter(Position=0)][string]$Fallback)
{
    if ([string]::IsNullOrEmpty($Value)) { return $Fallback; } else { return $Value; }
}

function Coalesce([Parameter(Mandatory, ValueFromPipeline)][bool]$Condition, [Parameter(Mandatory, Position = 0)]$TrueValue, [Parameter(Mandatory, Position = 1)]$FalseValue)
{
	if ($Condition) { return $TrueValue; } else { return $FalseValue; }
}

#endregion

FormatTaskName "$(Write-Header -AsValue)`r`n  {0}`r`n$(Write-Header -AsValue)";