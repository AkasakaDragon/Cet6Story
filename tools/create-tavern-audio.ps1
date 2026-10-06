[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$taskScript=Join-Path $PSScriptRoot 'create-tavern-audio.py'
& python $taskScript
if($LASTEXITCODE -ne 0){throw 'Neural voice generation failed; previous published audio retained.'}
