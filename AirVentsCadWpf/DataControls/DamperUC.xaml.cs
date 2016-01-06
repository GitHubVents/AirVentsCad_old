using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AirVentsCadWpf.AirVentsClasses;
using AirVentsCadWpf.Properties;
using VentsMaterials;
using ModelSw = AirVentsCadWpf.AirVentsClasses.UnitsBuilding.ModelSw;

namespace AirVentsCadWpf.DataControls
{
    /// <summary>
    /// Interaction logic for DamperUC.xaml
    /// </summary>
    public partial class DamperUc
    {
        readonly SqlBaseData _sqlBaseData = new SqlBaseData();
        readonly SetMaterials _setMaterials = new SetMaterials();
        readonly ToSQL _toSql = new ToSQL();


        /// <summary>
        /// Initializes a new instance of the <see cref="DamperUc"/> class.
        /// </summary>
        public DamperUc()
        {
            InitializeComponent();

            ToSQL.Conn = Settings.Default.ConnectionToSQL;

            MaterialP1.ItemsSource = ((IListSource)_sqlBaseData.MaterialsTable()).GetList();
            MaterialP1.DisplayMemberPath = "MaterialsName";
            MaterialP1.SelectedValuePath = "LevelID";
            MaterialP1.SelectedIndex = 0;


            ТолщинаВнешней.ItemsSource = new List<ComboBoxItem>
            {
                new ComboBoxItem {Content = "0.5"},
                new ComboBoxItem {Content = "0.6"},
                new ComboBoxItem {Content = "0.8"},
                new ComboBoxItem {Content = "1.0"},
                new ComboBoxItem {Content = "1.2"}
            };
            ТолщинаВнешней.SelectedIndex = 2;
        }

        void BuildDamper_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //#region

                //var mat1Code = "";
                //var viewRowMat1 = (DataRowView)MaterialP1.SelectedItem;
                //var row1 = viewRowMat1.Row;
                //if (row1 != null)
                //    mat1Code = row1.Field<string>("CodeMaterial");
                //var materialP1 = new[] { MaterialP1.SelectedValue.ToString(), ТолщинаВнешней.Text, MaterialP1.Text, mat1Code };

                //#endregion

                //var vcad = new VentsCadLibrary.VentsCad
                //{
                //    ConnectionToSql = Settings.Default.ConnectionToSQL,
                //    DestVaultName = Settings.Default.TestPdmBaseName,
                //    VaultName = Settings.Default.PdmBaseName
                //};

                //string unit;
                //vcad.DumperS(TypeOfDumper.Text, WidthDamper.Text, HeightDamper.Text, (IsOutDoor.IsChecked == true), materialP1, out unit, false);

                //MessageBox.Show("Finish");

                //return;

                var mat1Code = "";
                var viewRowMat1 = (DataRowView)MaterialP1.SelectedItem;
                var row1 = viewRowMat1.Row;
                if (row1 != null)
                    mat1Code = row1.Field<string>("CodeMaterial");
                var materialP1 = new[] { MaterialP1.SelectedValue.ToString(), ТолщинаВнешней.Text, MaterialP1.Text, mat1Code };

                var sw = new ModelSw();
                sw.Dumper(TypeOfDumper.Text, WidthDamper.Text, HeightDamper.Text, (IsOutDoor.IsChecked == true), materialP1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace, ex.Message);
            }
        }

        void WidthDamper_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuildDamper_Click(this, new RoutedEventArgs());
            }
        }

        void HeightDamper_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuildDamper_Click(this, new RoutedEventArgs());
            }
        }

        void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
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
    }
}
