[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$taskRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$checkRoot=Join-Path $taskRoot '.validation/kitchen-learning-check'
New-Item -ItemType Directory -Path $checkRoot -Force | Out-Null
foreach($folder in @('assets','chapters','tools')){
 $path=Join-Path $checkRoot $folder
 if(-not (Test-Path -LiteralPath $path)){New-Item -ItemType Junction -Path $path -Target (Join-Path $taskRoot $folder) | Out-Null}
}
$compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
$speech=Get-ChildItem (Join-Path $env:WINDIR 'Microsoft.NET/assembly/GAC_MSIL/System.Speech') -Recurse -Filter System.Speech.dll | Select-Object -First 1 -ExpandProperty FullName
$files=Get-ChildItem (Join-Path $taskRoot 'source') -Filter '*.cs' | ForEach-Object FullName
$checkExe=Join-Path $checkRoot 'check.exe'
& $compiler /nologo /target:exe /main:KitchenLearningChecks "/out:$checkExe" /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.Web.Extensions.dll "/r:$speech" /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll $files (Join-Path $PSScriptRoot 'KitchenLearningChecks.cs')
if($LASTEXITCODE -ne 0){throw '厨房共享学习检查编译失败。'}
& $checkExe
if($LASTEXITCODE -ne 0){throw '厨房共享学习检查失败。'}
