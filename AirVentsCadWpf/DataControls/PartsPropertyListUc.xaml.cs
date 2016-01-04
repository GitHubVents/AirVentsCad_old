using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AirVentsCadWpf.AirVentsClasses;
using BomPartList;
using ModelSw = AirVentsCadWpf.AirVentsClasses.UnitsBuilding.ModelSw;

namespace AirVentsCadWpf.DataControls
{
    
    public partial class PartsPropertyListUc
    {

        List<PartProperty.PartPropBomCells> _bom;
        List<PartProperty.PartPropBomCells> _bomPartsList;
        List<PartProperty.PartPropBomCells> _bomPartsListFiltered;
        List<PartProperty.PartPropBomCells> _bomPartsListEdit;

        readonly List<List<PartProperty.PartPropBomCells>> _bomListSelectedPrts = new List<List<PartProperty.PartPropBomCells>>();

        private PartProperty _bomClass;

        readonly ModelSw _sw = new ModelSw();

        /// <summary>
        /// Initializes a new instance of the <see cref="PartsPropertyListUc"/> class.
        /// </summary>
        public PartsPropertyListUc()
        {
            InitializeComponent();
        }

        bool Login()
        {
            //IEdmVault10 mVault = new EdmVault5Class();
            //try
            //{
            //    try
            //    {
            //        mVault.LoginAuto(Settings.Default.PdmBaseName, 0);
            //    }
            //    catch (Exception)
            //    {
            //        mVault.Login(Settings.Default.UserName, Settings.Default.Password, Settings.Default.PdmBaseName);
            //        return false;
            //    }
            //    mVault.CreateSearch();
            //}
            //catch (COMException ex)
            //{
            //    MessageBox.Show(ex.Message);
            //    return false;
            //}
            //var fileDialog = new Microsoft.Win32.OpenFileDialog
            //{
            //    InitialDirectory = mVault.RootFolderPath,
            //    Filter = "SolidWorks Assemblies|*.sldasm"
            //};
            //if (fileDialog.ShowDialog() != true) return false;

            //_bomClass = new PartProperty
            //{
            //    PdmBaseName = Settings.Default.PdmBaseName,
            //    UserName = Settings.Default.UserName,
            //    UserPassword = Settings.Default.Password,
            //    AssemblyPath = fileDialog.FileName,
            //};

            //_bom = _bomClass.BomList();
            //_bomPartsList = _bomClass.BomListPrt();
            return true;
        }
        
        readonly SolidColorBrush _orangeColorBrush = new SolidColorBrush(Colors.LavenderBlush);
        readonly SolidColorBrush _whiteColorBrush = new SolidColorBrush(Colors.White);
        readonly SolidColorBrush _lightCyanColorBrush = new SolidColorBrush(Colors.LightCyan);

        void BomTable_LoadingRow(object sender, DataGridRowEventArgs e)
        {
           var bomCells = (BomPartListClass.BomCells)e.Row.DataContext;

             if (bomCells.Errors == "")
             {
                 e.Row.Background = e.Row.GetIndex() % 2 == 1 ? _lightCyanColorBrush : _whiteColorBrush;
             }
             else
             {
                 e.Row.Background = _orangeColorBrush;
             }
        }
   

        private void BomTablePrt_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BomTablePrt2.SelectedValue == null) return;
            var row = ((BomPartListClass.BomCells)(BomTablePrt2.SelectedValue));
        }

        void ReadProperties(string filePath, string config, string pdmVault)
        {
            FileInfo file = null;
            var isReadOnly = true;
            try
            {
                file = new FileInfo(filePath);
                if (file.IsReadOnly)
                {
                    file.IsReadOnly = false;
                }
                else
                {
                    isReadOnly = false;
                }
            }
            catch (Exception)
            {
                pdmVault = "Tets_debag";
                _sw.GetLastVersionPdm(filePath, pdmVault);
                file = new FileInfo(filePath);
                if (file.IsReadOnly)
                {
                    file.IsReadOnly = false;
                }
                else
                {
                    isReadOnly = false;
                }
            }
            finally
            {
                var docmgr = new SwDocMgr {SDocFileName = filePath};
                try
                {
                    //  PartProperties.ItemsSource = docmgr.CustomPropertiesList();
                    //    PartPropertiesCombo.ItemsSource = (List<SwDocMgr.PartProperties>)docmgr.CustomPropertiesList();
                    //   PartPropertiesCombo.DataContext = "PropName";
                    var list = (List<SwDocMgr.PartProperties>)docmgr.CustomPropertiesList();
                    foreach (var partPropertiese in list)
                    {
                        MessageBox.Show(partPropertiese.PropName);
                    }
                    docmgr.GetCustomProperties();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
                finally
                {
                    if (file != null) file.IsReadOnly = isReadOnly;
                }
            }
        }

        private List<SwDocMgr.PartProperties> strList;


        private void OpenAsm_Click(object sender, RoutedEventArgs args)
        {
            try
            {
                if (!Login()) return;
                MessageBox.Show(_bomClass.AssemblyInfoLabel + "   " + _bomClass.Message);
                BomTablePrt2.ItemsSource = _bomPartsList.Where(x => x.ТолщинаЛиста == "");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}
