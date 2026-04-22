using Godot;

namespace Game;

public partial class EnemyTestScene : Node3D
{
    private Enemy _enemy;
    private Label _statusLabel;
    
    public override void _Ready()
    {
        _enemy = GetNode<Enemy>("Enemy");
        
        // 创建状态显示标签
        _statusLabel = new Label
        {
            Position = new Vector2(10, 10),
            Text = "按空格键击中敌人\n敌人将从飞行模式切换到地面模式3秒"
        };
        
        // 添加到CanvasLayer以确保显示在最上层
        var canvasLayer = new CanvasLayer();
        AddChild(canvasLayer);
        canvasLayer.AddChild(_statusLabel);
        
        GD.Print("[EnemyTest] Ready. Press SPACE to interrupt enemy flight.");
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
        {
            if (keyEvent.Keycode == Key.Space)
            {
                GD.Print("[EnemyTest] SPACE pressed - Interrupting enemy!");
                _enemy.OnHit();
                
                // 更新状态显示
                _statusLabel.Text = "敌人被击中！\n将在地面模式停留3秒...\n\n按空格键再次击中";
            }
        }
    }
}
