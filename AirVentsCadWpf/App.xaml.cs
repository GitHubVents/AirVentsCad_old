using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
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
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
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
    }
}