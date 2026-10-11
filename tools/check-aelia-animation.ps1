[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$taskRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$taskExe=Join-Path $taskRoot ('.aelia-check-'+[Guid]::NewGuid().ToString('N')+'.exe')
try {
 $compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
 & $compiler /nologo /target:exe "/out:$taskExe" "/r:$(Join-Path $taskRoot 'Cet6Story.exe')" /r:System.Drawing.dll /r:System.Windows.Forms.dll (Join-Path $PSScriptRoot 'AeliaPixelChecks.cs')
 if($LASTEXITCODE -ne 0){throw '女骑士动画检查编译失败。'}
 & $taskExe
 if($LASTEXITCODE -ne 0){throw '女骑士动画检查失败。'}
} finally {Remove-Item -LiteralPath $taskExe -ErrorAction SilentlyContinue}
