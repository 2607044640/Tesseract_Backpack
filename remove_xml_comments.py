#!/usr/bin/env python3
"""
批量删除 C# 文件中的 XML 文档注释标签
删除 /// <summary> 和 /// </summary>，保留中间的注释内容
"""

import os
import re
from pathlib import Path


def remove_xml_comment_tags(content: str) -> str:
    """
    删除 XML 注释标签，保留注释内容
    
    处理规则：
    1. 删除单独一行的 /// <summary>
    2. 删除单独一行的 /// </summary>
    3. 保留其他所有 /// 注释内容
    """
    lines = content.split('\n')
    result_lines = []
    
    for line in lines:
        # 检查是否是纯 <summary> 或 </summary> 标签行
        stripped = line.strip()
        
        # 跳过只包含 /// <summary> 的行（可能有前导空白）
        if re.match(r'^\s*///\s*<summary>\s*$', line):
            continue
            
        # 跳过只包含 /// </summary> 的行（可能有前导空白）
        if re.match(r'^\s*///\s*</summary>\s*$', line):
            continue
        
        # 保留其他所有行
        result_lines.append(line)
    
    return '\n'.join(result_lines)


def process_csharp_file(file_path: Path) -> bool:
    """
    处理单个 C# 文件
    返回 True 如果文件被修改
    """
    try:
        # 读取文件内容
        with open(file_path, 'r', encoding='utf-8') as f:
            original_content = f.read()
        
        # 删除 XML 注释标签
        new_content = remove_xml_comment_tags(original_content)
        
        # 如果内容有变化，写回文件
        if new_content != original_content:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(new_content)
            return True
        
        return False
    
    except Exception as e:
        print(f"❌ 处理文件失败 {file_path}: {e}")
        return False


def main():
    """主函数：扫描并处理所有 C# 文件"""
    
    # 项目根目录
    project_root = Path(__file__).parent
    
    # 要处理的目录列表
    target_dirs = [
        project_root / "addons",
        project_root / "B1Scripts",
        project_root / ".backup_old_components"
    ]
    
    print("🔍 开始扫描 C# 文件...")
    print(f"📁 项目根目录: {project_root}")
    print()
    
    modified_files = []
    total_files = 0
    
    # 遍历所有目标目录
    for target_dir in target_dirs:
        if not target_dir.exists():
            print(f"⚠️  目录不存在，跳过: {target_dir}")
            continue
        
        print(f"📂 扫描目录: {target_dir.relative_to(project_root)}")
        
        # 递归查找所有 .cs 文件
        for cs_file in target_dir.rglob("*.cs"):
            total_files += 1
            
            # 处理文件
            if process_csharp_file(cs_file):
                relative_path = cs_file.relative_to(project_root)
                modified_files.append(relative_path)
                print(f"  ✅ 已修改: {relative_path}")
    
    # 输出统计信息
    print()
    print("=" * 60)
    print(f"📊 处理完成！")
    print(f"   总文件数: {total_files}")
    print(f"   修改文件数: {len(modified_files)}")
    print(f"   未修改文件数: {total_files - len(modified_files)}")
    print("=" * 60)
    
    if modified_files:
        print()
        print("📝 修改的文件列表:")
        for file_path in modified_files:
            print(f"   - {file_path}")


if __name__ == "__main__":
    main()
