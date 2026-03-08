# Windows 任务栏控制方法研究

## 问题
THide 和 AutoHotkey 基础方法都不起作用，需要找到可靠的任务栏隐藏/控制方法。

## 方法汇总

### 方法1：PowerShell 直接控制（最可靠）

**隐藏任务栏：**
```powershell
$p='HKCU:SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StuckRects3'
$v=(Get-ItemProperty -Path $p).Settings
$v[8]=3
Set-ItemProperty -Path $p -Name Settings -Value $v
Stop-Process -f -ProcessName explorer
```

**显示任务栏：**
```powershell
$p='HKCU:SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StuckRects3'
$v=(Get-ItemProperty -Path $p).Settings
$v[8]=2
Set-ItemProperty -Path $p -Name Settings -Value $v
Stop-Process -f -ProcessName explorer
```

**原理：** 直接修改注册表 `StuckRects3` 的 Settings 值的第8个字节
- `$v[8]=3` = 隐藏任务栏
- `$v[8]=2` = 显示任务栏
- 修改后重启 explorer.exe 生效

### 方法2：注册表手动编辑

1. 按 `Win + R`，输入 `regedit`
2. 导航到：`HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\StuckRects3`
3. 找到 `Settings` 值，右键 → 修改
4. 在二进制数据中，找到第二行第一个值（通常是 `02` 或 `7A`）
   - Windows 10: 将 `02` 改为 `03`（隐藏）
   - Windows 11: 将 `7A` 改为 `7B`（隐藏）
5. 打开任务管理器（Ctrl+Shift+Esc），重启 `Windows 资源管理器`

### 方法3：AutoHotkey + PowerShell 组合（推荐）

```autohotkey
#NoEnv
SendMode Input

; Ctrl + ~ 切换任务栏
^SC029::
Run, powershell.exe -WindowStyle Hidden -Command "$p='HKCU:SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StuckRects3';$v=(Get-ItemProperty -Path $p).Settings;if($v[8] -eq 2){$v[8]=3}else{$v[8]=2};Set-ItemProperty -Path $p -Name Settings -Value $v;Stop-Process -f -ProcessName explorer", , Hide
return
```

**优点：** 
- 不依赖 THide
- 直接调用 PowerShell 修改注册表
- 一键切换隐藏/显示

### 方法4：第三方工具（备选）

如果上述方法都不行，可以尝试：

1. **Buttery Taskbar** - GitHub: LuisThiamNye/ButteryTaskbar2
   - 只在按 Win 键时显示任务栏
   
2. **Taskbar Hide** - 轻量级隐藏工具
   
3. **Task-Homie** - 快捷键控制

## 测试步骤

1. 先在 PowerShell 中手动测试隐藏/显示命令
2. 确认命令有效后，再集成到 AHK 脚本
3. 如果 PowerShell 命令也不行，说明系统权限或策略有问题

## 注意事项

- 修改注册表前建议创建系统还原点
- `StuckRects3` 是 Windows 10/11 的路径，老版本可能是 `StuckRects2`
- 重启 explorer.exe 会导致屏幕闪烁，这是正常现象
- 某些企业版 Windows 可能被组策略锁定，无法修改

## 来源

- https://www.itechtics.com/hide-show-taskbar/
- https://mundobytes.com/en/hide-taskbar-windows-11/
- https://www.airdroid.com/uem/how-to-hide-taskbar/
