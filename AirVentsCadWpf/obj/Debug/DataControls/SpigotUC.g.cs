﻿#pragma checksum "..\..\..\DataControls\SpigotUC.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "0124E5E5EF5792BDDF42AD5C90D83166"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace AirVentsCadWpf.DataControls {
    
    
    /// <summary>
    /// SpigotUc
    /// </summary>
    public partial class SpigotUc : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 31 "..\..\..\DataControls\SpigotUC.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox TypeOfSpigot;
        
        #line default
        #line hidden
        
        
        #line 42 "..\..\..\DataControls\SpigotUC.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox HeightSpigot;
        
        #line default
        #line hidden
        
        
        #line 43 "..\..\..\DataControls\SpigotUC.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label HeightLabel;
        
        #line default
        #line hidden
        
        
        #line 44 "..\..\..\DataControls\SpigotUC.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox WidthSpigot;
        
        #line default
        #line hidden
        
        
        #line 45 "..\..\..\DataControls\SpigotUC.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label WidthLabel;
        
        #line default
        #line hidden
        
        
        #line 52 "..\..\..\DataControls\SpigotUC.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BuildSpigot;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/AirVentsCad;component/datacontrols/spigotuc.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\DataControls\SpigotUC.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.TypeOfSpigot = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 2:
            this.HeightSpigot = ((System.Windows.Controls.TextBox)(target));
            
            #line 42 "..\..\..\DataControls\SpigotUC.xaml"
            this.HeightSpigot.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.NumberValidationTextBox);
            
            #line default
            #line hidden
            
            #line 42 "..\..\..\DataControls\SpigotUC.xaml"
            this.HeightSpigot.KeyDown += new System.Windows.Input.KeyEventHandler(this.HeightSpigot_KeyDown);
            
            #line default
            #line hidden
            return;
            case 3:
            this.HeightLabel = ((System.Windows.Controls.Label)(target));
            return;
            case 4:
            this.WidthSpigot = ((System.Windows.Controls.TextBox)(target));
            
            #line 44 "..\..\..\DataControls\SpigotUC.xaml"
            this.WidthSpigot.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.NumberValidationTextBox);
            
            #line default
            #line hidden
            
            #line 44 "..\..\..\DataControls\SpigotUC.xaml"
            this.WidthSpigot.KeyDown += new System.Windows.Input.KeyEventHandler(this.WidthSpigot_KeyDown);
            
            #line default
            #line hidden
            return;
            case 5:
            this.WidthLabel = ((System.Windows.Controls.Label)(target));
            return;
            case 6:
            this.BuildSpigot = ((System.Windows.Controls.Button)(target));
            
            #line 53 "..\..\..\DataControls\SpigotUC.xaml"
            this.BuildSpigot.Click += new System.Windows.RoutedEventHandler(this.BuildSpigot_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

