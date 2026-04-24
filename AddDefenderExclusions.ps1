# ============================================================
# Godot + .NET 编译加速 - Windows Defender 排除项自动添加脚本
# ============================================================
# 用途: 将 Godot 项目、dotnet SDK、Godot 编辑器路径加入 Defender 白名单
# 执行方式: 右键 -> 以管理员身份运行 PowerShell
# ============================================================

# 检查管理员权限
if (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "❌ 错误: 此脚本需要管理员权限!" -ForegroundColor Red
    Write-Host "请右键点击 PowerShell 并选择 '以管理员身份运行'，然后重新执行此脚本。" -ForegroundColor Yellow
    pause
    exit
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Godot 编译加速 - Defender 排除项配置" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 定义需要排除的路径
$exclusionPaths = @(
    # Godot 项目目录（两个工作区）
    "C:\Godot\3d-practice",
    "C:\Godot\AISpace",
    
    # .NET SDK 目录
    "C:\Program Files\dotnet",
    
    # Godot 编辑器安装目录
    "C:\GodotEngine\Godot_v4.6.1-stable_mono_win64"
)

# 添加排除项
$successCount = 0
$failCount = 0

foreach ($path in $exclusionPaths) {
    if (Test-Path $path) {
        try {
            Add-MpPreference -ExclusionPath $path -ErrorAction Stop
            Write-Host "✅ 已添加: $path" -ForegroundColor Green
            $successCount++
        } catch {
            Write-Host "⚠️  添加失败: $path" -ForegroundColor Yellow
            Write-Host "   原因: $_" -ForegroundColor Gray
            $failCount++
        }
    } else {
        Write-Host "⏭️  跳过（路径不存在）: $path" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "执行完成: 成功 $successCount 项, 失败 $failCount 项" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 显示当前所有排除项（验证）
Write-Host "当前 Defender 排除项列表:" -ForegroundColor Yellow
Get-MpPreference | Select-Object -ExpandProperty ExclusionPath | ForEach-Object {
    if ($_ -match "Godot|dotnet") {
        Write-Host "  - $_" -ForegroundColor White
    }
}

Write-Host ""
Write-Host "💡 提示: 脚本已配置当前引擎路径 C:\GodotEngine\Godot_v4.6.1-stable_mono_win64" -ForegroundColor Yellow
Write-Host "   如果将来更换引擎版本或位置，请编辑此脚本更新路径。" -ForegroundColor Gray
Write-Host ""

pause
