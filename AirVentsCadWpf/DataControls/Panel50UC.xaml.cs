using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AirVentsCadWpf.AirVentsClasses;
using AirVentsCadWpf.AirVentsClasses.UnitsBuilding;
using AirVentsCadWpf.Properties;
using VentsMaterials;

//using ModelSw = AirVentsCadWpf.AirVentsClasses.UnitsBuilding.ModelSw;

namespace AirVentsCadWpf.DataControls
{
    /// <summary>
    /// Interaction logic for Panel50UC.xaml
    /// </summary>
    public partial class Panel50Uc
    {

        readonly SqlBaseData _sqlBaseData = new SqlBaseData();
        readonly SetMaterials _setMaterials = new SetMaterials();
        readonly ToSQL _toSql = new ToSQL();

        /// <summary>
        /// Initializes a new instance of the <see cref="Panel50Uc"/> class.
        /// </summary>
        public Panel50Uc()
        {
            InitializeComponent();

            ToSQL.Conn = Settings.Default.ConnectionToSQL;

            ТолщинаВнешней.ItemsSource = new List<ComboBoxItem>
            {
                new ComboBoxItem {Content = "0.5"},
                new ComboBoxItem {Content = "0.6"},
                new ComboBoxItem {Content = "0.8"},
                new ComboBoxItem {Content = "1.0"},
                new ComboBoxItem {Content = "1.2"}
            };
            ТолщинаВнешней.SelectedIndex = 2;

            ТолщинаВннутренней.ItemsSource = new List<ComboBoxItem>
            {
                new ComboBoxItem {Content = "0.5"},
                new ComboBoxItem {Content = "0.6"},
                new ComboBoxItem {Content = "0.8"},
                new ComboBoxItem {Content = "1.0"},
                new ComboBoxItem {Content = "1.2"}
            };
            ТолщинаВннутренней.SelectedIndex = 2;

            TypeOfPanel50.ItemsSource = ((IListSource)_sqlBaseData.PanelsTable()).GetList();
            TypeOfPanel50.DisplayMemberPath = "PanelTypeName";
            TypeOfPanel50.SelectedValuePath = "PanelTypeCode";
            TypeOfPanel50.SelectedIndex = 0;

            MaterialP1.ItemsSource = ((IListSource)_sqlBaseData.MaterialsTable()).GetList();
            MaterialP1.DisplayMemberPath = "MaterialsName";
            MaterialP1.SelectedValuePath = "LevelID";
            MaterialP1.SelectedIndex = 0;

            MaterialP2.ItemsSource = ((IListSource)_sqlBaseData.MaterialsTable()).GetList();
            MaterialP2.DisplayMemberPath = "MaterialsName";
            MaterialP2.SelectedValuePath = "LevelID";
            MaterialP2.SelectedIndex = 0;

            Ral1.ItemsSource = ((IListSource)_toSql.RalTable()).GetList();
            Ral1.DisplayMemberPath = "RAL";
            Ral1.SelectedValuePath = "Hex";
            Ral1.SelectedIndex = 0;

            Ral2.ItemsSource = ((IListSource)_toSql.RalTable()).GetList();
            Ral2.DisplayMemberPath = "RAL";
            Ral2.SelectedValuePath = "Hex";
            Ral2.SelectedIndex = 0;
            
            Ral1.Visibility = Visibility.Hidden;
            Ral2.Visibility = Visibility.Hidden;

            CoatingType1.ItemsSource = ((IListSource)_setMaterials.CoatingTypeDt()).GetList();
            CoatingType1.DisplayMemberPath = "Name";
            CoatingType1.SelectedValuePath = "Code";
            CoatingType1.SelectedIndex = 0;

            CoatingType2.ItemsSource = ((IListSource)_setMaterials.CoatingTypeDt()).GetList();
            CoatingType2.DisplayMemberPath = "Name";
            CoatingType2.SelectedValuePath = "Code";
            CoatingType2.SelectedIndex = 0;

            CoatingClass1.ItemsSource = _setMaterials.CoatingListClass();
            CoatingClass2.ItemsSource = _setMaterials.CoatingListClass();
        }

