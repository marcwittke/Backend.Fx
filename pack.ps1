$loc = Get-Location
dotnet pack --output $loc'\nupkg' /p:PackageVersion=99.0.0-alpha