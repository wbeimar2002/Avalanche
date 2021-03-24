dotnet tool install dotnet-reportgenerator-globaltool --global --configfile NuGet.Config
PowerShell.exe -ExecutionPolicy Bypass -File build_solution.ps1
PowerShell.exe -ExecutionPolicy Bypass -File publish_ado_code_coverage.ps1 "%HOMEPATH%\avalanche-api" "..\Code\Test" "Html" "false" "true"
