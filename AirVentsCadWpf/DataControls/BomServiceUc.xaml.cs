﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AirVentsCadWpf.AirVentsClasses;
using AirVentsCadWpf.Properties;
using BomPartList;
using VentsMaterials;
using VentsPDM_dll;
using ModelSw = AirVentsCadWpf.AirVentsClasses.UnitsBuilding.ModelSw;


namespace AirVentsCadWpf.DataControls
{

    public partial class BomServiceUc
    {
        //private SpellServiceSoap spellServiceSoap;

        #region Fields and Constructor
        
        //List<BomPartListClass.BomCells> _bomPartsList;
        //List<BomPartListClass.BomCells> _bomPartsListFiltered;
        //List<BomPartListClass.BomCells> _bomPartsListOriginal;

         //List<BomPartListClassBomCells> _bomPartsList;
         //List<BomPartListClassBomCells> _bomPartsListFiltered;
         //List<BomPartListClassBomCells> _bomPartsListOriginal;
        
        

        //readonly List<BomPartListClass.BomCells> _bomListSelectedPrts = new List<BomPartListClass.BomCells>();
         //readonly List<BomPartListClassBomCells> _bomListSelectedPrts = new List<BomPartListClassBomCells>();


        private BomPartListClass _bomClass;

        readonly SetMaterials _swMaterials = new SetMaterials();
        private readonly ToSQL _connectToSql = new ToSQL();

        readonly ModelSw _modelSwClass = new ModelSw();

        private string _assemblyPath;

        public BomServiceUc()
        {
            InitializeComponent();

            #region Test

            ChangedDataGrid(false);

            ToSQL.Conn = Settings.Default.ConnectionToSQL;
            MaterialsList.ItemsSource = _connectToSql.GetSheetMetalMaterialsName();
            MaterialsList.DisplayMemberPath = "MatName";
            MaterialsList.SelectedValuePath = "MatName";
            MaterialsList.SelectedIndex = 0;

//            AdditionalGrid.Visibility = Visibility.Collapsed;
         //   FindByOrder.Visibility = Visibility.Collapsed;

//            PartPropsConfig.Visibility = Visibility.Collapsed;
 //           PartPropsCopy.Visibility = Visibility.Collapsed;

            // Buttons
//            BeginUpdatePartPropConfig.Visibility = Visibility.Collapsed;
 //           BeginUpdatePartProp.Visibility = Visibility.Collapsed;
  //          BeginUpdatePartPropThikness.Visibility = Visibility.Collapsed;

            #endregion


            #region Service

            try
            {
                //MessageBox.Show(_serviceClient.
                //_allAsmInPdm = _serviceClient.AsmNames();
                FindAsm();
            }
            catch (Exception e)
            {
                MessageBox.Show("Не удалость подключиться к сервису! \n" + e.Message);
            }
            

            #endregion
        }

        #endregion

        #region Main Methods

    
        //readonly BomTableServiceClient _serviceClient = new BomTableServiceClient();

        private readonly string[] _allAsmInPdm;

        //IEnumerable<BomPartListClass.BomCells> BomList
        //{
        //    get
        //    {
        //        return _serviceClient.BomParts(_assemblyPath, Settings.Default.PdmBaseName, Settings.Default.userName, Settings.Default.Password, Settings.Default.ConnectionToSQL);
        //    }
        //}
        
        void ChangedDataGrid(bool isChanged)
        {
            if (isChanged)
            {
                СосотяниеСборки.Content = "В детали сборки внесены изменения. Необходимо применить, для вступления в силу!";
                СосотяниеСборки.Foreground = Brushes.Tomato;
                BeginUpdate1.IsEnabled = true;
            }
            else
            {
                СосотяниеСборки.Content = " Детали сборки";
                СосотяниеСборки.Foreground = Brushes.Black;
                BeginUpdate1.IsEnabled = false;
            }
        }

        private static IEnumerable<BomPartListClass.BomCells> ChangeListMaterial(string partName, string partConfig, string partMaterial, IReadOnlyList<BomPartListClass.BomCells> listToChange)
        {
            foreach (var bomCells in listToChange.Where(bomCells => bomCells.ОбозначениеDocMgr == partName).Where(bomCells => bomCells.Конфигурация == partConfig))
            {
                bomCells.МатериалЦмиDocMgr = partMaterial;
                bomCells.Материал = partMaterial;
            }
            return listToChange;
        }
        
        #endregion

        #region User Interface

        private void MaterialsList_LayoutUpdated(object sender, EventArgs e)
        {
            MaterialsList.Background = MaterialsList.Text == "" ? Brushes.Pink : Brushes.Azure;
            Accept.Content = MaterialsList.Text != "" ? "Задать материал" : "Выбрать материал";
        }

