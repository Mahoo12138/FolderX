param(
  [string]$PublishDir = 'publish/win-x64'
)
$cert = Get-ChildItem Cert:\CurrentUser\My | Where-Object { $_.Subject -like '*FolderX Self-Signed*' } | Select-Object -First 1
if ($cert -eq $null) { Write-Output "Cert not found"; exit 1 }
Get-ChildItem $PublishDir -Filter *.exe -File -Recurse | ForEach-Object {
  Write-Output "Signing $($_.FullName)"
  $res = Set-AuthenticodeSignature -FilePath $_.FullName -Certificate $cert -HashAlgorithm SHA256
  Write-Output $res.Status
  Write-Output $res.SignerCertificate.Subject
}
Get-ChildItem $PublishDir -Filter *.dll -File -Recurse | ForEach-Object {
  Write-Output "Signing $($_.FullName)"
  $res = Set-AuthenticodeSignature -FilePath $_.FullName -Certificate $cert -HashAlgorithm SHA256
  Write-Output $res.Status
  Write-Output $res.SignerCertificate.Subject
}
Write-Output "Done"
