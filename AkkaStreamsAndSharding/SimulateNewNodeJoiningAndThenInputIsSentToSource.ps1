$currentLocation = Get-Location
$configuration='Release'
$pathToExe = "$currentLocation\bin\$configuration\AkkaStreamsAndSharding.exe"
if (-not(Test-Path $pathToExe)) { 
    echo "Running msbuild..."
    msbuild -p:RestorePackagesConfig=true -t:restore /m:4 /t:build /p:Configuration=$configuration /p:Optimize=True "..\AkkaStreamsAndSharding.sln"
}
echo $pathToExe

#Start first instance
Start-Process -FilePath $pathToExe
Start-Sleep -Milliseconds 500

$wshell = New-Object -ComObject wscript.shell
$wshell.AppActivate('AkkaStreamsAndSharding.exe')
Start-Sleep -Milliseconds 100

#Start node 1
$wshell.SendKeys('1')

#Start second instance
Start-Process -FilePath $pathToExe -ArgumentList "5006 false"
Start-Sleep -Milliseconds 500

$wshell = New-Object -ComObject wscript.shell
$wshell.AppActivate('AkkaStreamsAndSharding.exe5006')
Start-Sleep -Milliseconds 100

#Start node 2
$wshell.SendKeys('1')

#Wait for nodes to join cluster
Start-Sleep -Milliseconds 2000

#Start sending input to cluster
$wshell.AppActivate('AkkaStreamsAndSharding.exe')
$wshell.SendKeys('2')

#Both nodes start creating actors, sharding works