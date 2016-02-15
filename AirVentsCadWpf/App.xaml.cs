using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using AirVentsCadWpf.Properties;

namespace AirVentsCadWpf
{

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        static App()
        {
            try
            {
                FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static int VaultSystemType { get; set; }

        enum VaultTypes : int
        {
            None,
            SwEpdm,
            Intermech
        }

        /// <summary>
        /// 
        /// </summary>
        public static int VaultType
        {
            get { return (int)VaultTypes.Intermech; }
        }



        /// <summary>
        /// Gets the SQL connection string.
        /// </summary>
        /// <value>
        /// The SQL connection string.
        /// </value>
        public static string SqlTestConnectionString
        {
            get
            {
                var builder = new SqlConnectionStringBuilder
                {
                    ["Data Source"] = "192.168.14.11",
                    ["Initial Catalog"] = Settings.Default.TestSqlConnection,
                    ["User ID"] = "sa",
                    ["Password"] = "PDMadmin",
                    ["Persist Security Info"] = true
                };
         
                return builder.ConnectionString;
            }
        }

        /// <summary>
        /// Gets the SQL connection string.
        /// </summary>
        /// <value>
        /// The SQL connection string.
        /// </value>
        public static string SqlConnectionString
        {
            get
            {
                var builder = new SqlConnectionStringBuilder
                {
                    ["Data Source"] = "192.168.14.11",
                    ["Initial Catalog"] = Settings.Default.WorkingSqlConnection,
                    ["User ID"] = "sa",
                    ["Password"] = "PDMadmin",
                    ["Persist Security Info"] = true
                };
                
                return builder.ConnectionString;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class ElementVisibility
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="picturePath"></param>
            /// <param name="imageUi"></param>
            public static void SetImage(string picturePath, Image imageUi)
            {
                var mapLoader = new BitmapImage();
                mapLoader.BeginInit();
                mapLoader.UriSource = new Uri(picturePath, UriKind.RelativeOrAbsolute);
                mapLoader.EndInit();
                if (imageUi != null) imageUi.Source = mapLoader;
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="list"></param>
            /// <param name="visibility"></param>
            public static void ForUiElementsList(IEnumerable<UIElement> list, Visibility visibility)
            {
                foreach (var uiElement in list)
                {
                    try
                    {
                        uiElement.Visibility = visibility;
                    }
                    catch (Exception)
                    {
                        //
                    }
                }
            }
        }
    }
}