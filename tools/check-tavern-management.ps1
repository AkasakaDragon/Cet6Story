[CmdletBinding()]param()
$ErrorActionPreference='Stop'
$taskRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$taskValidation=Join-Path $taskRoot '.validation/management'
New-Item -ItemType Directory -Path $taskValidation -Force | Out-Null
foreach($taskName in @('assets','chapters','tools')){if(-not(Test-Path (Join-Path $taskValidation $taskName))){New-Item -ItemType Junction -Path (Join-Path $taskValidation $taskName) -Target (Join-Path $taskRoot $taskName) | Out-Null}}
& (Join-Path $PSScriptRoot 'build-game.ps1')
Copy-Item (Join-Path $taskRoot 'Cet6Story.exe') (Join-Path $taskValidation 'Cet6Story.exe') -Force
$taskCompiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
& $taskCompiler /nologo /target:exe "/out:$taskValidation/ManagementChecks.exe" "/r:$taskValidation/Cet6Story.exe" /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.Web.Extensions.dll (Join-Path $PSScriptRoot 'TavernManagementChecks.cs')
if($LASTEXITCODE -ne 0){throw '验证程序编译失败'}
& (Join-Path $taskValidation 'ManagementChecks.exe')
if($LASTEXITCODE -ne 0){throw '经营验证失败'}
& $taskCompiler /nologo /target:exe "/out:$taskValidation/HallVisualChecks.exe" "/r:$taskValidation/Cet6Story.exe" /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.Web.Extensions.dll (Join-Path $PSScriptRoot 'TavernHallVisualChecks.cs')
if($LASTEXITCODE -ne 0){throw '界面验证程序编译失败'}
& (Join-Path $taskValidation 'HallVisualChecks.exe')
if($LASTEXITCODE -ne 0){throw '动作及字幕稳定性验证失败'}
