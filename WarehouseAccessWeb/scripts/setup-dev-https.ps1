$ErrorActionPreference = "Stop"

$certDir = Join-Path $PSScriptRoot "..\.cert"
$certDir = [System.IO.Path]::GetFullPath($certDir)
New-Item -ItemType Directory -Force -Path $certDir | Out-Null

$pfxPath = Join-Path $certDir "dev-cert.pfx"
$cerPath = Join-Path $certDir "dev-cert.cer"
$passwordPath = Join-Path $certDir "dev-cert-password.txt"
$password = "warehouse-access-dev"

$localIps = [System.Net.Dns]::GetHostAddresses([System.Net.Dns]::GetHostName()) |
  Where-Object {
    $_.AddressFamily -eq [System.Net.Sockets.AddressFamily]::InterNetwork -and
    $_.IPAddressToString -notlike "127.*" -and
    $_.IPAddressToString -notlike "169.254.*"
  } |
  ForEach-Object { $_.IPAddressToString } |
  Select-Object -Unique

$dnsNames = @("localhost")
$dnsNames += $localIps
$dnsNames = $dnsNames | Select-Object -Unique

Write-Host "Creating HTTPS certificate for:"
$dnsNames | ForEach-Object { Write-Host "- $_" }

$existing = Get-ChildItem Cert:\CurrentUser\My |
  Where-Object { $_.FriendlyName -eq "Warehouse Access Dev HTTPS" }

if ($existing) {
  $existing | Remove-Item -Force
}

$cert = New-SelfSignedCertificate `
  -DnsName $dnsNames `
  -CertStoreLocation "Cert:\CurrentUser\My" `
  -FriendlyName "Warehouse Access Dev HTTPS" `
  -NotAfter (Get-Date).AddYears(2) `
  -KeyAlgorithm RSA `
  -KeyLength 2048 `
  -HashAlgorithm SHA256 `
  -KeyExportPolicy Exportable

$securePassword = ConvertTo-SecureString -String $password -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath $pfxPath -Password $securePassword | Out-Null
Export-Certificate -Cert $cert -FilePath $cerPath | Out-Null
Set-Content -Path $passwordPath -Value $password -NoNewline

Write-Host "Done. Files created:"
Write-Host "- $pfxPath"
Write-Host "- $cerPath"
Write-Host "- $passwordPath"
Write-Host ""
Write-Host "Run: npm run dev:https"
Write-Host "Open the HTTPS LAN URL on mobile, for example: https://<LAN-IP>:5173/check-in-mobile"
Write-Host "Install/trust dev-cert.cer on the mobile device. Mobile browsers only allow camera on trusted HTTPS origins."
