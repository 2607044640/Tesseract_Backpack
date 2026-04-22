using Godot;
using Godot.Composition;
using System;

/// [组件名称] - [简短描述]
/// 
/// [详细描述组件的职责和功能]
/// 
/// 依赖：
/// - [依赖的组件 1]
/// - [依赖的组件 2]
/// 
/// 事件：
/// - [发出的事件 1]
/// - [发出的事件 2]
/// 
/// 适用于：
/// - [适用的实体类型]
[GlobalClass]  // 可选：让组件在编辑器中全局可见
[Component(typeof(Node3D))]  // 必需：指定父节点类型
// [ComponentDependency(typeof(OtherComponent))]  // 可选：声明依赖（取消注释并替换为实际类型）
public partial class MyComponent : Node
{
    #region Events (向外发出的事件)
    
    /// [事件描述]
    public event Action<int> OnSomethingHappened;
    
    /// [事件描述]
    public event Action OnAnotherEvent;
    
    #endregion

    #region Export Properties (可在编辑器中配置)
    
    /// [属性描述]
    [Export] public float SomeValue { get; set; } = 1.0f;
    
    /// [属性描述]
    [Export] public bool IsEnabled { get; set; } = true;
    
    /// [属性描述]
    [Export] public NodePath SomeNodePath { get; set; } = "NodeName";
    
    #endregion

    #region Private Fields (内部状态)
    
    private float _internalState = 0.0f;
    private Node3D _cachedNode;
    
    #endregion

    #region Godot Lifecycle (生命周期)
    
    /// 组件初始化
    public override void _Ready()
    {
        // 必需：初始化组件系统
        InitializeComponent();
        
        // 可选：初始化内部状态
        InitializeInternalState();
        
        GD.Print($"{GetType().Name}: 已初始化 ✓");
    }
    
    /// Entity 初始化完成后自动调用
    /// 在这里订阅其他组件的事件
    public void OnEntityReady()
    {
        // 订阅依赖组件的事件
        // otherComponent 是自动生成的变量（首字母小写）
        // otherComponent.OnSomeEvent += HandleSomeEvent;
        
        GD.Print($"{GetType().Name}: 已订阅事件 ✓");
    }
    
    /// 每帧更新（可选）
    public override void _Process(double delta)
    {
        if (!IsEnabled) return;
        
        // 每帧逻辑
        UpdateLogic(delta);
    }
    
    /// 物理帧更新（可选）
    public override void _PhysicsProcess(double delta)
    {
        if (!IsEnabled) return;
        
        // 物理逻辑
        UpdatePhysics(delta);
    }
    
    /// 输入处理（可选）
    public override void _UnhandledInput(InputEvent @event)
    {
        if (!IsEnabled) return;
        
        // 输入逻辑
        HandleInput(@event);
    }
    
    /// 组件销毁时清理
    public override void _ExitTree()
    {
        // 取消订阅事件（防止内存泄漏）
        // if (otherComponent != null)
        // {
        //     otherComponent.OnSomeEvent -= HandleSomeEvent;
        // }
    }
    
    #endregion

    #region Initialization (初始化逻辑)
    
    /// 初始化内部状态
    private void InitializeInternalState()
    {
        // 获取场景节点引用
        _cachedNode = parent.GetNodeOrNull<Node3D>(SomeNodePath);
        
        if (_cachedNode == null)
        {
            GD.PushWarning($"{GetType().Name}: 节点未找到: {SomeNodePath}");
        }
    }
    
    #endregion

    #region Event Handlers (事件处理)
    
    /// 处理其他组件的事件
    private void HandleSomeEvent(int value)
    {
        GD.Print($"{GetType().Name}: 收到事件，值={value}");
        
        // 处理逻辑
        _internalState += value;
        
        // 可以发出自己的事件
        OnSomethingHappened?.Invoke(value);
    }
    
    #endregion

    #region Update Logic (更新逻辑)
    
    /// 每帧更新逻辑
    private void UpdateLogic(double delta)
    {
        // 实现每帧逻辑
    }
    
    /// 物理帧更新逻辑
    private void UpdatePhysics(double delta)
    {
        // 实现物理逻辑
        // 可以访问 parent（自动生成的变量）
        // parent.Position += ...;
    }
    
    /// 输入处理逻辑
    private void HandleInput(InputEvent @event)
    {
        // 实现输入逻辑
    }
    
    #endregion

    #region Public API (公共接口)
    
    /// 公共方法：供其他组件调用
    public void DoSomething(float value)
    {
        if (!IsEnabled) return;
        
        _internalState = value;
        GD.Print($"{GetType().Name}: DoSomething({value})");
        
        // 发出事件通知其他组件
        OnSomethingHappened?.Invoke((int)value);
    }
    
    /// 公共方法：获取状态
    public float GetState()
    {
        return _internalState;
    }
    
    #endregion

    #region Helper Methods (辅助方法)
    
    /// 私有辅助方法
    private void HelperMethod()
    {
        // 辅助逻辑
    }
    
    #endregion
}

/* 
 * ============================================
 * 使用示例
 * ============================================
 * 
 * 1. 创建 Entity：
 * 
 * [Entity]
 * public partial class MyEntity : Node3D
 * {
 *     public override void _Ready()
 *     {
 *         InitializeEntity();
 *     }
 * }
 * 
 * 2. 场景结构：
 * 
 * MyEntity (Node3D)
 * ├── MyComponent (Node)
 * └── OtherComponent (Node)
 * 
 * 3. 在其他组件中使用：
 * 
 * [ComponentDependency(typeof(MyComponent))]
 * public partial class AnotherComponent : Node
 * {
 *     public void OnEntityReady()
 *     {
 *         // myComponent 是自动生成的变量
 *         myComponent.OnSomethingHappened += HandleSomething;
 *         myComponent.DoSomething(42);
 *     }
 * }
 * 
 * ============================================
 * 设计原则
 * ============================================
 * 
 * 1. 单一职责：组件只做一件事
 * 2. 事件驱动：通过事件通信，避免紧耦合
 * 3. 可配置：使用 [Export] 暴露参数
 * 4. 可复用：不依赖具体的 Entity 类型
 * 5. 清晰命名：方法名清楚表达意图
 * 
 * ============================================
 * 注意事项
 * ============================================
 * 
 * 1. 必须是 partial class
 * 2. 必须调用 InitializeComponent()
 * 3. 事件订阅在 OnEntityReady() 中
 * 4. 记得在 _ExitTree() 中取消订阅
 * 5. 使用 parent 访问父节点
 * 6. 依赖注入的变量名是首字母小写的类型名
 */
