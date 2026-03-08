using Godot;

/// <summary>
/// 角色动画配置 - 类似UE的Data Asset
/// 集中管理角色的所有动画
/// </summary>
[GlobalClass]
public partial class CharacterAnimationConfig : Resource
{
    [Export] public string CharacterName = "角色名称"; // 支持日文
    
    [Export] public AnimationSet AnimationSet; // 动画集
    
    /// <summary>
    /// 将配置应用到AnimationPlayer
    /// </summary>
    public void ApplyToAnimationPlayer(AnimationPlayer player)
    {
        if (player == null || AnimationSet == null) return;
        
        // 自动设置所有动画的循环模式
        AnimationSet.SetupLoopModes();
        
        // 清空现有动画
        var libraries = player.GetAnimationLibraryList();
        foreach (var libName in libraries)
        {
            player.RemoveAnimationLibrary(libName);
        }
        
        // 创建新的AnimationLibrary
        var library = new AnimationLibrary();
        
        // 添加所有动画
        AddAnimation(library, "Idle", AnimationSet.IdleAnimation);
        AddAnimation(library, "Walk", AnimationSet.WalkAnimation);
        AddAnimation(library, "Run", AnimationSet.RunAnimation);
        AddAnimation(library, "Sprint", AnimationSet.SprintAnimation);
        AddAnimation(library, "JumpStart", AnimationSet.JumpStartAnimation);
        AddAnimation(library, "JumpLoop", AnimationSet.JumpLoopAnimation);
        AddAnimation(library, "Fall", AnimationSet.FallAnimation);
        AddAnimation(library, "Land", AnimationSet.LandAnimation);
        AddAnimation(library, "Attack1", AnimationSet.Attack1Animation);
        AddAnimation(library, "Attack2", AnimationSet.Attack2Animation);
        AddAnimation(library, "Attack3", AnimationSet.Attack3Animation);
        AddAnimation(library, "Dodge", AnimationSet.DodgeAnimation);
        AddAnimation(library, "Hit", AnimationSet.HitAnimation);
        AddAnimation(library, "Death", AnimationSet.DeathAnimation);
        
        // 添加库到AnimationPlayer
        player.AddAnimationLibrary("", library);
    }
    
    private void AddAnimation(AnimationLibrary library, string name, Animation anim)
    {
        if (anim != null)
        {
            library.AddAnimation(name, anim);
        }
    }
}
