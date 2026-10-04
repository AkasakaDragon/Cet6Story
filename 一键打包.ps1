[CmdletBinding()]
param([switch]$IncludeSave)
$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath($PSScriptRoot)
$outputDirectory = Join-Path $projectRoot '打包输出'
$packageName = 'Cet6Story-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + [Guid]::NewGuid().ToString('N').Substring(0,4)
$zipPath = Join-Path $outputDirectory ($packageName + '.zip')
$partialPath = $zipPath + '.partial'
$archiveRoot = '六级物语'
$required = @('Cet6Story.exe','assets','chapters','tools\ffmpeg\ffmpeg.exe','tools\ffmpeg\LICENSE','项目文档\使用说明.md','项目文档\词域远征-玩法说明.md')
foreach ($relative in $required) {
    if (-not (Test-Path -LiteralPath (Join-Path $projectRoot $relative))) { throw "缺少运行文件：$relative" }
}
New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
$files = New-Object 'System.Collections.Generic.List[System.IO.FileInfo]'
foreach ($relative in @('Cet6Story.exe','项目文档\使用说明.md','项目文档\词域远征-玩法说明.md','项目文档\配音来源与发行许可.md','项目文档\双星卡牌战斗-玩法说明.md')) {
    $fullPath = Join-Path $projectRoot $relative
    if (Test-Path -LiteralPath $fullPath -PathType Leaf) { $files.Add((Get-Item -LiteralPath $fullPath)) }
}
foreach ($relative in @('assets','chapters','tools\ffmpeg')) {
    foreach ($file in (Get-ChildItem -LiteralPath (Join-Path $projectRoot $relative) -File -Recurse)) {
        $files.Add($file)
    }
}
if ($IncludeSave -and (Test-Path -LiteralPath (Join-Path $projectRoot 'save.json'))) {
    $files.Add((Get-Item -LiteralPath (Join-Path $projectRoot 'save.json')))
}
Write-Host '正在打包程序、剧情、配音、图片、词库和倍速工具……'
Write-Host '默认分享包不包含个人存档；原有 save.json 不会修改。'
$archive = $null
try {
    $archive = [IO.Compression.ZipFile]::Open($partialPath,[IO.Compression.ZipArchiveMode]::Create)
    $count = 0
    foreach ($file in $files) {
        # Whitelist of runtime directories, never traverse the output or developer folders.
        $relative = $file.FullName.Substring($projectRoot.Length).TrimStart([char]'\',[char]'/')
        $entry = $archiveRoot + '/' + $relative.Replace('\','/')
        [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive,$file.FullName,$entry,[IO.Compression.CompressionLevel]::Optimal) | Out-Null
        $count++
        Write-Progress -Activity '六级物语打包' -Status "$count / $($files.Count)" -PercentComplete ($count*100/$files.Count)
    }
    $archive.Dispose(); $archive = $null
    Move-Item -LiteralPath $partialPath -Destination $zipPath
    Write-Progress -Activity '六级物语打包' -Completed
    Write-Host "打包成功：$zipPath" -ForegroundColor Green
    Write-Host '收件人完整解压后，双击文件夹里的 Cet6Story.exe 即可。'
    Write-Host ('大小：{0:N1} MB；文件数：{1}' -f ((Get-Item -LiteralPath $zipPath).Length/1MB),$count)
} catch {
    if ($archive) { $archive.Dispose() }
    if (Test-Path -LiteralPath $partialPath) { Remove-Item -LiteralPath $partialPath }
    throw
}




