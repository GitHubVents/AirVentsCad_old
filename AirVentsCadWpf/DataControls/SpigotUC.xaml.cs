using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using AirVentsCadWpf.Properties;

namespace AirVentsCadWpf.DataControls
{
    /// <summary>
    /// Interaction logic for SpigotUC.xaml
    /// </summary>
    public partial class SpigotUc
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpigotUc"/> class.
        /// </summary>
        public SpigotUc()
        {
            InitializeComponent();
        }

        void BuildSpigot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var vcad = new VentsCadLibrary.VentsCad
                {
                    ConnectionToSql = Settings.Default.ConnectionToSQL,
                    DestVaultName = Settings.Default.TestPdmBaseName,
                    VaultName = Settings.Default.PdmBaseName
                };

                string unit;
                vcad.Spigot(TypeOfSpigot.Text, WidthSpigot.Text, HeightSpigot.Text, out unit);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void Grid_Loaded_1(object sender, RoutedEventArgs e)
        {
            //var sw = new ModelSw();
            //sw.CreateDistDirectory(string.Format(@"{0}\{1}", @Properties.Settings.Default.DestinationFolder, sw.SpigotDestinationFolder));
        }

        void WidthSpigot_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuildSpigot_Click(this, new RoutedEventArgs());
            }
        }

        void HeightSpigot_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuildSpigot_Click(this, new RoutedEventArgs());
            }
        }

        void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
