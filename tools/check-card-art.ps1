[CmdletBinding()]
param([string]$PreviewPath)
$ErrorActionPreference='Stop'
$taskRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$exe=Join-Path $taskRoot ('.card-art-check-'+[Guid]::NewGuid().ToString('N')+'.exe')
try {
 & "$env:WINDIR/Microsoft.NET/Framework64/v4.0.30319/csc.exe" /nologo /target:exe "/out:$exe" "/r:$(Join-Path $taskRoot 'Cet6Story.exe')" /r:System.Drawing.dll /r:System.Windows.Forms.dll (Join-Path $PSScriptRoot 'CardArtChecks.cs')
 if($LASTEXITCODE -ne 0){throw '卡面检查编译失败。'}
 Push-Location $taskRoot
 try { if($PreviewPath){& $exe $PreviewPath}else{& $exe}; if($LASTEXITCODE -ne 0){throw '独立卡面检查失败。'} } finally { Pop-Location }
} finally { Remove-Item -LiteralPath $exe -ErrorAction SilentlyContinue }
