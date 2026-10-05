[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$taskRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
Add-Type -AssemblyName System.Speech
$taskChapterPath=Join-Path $taskRoot 'chapters/10-tavern-prologue.json'
$taskChapter=Get-Content -LiteralPath $taskChapterPath -Raw -Encoding UTF8 | ConvertFrom-Json
$taskOutput=Join-Path $taskRoot 'chapters/audio/tavern'
New-Item -ItemType Directory -Force -Path $taskOutput | Out-Null
$taskSynth=New-Object System.Speech.Synthesis.SpeechSynthesizer
$taskVoices=@($taskSynth.GetInstalledVoices() | Where-Object {$_.Enabled -and $_.VoiceInfo.Culture.TwoLetterISOLanguageName -eq 'en'})
if($taskVoices.Count -eq 0){throw 'No installed English speech voice.'}
$taskFormat=New-Object System.Speech.AudioFormat.SpeechAudioFormatInfo(22050,[System.Speech.AudioFormat.AudioBitsPerSample]::Sixteen,[System.Speech.AudioFormat.AudioChannel]::Mono)
$taskBuffer=New-Object IO.MemoryStream
$taskClock=0.0
try {
 for($taskIndex=0;$taskIndex -lt $taskChapter.lines.Count;$taskIndex++){
  $taskLine=$taskChapter.lines[$taskIndex]
  $taskFemale=$taskLine.speaker -in @('伊瑟雅','艾莉娅')
  $taskVoice=$taskVoices | Where-Object {($_.VoiceInfo.Gender -eq 'Female') -eq $taskFemale} | Select-Object -First 1
  if(-not $taskVoice){$taskVoice=$taskVoices[0]}
  $taskSynth.SelectVoice($taskVoice.VoiceInfo.Name)
  $taskSynth.Rate=if($taskLine.speaker -eq '伊瑟雅'){-1}else{0}
  $taskFile=Join-Path $taskOutput ('line-{0:D2}.wav' -f $taskIndex)
  $taskSynth.SetOutputToWaveFile($taskFile,$taskFormat)
  $taskSynth.Speak($taskLine.text)
  $taskSynth.SetOutputToNull()
  $taskReader=New-Object IO.BinaryReader([IO.File]::OpenRead($taskFile))
  try {
   $taskReader.BaseStream.Position=12
   while($taskReader.BaseStream.Position -lt $taskReader.BaseStream.Length){
    $taskTag=[Text.Encoding]::ASCII.GetString($taskReader.ReadBytes(4));$taskSize=$taskReader.ReadUInt32()
    if($taskTag -eq 'data'){$taskPcm=$taskReader.ReadBytes($taskSize);break}
    $taskReader.BaseStream.Position+=$taskSize+($taskSize%2)
   }
  } finally {$taskReader.Dispose()}
  $taskLine.start=[Math]::Round($taskClock,4)
  $taskBuffer.Write($taskPcm,0,$taskPcm.Length)
  $taskClock+=$taskPcm.Length/44100.0
  $taskLine.end=[Math]::Round($taskClock,4)
  $taskGap=New-Object byte[] 11026
  $taskBuffer.Write($taskGap,0,$taskGap.Length);$taskClock+=$taskGap.Length/44100.0
  Remove-Item -LiteralPath $taskFile
 }
 $taskWriter=New-Object IO.BinaryWriter([IO.File]::Create((Join-Path $taskOutput 'prologue.wav')))
 try {
  $taskWriter.Write([Text.Encoding]::ASCII.GetBytes('RIFF'));$taskWriter.Write([int](36+$taskBuffer.Length));$taskWriter.Write([Text.Encoding]::ASCII.GetBytes('WAVEfmt '));$taskWriter.Write([int]16);$taskWriter.Write([int16]1);$taskWriter.Write([int16]1);$taskWriter.Write([int]22050);$taskWriter.Write([int]44100);$taskWriter.Write([int16]2);$taskWriter.Write([int16]16);$taskWriter.Write([Text.Encoding]::ASCII.GetBytes('data'));$taskWriter.Write([int]$taskBuffer.Length);$taskWriter.Write($taskBuffer.ToArray())
 } finally {$taskWriter.Dispose()}
 $taskChapter | ConvertTo-Json -Depth 15 | Set-Content -LiteralPath $taskChapterPath -Encoding UTF8
 Write-Output ('Built English prologue audio: {0:N1} seconds; voices: {1}' -f $taskClock,($taskVoices.VoiceInfo.Name -join ', '))
} finally {$taskSynth.Dispose();$taskBuffer.Dispose()}
