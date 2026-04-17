# Tetris Backpack Item Templates

这个文件夹包含两个物品模板，用于快速创建背包物品。

## 📦 模板列表

### 1. BasicItem.tscn - 最小化模板
**用途**：最简单的可点击物品，只有基础的状态切换功能。

**结构**：
```
BasicItem (Control)
├── ClickableBackground (ColorRect) - 可点击区域
└── StateChart
	└── Root
		├── Idle - 空闲状态
		└── Clicked - 点击后状态
```

**状态机**：
- `Idle` → `clicked` 事件 → `Clicked`
- `Clicked` → 0.5秒后自动 → `Idle`

**扩展示例**：
```csharp
// 1. 创建 C# 脚本监听状态变化
public partial class BasicItemController : Node
{
	public override void _Ready()
	{
		var stateChart = GetNode("%StateChart");
		var clickedState = GetNode("%Clicked");
		
		// 监听 Clicked 状态进入
		clickedState.Connect("state_entered", Callable.From(() => {
			GD.Print("物品被点击了！");
			// 播放音效、改变颜色、触发效果等
		}));
	}
}

// 2. 添加点击触发逻辑
public override void _Ready()
{
	var background = GetNode<ColorRect>("%ClickableBackground");
	background.GuiInput += (InputEvent @event) => {
		if (@event is InputEventMouseButton mouseEvent && 
			mouseEvent.Pressed && 
			mouseEvent.ButtonIndex == MouseButton.Left)
		{
			GetNode("%StateChart").Call("send_event", "clicked");
		}
	};
}
```

**适用场景**：
- 消耗品（点击使用）
- 装备物品（点击装备）
- 任务物品（点击触发剧情）
- 需要自定义交互的物品

---

### 2. TetrisDraggableItem.tscn - 完整拖拽模板
**用途**：完整的可拖拽物品，支持拖拽、旋转、网格形状。

**结构**：
```
TetrisDraggableItem (Control)
├── ClickableBackground (ColorRect) - 可点击区域
├── VisualContainer (Control)
│   └── ItemIcon (TextureRect) - 物品图标
├── StateChart
│   └── Root
│       ├── Idle - 空闲状态
│       └── Dragging - 拖拽状态
│           └── FollowMouseUIComponent - 跟随鼠标
├── DraggableItemComponent - 拖拽逻辑
└── GridShapeComponent - 网格形状
```

**状态机**：
- `Idle` → `drag_start` 事件 → `Dragging`
- `Dragging` → `drag_end` 事件 → `Idle`

**功能**：
- ✅ 左键拖拽物品
- ✅ 右键旋转物品（90度）
- ✅ 支持 Tetris 形状定义
- ✅ 自动跟随鼠标
- ✅ 状态机管理

**使用方法**：

1. **实例化场景**：
   ```
   在 Godot 编辑器中：
   - 右键点击场景树
   - 选择 "Instantiate Child Scene"
   - 选择 TetrisDraggableItem.tscn
   ```

2. **配置物品数据**：
   ```csharp
   // 创建 ItemDataResource
   var itemData = new ItemDataResource {
	   ItemID = "sword_001",
	   ItemName = "铁剑",
	   BaseShape = new Array<Vector2I> {
		   new Vector2I(0, 0),
		   new Vector2I(0, 1),
		   new Vector2I(0, 2)  // 1x3 竖条
	   }
   };
   
   // 设置到 GridShapeComponent
   var shapeComponent = item.GetNode<GridShapeComponent>("%GridShapeComponent");
   shapeComponent.Data = itemData;
   ```

3. **设置物品图标**：
   ```csharp
   var icon = item.GetNode<TextureRect>("VisualContainer/ItemIcon");
   icon.Texture = GD.Load<Texture2D>("res://icons/sword.png");
   ```

4. **注册到背包控制器**：
   ```csharp
   var controller = GetNode<BackpackInteractionController>("%BackpackInteractionController");
   controller.RegisterItem(item);
   ```

**适用场景**：
- Tetris 背包系统
- 网格背包系统
- 需要拖拽放置的物品
- 需要旋转的物品

---

## 🔧 自定义扩展

### 添加新状态
```csharp
// 在 StateChart 中添加新状态（如 Equipped）
// 1. 在 Godot 编辑器中添加 AtomicState 节点
// 2. 添加 Transition 连接状态
// 3. 在 C# 中监听状态变化

var equippedState = GetNode("%Equipped");
equippedState.Connect("state_entered", Callable.From(() => {
	GD.Print("物品已装备！");
	// 显示装备效果
}));
```

