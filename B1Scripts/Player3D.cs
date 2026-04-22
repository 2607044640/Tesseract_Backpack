using Godot;
using Godot.Composition;

[Entity]
public partial class Player3D : CharacterBody3D
{
    public override void _Ready()
    {
        // Godot.Composition 会自动收集所有子组件并解析依赖
        InitializeEntity();
        
        GD.Print("Player3D Entity: 初始化完成 ✓");
    }
}
