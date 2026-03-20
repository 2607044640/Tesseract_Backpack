"""
简单示例：创建一个主菜单UI
展示UIBuilder的基础用法
"""

import sys
import os
sys.path.insert(0, os.path.dirname(__file__))

from godot_ui_builder import UIBuilder


def create_main_menu():
    """创建主菜单"""
    
    # 创建构建器
    ui = UIBuilder("MainMenu")
    
    # 创建根节点（全屏）
    root = ui.create_control("Control", fullscreen=True)
    
    # 添加深色背景
    root.add_color_rect("Background_ColorRect", color=(0.1, 0.1, 0.15, 1))
    
    # 添加边距（上下左右各50像素）
    margin = root.add_margin_container("Margin_Container", uniform=50)
    
    # 添加垂直布局（居中）
    vbox = margin.add_vbox("Menu_VBoxContainer", separation=20)
    
    # 添加游戏标题
    vbox.add_label(
        "Title_Label",
        text="My Awesome Game",
        align="center",
        font_size=64
    )
    
    # 添加分隔线
    vbox.add_separator("Separator_HSeparator", separation=40)
    
    # 添加菜单按钮
    vbox.add_button("NewGame_Button", text="New Game", size_flags_h=4)
    vbox.add_button("Continue_Button", text="Continue", size_flags_h=4)
    vbox.add_button("Settings_Button", text="Settings", size_flags_h=4)
    vbox.add_button("Quit_Button", text="Quit", size_flags_h=4)
    
    # 显示树状图
    print("=" * 60)
    print("Main Menu UI Structure:")
    print("=" * 60)
    print(ui.generate_tree_view())
    print("=" * 60)
    
    # 保存
    ui.save("A1UIScenes/MainMenu.tscn")
    
    return ui


def create_hud():
    """创建游戏HUD"""
    
    ui = UIBuilder("GameHUD")
    
    # 根节点
    root = ui.create_control("Control", fullscreen=True)
    
    # 顶部信息栏
    top_margin = root.add_margin_container("Top_MarginContainer", left=20, top=20, right=20)
    top_hbox = top_margin.add_hbox("TopBar_HBoxContainer", separation=20)
    
    # 生命值
    hp_label = top_hbox.add_label("HP_Label", text="HP:", font_size=24)
    hp_bar = top_hbox.add_progress_bar("HP_ProgressBar", value=100, size_flags_h=3)
    
    # 魔法值
    mp_label = top_hbox.add_label("MP_Label", text="MP:", font_size=24)
    mp_bar = top_hbox.add_progress_bar("MP_ProgressBar", value=80, size_flags_h=3)
    
    # 显示树状图
    print("=" * 60)
    print("Game HUD Structure:")
    print("=" * 60)
    print(ui.generate_tree_view())
    print("=" * 60)
    
    # 保存
    ui.save("A1UIScenes/GameHUD.tscn")
    
    return ui


if __name__ == "__main__":
    print("\n🎮 Creating Main Menu...")
    create_main_menu()
    
    print("\n🎮 Creating Game HUD...")
    create_hud()
    
    print("\n✅ All UI scenes created successfully!")
