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

            var _binding = new BasicHttpBinding
            {
                ReceiveTimeout = TimeSpan.FromMinutes(15),
                SendTimeout = TimeSpan.FromMinutes(15),
                MaxBufferPoolSize = 2147483647, // 2147483647
                MaxBufferSize = 2147483647,
                MaxReceivedMessageSize = 2147483647, // 2147483647
                Name = "BasicHttpBinding_IVentsCadService"
            };

            var myReaderQuotas = new XmlDictionaryReaderQuotas();
            myReaderQuotas.MaxStringContentLength = 2147483647;
            myReaderQuotas.MaxArrayLength = 2147483647;
            myReaderQuotas.MaxBytesPerRead = 2147483647;
            myReaderQuotas.MaxDepth = 2000000;
            myReaderQuotas.MaxNameTableCharCount = 2147483647;

            _binding.GetType().GetProperty("ReaderQuotas").SetValue(_binding, myReaderQuotas, null);

            var _address = new EndpointAddress("http://localhost:8000/hello");

            using (var client = new VentsCadService.VentsCadServiceClient(_binding, _address))
            {
                int idPdm;
                int projId;

                client.Open();

                MessageBox.Show(client.BuildSpigot(TypeOfSpigot.Text, WidthSpigot.Text, HeightSpigot.Text, out idPdm).ToString());

                client.Close();
            }

            #region VentsCadService

            //    VentsCadService.VentsCadServiceClient sdfb = new VentsCadService.VentsCadServiceClient();
            //int idPdm;
            //int projId;
            //MessageBox.Show(sdfb.BuildSpigot(TypeOfSpigot.Text, WidthSpigot.Text, HeightSpigot.Text, out idPdm).ToString());
            //return;

            //try
            //{              
            //    using (var server = new VentsCadLibrary.VentsCad())
            //    {
            //        var newSpigot = new VentsCadLibrary.VentsCad.Spigot(TypeOfSpigot.Text, WidthSpigot.Text, HeightSpigot.Text);
            //        if (!newSpigot.Exist)
            //        {
            //            newSpigot.Build();
            //        }
            //        var place = newSpigot.GetPlace();
            //        MessageBox.Show(place.Path +"\n" +place.IdPdm + "\n" + place.ProjectId);            

            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}

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
