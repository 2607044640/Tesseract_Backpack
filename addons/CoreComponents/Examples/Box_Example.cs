using Godot;
using Godot.Composition;
using System;

/// <summary>
/// Box 实体示例
/// 
/// 展示如何创建可交互的物理对象（箱子、桶、石头等）。
/// 
/// 功能：
/// - 可被推动
/// - 可被破坏
/// - 物理模拟
/// 
/// 使用的组件：
/// - PushableComponent - 响应推力
/// - HealthComponent - 可破坏（复用 Enemy 的组件）
/// - BreakableComponent - 破碎效果
/// </summary>
[Entity]
public partial class Box : RigidBody3D
{
    public override void _Ready()
    {
        InitializeEntity();
    }
}

/* 
 * 场景结构：
 * 
 * Box (RigidBody3D) - 本脚本
 * ├── PushableComponent (Node)
 * ├── HealthComponent (Node) - 复用
 * ├── BreakableComponent (Node)
 * ├── CollisionShape3D
 * └── MeshInstance3D
 * 
 * 关键点：
 * 1. 使用 RigidBody3D 而不是 CharacterBody3D
 * 2. 复用 HealthComponent（Enemy 也用这个）
 * 3. 组件不关心父节点是什么类型的物体
 */

// ============================================
// PushableComponent 示例实现
// ============================================

/// <summary>
/// 可推动组件
/// 
/// 让物体可以被玩家或其他力量推动。
/// 适用于箱子、桶、石头等物理对象。
/// </summary>
[GlobalClass]
[Component(typeof(RigidBody3D))]
public partial class PushableComponent : Node
{
    #region Export Properties
    
    /// <summary>
    /// 推力倍数
    /// </summary>
    [Export] public float PushForceMultiplier { get; set; } = 1.0f;
    
    /// <summary>
    /// 最大推力
    /// </summary>
    [Export] public float MaxPushForce { get; set; } = 100.0f;
    
    /// <summary>
    /// 是否可以被推动
    /// </summary>
    [Export] public bool CanBePushed { get; set; } = true;
    
    #endregion

    #region Godot Lifecycle
    
    public override void _Ready()
    {
        InitializeComponent();
        
        // 监听碰撞事件
        parent.BodyEntered += OnBodyEntered;
    }
    
    public override void _ExitTree()
    {
        if (parent != null)
        {
            parent.BodyEntered -= OnBodyEntered;
        }
    }
    
    #endregion

    #region Collision Handling
    
    private void OnBodyEntered(Node body)
    {
        if (!CanBePushed) return;
        
        // 检查是否是玩家或其他可推动物体的实体
        if (body is CharacterBody3D character)
        {
            // 计算推力方向
            Vector3 pushDirection = (parent.GlobalPosition - character.GlobalPosition).Normalized();
            
            // 计算推力大小（基于速度）
            float pushForce = character.Velocity.Length() * PushForceMultiplier;
            pushForce = Mathf.Min(pushForce, MaxPushForce);
            
            // 应用推力
            ApplyPush(pushDirection, pushForce);
        }
    }
    
    #endregion

    #region Public API
    
    /// <summary>
    /// 应用推力
    /// </summary>
    public void ApplyPush(Vector3 direction, float force)
    {
        if (!CanBePushed) return;
        
        Vector3 impulse = direction * force;
        parent.ApplyImpulse(impulse);
        
        GD.Print($"PushableComponent: 应用推力 {force}");
    }
    
    #endregion
}

// ============================================
// BreakableComponent 示例实现
// ============================================

/// <summary>
/// 可破坏组件
/// 
/// 让物体在生命值为 0 时破碎。
/// 依赖 HealthComponent。
/// </summary>
[GlobalClass]
[Component(typeof(RigidBody3D))]
[ComponentDependency(typeof(HealthComponent))]
public partial class BreakableComponent : Node
{
    #region Export Properties
    
    /// <summary>
    /// 破碎粒子场景
    /// </summary>
    [Export] public PackedScene BreakParticles { get; set; }
    
