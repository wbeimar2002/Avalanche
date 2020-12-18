$repo="avalanche-api"
$homepath=$env:HOMEPATH
#$report_path="$homepath\$repo\codecoverage\"
#$targetdir="$homepath\$report_path\coveragereport"

$report_path="$homepath\$repo\codecoverage\report"
$targetdir="$homepath\$repo\codecoverage\target"

Remove-Item $report_path -Recurse
Remove-Item $targetdir -Recurse

dotnet tool install dotnet-reportgenerator-globaltool --global --configfile NuGet.Config

dotnet test ..\Code\Avalanche.Api.Tests\Avalanche.Api.Tests.csproj `
 -c release  /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=$report_path\Api

dotnet test ..\Code\Avalanche.Security.Server.Tests\Avalanche.Security.Server.Tests.csproj `
 -c release  /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=$report_path\Security

reportgenerator "-reports:$report_path\*.xml" "-targetdir:$targetdir" -reporttypes:Html

start $targetdir\index.Html

 