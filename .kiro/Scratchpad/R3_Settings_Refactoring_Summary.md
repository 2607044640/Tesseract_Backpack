# R3 Settings Refactoring Summary

## What Changed

### SettingsManager.cs
**Before:** Component-based coordinator that registered UI helpers and saved/loaded their states
**After:** Centralized state holder using `ReactiveProperty<T>` for all settings

**Key Changes:**
- Removed component registration system (`RegisterComponent`, `RegisterComponents`)
- Replaced with 8 `ReactiveProperty<T>` fields (MasterVolume, MusicVolume, etc.)
- Auto-loads from ConfigFile on initialization
- Auto-saves on any ReactiveProperty change using `.Subscribe().AddTo()`
- Eliminated manual event subscription/unsubscription

### SettingsMenu.cs
**Before:** Subscribed to component helper events using `Observable.FromEvent`
**After:** Bidirectional binding with SettingsManager's ReactiveProperties

**Key Changes:**
- Changed `[Export]` from component helpers to raw Godot controls (HSlider, CheckButton, OptionButton)
- Removed component registration and SettingKey configuration
- Removed `LoadSettingsDeferred` workaround
- Implemented bidirectional binding:
  - **Manager → UI:** Subscribe to ReactiveProperties to update UI + apply system settings
  - **UI → Manager:** Use R3.Godot extensions (OnValueChangedAsObservable) to update ReactiveProperty.Value

## Benefits

1. **No Manual Initial Sync:** ReactiveProperty emits current value on subscribe - UI automatically gets correct initial state
2. **Single Source of Truth:** SettingsManager holds all state, UI is just a view
3. **Automatic Memory Management:** `.AddTo(_disposables)` handles all cleanup
4. **Auto-Save:** Any state change automatically persists to ConfigFile
5. **Eliminated Workarounds:** No more `CallDeferred(LoadSettingsDeferred)` hacks

## Architecture Pattern

```
SettingsManager (State Layer)
  ├─ ReactiveProperty<float> MasterVolume
  ├─ ReactiveProperty<bool> Mute
  └─ ... (8 total properties)
       ↓ Subscribe
SettingsMenu (View Layer)
  ├─ HSlider MasterVolumeSlider
  ├─ CheckButton MuteToggle
  └─ ... (UI controls)
```

**Data Flow:**
- User changes slider → OnValueChangedAsObservable → ReactiveProperty.Value = x → Auto-save + Notify subscribers
- ReactiveProperty changes → Subscribe callback → Update UI + Apply system settings

## Migration Notes

**Breaking Changes:**
- SettingsMenu now expects raw Godot controls, not component helpers
- Need to update SettingsUI.tscn to export HSlider/CheckButton/OptionButton instead of SliderComponentHelper/ToggleComponentHelper
- Component helpers (SliderComponentHelper, etc.) are no longer used in settings system

**Scene Update Required:**
Update `A1UIScenes/SettingsUI.tscn` to connect raw controls to SettingsMenu exports.
