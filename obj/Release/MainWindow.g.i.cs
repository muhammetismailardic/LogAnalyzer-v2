﻿#pragma checksum "..\..\MainWindow.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "D74855398D0BEBFAEBBFDF0974DCB441C65295D9CCCDE572D0A476A7703AAEF6"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using DevExpress.Xpf.DXBinding;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.DataPager;
using DevExpress.Xpf.Editors.DateNavigator;
using DevExpress.Xpf.Editors.ExpressionEditor;
using DevExpress.Xpf.Editors.Filtering;
using DevExpress.Xpf.Editors.Flyout;
using DevExpress.Xpf.Editors.Popups;
using DevExpress.Xpf.Editors.Popups.Calendar;
using DevExpress.Xpf.Editors.RangeControl;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Editors.Settings.Extension;
using DevExpress.Xpf.Editors.Validation;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.ConditionalFormatting;
using DevExpress.Xpf.Grid.LookUp;
using DevExpress.Xpf.Grid.TreeList;
using LogAnalyzerV2;
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


namespace LogAnalyzerV2 {
    
    
    /// <summary>
    /// MainWindow
    /// </summary>
    public partial class MainWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 52 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cmbVSVAFServers;
        
        #line default
        #line hidden
        
        
        #line 53 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cmbVSVAFAgents;
        
        #line default
        #line hidden
        
        
        #line 56 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal DevExpress.Xpf.Grid.GridControl grdSessionReport;
        
        #line default
        #line hidden
        
        
        #line 94 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnExpCollectionResults;
        
        #line default
        #line hidden
        
        
        #line 110 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cmbCSVAFServers;
        
        #line default
        #line hidden
        
        
        #line 111 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cmbCSVAFAgents;
        
        #line default
        #line hidden
        
        
        #line 112 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cmbCSVAFSessions;
        
        #line default
        #line hidden
        
        
        #line 115 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal DevExpress.Xpf.Grid.GridControl grdScheduledjobsReport;
        
        #line default
        #line hidden
        
        
        #line 117 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal DevExpress.Xpf.Grid.TableView grdScheduledjobsReportTable;
        
        #line default
        #line hidden
        
        
        #line 143 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal DevExpress.Xpf.Grid.GridControl grdDailyColReports;
        
        #line default
        #line hidden
        
        
        #line 170 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cmbAgentFileTransfer;
        
        #line default
        #line hidden
        
        
        #line 173 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal DevExpress.Xpf.Grid.GridControl grdAgentFileTransfer;
        
        #line default
        #line hidden
        
        
        #line 196 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnUploadFiles;
        
        #line default
        #line hidden
        
        
        #line 197 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ProgressBar progressBar;
        
        #line default
        #line hidden
        
        
        #line 198 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label lblPercentage;
        
        #line default
        #line hidden
        
        
        #line 199 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox IsFileTransferEnabled;
        
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
            System.Uri resourceLocater = new System.Uri("/LogAnalyzerV2;component/mainwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\MainWindow.xaml"
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
            
            #line 22 "..\..\MainWindow.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.Onclick_OppositeInformation);
            
            #line default
            #line hidden
            return;
            case 2:
            this.cmbVSVAFServers = ((System.Windows.Controls.ComboBox)(target));
            
            #line 52 "..\..\MainWindow.xaml"
            this.cmbVSVAFServers.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.CmbVSVAFServers_SelectionChanged);
            
            #line default
            #line hidden
            
            #line 52 "..\..\MainWindow.xaml"
            this.cmbVSVAFServers.DropDownClosed += new System.EventHandler(this.cmbVSVAFServers_DropDownClosed);
            
            #line default
            #line hidden
            return;
            case 3:
            this.cmbVSVAFAgents = ((System.Windows.Controls.ComboBox)(target));
            
