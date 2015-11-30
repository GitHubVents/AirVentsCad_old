using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AirVentsCadWpf.DataControls
{
    /// <summary>
    /// Interaction logic for UnitElement.xaml
    /// </summary>
    public partial class UnitElement
    {
        public UnitElement()
        {
            InitializeComponent();
           // _uc.Lenght.TextChanged += LenghtOnTextChanged; 
           // WidthUnit.Content = Unit.Width.ToString();
            _uc.TitleUnit.Visibility = Visibility.Collapsed;
            _uc.Build.Visibility = Visibility.Collapsed;

            if (GridR.Children.Contains(_uc) == false)
            {
                BorderR.Visibility = Visibility.Visible;
                //MessageBox.Show("ItIS");
            }
        }


        public int SizeOfUnitIndex ;

        private static void LenghtOnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
           
        }

        readonly MonoBlock01Uc50 _uc = new MonoBlock01Uc50();

        private static void TypeOfFrameEvent(object sender, SelectionChangedEventArgs e)
        {
            MessageBox.Show("Working");
        }


        private void Border_MouseDown_1(object sender, MouseButtonEventArgs e)
        {

            GridR.Children.Clear();
         //   GridR.ColumnDefinitions.Add(new ColumnDefinition());
            GridR.Children.Add(new UnitElement
            {
             // BorderR = { Visibility = Visibility.Collapsed },
              SizeOfUnitIndex = SizeOfUnitIndex,
              GridL = {Visibility = Visibility.Collapsed},
              GridForm = { Visibility = Visibility.Collapsed }
            });
            BorderR.Visibility = Visibility.Collapsed;
            
        }

        private void Border_MouseDown_2(object sender, MouseButtonEventArgs e)
        {
            GridL.Children.Clear();
            GridL.Children.Add(new UnitElement
            {
                SizeOfUnitIndex = SizeOfUnitIndex ,
                GridR = { Visibility = Visibility.Collapsed },
                GridForm = { Visibility = Visibility.Collapsed }
            });
        }

        private void Border_MouseDown_3(object sender, MouseButtonEventArgs e)
        {
            //_uc.TypeOfFrame.SelectedIndex = 3;// = new uc { TypeOfFrame = { SelectedIndex = 3 } };

          //  MessageBox.Show("sdfg");

            _uc.SizeOfUnit.SelectedIndex = SizeOfUnitIndex;
            Dispatcher.InvokeAsync(() => Switcher.SwitchUnitData(_uc));

            //Switcher.SwitchUnitData(new MonoBlock01Unit50Uc
            //{
            //    TitleUnit = { Visibility = Visibility.Collapsed },
            //    Build = { Visibility = Visibility.Collapsed }
            //});
        }

        private void Unit_MouseEnter(object sender, MouseEventArgs e)
        {
            Unit.Background = Brushes.Green;
        }

        private void Unit_MouseLeave(object sender, MouseEventArgs e)
        {
            Unit.Background = Brushes.AliceBlue;
            var menu = new ContextMenu();
            menu.Items.Add("Delete");
            menu.Visibility = Visibility.Visible;
        }

        private void UserControl_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
            //Double.NaN size = GridForm.Width;
            var lenght = string.Format("{0}", Convert.ToString(GridForm.Width));
            // MessageBox.Show(lenght);
            WidthUnitS.Content = lenght;

            if (GridR.Children.Contains(_uc) == false)
            {
                BorderR.Visibility = Visibility.Visible;
                //MessageBox.Show("ItIS");
            }
        }

        private void Unit_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var menu = new ContextMenu();
            menu.Items.Add("Delete");
            menu.Visibility = Visibility.Visible;
        }

       

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (
                MessageBox.Show("Удалить блок?", "", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) ==
                MessageBoxResult.Yes)
            {
                Visibility = Visibility.Collapsed;
            }
            BorderR.Visibility = Visibility.Visible;
        }

        private void Unit_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (GridR.Children.Contains(_uc) == false)
            {
                BorderR.Visibility = Visibility.Visible;
                //MessageBox.Show("ItIS");
            }
        }
    }
}
