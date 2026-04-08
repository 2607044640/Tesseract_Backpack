using Godot;
using R3;
using System;

namespace GameSettings
{
    /// <summary>
    /// 设置绑定器基类 - 封装所有重复逻辑
    /// 负责：加载、保存、重置、提供 ReactiveProperty
    /// </summary>
    public abstract class SettingBinderBase<T>
    {
        protected readonly string Key;
        protected readonly T DefaultValue;
        protected readonly ReactiveProperty<T> Property;
        protected readonly ConfigFile Config;
        protected readonly string Section;
        
        public SettingBinderBase(
            string key, 
            T defaultValue, 
            ConfigFile config, 
            string section)
        {
            Key = key;
            DefaultValue = defaultValue;
            Config = config;
            Section = section;
            
            // 从配置文件加载或使用默认值
            Property = new ReactiveProperty<T>(LoadValue());
        }
        
        /// <summary>
        /// 获取 ReactiveProperty（只读访问）
        /// </summary>
        public ReactiveProperty<T> GetProperty() => Property;
        
        /// <summary>
        /// 从配置文件加载值（子类实现）
        /// </summary>
        protected abstract T LoadValue();
        
        /// <summary>
        /// 保存值到配置文件（子类实现）
        /// </summary>
        public abstract void SaveValue();
        
        /// <summary>
        /// 重置到默认值
        /// </summary>
        public void Reset()
        {
            Property.Value = DefaultValue;
        }
    }
    
    /// <summary>
    /// Float 类型绑定器（用于滑块）
    /// </summary>
    public class FloatSettingBinder : SettingBinderBase<float>
    {
        public FloatSettingBinder(
            string key, 
            float defaultValue, 
            ConfigFile config, 
            string section)
            : base(key, defaultValue, config, section)
        {
        }
        
        protected override float LoadValue()
        {
            if (Config.HasSectionKey(Section, Key))
            {
                return (float)Config.GetValue(Section, Key);
            }
            return DefaultValue;
        }
        
        public override void SaveValue()
        {
            Config.SetValue(Section, Key, Property.Value);
        }
    }
    
    /// <summary>
    /// Bool 类型绑定器（用于开关）
    /// </summary>
    public class BoolSettingBinder : SettingBinderBase<bool>
    {
        public BoolSettingBinder(
            string key, 
            bool defaultValue, 
            ConfigFile config, 
            string section)
            : base(key, defaultValue, config, section)
        {
        }
        
        protected override bool LoadValue()
        {
            if (Config.HasSectionKey(Section, Key))
            {
                return (bool)Config.GetValue(Section, Key);
            }
            return DefaultValue;
        }
        
        public override void SaveValue()
        {
            Config.SetValue(Section, Key, Property.Value);
        }
    }
    
    /// <summary>
    /// Int 类型绑定器（用于下拉框）
    /// </summary>
    public class IntSettingBinder : SettingBinderBase<int>
    {
        public IntSettingBinder(
            string key, 
            int defaultValue, 
            ConfigFile config, 
            string section)
            : base(key, defaultValue, config, section)
        {
        }
        
        protected override int LoadValue()
        {
            if (Config.HasSectionKey(Section, Key))
            {
                return (int)Config.GetValue(Section, Key);
            }
            return DefaultValue;
        }
        
        public override void SaveValue()
        {
            Config.SetValue(Section, Key, Property.Value);
        }
    }
}
