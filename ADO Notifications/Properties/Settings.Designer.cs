//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ADO_Notifications.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.0.3.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string AccessToken {
            get {
                return ((string)(this["AccessToken"]));
            }
            set {
                this["AccessToken"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool RemindAboutUnreviewedPRs {
            get {
                return ((bool)(this["RemindAboutUnreviewedPRs"]));
            }
            set {
                this["RemindAboutUnreviewedPRs"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("60")]
        public int RemindAboutUnreviewedPRIntervalMinutes {
            get {
                return ((int)(this["RemindAboutUnreviewedPRIntervalMinutes"]));
            }
            set {
                this["RemindAboutUnreviewedPRIntervalMinutes"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool NotifyOnNewPR {
            get {
                return ((bool)(this["NotifyOnNewPR"]));
            }
            set {
                this["NotifyOnNewPR"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool NotifyOnNewPRIncludeDraft {
            get {
                return ((bool)(this["NotifyOnNewPRIncludeDraft"]));
            }
            set {
                this["NotifyOnNewPRIncludeDraft"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool FirstLaunch {
            get {
                return ((bool)(this["FirstLaunch"]));
            }
            set {
                this["FirstLaunch"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool NotifyOnNewBuilds {
            get {
                return ((bool)(this["NotifyOnNewBuilds"]));
            }
            set {
                this["NotifyOnNewBuilds"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool NotifyOnCompletedBuilds {
            get {
                return ((bool)(this["NotifyOnCompletedBuilds"]));
            }
            set {
                this["NotifyOnCompletedBuilds"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool NotifyOnBuildStatusChanges {
            get {
                return ((bool)(this["NotifyOnBuildStatusChanges"]));
            }
            set {
                this["NotifyOnBuildStatusChanges"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool NotifyOnCompletedSuccessfulBuilds {
            get {
                return ((bool)(this["NotifyOnCompletedSuccessfulBuilds"]));
            }
            set {
                this["NotifyOnCompletedSuccessfulBuilds"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool NotifyOnCompletedFailedBuilds {
            get {
                return ((bool)(this["NotifyOnCompletedFailedBuilds"]));
            }
            set {
                this["NotifyOnCompletedFailedBuilds"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool NotifyOnUpdatedPullRequests {
            get {
                return ((bool)(this["NotifyOnUpdatedPullRequests"]));
            }
            set {
                this["NotifyOnUpdatedPullRequests"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool StartMinimised {
            get {
                return ((bool)(this["StartMinimised"]));
            }
            set {
                this["StartMinimised"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool MinimiseToTray {
            get {
                return ((bool)(this["MinimiseToTray"]));
            }
            set {
                this["MinimiseToTray"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool NotifyWhenAddedAsReviewer {
            get {
                return ((bool)(this["NotifyWhenAddedAsReviewer"]));
            }
            set {
                this["NotifyWhenAddedAsReviewer"] = value;
            }
        }
    }
}
