using Godot;
using System;

/// <summary>
/// MarginContainer辅助脚本，简化margin参数调整
/// 提供3种模式：统一、双轴、独立调整
/// </summary>
[Tool]
[GlobalClass]
public partial class MarginContainerHelper : MarginContainer
{
    public enum MarginMode
    {
        Uniform,      // 统一：1个参数控制所有
        TwoAxis,      // 双轴：上下 + 左右
        Individual    // 独立：4个参数分别控制
    }

    // ===== 主要参数（显示在最上面） =====
    private MarginMode _mode = MarginMode.Uniform;
    
    [ExportCategory("Margin Settings")]
    [Export] 
    public MarginMode Mode 
    { 
        get => _mode;
        set
        {
            _mode = value;
            NotifyPropertyListChanged(); // 触发属性列表更新
            UpdateMargins();
        }
    }

    // ----- Uniform Mode -----
    private int _uniformMargin = 10;
    
    [Export] 
    public int UniformMargin 
    { 
        get => _uniformMargin;
        set
        {
            _uniformMargin = value;
            if (Mode == MarginMode.Uniform)
                UpdateMargins();
        }
    }

    // ----- TwoAxis Mode -----
    private int _verticalMargin = 10;
    
    [Export] 
    public int VerticalMargin 
    { 
        get => _verticalMargin;
        set
        {
            _verticalMargin = value;
            if (Mode == MarginMode.TwoAxis)
                UpdateMargins();
        }
    }

    private int _horizontalMargin = 10;
    
    [Export] 
    public int HorizontalMargin 
    { 
        get => _horizontalMargin;
        set
        {
            _horizontalMargin = value;
            if (Mode == MarginMode.TwoAxis)
                UpdateMargins();
        }
    }

    // ----- Individual Mode -----
    private int _marginTop = 10;
    
    [Export] 
    public int MarginTopValue 
    { 
        get => _marginTop;
        set
        {
            _marginTop = value;
            if (Mode == MarginMode.Individual)
                UpdateMargins();
        }
    }

    private int _marginRight = 10;
    
    [Export] 
    public int MarginRightValue 
    { 
        get => _marginRight;
        set
        {
            _marginRight = value;
            if (Mode == MarginMode.Individual)
                UpdateMargins();
        }
    }

    private int _marginBottom = 10;
    
    [Export] 
    public int MarginBottomValue 
    { 
        get => _marginBottom;
        set
        {
            _marginBottom = value;
            if (Mode == MarginMode.Individual)
                UpdateMargins();
        }
    }

    private int _marginLeft = 10;
    
    [Export] 
    public int MarginLeftValue 
    { 
        get => _marginLeft;
        set
        {
            _marginLeft = value;
            if (Mode == MarginMode.Individual)
                UpdateMargins();
        }
    }

    public override void _Ready()
    {
        UpdateMargins(); // 初始化时应用默认值
    }

    /// <summary>
    /// 验证属性，根据模式隐藏不需要的参数
    /// </summary>
    public override void _ValidateProperty(Godot.Collections.Dictionary property)
    {
        string propertyName = property["name"].AsStringName();

        switch (Mode)
        {
            case MarginMode.Uniform:
                // 只显示UniformMargin
                if (propertyName == "VerticalMargin" || propertyName == "HorizontalMargin" ||
                    propertyName == "MarginTopValue" || propertyName == "MarginRightValue" ||
                    propertyName == "MarginBottomValue" || propertyName == "MarginLeftValue")
                {
                    property["usage"] = (int)PropertyUsageFlags.NoEditor;
                }
                break;

            case MarginMode.TwoAxis:
                // 只显示VerticalMargin和HorizontalMargin
                if (propertyName == "UniformMargin" ||
                    propertyName == "MarginTopValue" || propertyName == "MarginRightValue" ||
                    propertyName == "MarginBottomValue" || propertyName == "MarginLeftValue")
                {
                    property["usage"] = (int)PropertyUsageFlags.NoEditor;
                }
                break;

            case MarginMode.Individual:
                // 只显示4个独立参数
                if (propertyName == "UniformMargin" ||
                    propertyName == "VerticalMargin" || propertyName == "HorizontalMargin")
                {
                    property["usage"] = (int)PropertyUsageFlags.NoEditor;
                }
                break;
        }
    }

    /// <summary>
    /// 根据当前模式更新MarginContainer的margin值
    /// </summary>
    private void UpdateMargins()
    {
        // [Tool]特性允许在编辑器中运行
        switch (Mode)
        {
            case MarginMode.Uniform:
                AddThemeConstantOverride("margin_left", UniformMargin);
                AddThemeConstantOverride("margin_top", UniformMargin);
                AddThemeConstantOverride("margin_right", UniformMargin);
                AddThemeConstantOverride("margin_bottom", UniformMargin);
                break;

            case MarginMode.TwoAxis:
                AddThemeConstantOverride("margin_left", HorizontalMargin);
                AddThemeConstantOverride("margin_top", VerticalMargin);
                AddThemeConstantOverride("margin_right", HorizontalMargin);
                AddThemeConstantOverride("margin_bottom", VerticalMargin);
                break;

            case MarginMode.Individual:
                AddThemeConstantOverride("margin_left", MarginLeftValue);
                AddThemeConstantOverride("margin_top", MarginTopValue);
                AddThemeConstantOverride("margin_right", MarginRightValue);
                AddThemeConstantOverride("margin_bottom", MarginBottomValue);
                break;
        }
    }
}
