param([string[]]$Checks=@('InGameSettingsChecks','ChapterOneChecks','ChapterTwoChecks','ChapterContinuationChecks','SubtitleScoringChecks'))
$ErrorActionPreference='Stop'
$taskRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$taskChecks=Join-Path $taskRoot '.validation/chapter-update'
New-Item -ItemType Directory -Path $taskChecks -Force | Out-Null
foreach($taskResource in @('assets','chapters','tools')){
 $taskLink=Join-Path $taskChecks $taskResource
 if(-not (Test-Path -LiteralPath $taskLink)){New-Item -ItemType Junction -Path $taskLink -Target (Join-Path $taskRoot $taskResource) | Out-Null}
}
Copy-Item -LiteralPath (Join-Path $taskRoot 'Cet6Story.exe') -Destination (Join-Path $taskChecks 'Cet6Story.exe') -Force
$taskSave=Join-Path $taskRoot 'save.json'
$taskSaveHash=if(Test-Path -LiteralPath $taskSave){(Get-FileHash -LiteralPath $taskSave).Hash}else{''}
foreach($taskName in $Checks){
 $taskExe=Join-Path $taskChecks ($taskName+'.exe')
 & "$env:WINDIR/Microsoft.NET/Framework64/v4.0.30319/csc.exe" /nologo /target:exe "/out:$taskExe" "/r:$(Join-Path $taskRoot 'Cet6Story.exe')" /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.Web.Extensions.dll (Join-Path $PSScriptRoot ($taskName+'.cs'))
 if($LASTEXITCODE -ne 0){throw "Check compilation failed: $taskName"}
 & $taskExe
 if($LASTEXITCODE -ne 0){throw "Check failed: $taskName"}
}
if($taskSaveHash -and (Get-FileHash -LiteralPath $taskSave).Hash -ne $taskSaveHash){throw 'Player save was changed.'}
Write-Output "Player save preserved. Previews: $taskChecks"
