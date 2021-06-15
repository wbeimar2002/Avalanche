param (
        $testdir
      )

$files=Get-ChildItem -Path $testdir -Filter *Test.csproj -Recurse

$fullname=""
$name=""

foreach ($item in $files)
        {
             
                  $fullname=$item.FullName
                  $name=$item.Name
                  Write-Host "Processing test project"$fullname
                  dotnet test $fullname --no-build -c release /p:CollectCoverage=true /p:CoverletOutputFormat=opencover 
                  
        }
