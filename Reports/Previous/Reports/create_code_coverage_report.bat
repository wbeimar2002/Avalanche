dotnet tool install dotnet-reportgenerator-globaltool --global --configfile NuGet.Config
dotnet restore ..\Code\Avalanche.Api.sln
dotnet msbuild ..\Code\Avalanche.Api.sln /p:Configuration=Release
PowerShell.exe -ExecutionPolicy Bypass -File report_code_coverage..ps1