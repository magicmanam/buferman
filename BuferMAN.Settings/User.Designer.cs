﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BuferMAN.Settings {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.9.0.0")]
    internal sealed partial class User : global::System.Configuration.ApplicationSettingsBase {
        
        private static User defaultInstance = ((User)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new User())));
        
        public static User Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool ShowUserModeNotification {
            get {
                return ((bool)(this["ShowUserModeNotification"]));
            }
            set {
                this["ShowUserModeNotification"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int CurrentBuferBackgroundColor {
            get {
                return ((int)(this["CurrentBuferBackgroundColor"]));
            }
            set {
                this["CurrentBuferBackgroundColor"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int BuferDefaultBackgroundColor {
            get {
                return ((int)(this["BuferDefaultBackgroundColor"]));
            }
            set {
                this["BuferDefaultBackgroundColor"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("-8943463")]
        public int PinnedBuferBackgroundColor {
            get {
                return ((int)(this["PinnedBuferBackgroundColor"]));
            }
            set {
                this["PinnedBuferBackgroundColor"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2500")]
        public int FocusTooltipDuration {
            get {
                return ((int)(this["FocusTooltipDuration"]));
            }
            set {
                this["FocusTooltipDuration"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ShowFocusTooltip {
            get {
                return ((bool)(this["ShowFocusTooltip"]));
            }
            set {
                this["ShowFocusTooltip"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool RestorePreviousSession {
            get {
                return ((bool)(this["RestorePreviousSession"]));
            }
            set {
                this["RestorePreviousSession"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2")]
        public int EscHotKeyIntroductionCounter {
            get {
                return ((int)(this["EscHotKeyIntroductionCounter"]));
            }
            set {
                this["EscHotKeyIntroductionCounter"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2")]
        public int ClosingWindowExplanationCounter {
            get {
                return ((int)(this["ClosingWindowExplanationCounter"]));
            }
            set {
                this["ClosingWindowExplanationCounter"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool IsBuferClickingExplained {
            get {
                return ((bool)(this["IsBuferClickingExplained"]));
            }
            set {
                this["IsBuferClickingExplained"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2")]
        public int HttpUrlBuferExplanationCounter {
            get {
                return ((int)(this["HttpUrlBuferExplanationCounter"]));
            }
            set {
                this["HttpUrlBuferExplanationCounter"] = value;
            }
        }
    }
}
