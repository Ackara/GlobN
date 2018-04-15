<#
.SYNOPSIS
A psake bootstraper; This script runs one or more tasks defined in the psake file.

.EXAMPLE
.\build.ps1 -Help;
This example prints a list of all the available tasks.
#>

Param(
	[Alias('t')]
	[string[]]$Tasks = @("default"),

    [Alias('s', "keys")]
	[hashtable]$Secrets = @{},

	[Alias('c')]
	[ValidateSet("Debug", "Release")]
	[string]$Configuration = "Release",

	[Alias("sc", "no-build")]
	[switch]$SkipCompilation,
	
	[string]$TaskFile = "$PSScriptRoot\build\_.psake*.ps1",
	[switch]$Major,
	[switch]$Minor,
	[switch]$Help
)

# Getting the current branch of source control.
$branchName = $env:BUILD_SOURCEBRANCHNAME;
if ([string]::IsNullOrEmpty($branchName))
{
	$match = [Regex]::Match((& git branch), '\*\s*(?<name>\w+)');
	if ($match.Success) { $branchName = $match.Groups["name"].Value; }
}

# Installing then invoking the Psake tasks.
$psModulesDir = "$PSScriptRoot\build\powershell_modules";
$psakeModule = "$psModulesDir\psake\*\*.psd1";
if (-not (Test-Path $psakeModule))
{ 
	if (-not (Test-Path $psModulesDir)) { New-Item $psModulesDir -ItemType Directory | Out-Null; }
	Save-Module "psake" -Path $psModulesDir; 
}
Import-Module $psakeModule -Force;

if ($Help) { Invoke-Psake -buildFile $TaskFile -docs; }
else
{
	Write-Host -ForegroundColor DarkGray "User:          $env:USERNAME@$env:COMPUTERNAME";
	Write-Host -ForegroundColor DarkGray "Platform:      $env:OS";
	Write-Host -ForegroundColor DarkGray "Branch:        $branchName";
    Write-Host -ForegroundColor DarkGray "Configuration: $Configuration";
	Invoke-psake $taskFile -nologo -taskList $Tasks -properties @{
		"Secrets"=$Secrets;
		"Branch"=$branchName;
		"Major"=$Major.IsPresent;
		"Minor"=$Minor.IsPresent;
		"PoshModulesDir"=$psModulesDir;
		"Configuration"=$Configuration;
		"SkipCompilation"=$SkipCompilation.IsPresent;
        "SolutionName"=(Split-Path $PSScriptRoot -Leaf);
	}
	if (-not $psake.build_success) { exit 1; }
}