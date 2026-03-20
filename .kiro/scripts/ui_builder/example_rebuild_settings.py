"""
示例：使用UIBuilder重建SettingsUI.tscn
展示如何使用简洁的API创建复杂的UI结构
"""

import sys
import os

# 添加当前目录到路径
sys.path.insert(0, os.path.dirname(__file__))

from godot_ui_builder import UIBuilder


def rebuild_settings_ui():
    """重建SettingsUI - 完全复刻原始版本"""
    
    # 创建构建器
    ui = UIBuilder("SettingsUI", scene_uid="uid://cpurmg3xq1hd4")
    
    # 创建根节点（全屏）
    root = ui.create_control("Control", fullscreen=True)
    
    # 添加背景
    root.add_color_rect(
        "Background_ColorRect",
        color=(0.15686275, 0.15686275, 0.15686275, 1)
    ).set_property("offset_left", 1.0) \
     .set_property("offset_top", 1.0) \
     .set_property("offset_right", 1151.0) \
     .set_property("offset_bottom", 647.0)
    
    # 添加外边距容器（使用锚点模式）
    margin = root.add_margin_container(
        "MarginContainerHelper",
        uniform=30,
        script="res://addons/MyAddon/Helpers/MarginContainerHelper.cs",
        use_anchors=True  # 使用锚点模式
    )
    # 设置全屏锚点
    margin.set_property("anchors_preset", 15)
    margin.set_property("anchor_right", 1.0)
    margin.set_property("anchor_bottom", 1.0)
    margin.set_property("offset_top", -1.0)
    margin.set_property("offset_bottom", -1.0)
    margin.set_property("grow_horizontal", 2)
    margin.set_property("grow_vertical", 2)
    margin.set_property("VerticalMargin", 70)
    margin.set_property("HorizontalMargin", 128)
    margin.set_property('metadata/_custom_type_script', '"uid://bk83ics8idr7w"')
    
    # 添加面板容器
    panel = margin.add_panel_container("PanelContainer")
    
    # 添加内边距容器
    inner_margin = panel.add_margin_container(
        "MarginContainerHelper",
        uniform=33,
        script="res://addons/MyAddon/Helpers/MarginContainerHelper.cs"
    )
    inner_margin.set_property('metadata/_custom_type_script', '"uid://bk83ics8idr7w"')
    
    # 添加垂直布局容器
    vbox = inner_margin.add_vbox("VBoxContainer")
    
    # 添加标题
    vbox.add_label("Label", text="Setting", align="center")
    
    # 添加分隔线
    vbox.add_separator(
        "HSeparator",
        separation=56,
        style="res://A1UIPresets/new_style_box_line.tres"
    )
    
    # 添加第一行（Label + ProgressBar）
    hbox1 = vbox.add_hbox("HBoxContainer")
    hbox1.add_label(
        "Label2",
        text="text13243",
        font_size=41,
        min_size=(410, 0),
        size_flags_h=6  # Shrink Begin + Shrink Center
    ).set_property("horizontal_alignment", 1)  # 居中对齐
    
    hbox1.add_progress_bar(
        "ProgressBar",
        value=10.17,
        size_flags_h=3,  # Fill + Expand
        size_flags_v=4,  # Shrink Center
        size_flags_stretch_ratio=2.94,
        show_percentage=True
    )
    
    # 添加第二行（Label + ProgressBar）
    hbox2 = vbox.add_hbox("HBoxContainer2")
    hbox2.add_label(
        "Label2",
        text="text132",
        font_size=41,
        min_size=(410, 0),
        size_flags_h=6
    ).set_property("horizontal_alignment", 1)  # 居中对齐
    
    hbox2.add_progress_bar(
        "ProgressBar",
        value=10.17,
        size_flags_h=3,
        size_flags_v=4,
        size_flags_stretch_ratio=2.94,
        min_size=(0, 8),
        show_percentage=True
    )
    
    # 添加按钮
    vbox.add_button("Button", text="SeeMore\n", size_flags_h=4)
    
    # 生成树状图（给AI看）
    print("=" * 60)
    print("UI Structure Tree View:")
    print("=" * 60)
    print(ui.generate_tree_view())
    print("=" * 60)
    
    # 保存文件
    output_path = "A1UIScenes/SettingsUI_rebuilt.tscn"
    ui.save(output_path)
    
    return ui


if __name__ == "__main__":
    rebuild_settings_ui()
