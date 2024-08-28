$NugetExe = "$PSScriptRoot/nuget.exe"
$Artifacts = "$PSScriptRoot/artifacts"

$projects = Get-Item "$PSScriptRoot/src/**/*.csproj" `
  | Select-Object -ExpandProperty FullName

$projects | ForEach-Object { 
  & dotnet pack $_ --output $Artifacts
}