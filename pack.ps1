$loc = Get-Location
dotnet pack --output $loc'\nupkg' /p:PackageVersion=100.0.0-alpha