            #line 53 "..\..\MainWindow.xaml"
            this.cmbVSVAFAgents.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.CmbVSVAFAgents_SelectionChanged);
            
            #line default
            #line hidden
            
            #line 53 "..\..\MainWindow.xaml"
            this.cmbVSVAFAgents.DropDownClosed += new System.EventHandler(this.cmbVSVAFAgents_DropDownClosed);
            
            #line default
            #line hidden
            return;
            case 4:
            this.grdSessionReport = ((DevExpress.Xpf.Grid.GridControl)(target));
            return;
            case 5:
            this.btnExpCollectionResults = ((System.Windows.Controls.Button)(target));
            
            #line 94 "..\..\MainWindow.xaml"
            this.btnExpCollectionResults.Click += new System.Windows.RoutedEventHandler(this.btnExpCollectionResults_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.cmbCSVAFServers = ((System.Windows.Controls.ComboBox)(target));
            
            #line 110 "..\..\MainWindow.xaml"
            this.cmbCSVAFServers.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.CmbCSVAFServers_SelectionChanged);
            
            #line default
            #line hidden
            
            #line 110 "..\..\MainWindow.xaml"
            this.cmbCSVAFServers.DropDownClosed += new System.EventHandler(this.cmbCSVAFServers_DropDownClosed);
            
            #line default
            #line hidden
            return;
            case 7:
            this.cmbCSVAFAgents = ((System.Windows.Controls.ComboBox)(target));
            
            #line 111 "..\..\MainWindow.xaml"
            this.cmbCSVAFAgents.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.CmbCSVAFAgents_SelectionChanged);
            
            #line default
            #line hidden
            
            #line 111 "..\..\MainWindow.xaml"
            this.cmbCSVAFAgents.DropDownClosed += new System.EventHandler(this.cmbCSVAFAgents_DropDownClosed);
            
            #line default
            #line hidden
            return;
            case 8:
            this.cmbCSVAFSessions = ((System.Windows.Controls.ComboBox)(target));
            
            #line 112 "..\..\MainWindow.xaml"
            this.cmbCSVAFSessions.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.CmbCSVAFSessions_SelectionChanged);
            
            #line default
            #line hidden
            
            #line 112 "..\..\MainWindow.xaml"
            this.cmbCSVAFSessions.DropDownClosed += new System.EventHandler(this.cmbCSVAFSessions_DropDownClosed);
            
            #line default
            #line hidden
            return;
            case 9:
            this.grdScheduledjobsReport = ((DevExpress.Xpf.Grid.GridControl)(target));
            return;
            case 10:
            this.grdScheduledjobsReportTable = ((DevExpress.Xpf.Grid.TableView)(target));
            return;
            case 11:
            this.grdDailyColReports = ((DevExpress.Xpf.Grid.GridControl)(target));
            return;
            case 12:
            this.cmbAgentFileTransfer = ((System.Windows.Controls.ComboBox)(target));
            
            #line 170 "..\..\MainWindow.xaml"
            this.cmbAgentFileTransfer.DropDownClosed += new System.EventHandler(this.cmbAgentFileTransfer_DropDownClosed);
            
            #line default
            #line hidden
            
            #line 170 "..\..\MainWindow.xaml"
            this.cmbAgentFileTransfer.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.cmbAgentFileTransfer_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 13:
            this.grdAgentFileTransfer = ((DevExpress.Xpf.Grid.GridControl)(target));
            return;
            case 14:
            this.btnUploadFiles = ((System.Windows.Controls.Button)(target));
            
            #line 196 "..\..\MainWindow.xaml"
            this.btnUploadFiles.Click += new System.Windows.RoutedEventHandler(this.btnUploadFiles_Click);
            
            #line default
            #line hidden
            return;
            case 15:
            this.progressBar = ((System.Windows.Controls.ProgressBar)(target));
            return;
            case 16:
            this.lblPercentage = ((System.Windows.Controls.Label)(target));
            return;
            case 17:
            this.IsFileTransferEnabled = ((System.Windows.Controls.CheckBox)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

