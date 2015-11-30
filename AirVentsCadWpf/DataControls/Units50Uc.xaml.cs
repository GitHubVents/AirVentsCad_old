using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AirVentsCadWpf.DataControls
{
    /// <summary>
    /// Interaction logic for Units50Uc.xaml
    /// </summary>
    public partial class    Units50Uc
    {
        public Units50Uc()
        {
            InitializeComponent();
            Switcher.SwitcherUnits50Uc = this;
            
          //  Switcher.SwitchUnits(new UnitElement());
            Switcher.SwitchUnitData(new MonoBlock01Uc50
            {
                TitleUnit = { Visibility = Visibility.Collapsed },
                Build = { Visibility = Visibility.Collapsed },
                SizeOfUnit = { SelectedIndex = SizeOfUnit.SelectedIndex },
               
                     
            });
        }

        public void NavigateData(UserControl nextPage)
        {
            GridUnitData.Children.Clear();
            GridUnitData.Children.Add(nextPage);
        }

        //public void NavigateUnits(UserControl nextPage)
        //{
        //    GridViewUnits.Children.Clear();
        //    GridViewUnits.Children.Add(nextPage);
        //}

        
        private void BUILDING_Click(object sender, RoutedEventArgs e)
        {
            GridViewUnits.Children.Clear();
            GridViewUnits.Children.Add(new UnitElement { SizeOfUnitIndex = SizeOfUnit.SelectedIndex });





        }

        private void HeightU_KeyDown(object sender, KeyEventArgs e)
        {
           
        }

        private void SizeOfUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
        }

        private void nonstandard_Checked(object sender, RoutedEventArgs e)
        {
           
        }

        private void nonstandard_Unchecked(object sender, RoutedEventArgs e)
        {
            
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
           
        }

        private void WidthU_KeyDown(object sender, KeyEventArgs e)
        {
           
        }

        private void Lenght_KeyDown(object sender, KeyEventArgs e)
        {
           
        }
    }
}
