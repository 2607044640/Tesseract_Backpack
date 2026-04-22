using Godot;
using System;
using System.IO;

/// 主题切换组件Helper - 用于切换UI主题
/// 自动查找根Control节点并应用主题
/// 支持拖拽Theme资源到Inspector
[Tool]
[GlobalClass]
public partial class ThemeSwitcherComponentHelper : BaseSettingComponentHelper
{
	// ===== 暴露的配置参数 =====
	[ExportCategory("Theme Settings")]
	
	private Theme[] _themes = LoadDefaultThemes();
	[Export] 
	public Theme[] Themes 
	{ 
		get => _themes;
		set
		{
			_themes = value;
			UpdateControl();
		}
	}
	
	/// 加载默认主题资源
	private static Theme[] LoadDefaultThemes()
	{
		string[] defaultPaths = new string[]
		{
			"res://A1UIResources/themes (2)/arcade/theme_default.tres",
			"res://A1UIResources/themes/gravity.tres",
			"res://A1UIResources/themes/grow.tres",
			"res://A1UIResources/ThemeGen/generated/modern_game_theme.tres",
			"res://A1UIResources/themes (2)/theme_lobby.tres",
			"", // Empty = no theme (default Godot theme)
			"res://A1UIResources/themes/expedition.tres",
			"res://A1UIResources/themes/lab.tres",
			"res://A1UIResources/themes/lore.tres",
			"res://A1UIResources/themes/steal_this_theme.tres"
		};
		
		Theme[] themes = new Theme[defaultPaths.Length];
		for (int i = 0; i < defaultPaths.Length; i++)
		{
			if (string.IsNullOrEmpty(defaultPaths[i]))
			{
				themes[i] = null; // null = default Godot theme
			}
			else
			{
				themes[i] = ResourceLoader.Load<Theme>(defaultPaths[i]);
				if (themes[i] == null)
				{
					GD.PushWarning($"Failed to load default theme: {defaultPaths[i]}");
				}
			}
		}
		
		return themes;
	}
	
	private string[] _themeNames = Array.Empty<string>();
	[Export] 
	public string[] ThemeNames 
	{ 
		get => _themeNames;
		set
		{
			_themeNames = value;
			UpdateControl();
		}
	}
	
	private int _defaultThemeIndex = 0;
	[Export] 
	public int DefaultThemeIndex 
	{ 
		get => _defaultThemeIndex;
		set
		{
			_defaultThemeIndex = value;
			UpdateControl();
		}
	}
	
	// ===== 事件：C# event Action模式 =====
	public new event Action<int, string> ThemeChanged;
	
	// ===== 内部引用 =====
	[Export] public NodePath ThemeDropdownPath { get; set; } = "%ThemeDropdown_OptionButton";
	private OptionButton OptionButton_Theme;
	private int _currentIndex;
	
	protected override void InitializeSpecificNodes()
	{
		OptionButton_Theme = GetNodeOrNull<OptionButton>(ThemeDropdownPath);
		if (OptionButton_Theme == null)
		{
			GD.PushError($"[{Name}] ThemeDropdown not found: {ThemeDropdownPath}");
			return;
		}
	}
	
	protected override void ConnectSignals()
	{
		if (OptionButton_Theme != null)
			OptionButton_Theme.ItemSelected += OnThemeSelected;
	}
	
	protected override void UpdateControl()
	{
		UpdateThemeList();
		UpdateSelection();
	}
	
	private void UpdateThemeList()
	{
		if (OptionButton_Theme != null)
		{
			OptionButton_Theme.Clear();
			
			// 如果没有提供ThemeNames，自动从Theme资源文件名生成
			string[] displayNames = GetDisplayNames();
			
			for (int i = 0; i < displayNames.Length; i++)
			{
				OptionButton_Theme.AddItem(displayNames[i], i);
			}
		}
		UpdateSelection();
	}
	
