param (
    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [string]
    $OutputPath = '.\bin\Xpass'
)

Write-Host 'Building'

# 编译 x86 平台
dotnet publish `
    .\Xpass.csproj `
    -c Release `
    --self-contained false `
    -p:PublishReadyToRun=true `
    -p:PublishSingleFile=true `
    -r win-x86 `
    -o "$OutputPath"

if (-Not $?) {
    exit $lastExitCode
}

# 重命名输出文件并移除不必要的文件
if (Test-Path -Path "$OutputPath\Xpass.exe") {
    mv -Path "$OutputPath\Xpass.exe" -Destination "$OutputPath\Xpass_x86.exe" -Force
    rm -Force "$OutputPath\*.pdb"
    rm -Force "$OutputPath\*.xml"
}

# 编译 x64 平台
dotnet publish `
    .\Xpass.csproj `
    -c Release `
    --self-contained false `
    -p:PublishReadyToRun=true `
    -p:PublishSingleFile=true `
    -r win-x64 `
    -o "$OutputPath"

if (-Not $?) {
    exit $lastExitCode
}

# 重命名输出文件并移除不必要的文件
if (Test-Path -Path "$OutputPath\Xpass.exe") {
    mv -Path "$OutputPath\Xpass.exe" -Destination "$OutputPath\Xpass_x64.exe" -Force
    rm -Force "$OutputPath\*.pdb"
    rm -Force "$OutputPath\*.xml"
}

Write-Host 'Build done'

Write-Host 'Build done'

ls $OutputPath
exit 0