# import the utility Module
import-module BuildUtils -force


$dotCoverVersion = "2019.3.2"
$mergeFileName = "merge.dcvr"
$unitTestsNS = "tests"
$dotnetExe = (Get-Command dotnet).Path
$env:ASPNETCORE_ENVIRONMENT = 'Test'
$vcap = Get-Content '.\ci\win-env.json'
$env:VCAP_SERVICES = $vcap
$env:UseSharedCompilation = 'false'


$SonarAuthToken = (Get-ChildItem Env:sonar-auth-token).value
$SonarUrl = (Get-ChildItem Env:sonar-url).value
$Branch = (Get-ChildItem Env:Branch).value
$ProjectName = (Get-ChildItem Env:Project-Name).value
$OutputDir = -join ($pwd, "\analysis-output\")

$testProject = -join($pwd,"\Solera.MediaInfo.Test\Solera.MediaInfo.Test.csproj")



# This is needed to fix an error where the sonar scanner uses the container directory
# but the analysis engine is using the symlinked directory
# This causes analysis to fail due to a path mismatch
Write-output "INFO: Setting path to symlinked container"
$symlink = get-childitem $pwd.path | Where-Object {$_.Name -eq "git-repo" }  | Select-Object -ExpandProperty Target
write-host "Symlink:  "  $symlink
set-location $symlink




try {
	Invoke-Expression "ls"
    Write-Output "INFO: Installing dot cover nuget package."
    Invoke-Expression "nuget install JetBrains.dotCover.CommandLineTools -Version $dotCoverVersion"

    $dotCoverExe = "JetBrains.dotCover.CommandLineTools.$dotCoverVersion\tools\dotCover.exe"

    Write-Output "INFO: Running dotcover for unit tests project. All output is muted."
	Write-Output "$testProject"
    Invoke-Expression "$dotCoverExe cover /TargetExecutable='$dotnetExe' /ReturnTargetExitCode /TargetArguments='test .\Solera.MediaInfo.Test\Solera.MediaInfo.Test.csproj' /Output=$unitTestsNS.dcvr"
    $testsCode = $LASTEXITCODE
    if($testsCode -lt 0){
        Write-Output "ERROR: Unit and integration tests failed."
        exit $testsCode
    }

	Invoke-Expression "$dotCoverExe merge /Source='$unitTestsNS.dcvr' /Output=$mergeFileName"
    
    Write-Output "INFO: Converting coverage report to HTML format."
    Invoke-Expression "$dotCoverExe report /Source=$mergeFileName /Output=report.html /ReportType=HTML"

	Write-Output "INFO: Restoring required nuget packages to local folder."
    Invoke-Expression "dotnet restore"

    Write-Output "INFO: Starting static code analysis."
    $version = git describe
    
    # Dependency Check
	# This is a required step that will scan for OSS dependencies
	Write-Output "Running Dependency-Check"
	Invoke-Expression "dotnet publish Solera.MediaInfo.Service/Solera.MediaInfo.Service.csproj -c Release -o $symlink\publish"
	Invoke-Expression "dependency-check --project '$ProjectName' --scan $symlink\publish --format 'ALL'"
	# NOTE: this needs to be the full path
	$SonarOptions += "/d:sonar.dependencyCheck.reportPath=" + $($PWD.Path) + "'\dependency-check-report.xml' "
	$SonarOptions += "/d:sonar.dependencyCheck.htmlReportPath=" + $($PWD.Path) + "'\dependency-check-vulnerability.html' "

	Write-Output "INFO: ============== version: $version"
	Write-Output "INFO: ============== SonarUrl: $SonarUrl"

	Write-Output "INFO: ============== START dotnet sonarscanner"
	Invoke-Expression "cd src"
    Invoke-Expression "dotnet sonarscanner begin /k:'Solera.MediaInfo.Service' /n:'Solera::Mobile:Solera.MediaInfo.Service' /v:'$version' /d:sonar.host.url='$SonarUrl' /d:sonar.cs.dotcover.reportsPaths='report.html' /d:sonar.exclusions='**/Program.cs, **/Startup.cs, **/AssemblyVersionAttribute.cs, **/Exception/, **/MicrohexTemplate/Data/**, **/tests/**, **/wwwroot/**'" -ErrorAction Stop
	Write-Output "INFO: ============== END dotnet sonarscanner"

	Write-Output "INFO: ============== START dotnet build"
    Invoke-Expression "dotnet build"
	Write-Output "INFO: ============== END dotnet build"

    $testsCode = $LASTEXITCODE
    if($testsCode -lt 0){
        Write-Output "ERROR: Build failed."
        exit $testsCode
    }

	Write-Output "INFO: ============== START dotnet sonarscanner end | Tee-Object -Variable ScannerOutput"
    Invoke-Expression "dotnet sonarscanner end | Tee-Object -Variable ScannerOutput"
	Write-Output "INFO: ============== END dotnet sonarscanner end | Tee-Object -Variable ScannerOutput"


    # Read the scanner output to get the processing url
    if ( Test-Path ".sonarqube/out/.sonar/report-task.txt" ) {
        # Get the QG Details
        Write-Host "INFO: Getting the results from the Analysis"
        $QualityGateStatus = Get-QualityGateDetails -ApiKey $SonarAuthToken -AnalysisFile ".sonarqube/out/.sonar/report-task.txt" -verbose
    
    
     # Copy the analysis file to output for the metrics
        copy ".sonarqube/out/.sonar/report-task.txt" $OutputDir
    } else {
        Write-Host "Unable to find analysis file - Perhaps the Analysis didn't complete?"
        exit 1
    }

    # Write-Output "INFO: Sending Slack Message to $SlackURL"
    # Write-Output "INFO: Slack Channel: $SlackChannel"
    # Send-SonarqubeSlackMsg -ApiKEy $SonarAuthToken -AnalysisFile ".sonarqube/out/.sonar/report-task.txt" -SlackWebhookUrl $SlackURL -SlackChannel $SlackChannel

    # Display results
    if ( $QualityGateStatus -eq "OK" ) {
        Write-Host "INFO: Quality Gate Success!"
        exit 0
    } elseif ( $QualityGateStatus -eq "ERROR" ) {
        Write-Host "INFO: Failed due to insufficient code coverage"
        exit 1
    }  elseif ( $QualityGateStatus -eq "WARN") {
        Write-Host "INFO: You can pass, but check the SonarQube results"
        exit 0
    } else {
        #Didn't find a valid status
        Write-Host "INFO: Could not find a quality gate status"
        write-host "INFO: What we found: $QualityGateStatus"
        exit 0
    }

}
catch {
    Write-Output "ERROR: We trapped an exception, failing the step"
    $error[0]
    Exit 1
}

exit $testsCode