using Godot;

/// <summary>
/// 自动停止测试脚本
/// 在指定时间后自动停止场景运行
/// </summary>
public partial class AutoStopTest : Node
{
	[Export] public float StopAfterSeconds { get; set; } = 5.0f;
	
	private double _elapsedTime = 0.0;
	
	public override void _Ready()
	{
		GD.Print("═══════════════════════════════════════════════════════");
		GD.Print($"AutoStopTest: 场景将在 {StopAfterSeconds} 秒后自动停止");
		GD.Print("═══════════════════════════════════════════════════════");
	}
	
	public override void _Process(double delta)
	{
		_elapsedTime += delta;
		
		// 每秒打印倒计时
		if ((int)_elapsedTime != (int)(_elapsedTime - delta))
		{
			float remaining = StopAfterSeconds - (float)_elapsedTime;
			if (remaining > 0)
			{
				GD.Print($"⏱️  倒计时: {remaining:F1} 秒后自动停止...");
			}
		}
		
		if (_elapsedTime >= StopAfterSeconds)
		{
			GD.Print("═══════════════════════════════════════════════════════");
			GD.Print("AutoStopTest: 时间到，停止场景");
			GD.Print("═══════════════════════════════════════════════════════");
			GetTree().Quit();
		}
	}
}
