using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AirVentsCadWpf.AirVentsClasses;
using AirVentsCadWpf.Properties;
using VentsMaterials;
using ModelSw = AirVentsCadWpf.AirVentsClasses.UnitsBuilding.ModelSw;

namespace AirVentsCadWpf.DataControls
{
    /// <summary>
    /// Interaction logic for MontageFrameUC.xaml
    /// </summary>
    public partial class MontageFrameUc
    {
        readonly SqlBaseData _sqlBaseData = new SqlBaseData();
        readonly SetMaterials _setMaterials = new SetMaterials();
        readonly ToSQL _toSql = new ToSQL();

        /// <summary>
        /// Initializes a new instance of the <see cref="MontageFrameUc"/> class.
        /// </summary>
        public MontageFrameUc()
        {
            InitializeComponent();

            MaterialMontageFrame.ItemsSource = ((IListSource)_sqlBaseData.MaterialsForMontageFrame()).GetList();
            MaterialMontageFrame.DisplayMemberPath = "MaterialsName";
            MaterialMontageFrame.SelectedValuePath = "LevelID"; //"CodeMaterial";
            MaterialMontageFrame.SelectedIndex = 0;

            #region MontageFrame50 Initialize

            FrameOffset.MaxLength = 5;
            FrameOffset.IsReadOnly = true;
            
            LenghtBaseFrame.MaxLength = 5;
            WidthBaseFrame.MaxLength = 5;

            #endregion

            ToSQL.Conn = Settings.Default.ConnectionToSQL;

            Ral1.ItemsSource = ((IListSource)_toSql.RalTable()).GetList();
            Ral1.DisplayMemberPath = "RAL";
            Ral1.SelectedValuePath = "Hex";
            Ral1.SelectedIndex = 0;

            Ral1.Visibility = Visibility.Hidden;

            CoatingType1.ItemsSource = ((IListSource)_setMaterials.CoatingTypeDt()).GetList();
            CoatingType1.DisplayMemberPath = "Name";
            CoatingType1.SelectedValuePath = "Code";
            CoatingType1.SelectedIndex = 0;

            CoatingClass1.ItemsSource = _setMaterials.CoatingListClass();
        }

        void BUILDING_Click(object sender, RoutedEventArgs e)
        {
            if (LenghtBaseFrame.Text == "") return;

            #region
            //var typeOfFrameCopyValue =
            //   TypeOfFrame.SelectedItem.ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "");

            //if (typeOfFrameCopyValue != "3")
            //{
            //    FrameOffset.Text = "";
            //}
            #endregion

            if (FrameOffset.Text == "")
            {
                try
                {
                    FrameOffset.Text = Convert.ToString((Convert.ToDouble(LenghtBaseFrame.Text) / 2));
                }
                catch (Exception)
                {
                    FrameOffset.Text = Convert.ToString((Convert.ToDouble(LenghtBaseFrame.Text) / 2));
                }
            }

            var sw = new ModelSw();

            sw.MontageFrame(
                WidthBaseFrame.Text,
                LenghtBaseFrame.Text,
                Thikness.Text,
                TypeOfFrame.Text,
                FrameOffset.Text,
                MaterialMontageFrame.SelectedValue.ToString(),
                new[]
                {
                    Ral1.Text, CoatingType1.Text, CoatingClass1.Text,
                    Ral1.SelectedValue?.ToString() ?? ""
                });

            FrameOffset.Text = "";
        }

        void FrameOffset_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BUILDING_Click(this, new RoutedEventArgs());
            }
        }

        void LenghtBaseFrame_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BUILDING_Click(this, new RoutedEventArgs());
            }
        }

        void WidthBaseFrame_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BUILDING_Click(this, new RoutedEventArgs());
            }
        }
       
        void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        void TypeOfFrame_Copy_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var typeOfFrameCopyValue =
               TypeOfFrame.SelectedItem.ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "");
            if (FrameOffsetLabel == null || FrameOffset == null)
            {
                return;
            }
            if (typeOfFrameCopyValue != "3")
            {
                FrameOffsetLabel.Visibility = Visibility.Visible;
                FrameOffset.IsEnabled = false;
                FrameOffset.Visibility = Visibility.Visible;
                FrameOffset.Text = "";
            }
            else
            {
                FrameOffset.IsReadOnly = false;
                FrameOffsetLabel.Visibility = Visibility.Visible;
                FrameOffset.IsEnabled = true;
                FrameOffset.Visibility = Visibility.Visible;
            }
        }
        
        void TypeOfFrame_LayoutUpdated(object sender, EventArgs e)
        {
            const string picturePath = @"\DataControls\Pictures\Монтажная рама\";
            var pictureName = "10-4-800-650.PNG";
            switch (TypeOfFrame.Text)
            {
                case "1":
                    pictureName = "10-4-800-650-01.PNG";
                    break;
                case "2":
                    pictureName = "10-4-800-650-02.PNG";
                    break;
                case "3":
                    pictureName = "10-4-800-1000-03.PNG";
                    break;
            }
            var mapLoader = new BitmapImage();
            mapLoader.BeginInit();
            mapLoader.UriSource = new Uri(picturePath + pictureName, UriKind.RelativeOrAbsolute);
            mapLoader.EndInit();
            if (PictureMf != null) PictureMf.Source = mapLoader;
        }

        void MaterialMontageFrame_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Thikness.Items.Clear();
            switch (MaterialMontageFrame.SelectedIndex)
            {               
                case 0:
                    Thikness.Items.Add("2");
                    break;
                case 1:
                    Thikness.Items.Add("2");
                    Thikness.Items.Add("3");
                    break;
                case 2:
                    Thikness.Items.Add("4");                    
                    break;
                default:
                    MaterialMontageFrame.SelectedIndex = -1;
                    break;
            }
            Thikness.SelectedIndex = 0;
        }

        private void Ral1_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Ral1 == null) return;
            if (Ral1.SelectedIndex == 0)
            {
                CoatingType1.Visibility = Visibility.Hidden;
                CoatingClass1.Visibility = Visibility.Hidden;
            }
            else
            {
                CoatingType1.Visibility = Visibility.Visible;
                CoatingClass1.Visibility = Visibility.Visible;
            }
        }
    }
}
