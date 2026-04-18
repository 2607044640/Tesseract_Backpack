@echo off
chcp 65001 >nul 2>&1
set LOG_DIR=%~dp0..\KiroWorkingSpace\.kiro\CloudSync_Workflow\logs
if not exist "%LOG_DIR%" mkdir "%LOG_DIR%" 2>nul
for /f "tokens=2 delims==" %%I in ('wmic os get localdatetime /value') do set datetime=%%I
set LOG_FILE=%LOG_DIR%\sync_%datetime:~0,8%_%datetime:~8,6%.log
echo [%datetime:~0,4%-%datetime:~4,2%-%datetime:~6,2% %datetime:~8,2%:%datetime:~10,2%:%datetime:~12,2%] Sync started >> "%LOG_FILE%" 2>&1
python "%~dp0..\KiroWorkingSpace\.kiro\CloudSync_Workflow\kiro_sync_to_drive.py" >> "%LOG_FILE%" 2>&1
echo [%datetime:~0,4%-%datetime:~4,2%-%datetime:~6,2% %datetime:~8,2%:%datetime:~10,2%:%datetime:~12,2%] Sync completed >> "%LOG_FILE%" 2>&1
exit /b
