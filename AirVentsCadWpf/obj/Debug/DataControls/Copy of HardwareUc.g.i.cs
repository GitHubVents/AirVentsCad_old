﻿#pragma checksum "..\..\..\DataControls\Copy of HardwareUc.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "EC366F16EDB3969EBE92405142995F0F"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18063
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
using System.Windows.Data;
using System.Windows.Documents;
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
    /// HardwareUc
    /// </summary>
    public partial class HardwareUc : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 34 "..\..\..\DataControls\Copy of HardwareUc.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox CmbTypeLane;
        
        #line default
        #line hidden
        
        
        #line 37 "..\..\..\DataControls\Copy of HardwareUc.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox CmbBlockType;
        
        #line default
        #line hidden
        
        
        #line 43 "..\..\..\DataControls\Copy of HardwareUc.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid DataGridSql2;
        
        #line default
        #line hidden
        
        
        #line 59 "..\..\..\DataControls\Copy of HardwareUc.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnSaveEdit;
        
        #line default
        #line hidden
        
        
        #line 60 "..\..\..\DataControls\Copy of HardwareUc.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnAdd;
        
        #line default
        #line hidden
        
        
        #line 62 "..\..\..\DataControls\Copy of HardwareUc.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnDelete;
        
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
            System.Uri resourceLocater = new System.Uri("/AirVentsCad;component/datacontrols/copy%20of%20hardwareuc.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\DataControls\Copy of HardwareUc.xaml"
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
            
            #line 8 "..\..\..\DataControls\Copy of HardwareUc.xaml"
            ((System.Windows.Controls.Grid)(target)).Loaded += new System.Windows.RoutedEventHandler(this.Grid_Loaded_1);
            
            #line default
            #line hidden
            return;
            case 2:
            this.CmbTypeLane = ((System.Windows.Controls.ComboBox)(target));
            
            #line 36 "..\..\..\DataControls\Copy of HardwareUc.xaml"
            this.CmbTypeLane.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.CmbTypeLane_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            this.CmbBlockType = ((System.Windows.Controls.ComboBox)(target));
            
            #line 38 "..\..\..\DataControls\Copy of HardwareUc.xaml"
            this.CmbBlockType.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.CmbBlockType_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 4:
            this.DataGridSql2 = ((System.Windows.Controls.DataGrid)(target));
            
            #line 56 "..\..\..\DataControls\Copy of HardwareUc.xaml"
            this.DataGridSql2.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.DataGridSql2_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 5:
            this.BtnSaveEdit = ((System.Windows.Controls.Button)(target));
            
            #line 59 "..\..\..\DataControls\Copy of HardwareUc.xaml"
            this.BtnSaveEdit.Click += new System.Windows.RoutedEventHandler(this.BtnSaveEdit_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.BtnAdd = ((System.Windows.Controls.Button)(target));
            
            #line 60 "..\..\..\DataControls\Copy of HardwareUc.xaml"
            this.BtnAdd.Click += new System.Windows.RoutedEventHandler(this.BtnAdd_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.BtnDelete = ((System.Windows.Controls.Button)(target));
            
            #line 62 "..\..\..\DataControls\Copy of HardwareUc.xaml"
            this.BtnDelete.Click += new System.Windows.RoutedEventHandler(this.BtnDelete_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
