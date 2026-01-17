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
    -p:PublishSingleFile=true `
    -p:IncludeAllContentForSelfExtract=false `
    -p:DebugType=none `
    -p:DebugSymbols=false `
    -p:Optimize=true `
    -p:IncludeNativeLibrariesForSelfExtract=false `
    -p:EnableCompressionInSingleFile=false `
    -r win-x86 `
    -o "$OutputPath"

if (-Not $?) {
    exit $lastExitCode
}

# 重命名输出文件并清理所有其他文件
if (Test-Path -Path "$OutputPath\Xpass.exe") {
    mv -Path "$OutputPath\Xpass.exe" -Destination "$OutputPath\Xpass_x86.exe" -Force
    # 删除 runtimeconfig.json，只保留单个 exe
    Remove-Item -Path "$OutputPath\*.runtimeconfig.json" -Force -ErrorAction SilentlyContinue
    # 清理所有其他不必要的文件
    Get-ChildItem -Path "$OutputPath" -Exclude "Xpass_x86.exe" | Remove-Item -Force
}

# 编译 x64 平台
dotnet publish `
    .\Xpass.csproj `
    -c Release `
    --self-contained false `
    -p:PublishSingleFile=true `
    -p:IncludeAllContentForSelfExtract=false `
    -p:DebugType=none `
    -p:DebugSymbols=false `
    -p:Optimize=true `
    -p:IncludeNativeLibrariesForSelfExtract=false `
    -p:EnableCompressionInSingleFile=false `
    -r win-x64 `
    -o "$OutputPath"

if (-Not $?) {
    exit $lastExitCode
}

# 重命名输出文件并清理所有其他文件
if (Test-Path -Path "$OutputPath\Xpass.exe") {
    mv -Path "$OutputPath\Xpass.exe" -Destination "$OutputPath\Xpass_x64.exe" -Force
    # 删除 runtimeconfig.json，只保留单个 exe
    Remove-Item -Path "$OutputPath\*.runtimeconfig.json" -Force -ErrorAction SilentlyContinue
    # 清理所有其他不必要的文件，只保留两个 exe
    Get-ChildItem -Path "$OutputPath" -Exclude "Xpass_x64.exe","Xpass_x86.exe" | Remove-Item -Force
}

Write-Host 'Build done'

ls $OutputPath
exit 0