$gitversionresult = dotnet .\lib\GitVersion\GitVersion.dll | ConvertFrom-Json
$NuGetVersion = $gitversionresult.NuGetVersionV2
write-host packing $NuGetVersion
$loc = Get-Location
dotnet pack --output $loc'\nupkg' /p:PackageVersion=$NuGetVersion