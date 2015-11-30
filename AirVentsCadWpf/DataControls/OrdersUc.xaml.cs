using System.Windows;
using AirVentsCadWpf.AirVentsClasses;
using AirVentsCadWpf.DataControls.FrameLessUnit;

namespace AirVentsCadWpf.DataControls
{
    /// <summary>
    /// Interaction logic for OrdersUc.xaml
    /// </summary>
    partial class OrdersUc
    {
        readonly SqlBaseData _sqlBaseData = new SqlBaseData();
     
        /// <summary>
        /// Initializes a new instance of the <see cref="FrameLessUnit.FramelessUnitUc"/> class.
        /// </summary>
        public OrdersUc()
            {
            InitializeComponent();


            FinishDay.Text = "FinishDay";
            }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
          //  throw new System.NotImplementedException();
        }
    }
}
