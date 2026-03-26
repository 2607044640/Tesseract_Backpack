using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable All

/// <summary>
/// 组件查找扩展方法 - 简化组件依赖查找
/// 提供类似 Unity 的 GetComponent API
/// 
/// 注意：StateChart 相关方法已移至 StateChartAutoBindExtensions.cs
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
