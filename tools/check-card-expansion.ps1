[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$taskRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$exe=Join-Path $taskRoot ('.card-check-'+[Guid]::NewGuid().ToString('N')+'.exe')
try {
 & "$env:WINDIR/Microsoft.NET/Framework64/v4.0.30319/csc.exe" /nologo /target:exe "/out:$exe" "/r:$(Join-Path $taskRoot 'Cet6Story.exe')" /r:System.Web.Extensions.dll (Join-Path $PSScriptRoot 'CardExpansionChecks.cs')
 if($LASTEXITCODE -ne 0){throw '卡牌检查编译失败。'}
 Push-Location $taskRoot
 try { & $exe; if($LASTEXITCODE -ne 0){throw '卡牌检查失败。'} } finally { Pop-Location }
} finally { Remove-Item -LiteralPath $exe -ErrorAction SilentlyContinue }
