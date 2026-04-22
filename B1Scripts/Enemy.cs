using Godot;
using Godot.Composition;

namespace Game;

[Entity]
public partial class Enemy : CharacterBody3D
{
    public override void _Ready()
    {
        InitializeEntity();
    }
    
    public void OnHit()
    {
        GD.Print("[Enemy] Hit! Interrupting flight...");
        this.SendStateEvent("on_interrupted");
    }
}
