<#
.SYNOPSIS
  Read the PackageVersion from Directory.Build.props,
  if it exists on NuGet.org, update the minor version,
  write it back to props, and return the test version string.
#>
param(
    [Parameter(Mandatory = $true)]
    [string] $PropsPath
)

function Get-PackageVersionFromProps {
<#
.SYNOPSIS
  Read the PackageVersion from PropsPath
#>
    param(
        [Parameter(Mandatory = $true)]
        [string] $PropsPath
    )

    if (-not (Test-Path $PropsPath)) {
        Write-Error "File Not Exists: $PropsPath"
        exit 1
    }

    try {
        [xml]$xml = Get-Content $PropsPath -Encoding UTF8
    }
    catch {
        Write-Error "XML Format Error: $PropsPath"
        exit 1
    }

	$pvlist=$xml.GetElementsByTagName("PackageVersion")
	if ($pvlist -eq 0) {
        Write-Error "No PackageVersion"
        exit 1
	}
    $versionNode = $pvlist[0]

    return $versionNode.InnerText.Trim()
}

$originalVersion = Get-PackageVersionFromProps -PropsPath $PropsPath
Write-Host "$originalVersion"
