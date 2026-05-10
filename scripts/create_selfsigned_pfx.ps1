param(
  [string]$Password = 'mahoo12138-folderx',
  [string]$OutPfx = 'codesign-selfsigned.pfx',
  [string]$OutB64 = 'codesign-selfsigned.pfx.b64'
)
$p = ConvertTo-SecureString -String $Password -AsPlainText -Force
$cert = New-SelfSignedCertificate -Subject 'CN=FolderX Self-Signed' -Type CodeSigningCert -KeyExportPolicy Exportable -KeyAlgorithm RSA -KeyLength 2048 -HashAlgorithm SHA256 -CertStoreLocation 'Cert:\CurrentUser\My' -NotAfter (Get-Date).AddYears(10)
Export-PfxCertificate -Cert $cert -FilePath $OutPfx -Password $p
[Convert]::ToBase64String([IO.File]::ReadAllBytes($OutPfx)) | Out-File $OutB64 -Encoding ascii
Write-Output "Created $OutPfx and $OutB64"
