param (
    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [string]
    $OutputPath = '.\bin\Xpass'
)

Write-Host 'Building'

# 编译 x86 平台
$publishArgs = @(
    '.\Xpass.csproj',
    '-c', 'Release',
    '-p:PublishSelfContained=false',
    '-p:PublishSingleFile=true',
    '-r', 'win-x86',
    '-o', $OutputPath
)

Write-Host "Publishing x86 with args: $($publishArgs -join ' ')"
dotnet publish @publishArgs

if (-Not $?) {
    Write-Host "Build failed with exit code: $lastExitCode"
    exit $lastExitCode
}

# 重命名输出文件并移除不必要的文件
if (Test-Path -Path "$OutputPath\Xpass.exe") {
    mv -Path "$OutputPath\Xpass.exe" -Destination "$OutputPath\Xpass_x86.exe" -Force
    Get-ChildItem -Path "$OutputPath" -File | Where-Object { $_.Name -ne 'Xpass_x86.exe' } | ForEach-Object {
        Remove-Item -LiteralPath $_.FullName -Force
    }
    $x86Size = (Get-Item "$OutputPath\Xpass_x86.exe").Length / 1MB
    Write-Host "x86 build completed. File size: $([math]::Round($x86Size, 2)) MB"
}

# 编译 x64 平台
$publishArgs = @(
    '.\Xpass.csproj',
    '-c', 'Release',
    '-p:PublishSelfContained=false',
    '-p:PublishSingleFile=true',
    '-r', 'win-x64',
    '-o', $OutputPath
)

Write-Host "Publishing x64 with args: $($publishArgs -join ' ')"
dotnet publish @publishArgs

if (-Not $?) {
    Write-Host "Build failed with exit code: $lastExitCode"
    exit $lastExitCode
}

# 重命名输出文件并移除不必要的文件
if (Test-Path -Path "$OutputPath\Xpass.exe") {
    mv -Path "$OutputPath\Xpass.exe" -Destination "$OutputPath\Xpass_x64.exe" -Force
    Get-ChildItem -Path "$OutputPath" -File | Where-Object { $_.Name -notin @('Xpass_x64.exe', 'Xpass_x86.exe') } | ForEach-Object {
        Remove-Item -LiteralPath $_.FullName -Force
    }
    $x64Size = (Get-Item "$OutputPath\Xpass_x64.exe").Length / 1MB
    Write-Host "x64 build completed. File size: $([math]::Round($x64Size, 2)) MB"
}

Write-Host 'Build done'

ls $OutputPath
exit 0
