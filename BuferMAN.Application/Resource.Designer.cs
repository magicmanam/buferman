﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BuferMAN.Application {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("BuferMAN.Application.Resource", typeof(Resource).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to BuferMAN (ADMIN).
        /// </summary>
        internal static string AdminApplicationTitle {
            get {
                return ResourceManager.GetString("AdminApplicationTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to BuferMAN.
        /// </summary>
        internal static string ApplicationTitle {
            get {
                return ResourceManager.GetString("ApplicationTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Read more info in menu: Help -&gt;Documentation.
        /// </summary>
        internal static string DocumentationMentioning {
            get {
                return ResourceManager.GetString("DocumentationMentioning", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Clipboard last update was at .
        /// </summary>
        internal static string LastClipboardUpdate {
            get {
                return ResourceManager.GetString("LastClipboardUpdate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to E&amp;xit session.
        /// </summary>
        internal static string MenuFileExit {
            get {
                return ResourceManager.GetString("MenuFileExit", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You copied 1000 times! You are great copypaster ever!.
        /// </summary>
        internal static string NotifyIcon1000Congrats {
            get {
                return ResourceManager.GetString("NotifyIcon1000Congrats", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You copied 100 times! Congrats!.
        /// </summary>
        internal static string NotifyIcon100Congrats {
            get {
                return ResourceManager.GetString("NotifyIcon100Congrats", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to BuferMAN is paused. Alt+P to resume.
        /// </summary>
        internal static string PausedStatus {
            get {
                return ResourceManager.GetString("PausedStatus", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to BuferMAN is resumed. Enjoy copies!.
        /// </summary>
        internal static string ResumedStatus {
            get {
                return ResourceManager.GetString("ResumedStatus", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Bufer manual.
        /// </summary>
        internal static string TrayMenuBuferManual {
            get {
                return ResourceManager.GetString("TrayMenuBuferManual", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Options.
        /// </summary>
        internal static string TrayMenuOptions {
            get {
                return ResourceManager.GetString("TrayMenuOptions", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Click bufer to paste it into active window.
        ///Every bufer has a context menu.
        ///
        ///
        ///Hot keys:
        ///  - Launch the program: Alt + C
        ///  - Go to the first bufer: X (Home)
        ///  - Go to the last bufer: V (End)
        ///  - Tabify through 3 bufers: C
        ///  - Undo/Redo an action: Ctrl + Z, Ctrl + Y
        ///
        ///-&gt; Paste your nice copies!.
        /// </summary>
        internal static string UserManual {
            get {
                return ResourceManager.GetString("UserManual", resourceCulture);
            }
        }
    }
}
