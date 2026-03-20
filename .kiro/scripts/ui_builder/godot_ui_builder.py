"""
Godot UI Builder - 程序化生成Godot UI的Python工具
让AI能够方便地操作Godot UI，而不是直接面对冗长的.tscn文本
"""

import random
from typing import Optional, Tuple, List, Dict, Any


class UINode:
    """表示一个UI节点"""
    
    def __init__(self, name: str, node_type: str, unique_id: Optional[int] = None):
        self.name = name
        self.node_type = node_type
        self.unique_id = unique_id or random.randint(1, 2**31 - 1)
        self.parent_path = "."
        self.properties: Dict[str, Any] = {}
        self.children: List['UINode'] = []
        self.parent: Optional['UINode'] = None
    
    def _add_child(self, child: 'UINode') -> 'UINode':
        """内部方法：添加子节点"""
        child.parent = self
        self.children.append(child)
        # 计算parent_path
        if self.parent is None:
            # 根节点的子节点
            child.parent_path = "."
        else:
            # 非根节点的子节点
            if self.parent_path == ".":
                child.parent_path = self.name
            else:
                child.parent_path = f"{self.parent_path}/{self.name}"
        return child
    
    def set_property(self, key: str, value: Any) -> 'UINode':
        """设置属性（链式调用）"""
        self.properties[key] = value
        return self
    
    # === 通用容器方法 ===
    
    def add_margin_container(self, name: str, uniform: Optional[int] = None,
                            left: Optional[int] = None, top: Optional[int] = None,
                            right: Optional[int] = None, bottom: Optional[int] = None,
                            script: Optional[str] = None,
                            use_anchors: bool = False) -> 'UINode':
        """添加MarginContainer
        
        Args:
            use_anchors: 如果为True，使用锚点模式（layout_mode=1）而不是容器模式（layout_mode=2）
        """
        node = UINode(name, "MarginContainer")
        
        # 根据是否使用锚点选择layout_mode
        if use_anchors:
            node.properties["layout_mode"] = 1
        else:
            node.properties["layout_mode"] = 2
        
        if uniform is not None:
            node.properties["theme_override_constants/margin_left"] = uniform
            node.properties["theme_override_constants/margin_top"] = uniform
            node.properties["theme_override_constants/margin_right"] = uniform
            node.properties["theme_override_constants/margin_bottom"] = uniform
        else:
            if left is not None:
                node.properties["theme_override_constants/margin_left"] = left
            if top is not None:
                node.properties["theme_override_constants/margin_top"] = top
            if right is not None:
                node.properties["theme_override_constants/margin_right"] = right
            if bottom is not None:
                node.properties["theme_override_constants/margin_bottom"] = bottom
        
        if script:
            # 需要在UIBuilder中注册ext_resource
            node.properties["script"] = f'ExtResource("script_{name}")'
            node.properties["_script_path"] = script
            if uniform is not None:
                node.properties["UniformMargin"] = uniform
        
        return self._add_child(node)
    
    def add_panel_container(self, name: str) -> 'UINode':
        """添加PanelContainer"""
        node = UINode(name, "PanelContainer")
        node.properties["layout_mode"] = 2
        return self._add_child(node)
    
    def add_vbox(self, name: str, separation: Optional[int] = None) -> 'UINode':
        """添加VBoxContainer"""
        node = UINode(name, "VBoxContainer")
        node.properties["layout_mode"] = 2
        if separation is not None:
            node.properties["theme_override_constants/separation"] = separation
        return self._add_child(node)
    
    def add_hbox(self, name: str, separation: Optional[int] = None) -> 'UINode':
        """添加HBoxContainer"""
        node = UINode(name, "HBoxContainer")
        node.properties["layout_mode"] = 2
        if separation is not None:
            node.properties["theme_override_constants/separation"] = separation
        return self._add_child(node)
    
    # === UI控件方法 ===
    
    def add_color_rect(self, name: str, color: Tuple[float, float, float, float] = (0.15, 0.15, 0.15, 1)) -> 'UINode':
        """添加ColorRect"""
        node = UINode(name, "ColorRect")
        node.properties["layout_mode"] = 0
        node.properties["color"] = f"Color({color[0]}, {color[1]}, {color[2]}, {color[3]})"
        return self._add_child(node)
    
    def add_label(self, name: str, text: str = "", align: str = "left",
                 font_size: Optional[int] = None, min_size: Optional[Tuple[float, float]] = None,
                 size_flags_h: Optional[int] = None) -> 'UINode':
        """添加Label"""
        node = UINode(name, "Label")
        node.properties["layout_mode"] = 2
        
        if text:
            node.properties["text"] = f'"{text}"'
        
        # 对齐方式映射
        align_map = {"left": 0, "center": 1, "right": 2}
        if align in align_map:
            node.properties["horizontal_alignment"] = align_map[align]
        
        if font_size:
            node.properties["theme_override_font_sizes/font_size"] = font_size
        
        if min_size:
            node.properties["custom_minimum_size"] = f"Vector2({min_size[0]}, {min_size[1]})"
        
        if size_flags_h is not None:
            node.properties["size_flags_horizontal"] = size_flags_h
        
        return self._add_child(node)
    
    def add_progress_bar(self, name: str, value: float = 0, 
                        size_flags_h: Optional[int] = None,
                        size_flags_v: Optional[int] = None,
                        size_flags_stretch_ratio: Optional[float] = None,
                        min_size: Optional[Tuple[float, float]] = None,
                        show_percentage: bool = True) -> 'UINode':
        """添加ProgressBar"""
        node = UINode(name, "ProgressBar")
        node.properties["layout_mode"] = 2
        
        if size_flags_h is not None:
            node.properties["size_flags_horizontal"] = size_flags_h
        
        if size_flags_v is not None:
            node.properties["size_flags_vertical"] = size_flags_v
        
        if size_flags_stretch_ratio is not None:
            node.properties["size_flags_stretch_ratio"] = size_flags_stretch_ratio
        
        node.properties["value"] = value
        
        if min_size:
            node.properties["custom_minimum_size"] = f"Vector2({min_size[0]}, {min_size[1]})"
        
        if show_percentage:
            node.properties["show_percentage"] = "true"
        
        return self._add_child(node)
    
    def add_button(self, name: str, text: str = "", size_flags_h: Optional[int] = None) -> 'UINode':
        """添加Button"""
        node = UINode(name, "Button")
        node.properties["layout_mode"] = 2
        
        if text:
            node.properties["text"] = f'"{text}"'
        
        if size_flags_h is not None:
            node.properties["size_flags_horizontal"] = size_flags_h
        
        return self._add_child(node)
    
    def add_separator(self, name: str, separation: Optional[int] = None,
                     style: Optional[str] = None) -> 'UINode':
        """添加HSeparator"""
        node = UINode(name, "HSeparator")
        node.properties["layout_mode"] = 2
        
        if separation is not None:
            node.properties["theme_override_constants/separation"] = separation
        
        if style:
            # 需要在UIBuilder中注册ext_resource
            node.properties["theme_override_styles/separator"] = f'ExtResource("style_{name}")'
            node.properties["_style_path"] = style
        
        return self._add_child(node)


