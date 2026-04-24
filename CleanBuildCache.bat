@echo off
REM ============================================================
REM Godot + .NET 编译缓存清理脚本
REM ============================================================
REM 用途: 关闭 dotnet build server 并删除 Godot Mono 缓存
REM 执行时机: 修改 .csproj 配置后，或遇到编译卡顿时
REM ============================================================

echo ========================================
echo   Godot 编译缓存清理工具
echo ========================================
echo.

REM 1. 关闭 dotnet build server（防止进程残留）
echo [1/3] 正在关闭 dotnet build server...
dotnet build-server shutdown
if %ERRORLEVEL% EQU 0 (
    echo ✅ dotnet build server 已关闭
) else (
    echo ⚠️  dotnet build server 关闭失败（可能未运行）
)
echo.

REM 2. 删除 .godot/mono 缓存目录
echo [2/3] 正在删除 .godot\mono 缓存...
if exist ".godot\mono" (
    rmdir /s /q ".godot\mono"
    echo ✅ .godot\mono 已删除
) else (
    echo ⏭️  .godot\mono 不存在，跳过
)
echo.

REM 3. 删除 bin/obj 目录（可选，彻底清理）
echo [3/3] 正在删除 bin 和 obj 目录...
if exist "bin" (
    rmdir /s /q "bin"
    echo ✅ bin 已删除
) else (
    echo ⏭️  bin 不存在，跳过
)

if exist "obj" (
    rmdir /s /q "obj"
    echo ✅ obj 已删除
) else (
    echo ⏭️  obj 不存在，跳过
)
echo.

echo ========================================
echo   清理完成！
echo ========================================
echo.
echo 💡 下一步操作:
echo    1. 打开 Godot 编辑器
echo    2. 点击 Build 按钮（编辑器会自动重建缓存）
echo    3. 测试编译速度是否改善
echo.

pause
