[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$taskRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
$speech=Get-ChildItem (Join-Path $env:WINDIR 'Microsoft.NET/assembly/GAC_MSIL/System.Speech') -Recurse -Filter System.Speech.dll | Select-Object -First 1 -ExpandProperty FullName
if(-not $speech){throw '缺少 Windows System.Speech 组件。'}
$sourceFiles=Get-ChildItem (Join-Path $taskRoot 'source') -Filter *.cs | ForEach-Object {$_.FullName}
$exePath=Join-Path $taskRoot 'Cet6Story.exe'
& $compiler /nologo /target:winexe "/out:$exePath" /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.Web.Extensions.dll "/r:$speech" /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll $sourceFiles
if($LASTEXITCODE -ne 0){throw '编译失败。'}
Write-Output "编译成功：$exePath"