class UIBuilder:
    """UI构建器"""
    
    def __init__(self, scene_name: str, scene_uid: Optional[str] = None):
        self.scene_name = scene_name
        self.scene_uid = scene_uid or self._generate_uid()
        self.root: Optional[UINode] = None
        self.ext_resources: List[Dict[str, str]] = []
        self._resource_counter = 1
    
    def _generate_uid(self) -> str:
        """生成随机UID"""
        chars = "abcdefghijklmnopqrstuvwxyz0123456789"
        return "uid://" + "".join(random.choice(chars) for _ in range(14))
    
    def create_control(self, name: str = "Control", fullscreen: bool = True) -> UINode:
        """创建根Control节点"""
        self.root = UINode(name, "Control")
        
        if fullscreen:
            self.root.properties["layout_mode"] = 3
            self.root.properties["anchors_preset"] = 15
            self.root.properties["anchor_right"] = 1.0
            self.root.properties["anchor_bottom"] = 1.0
            self.root.properties["grow_horizontal"] = 2
            self.root.properties["grow_vertical"] = 2
        
        return self.root
    
    def _collect_ext_resources(self, node: UINode):
        """收集所有外部资源引用"""
        # 检查script
        if "_script_path" in node.properties:
            script_path = node.properties["_script_path"]
            if not any(r["path"] == script_path for r in self.ext_resources):
                self.ext_resources.append({
                    "type": "Script",
                    "path": script_path,
                    "id": f"{self._resource_counter}_dsrpe",
                    "uid": "uid://bk83ics8idr7w"  # MarginContainerHelper的固定uid
                })
                self._resource_counter += 1
        
        # 检查style
        if "_style_path" in node.properties:
            style_path = node.properties["_style_path"]
            if not any(r["path"] == style_path for r in self.ext_resources):
                self.ext_resources.append({
                    "type": "StyleBox",
                    "path": style_path,
                    "id": f"{self._resource_counter}_dsrpe",
                    "uid": "uid://dbfc62yrw0q43"  # new_style_box_line的固定uid
                })
                self._resource_counter += 1
        
        # 递归处理子节点
        for child in node.children:
            self._collect_ext_resources(child)
    
    def generate_tree_view(self) -> str:
        """生成树状图（给AI看）"""
        if not self.root:
            return "No root node"
        
        lines = []
        
        def _traverse(node: UINode, prefix: str = "", is_last: bool = True):
            # 节点信息
            info = f"{node.name} ({node.node_type})"
            
            # 添加关键属性
            extras = []
            if "text" in node.properties:
                extras.append(node.properties["text"].strip('"'))
            if "_script_path" in node.properties:
                extras.append("[script]")
            if node == self.root:
                extras.append("[root]")
            
            if extras:
                info += f" {' '.join(extras)}"
            
            # 绘制树形结构
            connector = "└── " if is_last else "├── "
            lines.append(prefix + connector + info)
            
            # 递归子节点
            child_prefix = prefix + ("    " if is_last else "│   ")
            for i, child in enumerate(node.children):
                _traverse(child, child_prefix, i == len(node.children) - 1)
        
        # 根节点特殊处理
        lines.append(f"{self.root.name} ({self.root.node_type}) [root]")
        for i, child in enumerate(self.root.children):
            _traverse(child, "", i == len(self.root.children) - 1)
        
        return "\n".join(lines)
    
    def generate_tscn(self) -> str:
        """生成.tscn文本"""
        if not self.root:
            raise ValueError("No root node created")
        
        # 收集外部资源
        self._collect_ext_resources(self.root)
        
        lines = []
        
        # 文件头
        lines.append(f'[gd_scene format=3 uid="{self.scene_uid}"]')
        lines.append("")
        
        # 外部资源
        for res in self.ext_resources:
            lines.append(f'[ext_resource type="{res["type"]}" uid="{res["uid"]}" path="{res["path"]}" id="{res["id"]}"]')
        
        if self.ext_resources:
            lines.append("")
        
        # 节点定义
        def _write_node(node: UINode):
            # 节点头
            if node == self.root:
                lines.append(f'[node name="{node.name}" type="{node.node_type}" unique_id={node.unique_id}]')
            else:
                lines.append(f'[node name="{node.name}" type="{node.node_type}" parent="{node.parent_path}" unique_id={node.unique_id}]')
            
            # 属性
            for key, value in node.properties.items():
                # 跳过内部属性
                if key.startswith("_"):
                    continue
                
                # 处理ExtResource引用
                if isinstance(value, str) and value.startswith("ExtResource"):
                    # 查找对应的资源ID
                    if "script" in key:
                        for res in self.ext_resources:
                            if res["type"] == "Script":
                                value = f'ExtResource("{res["id"]}")'
                                break
                    elif "separator" in key:
                        for res in self.ext_resources:
                            if res["type"] == "StyleBox":
                                value = f'ExtResource("{res["id"]}")'
                                break
                
                # 写入属性
                if isinstance(value, str):
                    lines.append(f"{key} = {value}")
                elif isinstance(value, (int, float)):
                    lines.append(f"{key} = {value}")
                else:
                    lines.append(f"{key} = {value}")
            
            lines.append("")
            
            # 递归子节点
            for child in node.children:
                _write_node(child)
        
        _write_node(self.root)
        
        return "\n".join(lines)
    
    def save(self, output_path: str):
        """保存到文件"""
        content = self.generate_tscn()
        with open(output_path, "w", encoding="utf-8") as f:
            f.write(content)
        print(f"✅ UI saved to: {output_path}")