        void FilterTextBox_OnSelectionChanged(object sender, TextChangedEventArgs e)
        {
            FilterBomTable();
            //_bomPartsListFiltered = _bomPartsList;
            //if (_bomPartsListFiltered != null) _bomPartsListFiltered = _bomPartsListFiltered.Where(x => x.Обозначение.Contains(FilterTextBox.Text)).ToList();
            //BomTablePrt.ItemsSource = _bomPartsListFiltered;
        }

         void FilterBomTable()
        {
            //_bomPartsListFiltered = _bomPartsList;
            //if (_bomPartsListFiltered != null) _bomPartsListFiltered = _bomPartsListFiltered.Where(x => x.Обозначение.Contains(FilterTextBox.Text)).ToList();
            //BomTablePrt.ItemsSource = _bomPartsListFiltered;
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
        
        void AssemblyInfo_LayoutUpdated(object sender, EventArgs e)
        {
            AsmComponents.Visibility = (string)AssemblyInfo.Content == "" ? Visibility.Hidden : Visibility.Visible;
           // PartProps.Visibility = (string)AssemblyInfo.Content == "" ? Visibility.Hidden : Visibility.Visible;
        }
        
        void BomTableSelectedPrt_LayoutUpdated(object sender, EventArgs e)
        {
            BomTableSelectedPrt.Visibility = BomTableSelectedPrt.HasItems ? Visibility.Visible : Visibility.Collapsed;

            ClearList.IsEnabled = BomTableSelectedPrt.HasItems;

            УдалитьВыбранную.IsEnabled = BomTableSelectedPrt.SelectedIndex != -1;

            BeginUpdateButton();

        }

        void BeginUpdateButton()
        {
            if (!BomTableSelectedPrt.HasItems)
            {
                BeginUpdate.IsEnabled = false;
                return;
            }
            if (UpdateCutList.IsChecked == true || MakeDxf.IsChecked == true)
            {
                BeginUpdate.IsEnabled = true;
            }
            else
            {
                BeginUpdate.IsEnabled = false;
            } 
        }

        void BomTableSelectedPrt_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            УдалитьВыбранную.IsEnabled = BomTableSelectedPrt.SelectedIndex != -1;
        }

        void BomTablePrt_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BomTablePrt.SelectedValue == null) return;
            //var row = ((BomPartListClass.BomCells)(BomTablePrt.SelectedValue));
            //var row = ((BomPartListClassBomCells)(BomTablePrt.SelectedValue));
            //MaterialsList.SelectedValue = row.Материал;

