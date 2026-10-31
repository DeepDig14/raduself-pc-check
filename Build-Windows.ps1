$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw 'Instaleaza .NET 10 SDK de pe https://dotnet.microsoft.com/download/dotnet/10.0 si redeschide PowerShell.' }
dotnet publish --source https://api.nuget.org/v3/index.json .\RaduSelf.PCCheck.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o .\dist\win-x64
if ($LASTEXITCODE -ne 0) { throw 'Build failed. Send the error text, not private files.' }
Write-Host 'Aplicatia: dist\win-x64\RaduSelf.PCCheck.exe'
