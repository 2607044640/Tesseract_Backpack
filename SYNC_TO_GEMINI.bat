@echo off
chcp 65001 >nul
echo.
echo ========================================
echo   🧠 Kiro -^> Gemini 代码同步工具
echo ========================================
echo.
echo 📂 源项目: C:\Godot\3d-practice
echo 🎯 目标: G:\My Drive\Kiro_Godot_Brain
echo 🔧 模式: 纯代码提取（过滤图片/音频/模型）
echo.

python "%~dp0..\KiroWorkingSpace\.kiro\CloudSync_Workflow\kiro_sync_to_drive.py"

echo.
echo ========================================
pause
