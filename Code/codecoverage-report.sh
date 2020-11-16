#usage:
# codecoverage-report.sh 'TestResults/*.xml' 'coverage'

coverlet_outpath=$1 #       'TestResults/*.xml'
report_outpath=$2 #         'coverage'

reportgenerator "-reports:$coverlet_outpath" "-targetdir:$report_outpath" -reporttypes:Html



