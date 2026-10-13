[CmdletBinding()]param()
$ErrorActionPreference='Stop'
$taskRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$taskValidation=Join-Path $taskRoot '.validation/dining'
New-Item -ItemType Directory -Path $taskValidation -Force | Out-Null
foreach($taskName in @('assets','chapters')){if(-not(Test-Path (Join-Path $taskValidation $taskName))){New-Item -ItemType Junction -Path (Join-Path $taskValidation $taskName) -Target (Join-Path $taskRoot $taskName) | Out-Null}}
& (Join-Path $PSScriptRoot 'build-game.ps1')
Copy-Item (Join-Path $taskRoot 'Cet6Story.exe') (Join-Path $taskValidation 'Cet6Story.exe') -Force
$taskCompiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
& $taskCompiler /nologo /target:exe "/out:$taskValidation/DiningChecks.exe" "/r:$taskValidation/Cet6Story.exe" /r:System.Drawing.dll /r:System.Windows.Forms.dll (Join-Path $PSScriptRoot 'TavernDiningChecks.cs')
if($LASTEXITCODE -ne 0){throw '用餐动作验证程序编译失败'}
& (Join-Path $taskValidation 'DiningChecks.exe')
if($LASTEXITCODE -ne 0){throw '用餐动作验证失败'}
