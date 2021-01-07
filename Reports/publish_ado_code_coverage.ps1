param (
        $homedir,
        $testdir, 
        $reporttype,
        $nobuild, 
        #$curdir,
        $localdisplay
      )


write-host "home="$homedir                      # root directory reports folder
write-host "testdir="$testdir                   # directory where reports are saved
write-host "reporttype="$reporttype             # report type e.g. xml, html
write-host "nobuild="$nobuild                   # when running tests if projects need to be rebuilt
#write-host "curdir="$curdir                     # directory from where this program is running 
write-host "localdisplay="$localdisplay         # pop-up web report if true

$report_path="$homedir\codecoverage\report"
$targetdir="$homedir\codecoverage\target"

write-host "report_path="$report_path
write-host "targetdir="$targetdir

$files=Get-ChildItem -Path $testdir -Filter *Test.csproj -Recurse

$fullname=""
$name=""

#todo build via MSBUILD -- msbuild /p:Configuration=Release

foreach ($item in $files)
{
        $fullname=$item.FullName
        $name=$item.Name
        Write-Host "Processing test project"$fullname
        if ($nobuild -eq 'true')
        {
                write-host "nobuild=true"
                dotnet test $FullName  -c release `
                /p:CollectCoverage=true  /p:CoverletOutputFormat=cobertura /p:CoverletOutput=$report_path\$name.xml                     
        }
         if ($nobuild -eq 'false')
        {
                write-host "nobuild=false"
                dotnet test  $FullName  -c release `
                --no-build /p:CollectCoverage=true  /p:CoverletOutputFormat=cobertura /p:CoverletOutput=$report_path\$name.xml
        }
        

}

reportgenerator "-reports:$report_path\*.xml" "-targetdir:$targetdir" -reporttypes:$reporttype

if ( $localdisplay -eq 'true') 
{
        start $targetdir\index.Html
}
