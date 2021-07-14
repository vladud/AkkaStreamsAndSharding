$currentLocation = Get-Location
$configuration='Release'
$pathToExe = "$currentLocation\bin\$configuration\AkkaStreamsAndSharding.exe"
if (-not(Test-Path $pathToExe)) { 
    echo "Running msbuild..."
    msbuild /m:4 /t:build /p:Configuration=$configuration /p:Optimize=True AkkaStreamsAndSharding.csproj
}
echo $pathToExe

#Start first instance
Start-Process -FilePath $pathToExe
Start-Sleep -Milliseconds 500

$wshell = New-Object -ComObject wscript.shell
$wshell.AppActivate('AkkaStreamsAndSharding.exe')

#Start node 1
Start-Sleep -Milliseconds 100
$wshell.SendKeys('1')

#Start sending input to cluster
Start-Sleep -Milliseconds 100
$wshell.SendKeys('2')


#Wait for all actors to be created on node 1
Start-Sleep -Milliseconds 10000

#Start node 1
Start-Process -FilePath $pathToExe -ArgumentList "5006 false"
Start-Sleep -Milliseconds 500

$wshell = New-Object -ComObject wscript.shell
$wshell.AppActivate('AkkaStreamsAndSharding.exe5006')

#Start node 2
Start-Sleep -Milliseconds 100
$wshell.SendKeys('1')


#Node 2 is marked as unresponsive (auto-down-unreachable-after = 5s)