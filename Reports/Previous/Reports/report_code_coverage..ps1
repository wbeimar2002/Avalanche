$repo="avalanche-api"
$homepath=$env:HOMEPATH
#$report_path="$homepath\$repo\codecoverage\"
#$targetdir="$homepath\$report_path\coveragereport"

$report_path="$homepath\$repo\codecoverage\report"
$targetdir="$homepath\$repo\codecoverage\target"

Remove-Item $report_path -Recurse
Remove-Item $targetdir -Recurse

dotnet tool install dotnet-reportgenerator-globaltool --global --configfile NuGet.Config

dotnet test ..\Code\Test\Avalanche.Api.Test\Avalanche.Api.Test.csproj `
 -c release  /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=$report_path\Api

dotnet test ..\Code\Test\Avalanche.Security.Server.Test\Avalanche.Security.Server.Test.csproj `
 -c release  /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=$report_path\Security

reportgenerator "-reports:$report_path\*.xml" "-targetdir:$targetdir" -reporttypes:Html

start $targetdir\index.Html

 