    /// <summary>
    /// 破碎音效
    /// </summary>
    [Export] public AudioStream BreakSound { get; set; }
    
    /// <summary>
    /// 碎片场景（可选）
    /// </summary>
    [Export] public PackedScene[] DebrisScenes { get; set; }
    
    #endregion

    #region Godot Lifecycle
    
    public override void _Ready()
    {
        InitializeComponent();
    }
    
    public void OnEntityReady()
    {
        // 订阅 HealthComponent 的死亡事件
        healthComponent.OnDied += OnDied;
    }
    
    public override void _ExitTree()
    {
        if (healthComponent != null)
        {
            healthComponent.OnDied -= OnDied;
        }
    }
    
    #endregion

    #region Event Handlers
    
    private void OnDied()
    {
        GD.Print("BreakableComponent: 物体破碎！");
        
        // 生成破碎粒子
        SpawnBreakParticles();
        
        // 播放破碎音效
        PlayBreakSound();
        
        // 生成碎片
        SpawnDebris();
        
        // 删除原物体
        parent.QueueFree();
    }
    
    #endregion

    #region Break Effects
    
    private void SpawnBreakParticles()
    {
        if (BreakParticles == null) return;
        
        var particles = BreakParticles.Instantiate<GpuParticles3D>();
        GetTree().Root.AddChild(particles);
        particles.GlobalPosition = parent.GlobalPosition;
        particles.Emitting = true;
        
        // 粒子播放完后自动删除
        GetTree().CreateTimer(2.0).Timeout += particles.QueueFree;
    }
    
    private void PlayBreakSound()
    {
        if (BreakSound == null) return;
        
        var audioPlayer = new AudioStreamPlayer3D();
        GetTree().Root.AddChild(audioPlayer);
        audioPlayer.GlobalPosition = parent.GlobalPosition;
        audioPlayer.Stream = BreakSound;
        audioPlayer.Play();
        
        // 音效播放完后自动删除
        audioPlayer.Finished += audioPlayer.QueueFree;
    }
    
    private void SpawnDebris()
    {
        if (DebrisScenes == null || DebrisScenes.Length == 0) return;
        
        foreach (var debrisScene in DebrisScenes)
        {
            if (debrisScene == null) continue;
            
            var debris = debrisScene.Instantiate<RigidBody3D>();
            GetTree().Root.AddChild(debris);
            debris.GlobalPosition = parent.GlobalPosition;
            
            // 给碎片一个随机的初速度
            Vector3 randomVelocity = new Vector3(
                (float)GD.RandRange(-5, 5),
                (float)GD.RandRange(2, 8),
                (float)GD.RandRange(-5, 5)
            );
            debris.LinearVelocity = randomVelocity;
            
            // 碎片 5 秒后自动删除
            GetTree().CreateTimer(5.0).Timeout += debris.QueueFree;
        }
    }
    
    #endregion
}

// ============================================
// 使用示例
// ============================================

/*
 * 创建一个可破坏的箱子：
 * 
 * 1. 创建场景：
 *    Box (RigidBody3D)
 *    ├── PushableComponent
 *    ├── HealthComponent (MaxHealth = 50)
 *    ├── BreakableComponent
 *    ├── CollisionShape3D
 *    └── MeshInstance3D
 * 
 * 2. 配置 HealthComponent：
 *    - MaxHealth = 50
 *    - CurrentHealth = 50
 * 
 * 3. 配置 BreakableComponent：
 *    - BreakParticles = 破碎粒子场景
 *    - BreakSound = 破碎音效
 *    - DebrisScenes = 碎片场景数组
 * 
 * 4. 在其他脚本中造成伤害：
 *    ```csharp
 *    if (box.GetNode<HealthComponent>("HealthComponent") is HealthComponent health)
 *    {
 *        health.TakeDamage(25);
 *    }
 *    ```
 * 
 * 5. 完成！箱子会在生命值为 0 时自动破碎。
 */
