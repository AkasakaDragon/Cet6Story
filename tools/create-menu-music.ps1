[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$taskRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$audioDirectory = Join-Path $taskRoot 'assets/rogue/audio'
New-Item -ItemType Directory -Path $audioDirectory -Force | Out-Null
$temporaryStem = Join-Path ([IO.Path]::GetTempPath()) ('cet-menu-music-' + [Guid]::NewGuid().ToString('N'))
$sourcePath = $temporaryStem + '.cs'
$exePath = $temporaryStem + '.exe'
@'
using System;using System.IO;
class ExportMonsterAudio {
 static void Main(string[] args){MonsterAudio.Write(Path.Combine(args[0],"menu-orbit.wav"),MenuMusic.Build());Console.WriteLine("Generated original 64-second menu music.");}
}
'@ | Set-Content -LiteralPath $sourcePath -Encoding utf8
try {
 $compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
 & $compiler /nologo /target:exe "/out:$exePath" (Join-Path $taskRoot 'source/MonsterAudio.cs') (Join-Path $taskRoot 'source/MenuMusic.cs') $sourcePath
 if ($LASTEXITCODE -ne 0) { throw '音效生成程序编译失败。' }
 & $exePath $audioDirectory
 if ($LASTEXITCODE -ne 0) { throw '音效生成失败。' }
} finally { Remove-Item -LiteralPath $sourcePath,$exePath -ErrorAction SilentlyContinue }
