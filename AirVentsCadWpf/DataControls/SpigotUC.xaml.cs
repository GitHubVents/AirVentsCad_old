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
            VentsCadService.VentsCadServiceClient sdfb = new VentsCadService.VentsCadServiceClient();
            MessageBox.Show(sdfb.GetData(8));
            return;

            try
            {              
                using (var server = new VentsCadLibrary.VentsCad())
                {
                    var newSpigot = new VentsCadLibrary.VentsCad.Spigot(TypeOfSpigot.Text, WidthSpigot.Text, HeightSpigot.Text);
                    if (!newSpigot.Exist)
                    {
                        newSpigot.Build();
                    }
                    var place = newSpigot.GetPlace();
                    MessageBox.Show(place.Path +"\n" +place.IdPdm + "\n" + place.ProjectId);

                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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
