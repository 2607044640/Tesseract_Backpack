using Godot;
using R3;
using System;

namespace GameSettings
{
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
            
            // 从配置文件读取值，若不存在则使用默认值
            Property = new ReactiveProperty<T>(LoadValue());
        }
        
        public ReactiveProperty<T> GetProperty() => Property;
        
        protected abstract T LoadValue();
        
        public abstract void SaveValue();
        
        public void Reset()
        {
            Property.Value = DefaultValue;
        }
    }
    
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
