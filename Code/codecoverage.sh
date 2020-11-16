
#usage: 
# sh codecoverage.sh 'Avalanche.Api.Tests/Avalanche.Api.Tests.csproj' 'release' '../TestResults/api'
# sh codecoverage.sh 'Avalanche.Security.Server.Tests/Avalanche.Security.Server.Tests.csproj' 'release' '../TestResults/security'
project_path=$1 #           'Avalanche.Api.Tests/Avalanche.Api.Tests.csproj'
build_configuration=$2 #    'release'
coverlet_outpath=$3 #       '../TestResults/api'

dotnet restore $project_path
dotnet build -c $build_configuration $project_path
# dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
dotnet test $project_path  -c $build_configuration  /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=$coverlet_outpath
# reportgenerator "-reports:coverage.cobertura.xml" "-targetdir:coveragereport" -reporttypes:Html




