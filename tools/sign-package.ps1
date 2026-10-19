# 用你自己的证书签名 appx / appxbundle
#
# 用途：GitHub Release 里提供的是「未签名」包，直接安装会提示证书不受信任。
# 如果你有自己安装过的侧载证书（例如 HoloLens 的 HoloLensSideload），可以用这个脚本签名后再装。
#
# 用法：
#   .\tools\sign-package.ps1 -Package ".\AppPackages\...\xxx.appxbundle" -Pfx "C:\path\mycert.pfx" -Password "123456"
#   或者用证书库里已安装的证书（按指纹）：
#   .\tools\sign-package.ps1 -Package ".\AppPackages\...\xxx.appxbundle" -Thumbprint "443CE17001E3C4AEC1DD0432DB3BF64AAF47BD34"
#
# 注意：包清单里的 Publisher 必须和证书 Subject 一致，否则会报 APPX0109/签名不匹配。
#       本项目 Package.appxmanifest 的 Publisher 是 CN=43A199A3-3813-4434-830F-536BE5EFB42E，
#       如果你自己的证书 Subject 不同，需要改清单里的 Publisher（或新生成一张 Subject 相同的证书）。

param(
    [Parameter(Mandatory = $true)]
    [string]$Package,

    [string]$Pfx,
    [string]$Password,
    [string]$Thumbprint,

    [string]$TimestampUrl = "http://timestamp.digicert.com"
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path $Package)) {
    throw "找不到要签名的包: $Package"
}

# 找 signtool
$signtool = Get-ChildItem "C:\Program Files (x86)\Windows Kits\10\bin\*\x64\signtool.exe" -ErrorAction SilentlyContinue |
    Sort-Object FullName -Descending | Select-Object -First 1
if (-not $signtool) {
    throw "没找到 signtool.exe，请先安装 Windows 10 SDK"
}
Write-Host "signtool: $($signtool.FullName)" -ForegroundColor Cyan

$signArgs = @('sign', '/fd', 'SHA256', '/td', 'SHA256', '/tr', $TimestampUrl)

if ($Pfx) {
    if (-not (Test-Path $Pfx)) { throw "找不到证书文件: $Pfx" }
    $signArgs += @('/f', $Pfx)
    if ($Password) { $signArgs += @('/p', $Password) }
}
elseif ($Thumbprint) {
    $signArgs += @('/sha1', $Thumbprint)
}
else {
    throw "请提供 -Pfx + -Password，或 -Thumbprint（证书库里已安装的证书）"
}

$signArgs += $Package
Write-Host "执行: signtool $($signArgs -join ' ')" -ForegroundColor Cyan
& $signtool.FullName @args

if ($LASTEXITCODE -ne 0) {
    Write-Host "`n签名失败。如果是因为时间戳服务器连不上，可以加参数 -TimestampUrl '' 跳过时间戳再试。" -ForegroundColor Yellow
    exit $LASTEXITCODE
}

Write-Host "`n验证签名..." -ForegroundColor Cyan
& $signtool.FullName verify /pa /v $Package
Write-Host "`n签名完成 ✅" -ForegroundColor Green
