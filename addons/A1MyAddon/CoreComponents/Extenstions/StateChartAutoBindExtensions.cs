using Godot;
using GodotStateCharts;

/// StateChart 自动绑定扩展 - Power Switch 模式组件挂载
/// 核心思想：组件作为 AtomicState 子节点，自动绑定生命周期
public static class StateChartAutoBindExtensions
{
    #region 自动绑定方法

    /// 组件自动挂载到父状态节点
    /// 目的：实现 Power Switch 模式，状态进入时唤醒组件，退出时休眠
    /// 示例：GroundMode 进入 -> GroundMovementComponent 启用；GroundMode 退出 -> GroundMovementComponent 禁用
    /// 算法：1. 验证父节点是 StateChart 状态 -> 2. 默认休眠组件 -> 3. 绑定状态进入/退出信号 -> 4. 信号触发时切换组件启用状态
    public static void AutoBindToParentState(this Node component)
    {
        var stateNode = component.GetParent();
        if (stateNode == null)
        {
            GD.PushError($"[架构错误] {component.Name} 尝试自动绑定，但没有父节点！");
            return;
        }

        var state = StateChartState.Of(stateNode);
        if (state == null)
        {
            GD.PushError($"[架构错误] {component.Name} 尝试自动绑定，但其父节点 {stateNode.Name} 不是一个合法的 StateChart 状态节点！");
            return;
        }

        component.SetProcess(false);
        component.SetPhysicsProcess(false);
        component.SetProcessInput(false);
        component.SetProcessUnhandledInput(false);

        GD.Print($"[PowerSwitch] 💤 {component.Name} 已挂载到状态 '{stateNode.Name}' 下，进入默认休眠。");

        state.Connect(StateChartState.SignalName.StateEntered, Callable.From(() =>
        {
            component.SetProcess(true);
            component.SetPhysicsProcess(true);
            component.SetProcessInput(true);
            component.SetProcessUnhandledInput(true);
            GD.Print($"[PowerSwitch] ⚡ {component.Name} 已通电唤醒！");
        }));

        state.Connect(StateChartState.SignalName.StateExited, Callable.From(() =>
        {
            component.SetProcess(false);
            component.SetPhysicsProcess(false);
            component.SetProcessInput(false);
            component.SetProcessUnhandledInput(false);
            GD.Print($"[PowerSwitch] 💤 {component.Name} 已断电休眠。");
        }));
    }

    #endregion

    #region 实体查找方法

    /// 向上查找实体控制器
    /// 目的：从深层组件节点获取顶层实体引用（如 Player3D）
    /// 示例：GroundMovementComponent -> GroundMode -> ... -> Player3D
    /// 算法：1. 优先使用 Owner 属性 -> 2. 兜底方案：向上遍历父节点直到找到匹配类型
    public static T GetEntity<T>(this Node component) where T : Node
    {
        if (component.Owner is T entity)
        {
            return entity;
        }

        Node current = component.GetParent();
        while (current != null)
        {
            if (current is T match)
            {
                return match;
            }
            current = current.GetParent();
        }

        GD.PushError($"[架构错误] {component.Name} 无法找到类型为 {typeof(T).Name} 的实体节点！");
        return null;
    }

    #endregion

    #region 状态事件方法

    public static void SendStateEvent(this Node node, string eventName)
    {
        var stateChartNode = node.GetNodeOrNull("%StateChart");
        if (stateChartNode == null)
        {
            GD.PushWarning($"[StateChart] StateChart not found in {node.Name}");
            return;
        }

        var stateChart = StateChart.Of(stateChartNode);
        stateChart.SendEvent(eventName);
    }

    #endregion
}
