<#
.SYNOPSIS
Builds a .csproj as a nuget package and publishes to a local nuget feed

.PARAMETER Project
Path to a .csproj file which can be packed using "nuget pack"

#>

param (
	[Parameter(Mandatory = $true)][string] $Project
)

$LocalNuget = $env:AVALANCHE_LOCAL_NUGET_FEED

if([string]::IsNullOrEmpty($LocalNuget)) {
    $LocalNuget = "C:\Olympus\nuget"
    Write-Warning -Message "Local Avalanche Nuget Feed is not configured. Defaulting to: $LocalNuget"     
}

# Pack and Publish nuget to local feed
$Epoch = Get-Date -UFormat %s
nuget pack $Project -Version "99.99.$Epoch"
nuget init . $LocalNuget

# Cleanup by deleting nupkg files
Get-ChildItem *.nupkg | foreach { Remove-Item -Path $_.FullName }