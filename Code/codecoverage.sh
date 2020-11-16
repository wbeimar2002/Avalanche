
#usage: 
# sh codecoverage.sh 'Avalanche.Api.Tests/Avalanche.Api.Tests.csproj' 'release' '../TestResults/api'
# sh codecoverage.sh 'Avalanche.Security.Server.Tests/Avalanche.Security.Server.Tests.csproj' 'release' '../TestResults/security'
<<<<<<< HEAD
project_path=$1 #           'Avalanche.Api.Tests/Avalanche.Api.Tests.csproj'
build_configuration=$2 #    'release'
coverlet_outpath=$3 #       '../TestResults/api'
report_outpath=$4 #         'coverage'

dotnet restore $project_path
dotnet build -c $build_configuration $project_path
# dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
dotnet test $project_path  -c $build_configuration  /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=$coverlet_outpath
# reportgenerator "-reports:coverage.cobertura.xml" "-targetdir:coveragereport" -reporttypes:Html
reportgenerator "-reports:TestResults/*.xml" "-targetdir:$report_outpath" -reporttypes:Html
=======

project_path=$1 #           'Avalanche.Api.Tests/Avalanche.Api.Tests.csproj'
build_configuration=$2 #    'release'
coverlet_outpath=$3 #       '../TestResults/api'

dotnet restore $project_path
dotnet build -c $build_configuration $project_path
dotnet test $project_path  -c $build_configuration  /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=$coverlet_outpath


>>>>>>> 07f292dd077a7642332d199e714d47138b5f37fb



