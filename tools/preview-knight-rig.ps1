[CmdletBinding()]
param([switch]$Check,[switch]$Export,[switch]$BuildOnly)
$ErrorActionPreference='Stop'
$taskRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
$speech=Get-ChildItem (Join-Path $env:WINDIR 'Microsoft.NET/assembly/GAC_MSIL/System.Speech') -Recurse -Filter System.Speech.dll | Select-Object -First 1 -ExpandProperty FullName
$output=Join-Path $taskRoot 'output/knight-rig'
New-Item -ItemType Directory -Path $output -Force | Out-Null
$exePath=Join-Path $output 'KnightRigPreview.exe'
$sources=@(Get-ChildItem (Join-Path $taskRoot 'source') -Filter *.cs | ForEach-Object {$_.FullName})
$sources+=Join-Path $PSScriptRoot 'KnightRigLauncher.cs'
& $compiler /nologo /target:exe /main:KnightRigLauncher "/out:$exePath" /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.Web.Extensions.dll "/r:$speech" /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll $sources
if($LASTEXITCODE -ne 0){throw '骨骼预览编译失败。'}
if($BuildOnly){Write-Output $exePath;return}
if($Check){& $exePath $taskRoot --check}
elseif($Export){& $exePath $taskRoot --export}
else{& $exePath $taskRoot}
if($LASTEXITCODE -ne 0){throw '骨骼预览或检查失败。'}