            //SelectedPart.ItemsSource = _bomPartsList.Where(x => x.Обозначение == row.ОбозначениеDocMgr);
        }

        void BomTablePrt_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (BomTablePrt.SelectedValue == null) return;
            //var row = ((BomPartListClass.BomCells)(BomTablePrt.SelectedValue));
            //var row = ((BomPartListClassBomCells)(BomTablePrt.SelectedValue));
            //if (_bomListSelectedPrts.Any(bomListSelectedPrt => row.Обозначение == bomListSelectedPrt.Обозначение)) return;
            //_bomListSelectedPrts.Add(row);
            //BomTableSelectedPrt.ItemsSource = _bomListSelectedPrts.OrderBy(x => x.Обозначение);
        }

        // Buttons

        void GetBomList()
        {
            try
            {
                //_assemblyPath = _serviceClient.PathByNameAsm(AsmNames.Text);
                //if (_assemblyPath == "") return;
                //var list = _serviceClient.Bom(1, _assemblyPath).ToList();
                //_bomPartsListOriginal = list;
                //_bomPartsList = list;
                //BomTablePrt.ItemsSource = _bomPartsList;
                //AssemblyInfo.Content = _assemblyPath;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }


        void OpenAsm_Click(object sender, RoutedEventArgs e)
        {
            GetBomList();


            //try
            //{
            //    _assemblyPath = _serviceClient.PathByNameAsm(AsmNames.Text);
            //    if (_assemblyPath == "") return;
            //    var list = _serviceClient.Bom(1, _assemblyPath).ToList();
            //    _bomPartsListOriginal = list;
            //    _bomPartsList = list;
            //    BomTablePrt.ItemsSource = _bomPartsList;
            //    AssemblyInfo.Content = _assemblyPath;
            //}
            //catch (Exception args)
            //{
            //    MessageBox.Show(args.Message);
            //}

        }



        void ClearList_Click(object sender, RoutedEventArgs e)
        {
            //_bomListSelectedPrts.Clear();
            //BomTableSelectedPrt.ItemsSource = _bomListSelectedPrts.OrderBy(x => x.Обозначение);
        }

        void УдалитьВыбранную_Click(object sender, RoutedEventArgs e)
        {
            //_bomListSelectedPrts.Remove(BomTableSelectedPrt.SelectedItem as BomPartListClass.BomCells);
            //_bomListSelectedPrts.Remove(BomTableSelectedPrt.SelectedItem as BomPartListClassBomCells);
            
            //BomTableSelectedPrt.ItemsSource = _bomListSelectedPrts.OrderBy(x => x.Обозначение);
        }

        void BeginUpdate_Click(object sender, RoutedEventArgs e)
        {
            //var partDataClass = new MakeDxfExportPartDataClass{ PdmBaseName = Settings.Default.PdmBaseName };
            //foreach (var selectedPrt in _bomListSelectedPrts)
            //{
            //    try
            //    {
            //        if (UpdateCutList.IsChecked == true)
            //        {
            //         //   partDataClass.CreateFlattPatternUpdateCutlistAndEdrawing(selectedPrt.Путь);
            //        }
            //        if (MakeDxf.IsChecked == true)
            //        {
            //           // partDataClass.CreateFlattPatternUpdateCutlistAndEdrawing(selectedPrt.Путь);
            //        }
            //    }
            //    catch (Exception args)
            //    {
            //        MessageBox.Show(args.Message);
            //        return;
            //    }
            //}

            //_bomListSelectedPrts.Clear();
            //BomTableSelectedPrt.ItemsSource = _bomListSelectedPrts.OrderBy(x => x.Обозначение);
            //Application.Current.MainWindow.Activate();
        }

        void Accept_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialsList.SelectedValue == null) return;
            var material = ((ToSQL.ColumnMatProps)(MaterialsList.SelectedItem));
            //var material = ((ToSQL.ColumnMatProps)(MaterialsList.SelectedValue));
            if (BomTablePrt.SelectedValue == null) return;
            var row = ((BomPartListClass.BomCells)(BomTablePrt.SelectedValue));

     //       BomTablePrt.ItemsSource = _bomPartsList = ChangeListMaterial(row.ОбозначениеDocMgr, row.Конфигурация, material.MatName, _bomPartsList).ToList();
            ChangedDataGrid(true);
     //       SelectedPart.ItemsSource = ChangeListMaterial(row.ОбозначениеDocMgr, row.Конфигурация, material.MatName, _bomPartsList.Where(x => x.Обозначение == row.ОбозначениеDocMgr).ToList()).ToList();

            FilterBomTable();

        }

        #endregion

        #region ToDelete

        void BeginUpdatePartProp_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialsList.SelectedValue == null) return;
            var material = ((ToSQL.ColumnMatProps)(MaterialsList.SelectedValue));
            if (BomTablePrt.SelectedValue == null) return;
            var row = ((BomPartListClass.BomCells)(BomTablePrt.SelectedValue));
            SetMaterialsProperty(row.Путь, row.Конфигурация, Convert.ToInt32(material.LevelID));
            GetBomList();
            ChangedDataGrid(false);
        }

       

        void SetMaterialsProperty(string partPath, string partConfig, int materialId)
        {
            var pdmBase = Settings.Default.PdmBaseName;
            MessageBox.Show(pdmBase);
            // Test
            //partPath = @Шляхи.Text;
            //pdmBase = "Tets_debag";

            try
            {
                if (!_modelSwClass.IsSheetMetalPart(partPath, pdmBase))
                {
                    //MessageBox.Show("Нелистовая деталь");
                    return;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка во время определения листовой детали" + e.Message, partPath);
            }

            try
            {
                _modelSwClass.CheckInOutPdm(partPath, false, pdmBase);
            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка во время разрегистрации " + e.Message, partPath);
            }

            try
            {
                _swMaterials.ApplyMaterial(partPath, partConfig, materialId, null);
            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка во время применения материала " + e.Message, partPath);
            }

            finally
            {
                try
                {
                    _modelSwClass.CheckInOutPdm(partPath, true, pdmBase);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Ошибка во время регистрации " + e.Message, partPath);
                }
            }
        }

        private List<SwDocMgr.PartProperties> strList;

        #endregion

        private void BeginUpdate1_Click(object sender, RoutedEventArgs args)
        {
            try
            {
              //  _bomClass.AcceptAllChanges(_bomPartsListOriginal, _bomPartsList);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            ChangedDataGrid(false);
        }

        

        private void UpdateCutList_Click(object sender, RoutedEventArgs e)
        {
            BeginUpdateButton();
        }

        private void MakeDxf_Click(object sender, RoutedEventArgs e)
        {
            BeginUpdateButton();
        }
        

         void FindAsm()
        {
            try
            {
                var allAsmInPdmFind = _allAsmInPdm.ToList();
                allAsmInPdmFind = allAsmInPdmFind.Where(x => x.Contains(NamePdm.Text)).ToList();
                AsmNames.ItemsSource = allAsmInPdmFind;
                AsmNames.SelectedIndex = 0;
                AsmNames.IsDropDownOpen = true;
            }
            catch (Exception)
            {
                //
            }
        }
        
        void namePdm_TextChanged(object sender, TextChangedEventArgs e)
        {
            FindAsm();
        }

        void AsmNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NamePdm.Text = AsmNames.Text;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var  pdmDll = new PDM {vaultname = "Vents-PDM"};

            MessageBox.Show(pdmDll.SearchDoc("02-05-50"));
        }
    }
}
