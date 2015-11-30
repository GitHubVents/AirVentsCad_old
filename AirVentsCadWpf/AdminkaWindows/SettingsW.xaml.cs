using System;
using System.Diagnostics;
using System.Windows;
using AirVentsCadWpf.Properties;
using EdmLib;
using LoggerUc = AirVentsCadWpf.DataControls.Loggers.LoggerUc;
using AirVentsCadWpf.Логирование;

namespace AirVentsCadWpf.AdminkaWindows
{
    /// <summary>
    /// Interaction logic for SettingsW.xaml
    /// </summary>
    public partial class SettingsW
    {
        /// <summary>
        /// 
        /// </summary>
        public SettingsW()
        {
            InitializeComponent();
            //DestBase.Visibility = Visibility.Collapsed;
            ResizeMode = ResizeMode.NoResize;
            SizeToContent = SizeToContent.WidthAndHeight;

          //  SQLBase.ItemsSource = new[] {"Рабочая","Тестовая"};
        }
        
        void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            PdmBaseName.Content = " PdmBaseName - " + Settings.Default.PdmBaseName;
            TestPdmBaseName.Content = " TestPdmBaseName - " + Settings.Default.TestPdmBaseName;
            SourceFolder.Content = " SourceFolder - " + Settings.Default.SourceFolder;
            DestinationFolder.Content = " DestinationFolder - " + Settings.Default.DestinationFolder;
            Developer.Content = " Developer - " + Settings.Default.Developer;
        }
      
        void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Логгер.Информация("Сохранение настроек программы", "", "Сохранение настроек программы", "SettingsW");

            var edmTestVault = new EdmVault5();
            edmTestVault.LoginAuto("Tets_debag", 0);

            var edmVentsVault = new EdmVault5();
            edmVentsVault.LoginAuto("Vents-PDM", 0);

            var edmVault5 = new EdmVault5();
            edmVault5.LoginAuto(VaultsComboBox.Text, 0);

            if (edmVault5.RootFolderPath == "") return;

            switch (VaultsComboBox.Text)
            {
                case "Tets_debag":
                    Settings.Default.SourceFolder = edmVentsVault.RootFolderPath;
                    MessageBox.Show(edmTestVault.RootFolderPath);
                    Settings.Default.DestinationFolder = edmTestVault.RootFolderPath + "\\Vents-PDM";//Path.Combine(edmTestVault.RootFolderPath,"Vents-PDM");
                    Settings.Default.Save();
                    MessageBox.Show(Settings.Default.DestinationFolder);

                    Settings.Default.TestPdmBaseName = @"Tets_debag";
                    break;

                case "Vents-PDM":
                    Settings.Default.PdmBaseName = VaultsComboBox.Text;
                    Settings.Default.TestPdmBaseName = VaultsComboBox.Text;
                    Settings.Default.SourceFolder = edmVault5.RootFolderPath; 
                    Settings.Default.DestinationFolder = edmVault5.RootFolderPath;
                    break;

                default:
                    Settings.Default.PdmBaseName = VaultsComboBox.Text;
                    Settings.Default.TestPdmBaseName = VaultsComboBox.Text;
                    Settings.Default.SourceFolder = edmVault5.RootFolderPath; 
                    Settings.Default.DestinationFolder = edmVault5.RootFolderPath;
                    break;
            }

            switch (SQLBase.Text)
            {
                case "Тестовая":
                    Settings.Default.ConnectionToSQL = App.SqlTestConnectionString;// Settings.Default.TestSqlConnection;
                    break;
                default:
                    Settings.Default.ConnectionToSQL = App.SqlConnectionString;// Settings.Default.WorkingSqlConnection;
                    break;   
            }
            
            Settings.Default.Save();

            PdmBaseName.Content = " PdmBaseName - " + Settings.Default.PdmBaseName;
            TestPdmBaseName.Content = " TestPdmBaseName - " + Settings.Default.TestPdmBaseName;
            SourceFolder.Content = " SourceFolder - " + Settings.Default.SourceFolder;
            DestinationFolder.Content = " DestinationFolder - " + Settings.Default.DestinationFolder;
            MessageBox.Show(Settings.Default.DestinationFolder);

            Логгер.Информация($"Сохранение настроек программы завершено для хранилища - {VaultsComboBox.Text},",
                "", "Сохранение настроек программы", "SettingsW");

            Visibility = Visibility.Collapsed;
        }

        void VaultsComboBox_Initialized(object sender, EventArgs e)
        {
            try
            {
                var v = new EdmVault5();
                var vault = v as IEdmVault8;
                Array views;
                Debug.Assert(vault != null, "vault != null");
                vault.GetVaultViews(out views, false);
                VaultsComboBox.Items.Clear();

                foreach (EdmViewInfo view in views)
                {
                    VaultsComboBox.Items.Add(view.mbsVaultName);
                }

                if (VaultsComboBox.Items.Count > 0)
                {
                    VaultsComboBox.Text = VaultsComboBox.Items[0].ToString();
                }
                VaultsComboBox.SelectedValue = Settings.Default.PdmBaseName;

                v.LoginAuto(Settings.Default.PdmBaseName, 0);

                if (PdmBaseName == null) return;
                PdmBaseName.Content = "Текущая корневая папка " + v.RootFolderPath;
            }
            catch (Exception)
            {
                VaultsComboBox.Items.Clear();
                VaultsComboBox.Items.Add("Не найдено доступных хранилищ");
            }
        }

        void ФайлЛоггера_Click(object sender, RoutedEventArgs e)
        {
            //var logClass = new LogerParse();
            try
            {
                var newWindow = new Window
                {
                    // Width = 500,
                    // Height = 300,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    // ResizeMode = ResizeMode.NoResize,
                    Title = "Логгер событий",
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Content = new LoggerUc()
                };
                newWindow.ShowDialog();
            }
            catch (Exception exception)
            {
               // MessageBox.Show(exception);
            }

            

            Visibility = Visibility.Collapsed;
        }

        private void VaultsComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //if (VaultsComboBox == null) return;
            //SQLBase.Text = VaultsComboBox.Text == "Vents-PDM" ? "Рабочая" : "Тестовая";
        }

        private void VaultsComboBox_LayoutUpdated(object sender, EventArgs e)
        {
           // MessageBox.Show(VaultsComboBox.SelectedValue)
            if (VaultsComboBox == null) return;
            SQLBase.Text = VaultsComboBox.SelectedValue.ToString() == "Vents-PDM" ? "Рабочая" : "Тестовая";
        }
    }
}
