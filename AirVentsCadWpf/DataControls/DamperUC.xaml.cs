using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using AirVentsCadWpf.Properties;
using ModelSw = AirVentsCadWpf.AirVentsClasses.UnitsBuilding.ModelSw;

namespace AirVentsCadWpf.DataControls
{
    /// <summary>
    /// Interaction logic for DamperUC.xaml
    /// </summary>
    public partial class DamperUc
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DamperUc"/> class.
        /// </summary>
        public DamperUc()
        {
            InitializeComponent();
        }

        void BuildDamper_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                //var vcad = new VentsCadLibrary.VentsCad
                //{
                //    ConnectionToSql = Settings.Default.ConnectionToSQL,
                //    DestVaultName = Settings.Default.TestPdmBaseName,
                //    VaultName = Settings.Default.PdmBaseName
                //};

                //string unit;
                //vcad.DumperS(TypeOfDumper.Text, WidthDamper.Text, HeightDamper.Text, (IsOutDoor.IsChecked == true), out unit, false);

                var sw = new ModelSw();
                sw.Dumper(TypeOfDumper.Text, WidthDamper.Text, HeightDamper.Text, (IsOutDoor.IsChecked == true));
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
    }
}
