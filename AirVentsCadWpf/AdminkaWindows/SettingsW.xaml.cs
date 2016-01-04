using System;
using System.Collections.Generic;
using System.Windows;
using AirVentsCadWpf.Properties;
using LoggerUc = AirVentsCadWpf.DataControls.Loggers.LoggerUc;
using AirVentsCadWpf.Логирование;
using VentsCadLibrary;

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
            ResizeMode = ResizeMode.NoResize;
            SizeToContent = SizeToContent.WidthAndHeight;
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
      
        void SaveSettingsClick(object sender, RoutedEventArgs e)
        {
            Логгер.Информация("Сохранение настроек программы", "", "Сохранение настроек программы", "SettingsW");

            if (string.IsNullOrEmpty(SwEpdm.GetSwEpdRootFolderPath("Tets_debag"))) return;

            var testVaultSwEpdmRootPath = SwEpdm.GetSwEpdRootFolderPath("Tets_debag");
            var pdmRootPath = SwEpdm.GetSwEpdRootFolderPath(VaultsComboBox.Text);

            switch (VaultsComboBox.Text)
            {
                case "Tets_debag":
                    Settings.Default.SourceFolder = testVaultSwEpdmRootPath;
                    MessageBox.Show(testVaultSwEpdmRootPath);
                    Settings.Default.DestinationFolder = testVaultSwEpdmRootPath + "\\Vents-PDM";
                    Settings.Default.Save();
                    MessageBox.Show(Settings.Default.DestinationFolder);

                    Settings.Default.TestPdmBaseName = @"Tets_debag";
                    break;

                case "Vents-PDM":
                    Settings.Default.PdmBaseName = VaultsComboBox.Text;
                    Settings.Default.TestPdmBaseName = VaultsComboBox.Text;
                    Settings.Default.SourceFolder = pdmRootPath; 
                    Settings.Default.DestinationFolder = pdmRootPath;
                    break;

                default:
                    Settings.Default.PdmBaseName = VaultsComboBox.Text;
                    Settings.Default.TestPdmBaseName = VaultsComboBox.Text;
                    Settings.Default.SourceFolder = pdmRootPath;
                    Settings.Default.DestinationFolder = pdmRootPath;
                    break;
            }

            switch (SQLBase.Text)
            {
                case "Тестовая":
                    Settings.Default.ConnectionToSQL = App.SqlTestConnectionString;
                    break;
                default:
                    Settings.Default.ConnectionToSQL = App.SqlConnectionString;
                    break;   
            }

            var selectedItem = (KeyValuePair<string, int>) VaultsComboBox.SelectedItem;
            Settings.Default.VaultSystemType = selectedItem.Value;

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
            VaultsComboBox.Items.Clear();
            VaultsComboBox.ItemsSource = SwEpdm.GetSwEpdmVaults();
            VaultsComboBox.SelectedValuePath = "Key";
            VaultsComboBox.DisplayMemberPath = "Key";

            VaultsComboBox.SelectedItem = 0;
            
            VaultsComboBox.SelectedValue = Settings.Default.PdmBaseName;
            if (PdmBaseName != null) 
                PdmBaseName.Content = "Текущая корневая папка " + SwEpdm.GetSwEpdRootFolderPath(Settings.Default.PdmBaseName);
        }

        void ФайлЛоггера_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newWindow = new Window
                {
                    SizeToContent = SizeToContent.WidthAndHeight,
                    Title = "Логгер событий",
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Content = new LoggerUc()
                };
                newWindow.ShowDialog();
            }
            catch (Exception)
            {
               // MessageBox.Show(e);
            }
            Visibility = Visibility.Collapsed;
        }
        
        private void VaultsComboBox_LayoutUpdated(object sender, EventArgs e)
        {
            if (VaultsComboBox == null) return;
            SQLBase.Text = VaultsComboBox.SelectedValue?.ToString() == "Vents-PDM" ? "Рабочая" : "Тестовая";
        }
    }
}
