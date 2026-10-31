$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$publish = Join-Path $PSScriptRoot 'dist/store-publish'
$stage = Join-Path $PSScriptRoot 'dist/store-stage'
$output = Join-Path $PSScriptRoot 'dist/store'
# All directories below are generated build outputs.
foreach ($dir in @($publish, $stage, $output)) {
    if (Test-Path $dir) { Remove-Item $dir -Recurse -Force }
    New-Item -ItemType Directory -Path $dir -Force | Out-Null
}
dotnet publish --source https://api.nuget.org/v3/index.json ./RaduSelf.PCCheck.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:StoreBuild=true -o $publish
if ($LASTEXITCODE -ne 0) { throw 'Store build failed.' }
Copy-Item (Join-Path $publish 'RaduSelf.PCCheck.exe') $stage
Copy-Item 'Store/AppxManifest.xml' $stage
Copy-Item 'LICENSE' (Join-Path $stage 'LICENSE.txt')
Copy-Item 'PRIVACY.md' $stage
$assets = Join-Path $stage 'Assets'
New-Item -ItemType Directory -Path $assets -Force | Out-Null
Add-Type -AssemblyName System.Drawing
$icon = [System.Drawing.Icon]::new((Join-Path $PSScriptRoot 'Assets/RADUSELF.ico'), 256, 256)
$image = $icon.ToBitmap()
try {
    foreach ($item in @(@('StoreLogo.png',50),@('Square44x44Logo.png',44),@('Square150x150Logo.png',150))) {
        $size = [int]$item[1]
        $bitmap = [System.Drawing.Bitmap]::new($size,$size)
        $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
        try {
            $graphics.Clear([System.Drawing.Color]::Transparent)
            $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $graphics.DrawImage($image,0,0,$size,$size)
            $bitmap.Save((Join-Path $assets $item[0]),[System.Drawing.Imaging.ImageFormat]::Png)
        } finally { $graphics.Dispose(); $bitmap.Dispose() }
    }
} finally { $image.Dispose(); $icon.Dispose() }
$sdk = Join-Path ${env:ProgramFiles(x86)} 'Windows Kits/10/bin'
$makeappx = Get-ChildItem "$sdk/*/x64/makeappx.exe" | Sort-Object { [version]$_.Directory.Parent.Name } -Descending | Select-Object -First 1
if (-not $makeappx) { throw 'Install the Windows SDK to obtain MakeAppx.exe.' }
$package = Join-Path $output 'RADUSELF-PC-Check-Store-x64.msix'
& $makeappx.FullName pack /d $stage /p $package /o
if ($LASTEXITCODE -ne 0) { throw 'MSIX packaging or manifest validation failed.' }
Get-FileHash $package -Algorithm SHA256 | Format-List
Write-Host "Unsigned Store submission package: $package"
Write-Host 'This unsigned package is for Partner Center submission; it is not ready for direct installation or public download.'
