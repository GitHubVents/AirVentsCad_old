using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AirVentsCadWpf.Properties;

namespace AirVentsCadWpf.DataControls.Loggers
{
    /// <summary>
    /// Interaction logic for HardwareUc.xaml
    /// </summary>
    public partial class LoggerUc
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerUc"/> class.
        /// </summary>
        public LoggerUc()
        {
            InitializeComponent();
            УровеньКомбо.ItemsSource = new[] { "ALL", "INFO", "DEBUG", "ERROR", "TRACE", "WARN" };
            УровеньКомбо.SelectedIndex = 0;

            Информация.Visibility = Visibility.Collapsed;
            УровеньGrid.Visibility = Visibility.Collapsed;
            Классы.Visibility = Visibility.Collapsed;
            ДатаВыборка.Visibility = Visibility.Collapsed;

            NLogGrid.Visibility = Visibility.Collapsed;
        }
        
        private IEnumerable<LogStucture> _logList;

        private IEnumerable<LogStucture> _logListFiltered;
        
        class LogStucture
        {
            public DateTime DataTime { get; private set; }
            public string Level { get; private set; }
            private string Place { get; set; }
            private string Text { get; set; }

            /// <summary>
            /// Logs the stuctures.
            /// </summary>
            /// <returns></returns>
            public static IEnumerable<LogStucture> LogStuctures()
            {
                var listLog = new List<LogStucture>();
                
                try
                {
                    var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"AirVentsCadLog.log");
                    using (var reader = new StreamReader(logPath, Encoding.Default))
                    {
                        string readLine;
                        while ((readLine = reader.ReadLine()) != null)
                        {
                            var row = readLine.Split('|');
                            var logStucture = new LogStucture();
                            if (row.Count() != 4) continue;
                            try
                            {
                                logStucture.DataTime = Convert.ToDateTime(row[0]);
                                logStucture.Level = row[1];
                                logStucture.Place = row[2];
                                logStucture.Text = row[3];

                                listLog.Add(logStucture);
                                //listLog.AddRange(row.Where(t => t != null).Select(t => new LogStucture
                                //{
                                //    DataTime = row[0],
                                //    Level = row[1],
                                //    Place = row[2],
                                //    Text = row[3]
                                //}));
                            }
                            catch (Exception exception)
                            {
                                MessageBox.Show(exception.Message);
                            }
                        }
                    }
                    return listLog;
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Ошибка при получении списка! \n" + exception.Message);
                    return null;
                }
            }
        }

        private IList<SqlLogStucture> SqlLogStuctures { get; set; }

        void СобытийТаблицаDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            /*

            var logStucture = new LogStucture();
            _logList = LogStucture.LogStuctures();
            var logStuctures = _logList as IList<LogStucture> ?? _logList.ToList();
            СобытийТаблицаDataGrid.ItemsSource = logStuctures;
            ScrollDown();
            ДатаComboBox.ItemsSource = logStuctures.GroupBy(i => i.DataTime.ToShortDateString()).Where(i => i.Count() > 1).ToList();
            ДатаComboBox.SelectedValuePath = "DataTime";
            ДатаComboBox.DisplayMemberPath = "DataTime";
            ДатаComboBox.ItemStringFormat = "dd-MM-yyyy";
            ДатаComboBox.SelectedIndex = -1;
            
            var sqlLogStucture = new SqlLogStucture();

            */

            _sqlLogStuctures = SqlLogStucture.LogStuctures();

            var sqlLogStuctures = _sqlLogStuctures as IList<SqlLogStucture> ?? _sqlLogStuctures.ToList();

            SqlLogStuctures = sqlLogStuctures;
            ТаблицаДанных.ItemsSource = SqlLogStucture.LogStuctures();

            SqlLog.Header = "SQL " + SqlLogStucture.LogStuctures().Count() + " записи(ей)";

            Имя.ItemsSource = sqlLogStuctures.GroupBy(i => i.userName).Where(i => i.Count() > 1).ToList();
            Имя.SelectedValuePath = "userName";
            Имя.DisplayMemberPath = "userName";
            Имя.SelectedIndex = -1;

            Дата.ItemsSource = sqlLogStuctures.GroupBy(i => i.errorTime.ToShortDateString()).Where(i => i.Count() > 1).ToList();
            Дата.SelectedValuePath = "errorTime";
            Дата.DisplayMemberPath = "errorTime";
            Дата.ItemStringFormat = "dd-MM-yyyy";
            Дата.SelectedIndex = -1;
            
            ИмяКласса.ItemsSource = sqlLogStuctures.GroupBy(i => i.errorModule).Where(i => i.Count() > 1).ToList();
            ИмяКласса.SelectedValuePath = "errorModule";
            ИмяКласса.DisplayMemberPath = "errorModule";
            ИмяКласса.SelectedIndex = -1;

            УровеньCombo.ItemsSource = sqlLogStuctures.GroupBy(i => i.errorState).Where(i => i.Count() > 1).ToList();
            УровеньCombo.SelectedValuePath = "errorState";
            УровеньCombo.DisplayMemberPath = "errorState";
            УровеньCombo.SelectedIndex = -1;

            //float ver = 3;
            //MessageBox.Show(String.Format("{0:0.00}", ver));
             
             
        }

        void ScrollDown()
        {
            if (СобытийТаблицаDataGrid.Items.Count <= 0) return;
            var border = VisualTreeHelper.GetChild(СобытийТаблицаDataGrid, 0) as Decorator;
            if (border == null) return;
            var scroll = border.Child as ScrollViewer;
            if (scroll != null) scroll.ScrollToEnd();
        }

        void УровеньКомбо_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (УровеньКомбо != null)
                {
                    switch (УровеньКомбо.SelectedValue.ToString())
                    {
                        case "ALL":
                            _logListFiltered = _logList;
                            break;
                        case "INFO":
                            _logListFiltered = _logList.Where(x => x.Level == "INFO");
                            break;
                        case "DEBUG":
                            _logListFiltered = _logList.Where(x => x.Level == "DEBUG");
                            break;
                        case "ERROR":
                            _logListFiltered = _logList.Where(x => x.Level == "ERROR");
                            break;
                        case "TRACE":
                            _logListFiltered = _logList.Where(x => x.Level == "TRACE");
                            break;
                        case "WARN":
                            _logListFiltered = _logList.Where(x => x.Level == "WARN");
                            break;
                    }
                }

                СобытийТаблицаDataGrid.ItemsSource = _logListFiltered;
                ScrollDown();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        void ДатаComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ДатаComboBox != null)
                {

                    var selectedDate = (DateTime)ДатаComboBox.SelectedValue;

                    if (_logListFiltered != null)
                    {
                        _logListFiltered =
                       _logListFiltered.Where(x => x.DataTime.ToShortDateString().ToString() == selectedDate.ToShortDateString().ToString());
                    }
                    else
                    {
                        _logListFiltered =
                      _logList.Where(x => x.DataTime.ToShortDateString().ToString() == selectedDate.ToShortDateString().ToString());
                    }


                }

                СобытийТаблицаDataGrid.ItemsSource = _logListFiltered;
                ScrollDown();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            
        }

        #region DataTable From SQL to List<Class>

        private IEnumerable<SqlLogStucture> _sqlLogStuctures;

        /// <summary>
        /// 
        /// </summary>
        class SqlLogStucture
        {
             public DateTime errorTime { get; private set; }
             public string userName { get; private set; }
             public string errorModule { get; private set; }
            private string errorMessage { get; set; }
             public string errorState { get; private set; }
            private string errorFunction { get; set; }
            private string errorCode { get; set; }

            /// <summary>
            /// Logs the stuctures.
            /// </summary>
            /// <returns></returns>
            public static IEnumerable<SqlLogStucture> LogStuctures()
            {
                try
                {
                    var listLog = (from DataRow row in ErrorLogTable().Rows
                        select new SqlLogStucture
                        {
                            errorTime = Convert.ToDateTime(row["errorTime"]),
                            userName = row["userName"].ToString(),
                            errorModule = row["errorModule"].ToString(),
                            errorMessage = row["errorMessage"].ToString(),
                            errorState = row["errorState"].ToString(),
                            errorFunction = row["errorFunction"].ToString(),
                            errorCode = row["errorCode"].ToString()
                        }).ToList();
                    return listLog;
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Ошибка при получении списка! \n" + exception.Message);
                    return null;
                }
            }

            static DataTable ErrorLogTable()
            {
                var errorLog = new DataTable();
                using (var sqlConnection = new SqlConnection(Settings.Default.ConnectionToSQL))
                {
                    try
                    {
                        sqlConnection.Open();
                        var sqlCommand = new SqlCommand(@"SELECT * FROM ErrorLog",// where ErrorTime Like '%" +//DateTime.Now.ToString("dd.mm.yyyy") "2015"+"%'", 
                            sqlConnection);
                        var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                        sqlDataAdapter.Fill(errorLog);
                        sqlDataAdapter.Dispose();
                        //ordersList.Columns["LastName"].ColumnName = "Фамилия";
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message, "Ошибка выгрузки данных из базы");
                    }
                    finally
                    {
                        sqlConnection.Close();
                    }
                }
                return errorLog;
            }

            #endregion
        }

        private void ТаблицаДанных_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var style = new Style(typeof(DataGridCell));
            style.Setters.Add(new Setter(ContentTemplateProperty, Resources["templ"]));
            e.Column.CellStyle = style;
        }

        private void ОбновитьДанные_Click(object sender, RoutedEventArgs e)
        {
            СобытийТаблицаDataGrid_Loaded(sender, null);
        }



       

        private string Пользователь { get; set; }
        private string Класс { get; set; }
        private DateTime Время { get; set; }
        private string Уровень { get; set; }


        private IList<SqlLogStucture> tempList;

        void ФильтрацияТаблициДанных()
        {
            //http://answerstop.org/question/77212/wpf-datagrid-is-filled-except-when-i-use-linq-to-filter-its-items
            if (SqlLogStuctures == null)return;

            //var tempList = SqlLogStuctures;
            tempList = SqlLogStuctures;


            if (Пользователь != null)
            {
                tempList = SqlLogStuctures.Where(x => x.userName == Пользователь).ToList();
            }
            
            //if (Класс != null)
            //{
            //    tempList = tempList.Where(x => x.errorModule == Класс).ToList();
            //}
            
            //if (Уровень != null)
            //{
            //    tempList = tempList.Where(x => x.errorState == Уровень).ToList();
            //}

            //tempList = Информация.IsChecked == true ? tempList.Where(x => x.errorState == "Info").ToList() : tempList.ToList();

            if (Ошибки.IsChecked == true)
            {
                tempList = tempList.Where(x => x.errorState == "Error").ToList();
            }

            //MessageBox.Show("Уровень " + Уровень + SqlLogStuctures.Count + "  " + tempList.Count);

            //if (Время.ToShortDateString() != "")
            //{
            //    tempList = tempList.Where(x => x.errorTime.ToShortDateString() == Время.ToShortDateString()).ToList();
            //}

            //ТаблицаДанных.ItemsSource = SqlLogStuctures;
            //MessageBox.Show("tempList");

            ТаблицаДанных.ItemsSource = tempList;

            if (ТаблицаДанных.Items.Count <= 0) return;
            var border = VisualTreeHelper.GetChild(ТаблицаДанных, 0) as Decorator;
            if (border == null) return;
            var scroll = border.Child as ScrollViewer;
            if (scroll != null) scroll.ScrollToEnd();


            SqlLog.Header = "SQL " + tempList.Count + " записи(ей)";

            //ТаблицаДанных.ItemsSource = SqlLogStucture.LogStuctures().
            //    Where(x => x.userName == Пользователь).
            //    Where(x => x.errorModule == Класс).
            //    Where(x => x.errorFunction == Уровень).
            //    Where(x => x.errorTime.ToShortDateString() == Время.ToShortDateString());
        }


        private void Имя_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Имя.SelectedValue == null) return;
            Пользователь = Имя.SelectedValue.ToString();
            ФильтрацияТаблициДанных();
        }


        private void Дата_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Дата.SelectedValue == null) return;
            Время = Convert.ToDateTime(Дата.SelectedValue);
            tempList = SqlLogStuctures.Where(x => x.errorTime == Время).ToList();
            ФильтрацияТаблициДанных();
        }

        private void ИмяКласса_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (ИмяКласса.SelectedValue == null) return;
            //Класс = ИмяКласса.SelectedValue.ToString();
            //ФильтрацияТаблициДанных();
        }

        private void Функция_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (УровеньCombo.SelectedValue == null) return;
            //Уровень = УровеньCombo.SelectedValue.ToString();
            //ФильтрацияТаблициДанных();
        }

        private void Информация_Checked(object sender, RoutedEventArgs e)
        {
          //  ФильтрацияТаблициДанных();
        }

        private void Ошибки_Checked(object sender, RoutedEventArgs e)
        {
        //    ФильтрацияТаблициДанных();
        }




        private void Ошибки_Click(object sender, RoutedEventArgs e)
        {
            ФильтрацияТаблициДанных();
        }

        private void Информация_Click(object sender, RoutedEventArgs e)
        {
            ФильтрацияТаблициДанных();
        }

        private void ВсеПользователи_Click(object sender, RoutedEventArgs e)
        {
            if (ВсеПользователи.IsChecked == true)
            {
                Имя.IsEnabled = true;
                ФильтрацияТаблициДанных();
            }
            else
            {
                ТаблицаДанных.ItemsSource = SqlLogStuctures;
                SqlLog.Header = "SQL " + SqlLogStuctures.Count() + " записи(ей)";
                Имя.IsEnabled = false;
            }
        }
        
    }
}
