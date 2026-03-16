using Godot;
using GodotStateCharts;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable All

/// <summary>
/// 组件查找扩展方法 - 简化组件依赖查找
/// 提供类似 Unity 的 GetComponent API
/// </summary>
public static class ComponentExtensions
{
    #region 基础查找方法

    /// <summary>
    /// 在子节点中查找指定类型的组件（支持多态）
    /// </summary>
    /// <typeparam name="T">组件类型</typeparam>
    /// <param name="node">父节点</param>
    /// <returns>找到的组件，未找到返回 null</returns>
    private static T GetComponentInChildren<T>(this Node node) where T : Node
    {
        return node.GetChildren().OfType<T>().FirstOrDefault();
    }

    /// <summary>
    /// 在子节点中查找所有指定类型的组件
    /// </summary>
    /// <typeparam name="T">组件类型</typeparam>
    /// <param name="node">父节点</param>
    /// <returns>找到的所有组件列表</returns>
    public static List<T> GetComponentsInChildren<T>(this Node node) where T : Node
    {
        return node.GetChildren().OfType<T>().ToList();
    }

    /// <summary>
    /// 在子节点中查找指定类型的组件，未找到则报错
    /// </summary>
    /// <typeparam name="T">组件类型</typeparam>
    /// <param name="node">父节点</param>
    /// <returns>找到的组件</returns>
    public static T GetRequiredComponentInChildren<T>(this Node node) where T : Node
    {
        var component = node.GetComponentInChildren<T>();
        if (component == null)
        {
            GD.PushError($"Required component {typeof(T).Name} not found in {node.Name}");
        }

        return component;
    }

    #endregion

    #region 输入组件专用方法

    /// <summary>
    /// 查找并订阅输入组件（专用辅助方法）
    /// 自动查找 BaseInputComponent 并订阅事件
    /// </summary>
    /// <param name="parent">父节点</param>
    /// <param name="onMovement">移动输入回调</param>
    /// <param name="onJump">跳跃输入回调（可选）</param>
    /// <returns>找到的输入组件</returns>
    public static BaseInputComponent FindAndSubscribeInput(
        this Node parent,
        Action<Vector2> onMovement,
        Action onJump = null)
    {
        var input = parent.GetRequiredComponentInChildren<BaseInputComponent>();

        if (input == null)
        {
            return null;
        }

        // 订阅事件
        if (onMovement != null)
        {
            input.OnMovementInput += onMovement;
        }

        if (onJump != null)
        {
            input.OnJumpJustPressed += onJump;
        }

        GD.Print($"✓ 已订阅 {input.GetType().Name} 事件");
        return input;
    }

    /// <summary>
    /// 取消订阅输入组件
    /// </summary>
    /// <param name="input">输入组件</param>
    /// <param name="onMovement">移动输入回调</param>
    /// <param name="onJump">跳跃输入回调（可选）</param>
    public static void UnsubscribeInput(
        this BaseInputComponent input,
        Action<Vector2> onMovement,
        Action onJump = null)
    {
        if (input == null) return;

        input.OnMovementInput -= onMovement;
        if (onJump != null)
        {
            input.OnJumpJustPressed -= onJump;
        }
    }

    #endregion

    #region StateChart 扩展方法

    /// <summary>
    /// 获取节点下的 StateChart（自动查找并包装）
    /// 使用示例：var stateChart = this.GetStateChart();
    /// </summary>
    /// <param name="node">父节点</param>
    /// <returns>StateChart 包装类，未找到返回 null</returns>
    public static StateChart GetStateChart(this Node node)
    {
        var stateChartNode = node.GetNodeOrNull("StateChart");
        if (stateChartNode == null)
        {
            // 尝试在子节点中查找
            stateChartNode = node.GetComponentInChildren<Node>();
            if (stateChartNode == null || stateChartNode.GetScript().As<Script>()?.ResourcePath?.Contains("state_chart.gd") != true)
            {
                return null;
            }
        }

        return StateChart.Of(stateChartNode);
    }

    /// <summary>
    /// 快速发送状态机事件
    /// 使用示例：this.SendStateEvent("jump_pressed");
    /// </summary>
    /// <param name="node">包含 StateChart 的节点</param>
    /// <param name="eventName">事件名称</param>
    public static void SendStateEvent(this Node node, string eventName)
    {
        var stateChart = node.GetStateChart();
        if (stateChart == null)
        {
            GD.PushWarning($"SendStateEvent: StateChart not found in {node.Name}");
            return;
        }

        stateChart.SendEvent(eventName);
    }

    /// <summary>
    /// 连接到指定状态的进入/退出信号
    /// 使用示例：this.ConnectToState("Movement", isActive => _canMove = isActive);
    /// </summary>
    /// <param name="node">包含 StateChart 的节点</param>
    /// <param name="stateName">状态名称（使用 % 前缀表示唯一名称）</param>
    /// <param name="callback">回调函数，参数为 true 表示进入，false 表示退出</param>
    public static void ConnectToState(this Node node, string stateName, Action<bool> callback)
    {
        // 支持唯一名称语法
        var stateNode = stateName.StartsWith("%") 
            ? node.GetNodeOrNull(stateName) 
            : node.GetNodeOrNull($"StateChart/{stateName}");

        if (stateNode == null)
        {
            GD.PushWarning($"ConnectToState: State '{stateName}' not found in {node.Name}");
            return;
        }

        var state = StateChartState.Of(stateNode);

        // 连接进入和退出信号
        state.Connect(StateChartState.SignalName.StateEntered, Callable.From(() => callback(true)));
        state.Connect(StateChartState.SignalName.StateExited, Callable.From(() => callback(false)));

        GD.Print($"✓ 已连接到状态 '{stateName}' 的进入/退出信号");
    }

    /// <summary>
    /// 获取指定状态节点的包装类
    /// 使用示例：var state = this.GetState("Movement");
    /// </summary>
    /// <param name="node">包含 StateChart 的节点</param>
    /// <param name="stateName">状态名称</param>
    /// <returns>StateChartState 包装类，未找到返回 null</returns>
    public static StateChartState GetState(this Node node, string stateName)
    {
        var stateNode = stateName.StartsWith("%") 
            ? node.GetNodeOrNull(stateName) 
            : node.GetNodeOrNull($"StateChart/{stateName}");

        if (stateNode == null)
        {
            GD.PushWarning($"GetState: State '{stateName}' not found in {node.Name}");
            return null;
        }

        return StateChartState.Of(stateNode);
    }

    /// <summary>
    /// 设置状态机表达式属性
    /// 使用示例：this.SetStateProperty("player_health", 100);
    /// </summary>
    /// <param name="node">包含 StateChart 的节点</param>
    /// <param name="propertyName">属性名称</param>
    /// <param name="value">属性值</param>
    public static void SetStateProperty(this Node node, string propertyName, Variant value)
    {
        var stateChart = node.GetStateChart();
        if (stateChart == null)
        {
            GD.PushWarning($"SetStateProperty: StateChart not found in {node.Name}");
            return;
        }

        stateChart.SetExpressionProperty(propertyName, value);
    }

    #endregion

    #region 调试辅助方法

    /// <summary>
    /// 打印节点的所有子组件（调试用）
    /// </summary>
    public static void PrintComponents(this Node node)
    {
        GD.Print($"=== Components in {node.Name} ===");
        foreach (var child in node.GetChildren())
        {
            GD.Print($"  - {child.Name} ({child.GetType().Name})");
        }
    }

    #endregion
}