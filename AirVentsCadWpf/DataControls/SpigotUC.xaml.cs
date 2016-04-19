using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using AirVentsCadWpf.Properties;
using System.ServiceModel;
using System.Xml;

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

            #region VentsCadService

            //try
            //{   
            //    using (var client = new VentsCadService.VentsCadServiceClient(App.Service.Binding, App.Service.Address))
            //    {
            //        int idPdm; int projId;
            //        client.Open();
            //        MessageBox.Show(client.BuildSpigot(TypeOfSpigot.Text, WidthSpigot.Text, HeightSpigot.Text, out idPdm).ToString());
            //        client.Close();
            //    }
            //}
            //catch (Exception ex)
            //{
            //   // MessageBox.Show(App.Service.Address.ToString());
            //    MessageBox.Show(ex.Message);
            //}

            #endregion

            VentsCadLibrary.VentsCad.ProductFactory serviceObj = new VentsCadLibrary.VentsCad.ProductFactory(new[] { "spigot", TypeOfSpigot.Text, WidthSpigot.Text, HeightSpigot.Text } );

            MessageBox.Show(serviceObj.product.Exist.ToString());

            MessageBox.Show(serviceObj.product.Place?.IdPdm.ToString());

            serviceObj.product.Build();

            MessageBox.Show(serviceObj.product.Exist.ToString());
            MessageBox.Show(serviceObj.product.Place?.IdPdm.ToString());
            return;

            #region VentsCadLibrary         

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
                    MessageBox.Show(place.Path + "\n" + place.IdPdm + "\n" + place.ProjectId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            #endregion

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
