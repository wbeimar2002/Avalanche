dotnet restore 'Avalanche.Api.Tests.csproj'
dotnet build -c release 'Avalanche.Api.Tests.csproj'
# dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
dotnet test 'Avalanche.Api.Tests.csproj'  -c release  /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput="../TestResults/api"
# reportgenerator "-reports:coverage.cobertura.xml" "-targetdir:coveragereport" -reporttypes:Html




