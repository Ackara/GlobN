# SYNOPSIS: This is a psake task file.

$localTasks = "$PSScriptRoot\tasks.psake.ps1";
if (Test-Path $localTasks) { Include $localTasks; }

Properties {
	# Constants
	$RootDir = "$(Split-Path $PSScriptRoot -Parent)";
	$ManifestJson = "$PSScriptRoot/manifest.json";
	$SecretsJson = "$PSScriptRoot/secrets.json";
	$ArtifactsDir = "$RootDir/artifacts";
	$MigrationsDir = "";
    $SolutionName = "";
    $ToolsDir = "";

	# Args
    $DeleteExistingFiles = $false;
	$FallbackBranch = "preview";
	$SkipCompilation = $false;
	$NonInteractive = $false;
	$Configuration = "";
    $Commit = $true;
	$Debug = $false;
	$Secrets = @{ };
	$Major = $false;
	$Minor = $false;
	$Branch = "";
}

Task "Default" -depends @("restore", "compile", "test", "pack");

#region ----- COMPILATION -----

Task "Import-Dependencies" -alias "restore" -description "This task imports all build dependencies." -action {
	#  Importing all required powershell modules.
	foreach ($moduleId in @("Ncrement", "Pester", "VSSetup"))
	{
		$modulePath = "$ToolsDir/$moduleId/*/*.psd1";
		if (-not (Test-Path $modulePath))
		{
			Save-Module $moduleId -Path $ToolsDir;
		}
		Import-Module $modulePath -Force;
		Write-Host "  * imported the '$moduleId.$(Split-Path (Get-Item $modulePath).DirectoryName -Leaf)' powershell module.";
	}

    # Creating the 'manifest.json' file.
    if (-not (Test-Path $ManifestJson))
    {
        New-NcrementManifest $ManifestJson -Author $env:USERNAME | ConvertTo-Json | Out-File $ManifestJson -Encoding utf8;
    }

    # Create the 'secrets.json' file
    if (-not (Test-Path $SecretsJson))
    {
        $credentials = '{ "jdbcurl": "jdbc:mysql://{0}/{1}", "userStore": "server=;user=;password=;database=;", "database": "server=;user=;password=;database=;" }';
        [string]::Format('{{ "nugetKey": null, "psGalleryKey": null, "local": {0}, "preview": {0} }}', $credentials) | Out-File $SecretsJson -Encoding utf8;
    }
}

