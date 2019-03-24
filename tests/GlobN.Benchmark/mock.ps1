Param(
	[ValidateNotNullOrEmpty()]
	[string]$Configuration = "Debug",

	[ValidateNotNullOrEmpty()]
	[string]$ProjectName = "GlobN"
)

Clear-Host;
$Destination = Join-Path ([System.IO.Path]::GetTempPath()) $ProjectName;

$fileList = Join-Path $PSScriptRoot "fileList.csv";
if (-not (Test-Path $fileList)) { throw "Could not find file-list at '$fileList'."; }
$filenames =  Get-Content $fileList | ConvertFrom-Csv | Select-Object -ExpandProperty Name;

foreach ($folder in @("mock", "mock/lvl-A"))
{
	$dir = Join-Path $Destination $folder;
	if (Test-Path $dir) { continue; }

	foreach($name in $filenames)
	{
		$path = Join-Path $dir $name;
		if (-not (Test-Path $dir)) { New-Item $dir -ItemType Directory | Out-Null; }
		if (-not (Test-Path $path)) { New-Item $path -ItemType File | Out-Null; }
	}
}