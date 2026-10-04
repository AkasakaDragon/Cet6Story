[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$taskRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$audioDirectory = Join-Path $taskRoot 'assets/rogue/audio/monsters'
New-Item -ItemType Directory -Path $audioDirectory -Force | Out-Null
$temporaryStem = Join-Path ([IO.Path]::GetTempPath()) ('cet-monster-audio-' + [Guid]::NewGuid().ToString('N'))
$sourcePath = $temporaryStem + '.cs'
$exePath = $temporaryStem + '.exe'
@'
using System;using System.IO;
class ExportMonsterAudio {
 static void Main(string[] args){for(int p=0;p<MonsterAudio.Profiles.Length;p++)for(int e=0;e<MonsterAudio.Events.Length;e++)MonsterAudio.Write(Path.Combine(args[0],MonsterAudio.Profiles[p]+"-"+MonsterAudio.Events[e]+".wav"),MonsterAudio.Build(p,e));MonsterAudio.Write(Path.Combine(Path.GetDirectoryName(args[0]),"coin-gain.wav"),CurrencyAudio.Build());Console.WriteLine("Generated coin-gain and 40 original monster sounds (44.1 kHz, stereo, PCM16).");}
}
'@ | Set-Content -LiteralPath $sourcePath -Encoding utf8
try {
 $compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
 & $compiler /nologo /target:exe "/out:$exePath" (Join-Path $taskRoot 'source/MonsterAudio.cs') (Join-Path $taskRoot 'source/CurrencyAudio.cs') $sourcePath
 if ($LASTEXITCODE -ne 0) { throw '音效生成程序编译失败。' }
 & $exePath $audioDirectory
 if ($LASTEXITCODE -ne 0) { throw '音效生成失败。' }
} finally { Remove-Item -LiteralPath $sourcePath,$exePath -ErrorAction SilentlyContinue }
