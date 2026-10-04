[CmdletBinding()]
param([string]$PreviewDirectory)
$ErrorActionPreference = 'Stop'
$taskRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
# Place the temporary test executable beside the game to resolve its assembly and resources.
$testExe = Join-Path $taskRoot ('.monster-check-' + [Guid]::NewGuid().ToString('N') + '.exe')
try {
 $compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
 & $compiler /nologo /target:exe "/out:$testExe" "/r:$(Join-Path $taskRoot 'Cet6Story.exe')" /r:System.Drawing.dll /r:System.Windows.Forms.dll (Join-Path $PSScriptRoot 'MonsterCombatChecks.cs')
 if ($LASTEXITCODE -ne 0) { throw '怪物检查程序编译失败。' }
 if ($PreviewDirectory) { & $testExe $PreviewDirectory } else { & $testExe }
 if ($LASTEXITCODE -ne 0) { throw '怪物动作或音效检查失败。' }
} finally { Remove-Item -LiteralPath $testExe -ErrorAction SilentlyContinue }
