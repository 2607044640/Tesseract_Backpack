using Godot;
using Godot.Collections;

/// <summary>
/// 羁绊数据资源 - 《Backpack Battles》风格的物品协同配置
/// 
/// 用途：
/// - 定义物品提供的标签（如 "Food", "Weapon", "Magic"）
/// - 定义星星触发位置（相对于物品的局部偏移）
/// - 定义激活条件（需要相邻物品的标签）
/// - 定义羁绊效果（属性加成、技能触发等）
/// 
/// 羁绊系统原理：
/// 
/// 1. 物品提供标签（ProvidedTags）
///    示例：香蕉提供 ["Food", "Fruit"]
/// 
/// 2. 物品有星星位置（StarOffsets）
///    示例：香蕉在 (1, 0) 和 (-1, 0) 有两颗星星
/// 
/// 3. 星星需要特定标签激活（RequiredTag）
///    示例：香蕉的星星需要相邻物品有 "Food" 标签
/// 
/// 4. 激活后触发效果（SynergyEffect）
///    示例：每颗星星激活 +10% 攻击速度
/// 
/// 使用场景：
/// ```
/// 背包布局：
/// [苹果] [香蕉] [橙子]
///   ↓      ↓      ↓
/// 苹果提供 "Food" → 激活香蕉左侧星星
/// 橙子提供 "Food" → 激活香蕉右侧星星
/// 结果：香蕉获得 +20% 攻击速度
/// ```
/// 
/// 在编辑器中创建：
/// 右键 → 新建资源 → SynergyDataResource → 配置属性 → 保存为 .tres
/// </summary>
[GlobalClass]
public partial class SynergyDataResource : Resource
{
	#region Export Properties
	
	/// <summary>
	/// 物品提供的标签列表
	/// 
	/// 用途：
	/// - 供其他物品检测羁绊时使用
	/// - 一个物品可以提供多个标签
	/// 
	/// 示例：
	/// - 香蕉：["Food", "Fruit"]
	/// - 铁剑：["Weapon", "Metal"]
	/// - 火球术：["Magic", "Fire"]
	/// - 生命药水：["Potion", "Healing"]
	/// 
	/// 命名规范：
	/// - 使用 PascalCase（首字母大写）
	/// - 保持简洁（单个单词）
	/// - 避免重复（如 "FoodItem" 应简化为 "Food"）
	/// </summary>
	[Export] public string[] ProvidedTags { get; set; } = System.Array.Empty<string>();
	
	/// <summary>
	/// 星星触发的局部坐标偏移
	/// 
	/// 坐标系统：
	/// - 相对于物品左上角 (0, 0)
	/// - X 轴向右，Y 轴向下
	/// - 使用网格坐标（整数）
	/// 
	/// 示例：
	/// 
	/// 2x1 横条物品（香蕉）：
	/// ■■
	/// 
	/// 星星配置：[(-1, 0), (2, 0)]
	/// 效果：
	/// ★ ■■ ★
	/// 
	/// L 形物品：
	/// ■
	/// ■■
	/// 
	/// 星星配置：[(0, -1), (2, 1)]
	/// 效果：
	///   ★
	/// ■
	/// ■■ ★
	/// 
	/// 注意：
	/// - 星星可以在物品外部（负坐标或超出范围）
	/// - 星星位置会随物品旋转而旋转
	/// - 配置时使用初始状态的坐标（未旋转）
	/// </summary>
	[Export] public Array<Vector2I> StarOffsets { get; set; } = new Array<Vector2I>();
	
	/// <summary>
	/// 激活星星需要的相邻物品标签
	/// 
	/// 用途：
	/// - 检测星星位置的物品是否提供此标签
	/// - 只需匹配一个标签即可激活
	/// 
	/// 示例：
	/// - 香蕉的星星需要 "Food" → 相邻苹果/橙子可激活
	/// - 铁剑的星星需要 "Weapon" → 相邻其他武器可激活
	/// - 火球术的星星需要 "Magic" → 相邻其他法术可激活
	/// 
	/// 特殊值：
	/// - 空字符串或 null：不需要任何标签（总是激活）
	/// - "Any"：任何物品都可以激活
	/// </summary>
	[Export] public string RequiredTag { get; set; } = "";
	
	/// <summary>
	/// 羁绊效果描述或 ID
	/// 
	/// 用途：
	/// - 描述激活后的效果
	/// - 可以是人类可读的描述（用于 UI 显示）
	/// - 可以是效果 ID（用于代码逻辑）
	/// 
	/// 示例（描述格式）：
	/// - "每颗星星 +10% 攻击速度"
	/// - "每颗星星 +5 攻击力"
	/// - "激活 2 颗星星：触发火焰爆炸"
	/// 
	/// 示例（ID 格式）：
	/// - "Add_AttackSpeed_10"
	/// - "Add_Damage_5"
	/// - "Trigger_FireExplosion"
	/// 
	/// 实现建议：
	/// - 使用结构化格式（如 JSON）存储复杂效果
	/// - 或使用枚举 + 参数的方式
	/// - 或直接在代码中根据 ID 查表
	/// </summary>
	[Export] public string SynergyEffect { get; set; } = "";
	
	#endregion
	
	#region Helper Methods
	
	/// <summary>
	/// 检查是否提供指定标签
	/// </summary>
	public bool HasTag(string tag)
	{
		if (string.IsNullOrEmpty(tag) || ProvidedTags == null)
			return false;
		
		foreach (var providedTag in ProvidedTags)
		{
			if (providedTag == tag)
				return true;
		}
		
		return false;
	}
	
	/// <summary>
	/// 获取星星数量
	/// </summary>
	public int GetStarCount()
	{
		return StarOffsets?.Count ?? 0;
	}
	
	/// <summary>
	/// 验证配置有效性
	/// </summary>
	public bool IsValid()
	{
		if (StarOffsets == null || StarOffsets.Count == 0)
		{
			GD.PushWarning("SynergyDataResource: StarOffsets 为空");
			return false;
		}
		
		if (string.IsNullOrEmpty(RequiredTag))
		{
			GD.PushWarning("SynergyDataResource: RequiredTag 为空");
			return false;
		}
		
		return true;
	}
	
	#endregion
}