        void BUILDING_Click(object sender, RoutedEventArgs e)
        {
            
            var mat1Code = "";
            var mat2Code = "";

            var viewRowMat1 = (DataRowView)MaterialP1.SelectedItem;
            var row1 = viewRowMat1.Row;
            if (row1 != null)
                mat1Code = row1.Field<string>("CodeMaterial");
            var viewRowMat2 = (DataRowView)MaterialP2.SelectedItem;
            var row2 = viewRowMat2.Row;
            if (row2 != null)
                mat2Code = row2.Field<string>("CodeMaterial");
            
            var materialP1 = new[] { MaterialP1.SelectedValue.ToString(), ТолщинаВнешней.Text, MaterialP1.Text, mat1Code };
            var materialP2 = new[] { MaterialP2.SelectedValue.ToString(), ТолщинаВннутренней.Text, MaterialP2.Text, mat2Code };

            var thicknessOfPanel = ((ComboBoxItem)TypeOfPanel.SelectedItem).Content.ToString().Remove(2);

            //MessageBox.Show(thicknessOfPanel);

            #region VentsCadLibrary

            //MessageBox.Show(Ral1.SelectedValue + " " + CoatingType1.Text, CoatingClass1.Text);

            //var vcad = new VentsCadLibrary.VentsCad
            //{
            //    ConnectionToSql = Settings.Default.ConnectionToSQL,
            //    DestVaultName = Settings.Default.TestPdmBaseName,
            //    VaultName = Settings.Default.PdmBaseName
            //};

            //MessageBox.Show(TypeOfPanel50.SelectedValue 
            //    + " \n " + TypeOfPanel50.Text
            //    + " \n " + Ral1.Text
            //    + " \n " + CoatingType1.Text
            //    + " \n " + CoatingClass1.Text
            //    );

            //vcad.Panels50(
            //    typeOfPanel: new[] { TypeOfPanel50.SelectedValue.ToString(), TypeOfPanel50.Text },
            //    width: WidthPanel.Text,
            //    lenght: HeightPanel.Text,
            //    materialP1: materialP1,
            //    materialP2: materialP2,
            //    покрытие: new[]
            //    {
            //        Ral1.Text, CoatingType1.Text, CoatingClass1.Text,
            //        Ral2.Text, CoatingType2.Text, CoatingClass2.Text,
            //        Ral1.SelectedValue?.ToString() ?? "",
            //        Ral2.SelectedValue?.ToString() ?? ""
            //    },
            //    onlyPath: false);

            #endregion

            var sw = new ModelSw();

            switch (thicknessOfPanel)
            {
                case "30":
                    string path;
                    sw.Panels30Build(
                        typeOfPanel:
                            new[] {TypeOfPanel50.SelectedValue.ToString(), TypeOfPanel50.Text, thicknessOfPanel},
                        width: WidthPanel.Text,
                        height: HeightPanel.Text,
                        materialP1: materialP1,
                        materialP2: materialP2,
                        покрытие: new[]
                        {
                            Ral1.Text, CoatingType1.Text, CoatingClass1.Text,
                            Ral2.Text, CoatingType2.Text, CoatingClass2.Text,
                            Ral1.SelectedValue?.ToString() ?? "",
                            Ral2.SelectedValue?.ToString() ?? ""
                        },
                        path: out path);
                    break;
                case "50":
                case "70":
                    sw.Panels50Build(
               typeOfPanel: new[] { TypeOfPanel50.SelectedValue.ToString(), TypeOfPanel50.Text, thicknessOfPanel },
               width: WidthPanel.Text,
               height: HeightPanel.Text,
               materialP1: materialP1,
               meterialP2: materialP2,
               покрытие: new[]
               {
                    Ral1.Text, CoatingType1.Text, CoatingClass1.Text,
                    Ral2.Text, CoatingType2.Text, CoatingClass2.Text,
                    Ral1.SelectedValue?.ToString() ?? "",
                    Ral2.SelectedValue?.ToString() ?? ""
               });
                    break;
            }
        }

        void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        void HeightPanel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BUILDING_Click(this, new RoutedEventArgs());
            }
        }

        void WidthPanel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BUILDING_Click(this, new RoutedEventArgs());
            }
        }

        void UserControl_Loaded_1(object sender, RoutedEventArgs e)
        {
            HeightPanel.MaxLength = 5;
            WidthPanel.MaxLength = 5;
        }

        void TypeOfPanel50_LayoutUpdated(object sender, EventArgs e)
        {
            const string picturePath = @"\DataControls\Pictures\Панели\";
            var pictureName = "02-01-500-750-50-Az-Az-MW.JPG";
            switch (TypeOfPanel50.Text)
            {
                case "Съемная":
                    pictureName = "02-04-500-750-50-Az-Az-MW.JPG";
                    break;
                case "Панель теплообменника":
                    pictureName = "02-05-500-750-50-Az-Az-MW.JPG";
                    break;
                case "Панель двойная":
                    pictureName = "02-01-1000-750-50-Az-Az-MW.JPG";
                    break;
            }
            var mapLoader = new BitmapImage();
            mapLoader.BeginInit();
            mapLoader.UriSource = new Uri(picturePath + pictureName, UriKind.RelativeOrAbsolute);
            mapLoader.EndInit();
            if (PicturePanel != null) PicturePanel.Source = mapLoader;
        }
       

        private void Ral1_LayoutUpdated(object sender, EventArgs e)
        {
            if (Ral1.Text == "Без покрытия")
            {
                CoatingType1.Visibility = Visibility.Collapsed;
                CoatingClass1.Visibility = Visibility.Collapsed;
            }
            else
            {
                CoatingType1.Visibility = Visibility.Visible;
                CoatingClass1.Visibility = Visibility.Visible;
            }
        }

        private void Ral2_LayoutUpdated(object sender, EventArgs e)
        {
            if (Ral2.Text == "Без покрытия")
            {
                CoatingType2.Visibility = Visibility.Hidden;
                CoatingClass2.Visibility = Visibility.Hidden;
            }
            else
            {
                CoatingType2.Visibility = Visibility.Visible;
                CoatingClass2.Visibility = Visibility.Visible;
            }
        }

        private void MaterialP1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MaterialP1 == null) return;
            if (MaterialP1.SelectedIndex == 0)
            {
                ТолщинаВнешнейLbl.Visibility = Visibility.Hidden;
                ТолщинаВнешней.Visibility = Visibility.Hidden;
            }
            else
            {
                ТолщинаВнешнейLbl.Visibility = Visibility.Visible;
                ТолщинаВнешней.Visibility = Visibility.Visible;
            }
        }

        private void MaterialP2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MaterialP2 == null) return;
            if (MaterialP2.SelectedIndex == 0)
            {
                ТолщинаВннутреннейLbl.Visibility = Visibility.Hidden;
                ТолщинаВннутренней.Visibility = Visibility.Hidden;
            }
            else
            {
                ТолщинаВннутреннейLbl.Visibility = Visibility.Visible;
                ТолщинаВннутренней.Visibility = Visibility.Visible;
            }
        }
                
    }
}