Task "Increment-VersionNumber" -alias "version" -description "This task increments the project's version numbers" `
-depends @("restore") -action {
    $manifest = Get-NcrementManifest $ManifestJson;

    $releaseNotes = join-path $RootDir "releaseNotes.txt";
    if (Test-Path $releaseNotes)
    {
        $manifest.ReleaseNotes = Get-Content $releaseNotes | Out-String;
    }

    $oldVersion = $manifest | Convert-NcrementVersionNumberToString;
	$result = $manifest | Step-NcrementVersionNumber $Branch -Break:$Major -Feature:$Minor -Patch | Update-NcrementProjectFile "$RootDir/src" -Commit:$Commit;
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
	Exec { &dotnet msbuild $((Get-Item "$RootDir/*.sln").FullName) "/p:Configuration=$Configuration" "/verbosity:minimal"; }
}

Task "Run-Tests" -alias "test" -description "This task invoke all tests within the 'tests' folder." `
-depends @("restore") -action {
	try
	{
        # Running all MSTest assemblies.
        Push-Location $RootDir;
		foreach ($testFile in (Get-ChildItem "$RootDir/tests/*/bin/$Configuration" -Recurse -Filter "*$SolutionName*test*.dll" -ErrorAction Ignore))
		{
			Write-Header "dotnet: vstest '$($testFile.BaseName)'";
			Exec { &dotnet vstest $testFile.FullName; }
		}

		# Running all Pester scripts.
		$testsFailed = 0;
		foreach ($testFile in (Get-ChildItem "$RootDir/tests/*/" -Recurse -Filter "*tests.ps1" -ErrorAction Ignore))
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
	$benchmarkProject = Get-ChildItem $RootDir -Recurse -Filter "*Benchmark*.csproj" | Select-Object -First 1;

	if (Test-Path $benchmarkProject.FullName)
	{
		Write-Header "dotnet: clean + build";
		Exec { &dotnet clean $((Get-Item "$RootDir/*.sln").FullName); }
		Exec { &dotnet build $((Get-Item "$RootDir/*.sln").FullName) --configuration Release; }

		try
		{
			$dll = Get-ChildItem "$($benchmarkProject.DirectoryName)/bin/Release" -Recurse -Filter "*Benchmark.dll" | Select-Object -First 1;
			Push-Location $dll.DirectoryName;

			Write-Header "dotnet: run benchmarks";
			Exec { &dotnet $dll.FullName; }

			# Copying benchmark results to report.
			$reportFile = Get-Item "$($benchmarkProject.DirectoryName)/*.md";
			if (Test-Path $reportFile)
			{
				$summary = Get-Item "$($dll.DirectoryName)/*artifacts*/*/*.md" | Get-Content | Out-String;
				$report = $reportFile | Get-Content | Out-String;
				$match = [Regex]::Match($report, '(?i)#+\s+(Summary|Results?|Report)');
				$report = $report.Substring(0, ($match.Index + $match.Length));
				"$report`r`n`r`n$summary" | Out-File $reportFile -Encoding utf8;
				Get-Item "$($dll.DirectoryName)/*artifacts*/*/*.html" | Invoke-Item;
			}
		}
		finally { Pop-Location; }
	}
    else { Write-Host " no benchmarks found." -ForegroundColor Yellow; }
}

#endregion

#region ----- DB Migration -----

Task "Rebuild-FlywayLocalDb" -alias "rebuild-db" -description "This task rebuilds the local database using flyway." `
-depends @("restore") -action{
	[string]$flyway = Get-Flyway;
	$secret = Get-Secret "local";
	Assert (-not [string]::IsNullOrEmpty($secret.database)) "A connection string for your local database was not provided.";

	$db = [ConnectionInfo]::new($secret, $secret.database);
	Write-Header "flyway: clean ($($db.GetFlywayUrl()))";
	Exec { &$flyway clean $db.GetFlywayUrl() $db.GetFlyUser() $db.GetFlyPassword(); }
	Write-Header "flyway: migrate ($($db.GetFlywayUrl()))";
	Exec { &$flyway migrate $db.GetFlywayUrl() $db.GetFlyUser() $db.GetFlyPassword() $([ConnectionInfo]::ConvertToFlywayLocation($MigrationDirectory)); }
	Exec { &$flyway info $db.GetFlywayUrl() $db.GetFlyUser() $db.GetFlyPassword() $([ConnectionInfo]::ConvertToFlywayLocation($MigrationDirectory)); }
}

#endregion

#region ----- PUBLISHING -----

Task "Publish-NuGetPackages" -alias "push-nuget" -description "This task publish all nuget packages to nuget.org." `
-precondition { return Test-Path $ArtifactsDir -PathType Container } `
-depends @("restore") -action {
	$apiKey = Get-Secret "nugetKey" -Assert;

	foreach ($nupkg in (Get-ChildItem $ArtifactsDir -Recurse -Filter "*.nupkg"))
	{
		Write-Header "dotnet: nuget push '$($nupkg.Name)'";
		Exec { &dotnet nuget push $nupkg.FullName --source "https://api.nuget.org/v3/index.json" --api-key $apiKey; }
	}
}

Task "Publish-PowershellModules" -alias "push-ps" -description "This task publish all powershell modules to powershellgallery.org." `
-depends @("restore") -action {
    $apiKey = Get-Secret "psGalleryKey" -Assert;

    foreach ($psd1 in (Get-ChildItem $ArtifactsDir -Recurse -Filter "*.psd1"))
    {
        if (Test-ModuleManifest $psd1.FullName)
        {
            Write-Header "PS: publish '$($psd1.BaseName)'";
            Publish-Module -Path $psd1.DirectoryName -NuGetApiKey $apiKey;
        }
    }
}

Task "Publish-Database" -alias "push-db" -description "This task publishes the application database to the appropriate host." `
-depends @("restore", "rebuild-db") -action {
	$secret = $null;
	foreach ($key in @($Branch, $FallbackBranch))
	{
		$secret = Get-Secret $key;
		if (-not [string]::IsNullOrEmpty($secret)) { break; }
	}
	Assert(-not [string]::IsNullOrEmpty($secret.database)) "Unable to update database because no connection info was provided for the '$Branch' branch. Verify the secrets.json file.";

	$db = [ConnectionInfo]::new($secret, $secret.database);
	Write-Header "flyway: migrate ($($db.GetFlywayUrl()))";
	[string]$flyway = Get-Flyway;
	Exec { &$flyway migrate $db.GetFlywayUrl() $db.GetFlyUser() $db.GetFlyPassword() $([ConnectionInfo]::($MigrationDirectory)); }
	Exec { &$flyway info $db.GetFlywayUrl() $db.GetFlyUser() $db.GetFlyPassword() $([ConnectionInfo]::ConvertToFlywayLocation($MigrationDirectory)); }
}

Task "Publish-Websites" -alias "push-web" -description "This task publish all websites to their respective host." `
-precondition { return Test-Path $ArtifactsDir -PathType Container } `
-depends @("restore")  -action {
	[string]$waws = Get-WAWSDeploy;

	foreach ($package in (Get-ChildItem $ArtifactsDir -Recurse -Filter "web-*"))
	{
		$id = $package.BaseName.TrimStart("web-");

		$secret = $null;
		$errorMsg = "Unable to publish '$($package.Name)' because the web-host password was not defined. Verify the secrets.json.";
		foreach ($key in @($Branch, $FallbackBranch))
		{
			$secret = Get-Secret $key;
			if (($secret -eq $null) -or ($secret.PSObject.Properties.Match($id) -eq $null)) { continue; }
			$webHost = [ConnectionInfo]::new($secret, $secret.$id);
			if ([string]::IsNullOrEmpty($webHost.Password)) { throw $errorMsg; } else { break; }
		}

		[string]$publishData = Get-ChildItem $PSScriptRoot -Recurse -Filter "*$id-$key.publishsettings" | Select-Object -First 1 -ExpandProperty FullName;
		if ([string]::IsNullOrEmpty($publishData)) { throw "Unable to publish '$($package.BaseName)' because a respective .publshsetting file do not exist."; }
		else
		{
            $del = $DeleteExistingFiles | CND "/deleteexistingfiles" "";
			Exec { &$waws $package.FullName $publishData /password $webHost.Password /appoffline $del; }
			if (-not $NonInteractive)
			{
				[xml]$doc = Get-Content $publishData;
				$appUrl = $doc.SelectSingleNode("//publishProfile[@destinationAppUrl]").Attributes["destinationAppUrl"].Value;
                if (-not [string]::IsNullOrEmpty($appUrl))
                {
                    Start-Process $appUrl;
                }
			}
		}
	}
}

Task "Tag-Release" -alias "tag" -description "This task tags the last commit with the version number." `
-depends @("restore") -action {
    $version = Get-NcrementManifest $ManifestJson | Convert-NcrementVersionNumberToString;
    if ($Branch -ieq "master")
    {
        Exec { &git tag v$version | Out-Null; }
        Exec { &git push "origin" --tags | Out-Null; }
    }
    else
    {
        Exec { &git push "origin" | Out-Null; }
    }
}

#endregion

#region ----- HELPER FUNCTIONS -----

function Get-MSBuild([string]$version = "*")
{
    $instance = Get-VSSetupInstance -All | Select-VSSetupInstance -Latest;
    return (Join-Path $instance.InstallationPath "msbuild/$version/bin/msbuild.exe" | Resolve-Path) -as [string];
}

function Get-Dotfuscator()
{
    $instance = Get-VSSetupInstance -All | Select-VSSetupInstance -Latest;
    $dotfuscator = Join-Path $instance.InstallationPath "Common7/IDE/Extensions/PreEmptiveSolutions/DotfuscatorCE/dotfuscatorCLI.exe";

    return (Test-Path $dotfuscator) | CND $dotfuscator "";
}

function Get-Flyway([string]$version="5.1.4")
{
	[string]$flyway = Join-Path $ToolsDir "flyway/$version/flyway";
    [string]$url = "http://repo1.maven.org/maven2/org/flywaydb/flyway-commandline/{1}/flyway-commandline-{1}-{0}-x64.zip";
    switch ($env:OS)
    {
        default 
        {
            $flyway = "$flyway.cmd";
            $url = [string]::Format($url, "windows", $version);
        }
    }

	if (-not (Test-Path $flyway))
	{
		$zip = Join-Path $env:TEMP "flyway-$version.zip";
		try
		{
			Invoke-WebRequest $url -OutFile $zip;

			$dest = Join-Path $ToolsDir "flyway";
			Expand-Archive $zip -DestinationPath $dest -Force;
			Get-Item "$dest/*" | Rename-Item -NewName $version;
		}
		finally { if (Test-Path($zip)) { Remove-Item $zip -Force; } }
	}

    return $flyway;
}

function Get-WAWSDeploy([string]$version="1.8.0")
{
	[string]$waws = Join-Path $ToolsDir "/WAWSDeploy/$version/tools/WAWSDeploy.exe";

	if (-not (Test-Path $waws))
	{
		$zip = Join-Path $env:TEMP "wawsdeploy.zip";
		try
		{
			Invoke-WebRequest "https://chocolatey.org/api/v2/package/WAWSDeploy/$version" -OutFile $zip;
			Expand-Archive $zip -DestinationPath (Join-Path $ToolsDir "WAWSDeploy/$version") -Force;
		}
		finally { if (Test-Path $zip) { Remove-Item $zip -Force; } }
	}

    return $waws;
}

function Get-Secret([Parameter(ValueFromPipeline)][string]$key, [string]$customMsg = "", [switch]$Assert)
{
	$value = $Secrets.ContainsKey($key) | CND $Secrets.$key $null;
	if ([string]::IsNullOrEmpty($value) -and (Test-Path $SecretsJson))
	{
		$value = Get-Content $SecretsJson | Out-String | ConvertFrom-Json | Select-Object -ExpandProperty $key -ErrorAction Ignore;
	}

	if ($Assert) { Assert (-not [string]::IsNullOrEmpty($value)) ([string]::IsNullOrEmpty($customMsg) | CND "A '$key' property was not specified. Provided a value via the `$Secrets parameter eg. `$Secrets=@{'$key'='your_sercret_value'}" $customMsg); }
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

function CND([Parameter(Mandatory, ValueFromPipeline)][bool]$Condition, [Parameter(Position = 0)]$TrueValue, [Parameter(Position = 1)]$FalseValue)
{
	if ($Condition) { return $TrueValue; } else { return $FalseValue; }
}

Class ConnectionInfo {
	ConnectionInfo($dbNode, [string]$connectionString) {
		if ([string]::IsNullOrEmpty($connectionString)) { throw "The '`$connectionString' parameter cannot be null or empty."; }

		$this.Host = [Regex]::Match($connectionString, '(?i)(server|data source|host)=(?<value>[^;]+);?').Groups["value"].Value;
		$this.User = [Regex]::Match($connectionString, '(?i)(user|usr)=(?<value>[^;]+);?').Groups["value"].Value;
		$this.Password = [Regex]::Match($connectionString, '(?i)(password|pwd)=(?<value>[^;]+);?').Groups["value"].Value;
		$this.Resource = [Regex]::Match($connectionString, '(?i)(database|catalog)=(?<value>[^;]+);?').Groups["value"].Value;
		$this.ConnectionString = $connectionString;
		$this.JDBCUrl = $dbNode.JDBCUrl;
	}

	[string]$Host;
	[string]$User;
	[string]$Password;
	[string]$Resource;
	[string]$ConnectionString;

    [string]$JDBCUrl;

	[string] GetFlywayUrl(){
		return "-url=$([string]::Format($this.JDBCUrl, $this.Host, $this.Resource))";
	}

	[string] GetFlyUser() {
		return "-user=$($this.User)";
	}

	[string] GetFlyPassword() {
		return "-password=$($this.Password)";
	}

	static [string] ConvertToFlywayLocation([string]$path) {
		return "-locations=filesystem:$path";
	}
}

#endregion

FormatTaskName "$(Write-Header -AsValue)`r`n  {0}`r`n$(Write-Header -AsValue)";