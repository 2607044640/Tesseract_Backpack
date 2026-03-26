using Godot;
using Godot.Composition;

namespace Game;

/// <summary>
/// 敌人实体 - 飞行模式可被打断，强制进入地面模式3秒
/// 
/// 复用组件：
/// - FlyMovementComponent (from A1MyAddon)
/// - GroundMovementComponent (from A1MyAddon)
/// 
/// StateChart控制组件生命周期（Power Switch模式）
/// </summary>
[Entity]
public partial class Enemy : CharacterBody3D
{
    public override void _Ready()
    {
        InitializeEntity();
    }
    
    /// <summary>
    /// 当敌人被击中时调用（供外部调用）
    /// </summary>
    public void OnHit()
    {
        GD.Print("[Enemy] Hit! Interrupting flight...");
        this.SendStateEvent("on_interrupted");
    }
}