	/// 获取显示名称：优先使用ThemeNames，否则从Theme资源路径提取文件名
	private string[] GetDisplayNames()
	{
		// 如果ThemeNames数组长度匹配Themes数组，直接使用
		if (ThemeNames != null && ThemeNames.Length == Themes.Length)
		{
			return ThemeNames;
		}
		
		// 否则从Theme资源路径自动生成名称
		string[] names = new string[Themes.Length];
		for (int i = 0; i < Themes.Length; i++)
		{
			if (Themes[i] == null)
			{
				names[i] = "Default (No Theme)";
			}
			else
			{
				string path = Themes[i].ResourcePath;
				if (string.IsNullOrEmpty(path))
				{
					names[i] = $"Theme {i + 1}";
				}
				else
				{
					// 提取文件名（不含扩展名）并格式化
					string fileName = Path.GetFileNameWithoutExtension(path);
					names[i] = FormatThemeName(fileName);
				}
			}
		}
		return names;
	}
	
	/// 格式化主题名称：将下划线替换为空格，首字母大写
	private string FormatThemeName(string fileName)
	{
		// 替换下划线为空格
		string formatted = fileName.Replace("_", " ");
		
		// 首字母大写
		if (formatted.Length > 0)
		{
			formatted = char.ToUpper(formatted[0]) + formatted.Substring(1);
		}
		
		return formatted;
	}
	
	private void UpdateSelection()
	{
		if (OptionButton_Theme != null && Themes.Length > 0)
		{
			int index = Mathf.Clamp(DefaultThemeIndex, 0, Themes.Length - 1);
			_currentIndex = index;
			OptionButton_Theme.Selected = index;
		}
	}
	
	private void OnThemeSelected(long index)
	{
		_currentIndex = (int)index;
		ApplyTheme(_currentIndex);
		
		string[] displayNames = GetDisplayNames();
		if (_currentIndex < displayNames.Length)
		{
			ThemeChanged?.Invoke(_currentIndex, displayNames[_currentIndex]);
		}
	}
	
	/// 应用主题到根Control节点
	private void ApplyTheme(int index)
	{
		if (index < 0 || index >= Themes.Length)
		{
			GD.PrintErr($"Invalid theme index: {index}");
			return;
		}
		
		// 查找根Control节点
		Control rootControl = FindRootControl();
		if (rootControl == null)
		{
			GD.PrintErr("Could not find root Control node");
			return;
		}
		
		Theme theme = Themes[index];
		
		// null表示使用默认主题（无主题）
		if (theme == null)
		{
			rootControl.Theme = null;
			GD.Print($"Applied default theme to {rootControl.Name}");
			return;
		}
		
		// 应用主题
		rootControl.Theme = theme;
		string[] displayNames = GetDisplayNames();
		GD.Print($"Applied theme '{displayNames[index]}' to {rootControl.Name}");
	}
	
	/// 向上遍历查找根Control节点
	/// 根Control定义为：没有Control类型父节点的Control节点
	private Control FindRootControl()
	{
		Node current = this;
		Control lastControl = null;
		
		// 向上遍历直到找不到父节点
		while (current != null)
		{
			if (current is Control control)
			{
				lastControl = control;
			}
			current = current.GetParent();
		}
		
		return lastControl;
	}
	
	public override void ResetToDefault()
	{
		_currentIndex = DefaultThemeIndex;
		UpdateSelection();
		ApplyTheme(_currentIndex);
	}
	
	public override Variant GetSettingValue()
	{
		return _currentIndex;
	}
	
	public override void SetSettingValue(Variant value)
	{
		SetTheme((int)value);
	}
	
	public void SetTheme(int index)
	{
		if (OptionButton_Theme != null && index >= 0 && index < Themes.Length)
		{
			_currentIndex = index;
			OptionButton_Theme.Selected = index;
			ApplyTheme(index);
		}
	}
	
	public int GetCurrentIndex()
	{
		return _currentIndex;
	}
	
	public string GetCurrentThemeName()
	{
		string[] displayNames = GetDisplayNames();
		return _currentIndex < displayNames.Length ? displayNames[_currentIndex] : "";
	}
	
	protected override void DisconnectSignals()
	{
		if (OptionButton_Theme != null)
			OptionButton_Theme.ItemSelected -= OnThemeSelected;
	}
}
