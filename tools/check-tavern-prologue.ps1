[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$taskRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$taskChecks=Join-Path $taskRoot '.validation/tavern-prologue'
New-Item -ItemType Directory -Force -Path $taskChecks | Out-Null
foreach($taskResource in @('assets','chapters')){
 $taskLink=Join-Path $taskChecks $taskResource
 if(-not (Test-Path -LiteralPath $taskLink)){New-Item -ItemType Junction -Path $taskLink -Target (Join-Path $taskRoot $taskResource) | Out-Null}
}
Copy-Item -LiteralPath (Join-Path $taskRoot 'Cet6Story.exe') -Destination (Join-Path $taskChecks 'Cet6Story.exe') -Force
$taskExe=Join-Path $taskChecks 'TavernPrologueChecks.exe'
& "$env:WINDIR/Microsoft.NET/Framework64/v4.0.30319/csc.exe" /nologo /target:exe "/out:$taskExe" "/r:$(Join-Path $taskRoot 'Cet6Story.exe')" /r:System.Drawing.dll /r:System.Windows.Forms.dll (Join-Path $PSScriptRoot 'TavernPrologueChecks.cs')
if($LASTEXITCODE -ne 0){throw 'Tavern checks failed to compile.'}
$taskSaveHash=(Get-FileHash -LiteralPath (Join-Path $taskRoot 'save.json')).Hash
& $taskExe
if($LASTEXITCODE -ne 0){throw 'Tavern checks failed.'}
if((Get-FileHash -LiteralPath (Join-Path $taskRoot 'save.json')).Hash -ne $taskSaveHash){throw 'Player save was changed.'}
Write-Output "Preview files: $taskChecks"
