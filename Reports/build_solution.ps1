Install-PackageProvider -Name NuGet -Force
Install-Module VSSetup -Scope CurrentUser -Force
Install-Module BuildUtils -Scope CurrentUser -Force

$msbuildLocation = Get-LatestMsbuildLocation
set-alias msb $msbuildLocation 

dotnet restore ..\Code\Avalanche.Api.sln
msb ..\Code\Avalanche.Api.sln /p:Configuration=Release