### 添加自定义组件
```csharp
// 创建自定义组件（如 TooltipComponent）
[GlobalClass]
public partial class TooltipComponent : Node
{
	[Export] public string TooltipText { get; set; }
	
	public override void _Ready()
	{
		var background = GetParent().GetNode<Control>("%ClickableBackground");
		background.MouseEntered += () => ShowTooltip();
		background.MouseExited += () => HideTooltip();
	}
}

// 添加到物品场景
```

### 修改拖拽行为
```csharp
// 订阅 DraggableItemComponent 的事件
var draggable = item.GetNode<DraggableItemComponent>("%DraggableItemComponent");

draggable.OnDragStartedAsObservable
	.Subscribe(_ => {
		GD.Print("开始拖拽");
		// 自定义拖拽开始逻辑
	})
	.AddTo(disposables);

draggable.OnRotateRequestedAsObservable
	.Subscribe(_ => {
		GD.Print("请求旋转");
		// 自定义旋转逻辑
	})
	.AddTo(disposables);
```

---

## 📚 相关文档

- **组件文档**：
  - `DraggableItemComponent.cs` - 拖拽组件
  - `GridShapeComponent.cs` - 网格形状组件
  - `FollowMouseUIComponent.cs` - 跟随鼠标组件
  - `ItemDataResource.cs` - 物品数据资源

- **控制器文档**：
  - `BackpackInteractionController.cs` - 背包交互控制器
  - `BackpackGridComponent.cs` - 背包网格逻辑
  - `BackpackGridUIComponent.cs` - 背包网格 UI

- **架构文档**：
  - `.kiro/steering/Always/DesignPatterns.md` - 设计模式规范

---

## 🎯 快速开始

### 创建一个简单的消耗品
```csharp
// 1. 实例化 BasicItem.tscn
var item = basicItemScene.Instantiate<Control>();
AddChild(item);

// 2. 添加点击逻辑
var background = item.GetNode<ColorRect>("%ClickableBackground");
background.GuiInput += (InputEvent @event) => {
	if (@event is InputEventMouseButton mouseEvent && 
		mouseEvent.Pressed && 
		mouseEvent.ButtonIndex == MouseButton.Left)
	{
		item.GetNode("%StateChart").Call("send_event", "clicked");
		UseItem();  // 使用物品
	}
};
```

### 创建一个可拖拽的剑
```csharp
// 1. 实例化 TetrisDraggableItem.tscn
var sword = draggableItemScene.Instantiate<Control>();
backpackUI.AddChild(sword);

// 2. 配置数据
var itemData = new ItemDataResource {
	ItemID = "sword_001",
	ItemName = "铁剑",
	BaseShape = new Array<Vector2I> {
		new Vector2I(0, 0),
		new Vector2I(0, 1),
		new Vector2I(0, 2)
	}
};

var shapeComponent = sword.GetNode<GridShapeComponent>("%GridShapeComponent");
shapeComponent.Data = itemData;

var icon = sword.GetNode<TextureRect>("VisualContainer/ItemIcon");
icon.Texture = GD.Load<Texture2D>("res://icons/sword.png");

// 3. 注册到控制器
controller.RegisterItem(sword);
```

---

## 🐛 常见问题

**Q: 为什么点击没有反应？**
A: 检查 `ClickableBackground` 的 `mouse_filter` 是否设置为 `Stop`（值为 0）。

**Q: 拖拽时物品不跟随鼠标？**
A: 确保 `FollowMouseUIComponent` 的 `TargetUIPath` 正确指向物品根节点。

**Q: 旋转功能不工作？**
A: 需要在 `BackpackInteractionController` 中订阅 `OnRotateRequestedAsObservable` 事件并调用 `GridShapeComponent.Rotate90()`。

**Q: 如何修改物品大小？**
A: 修改根节点的 `custom_minimum_size` 属性。

---

## 🔄 更新日志

**2026-04-16**：
- ✅ 创建 BasicItem.tscn 最小化模板
- ✅ 创建 TetrisDraggableItem.tscn 完整拖拽模板
- ✅ 使用 Scene Unique Names (%) 替代相对路径
- ✅ 添加完整文档和使用示例
