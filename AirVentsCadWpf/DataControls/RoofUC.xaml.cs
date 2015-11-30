using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ModelSw = AirVentsCadWpf.AirVentsClasses.UnitsBuilding.ModelSw;

namespace AirVentsCadWpf.DataControls
{
    /// <summary>
    /// Interaction logic for RoofUC.xaml
    /// </summary>
    public partial class RoofUc
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoofUc"/> class.
        /// </summary>
        public RoofUc()
        {
            InitializeComponent();
        }
        
        //private static readonly Logger Logger = LogManager.GetLogger("RoofUC");

        void BuildRoof_Click(object sender, RoutedEventArgs e)
        {
           // MessageBox.Show(TypeOfRoof.Text);
            try
            {
                //        
                var sw = new ModelSw();
                sw.Roof(TypeOfRoof.Text, WidthRoof.Text, LenghtRoof.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //Logger.Log(LogLevel.Error, string.Format("Ошибка при генерации крыши: тип - {0}, ширина - {1}, высота - {2}", TypeOfRoof.Text, WidthRoof.Text, LenghtRoof.Text), ex);
            }
        }

        void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        void WidthRoof_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuildRoof_Click(this, new RoutedEventArgs());
            }
        }

        void LenghtRoof_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuildRoof_Click(this, new RoutedEventArgs());
            }
        }

        void Grid_Loaded_1(object sender, RoutedEventArgs e)
        {
            //var sw = new ModelSw();
            //sw.CreateDistDirectory(string.Format(@"{0}\{1}", @Properties.Settings.Default.DestinationFolder, sw.RoofDestinationFolder));
        }

        void TypeOfRoof_LayoutUpdated(object sender, EventArgs e)
        {
            const string picturePath = @"\DataControls\Pictures\Крыша\";
            var pictureName = "15-01-800-1100.JPG";
            switch (TypeOfRoof.Text)
            {
                case "2":
                    pictureName = "15-02-800-1100.JPG";
                    break;
                case "3":
                    pictureName = "15-03-800-1100.JPG";
                    break;
                case "4":
                    pictureName = "15-04-800-1100.JPG";
                    break;
            }
            var mapLoader = new BitmapImage();
            mapLoader.BeginInit();
            mapLoader.UriSource = new Uri(picturePath + pictureName, UriKind.RelativeOrAbsolute);
            mapLoader.EndInit();
            if (PictureRoof != null) PictureRoof.Source = mapLoader;
        }
    }
}
