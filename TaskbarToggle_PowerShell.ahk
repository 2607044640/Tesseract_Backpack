#NoEnv
SendMode Input
SetWorkingDir %A_ScriptDir%

; =======================================================
; 任务栏隐藏/显示开关 (Ctrl + ~)
; 使用 PowerShell 直接修改注册表，不依赖 THide
; =======================================================

^SC029::  ; Ctrl + ~ (波浪号键)
Run, powershell.exe -WindowStyle Hidden -Command "$p='HKCU:SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StuckRects3';$v=(Get-ItemProperty -Path $p).Settings;if($v[8] -eq 2){$v[8]=3}else{$v[8]=2};Set-ItemProperty -Path $p -Name Settings -Value $v;Stop-Process -f -ProcessName explorer", , Hide
return

; 测试快捷键 Ctrl + Alt + T
^!t::
MsgBox, TaskbarToggle 脚本正在运行！
return
