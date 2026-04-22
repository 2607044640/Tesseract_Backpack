using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable All

/// 组件查找扩展方法 - 简化组件依赖查找
/// 提供类似 Unity 的 GetComponent API
public static class ComponentExtensions
{
    #region 基础查找方法

    private static T GetComponentInChildren<T>(this Node node) where T : Node
    {
        return node.GetChildren().OfType<T>().FirstOrDefault();
    }

    public static List<T> GetComponentsInChildren<T>(this Node node) where T : Node
    {
        return node.GetChildren().OfType<T>().ToList();
    }

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
