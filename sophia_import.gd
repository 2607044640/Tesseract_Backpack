@tool
extends EditorScenePostImport

func _post_import(scene: Node) -> Object:
	print("=== Sophia Post-Import: Starting ===")
	
	# 查找AnimationPlayer
	var anim_player = find_animation_player(scene)
	if anim_player == null:
		push_warning("No AnimationPlayer found in scene")
		return scene
	
	print("Found AnimationPlayer with ", anim_player.get_animation_list().size(), " animations")
	
	# 修复骨骼路径（Skeleton3D → GeneralSkeleton）
	fix_skeleton_paths(anim_player)
	
	# 自动保存所有动画
	var save_dir = "res://player_Sophia/sophia_skin/animations/"
	
	# 确保目录存在
	if not DirAccess.dir_exists_absolute(save_dir):
		DirAccess.make_dir_recursive_absolute(save_dir)
		print("Created directory: ", save_dir)
	
	# 保存每个动画
	var saved_count = 0
	for anim_name in anim_player.get_animation_list():
		var anim = anim_player.get_animation(anim_name)
		var save_path = save_dir + anim_name + ".res"
		
		# 保存动画资源
		var err = ResourceSaver.save(anim, save_path)
		if err == OK:
			print("✓ Saved animation: ", anim_name, " -> ", save_path)
			saved_count += 1
		else:
			push_error("✗ Failed to save animation: ", anim_name, " (Error: ", err, ")")
	
	print("=== Sophia Post-Import: Complete (", saved_count, "/", anim_player.get_animation_list().size(), " saved) ===")
	
	return scene

func find_animation_player(node: Node) -> AnimationPlayer:
	if node is AnimationPlayer:
		return node
	
	for child in node.get_children():
		var result = find_animation_player(child)
		if result != null:
			return result
	
	return null

func fix_skeleton_paths(anim_player: AnimationPlayer) -> void:
	print("=== Fixing Skeleton Paths ===")
	
	# 首先找到实际的Skeleton3D节点
	var skeleton = find_skeleton(anim_player.get_parent())
	if skeleton == null:
		push_warning("No Skeleton3D found in scene!")
		return
	
	var skeleton_path = anim_player.get_parent().get_path_to(skeleton)
	print("Found Skeleton at path: ", skeleton_path)
	
	var fixed_count = 0
	var sample_paths = []
	
	for anim_name in anim_player.get_animation_list():
		var anim = anim_player.get_animation(anim_name)
		print("Processing animation: ", anim_name, " (", anim.get_track_count(), " tracks)")
		
		for track_idx in range(anim.get_track_count()):
			var track_path = anim.track_get_path(track_idx)
			var path_string = str(track_path)
			
			# 收集前3个路径作为样本
			if sample_paths.size() < 3:
				sample_paths.append(path_string)
			
			# 检查是否是骨骼动画轨道（包含冒号表示属性路径）
			if ":" in path_string:
				var parts = path_string.split(":")
				var node_path = parts[0]
				var property = parts[1]
				
				# 尝试多种可能的骨骼路径模式
				var needs_fix = false
				var new_node_path = node_path
				
				# 模式1: 包含 "Skeleton3D"
				if "Skeleton3D" in node_path:
					new_node_path = str(skeleton_path)
					needs_fix = true
				# 模式2: 包含 "Armature/Skeleton"
				elif "Armature" in node_path and "Skeleton" in node_path:
					new_node_path = str(skeleton_path)
					needs_fix = true
				# 模式3: 直接是 "Skeleton" 或其他骨骼名称
				elif node_path.ends_with("Skeleton") or node_path.ends_with("GeneralSkeleton"):
					new_node_path = str(skeleton_path)
					needs_fix = true
				
				if needs_fix:
					var new_path = new_node_path + ":" + property
					anim.track_set_path(track_idx, NodePath(new_path))
					fixed_count += 1
	
	print("Sample track paths found:")
	for sample in sample_paths:
		print("  - ", sample)
	
	if fixed_count > 0:
		print("✓ Fixed ", fixed_count, " animation tracks to use path: ", skeleton_path)
	else:
		print("⚠ No skeleton path fixes applied - animations may not work!")

func find_skeleton(node: Node) -> Skeleton3D:
	if node is Skeleton3D:
		return node
	
	for child in node.get_children():
		var result = find_skeleton(child)
		if result != null:
			return result
	
	return null
