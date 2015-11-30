using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using AirVentsCadWpf.AirVentsClasses;
using AirVentsCadWpf.AirVentsClasses.UnitsBuilding;
using AirVentsCadWpf.Properties;
using BomPartList.Спецификации;
using EdmLib;
using HostingWindowsForms.EPDM;
using MakeDxfUpdatePartData;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

//using AirVentsCadWpf.ServiceReference6;

namespace AirVentsCadWpf.DataControls.Specification
{

    /// <summary>
    /// 
    /// </summary>
    public partial class SpecificationUc
    {
        List<string> AsmsNames { get; set; }
        string SelectedName { get; set; }
        
        #region Основные поля

        string _путьКСборке;
        //List<СпецификацияДляВыгрузкиСборки.ДанныеДляВыгрузки> _спецификация;
        private readonly СпецификацияДляВыгрузкиСборки _работаСоСпецификацией;

        #endregion

        #region Конструктор класса

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecificationUc"/> class.
        /// </summary>
        public SpecificationUc()
        {
          
            InitializeComponent();


            button.Visibility = Visibility.Collapsed;

            ТаблицаСпецификации.Visibility = Visibility.Collapsed;
            СпецификацияТаблица.Visibility = Visibility.Collapsed;
            Exp1.Visibility = Visibility.Collapsed;
            Exp2.Visibility = Visibility.Collapsed;
            Exp3.Visibility = Visibility.Collapsed;

            Grid11.Visibility = Visibility.Collapsed;


            ДобавитьТаблицу.Visibility = Visibility.Collapsed;

            ВыгрузитьВXml.IsEnabled = false;

            ПереченьДеталей.Visibility = Visibility.Hidden;

           ПереченьДеталей_Copy.Visibility = Visibility.Hidden;

            PartsList.Visibility = Visibility.Hidden;
            XmlParts.Visibility = Visibility.Hidden;

            _работаСоСпецификацией = new СпецификацияДляВыгрузкиСборки
            {
                ИмяХранилища = Settings.Default.PdmBaseName,
                ИмяПользователя = Settings.Default.UserName,
                ПарольПользователя = Settings.Default.Password,
            };

            //СпецификацияДляВыгрузкиСборки.ConnectionString = Settings.Default.ConnectionToSQL;
            //MessageBox.Show(СпецификацияДляВыгрузкиСборки.ConnectionString);
        }

        
        void LoadAsmsList()
        {
            var epdmSearch = new EpdmSearch { VaultName = Settings.Default.PdmBaseName };
            List<EpdmSearch.FindedDocuments> найденныеФайлы;
            epdmSearch.SearchDoc("900", EpdmSearch.SwDocType.SwDocAssembly, out найденныеФайлы);
            if (найденныеФайлы == null) return;
            var newNames = найденныеФайлы.Select(findedDocumentse => findedDocumentse.Path).ToList();
            AsmsNames = newNames.ConvertAll(FileNameWithoutExt);

           // static AutoCompleteBehavior BomByPage;
        }

        void НайтиПолучитьСборкуЕеКонфигурацииПоИмени(string имяСборки)
        {
            //имяСборки = "ВНС-904.40.300";

            var emdpService = new Epdm();// new EpdmServiceClient();
            var path = emdpService.SearchDoc(AutoCompleteTextBox1.Text + ".SLDASM");
            var configs = emdpService.GetConfiguration(path[0].FilePath);
            var itemsSource = configs as IList<string> ?? configs.ToList();
            Конфигурация.ItemsSource = itemsSource;

            _путьКСборке = path[0].FilePath;
            //_specBomCells = emdpService.Bom(path[0].FilePath, configs[0]);

            var bomClass = new Epdm
                    {
                        BomId = 8,
                        AssemblyPath = path[0].FilePath
                    };
                    //return bomClass.BomList(filePath, config);

            _specBomCells = bomClass.BomList(path[0].FilePath, itemsSource[0]).ToArray();

            // MessageBox.Show(specBomCells[0].Обозначение, configs[0]);
            //var спецификация = emdpService.Bom(path[0].FilePath, configs[0]);
            //emdpService.Close();

            #region to delete

            //try
            //{
            //    _путьКСборке = _работаСоСпецификацией.ПолучениеПутиКСборке(имяСборки);

            //    //MessageBox.Show(_путьКСборке, "20");

            //    if (_путьКСборке != "")
            //    {
            //        Конфигурации.ItemsSource = _работаСоСпецификацией.ПолучениеКонфигураций(_путьКСборке).ToList();
            //    }
            //    else
            //    {
            //        MessageBox.Show("Сборка не найдена!");
            //    }

            //    _спецификация = _работаСоСпецификацией.Спецификация(_путьКСборке, 20, "00");
            //    ТаблицаСпецификации.ItemsSource = _спецификация;

            //}
            //catch (Exception exception)
            //{
            //    MessageBox.Show(exception.ToString());
            //}

            #endregion

        }

        #endregion


        #region ВыгрузитьВXml
        
        void ВыгрузитьВXml_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //LoggerInfo(string.Format("Запуск выгрузки в XML"));
                НайтиПолучитьСборкуЕеКонфигурацииПоИмени(AutoCompleteTextBox1.Text);
                ВыгрузкаСборкиВxml(Convert.ToBoolean(IncludeAsms.IsChecked));
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }
        
        void ИмяСборки_KeyDown(object sender, KeyEventArgs e)
        {
            ВыгрузитьВXmlClickHandler(e);
        }
        
        void ВыгрузитьВXmlClickHandler(KeyEventArgs e)
         {
             if (e.Key == Key.Enter)
             {
                 ВыгрузитьВXml_Click(this, new RoutedEventArgs());
             }
         }
        
        void ВыгрузитьСборку(string путьКСборке)
        {
            var имяСборки = new FileInfo(путьКСборке).Name.Replace(new FileInfo(путьКСборке).Extension, "");

            var emdpService = new Epdm(); //new EpdmServiceClient();
            var path = emdpService.SearchDoc(имяСборки + ".SLDASM");
            
            var списокКонфигурацийСборки = emdpService.GetConfiguration(path[0].FilePath);

            //emdpService.Close();

            #region Конфигурация

            //foreach (var s in списокКонфигурацийСборки)
            //{
            //    MessageBox.Show(s);
            //}

            //Конфигурация.ItemsSource = configs;

            //var спецификация = emdpService.Bom(path[0].FilePath, списокКонфигурацийСборки[0]);

            //var списокКонфигурацийСборки = _работаСоСпецификацией.ПолучениеКонфигураций(путьКСборке).ToList().Distinct();

            #endregion

            try
            {
                // const string xmlPath = @"\\srvkb\XML\";
                const string xmlPath = @"\\srvkb\SolidWorks Admin\XML\";

                //const string xmlPath = @"C:\Temp\";

                var myXml = new XmlTextWriter(xmlPath + имяСборки + ".xml", Encoding.UTF8);
                 
                 myXml.WriteStartDocument();
                 myXml.Formatting = Formatting.Indented;
                 myXml.Indentation = 2;
                 
                 // создаем элементы
                 myXml.WriteStartElement("xml");
                 myXml.WriteStartElement("transactions");
                 myXml.WriteStartElement("transaction");

                 myXml.WriteAttributeString("vaultName", "Vents-PDM");
                 myXml.WriteAttributeString("type", "wf_export_document_attributes");
                 myXml.WriteAttributeString("date", "1416475021");

                 // document
                 myXml.WriteStartElement("document");
                 myXml.WriteAttributeString("pdmweid", "73592");
                 myXml.WriteAttributeString("aliasset", "Export To ERP");

                 foreach (var config in списокКонфигурацийСборки)
                 {
                    var emdpService2 = new Epdm();// new EpdmServiceClient();
                    //var спецификация = emdpService2.Bom(path[0].FilePath, config);
                    //var спецификация = _работаСоСпецификацией.Спецификация(путьКСборке, config);


                    var bomClass = new Epdm
                    {
                        BomId = 8,
                        AssemblyPath = path[0].FilePath
                    };
                    //return bomClass.BomList(filePath, config);
                    var спецификация = bomClass.BomList(path[0].FilePath, config);

                    foreach (var topAsm in спецификация.Where(x => x.Уровень == 0))
                    {
                        #region XML

                        // Конфигурация
                        myXml.WriteStartElement("configuration");
                        myXml.WriteAttributeString("name", topAsm.Конфигурация);

                        // Версия
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Версия");
                        myXml.WriteAttributeString("value", topAsm.ПоследняяВерсия.ToString());
                        myXml.WriteEndElement();
                        
                        // Масса
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Масса");
                        myXml.WriteAttributeString("value", topAsm.Weight);
                        myXml.WriteEndElement();

                        // Наименование
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Наименование");
                        myXml.WriteAttributeString("value", topAsm.Наименование);
                        myXml.WriteEndElement();

                        // Обозначение
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Обозначение");
                        myXml.WriteAttributeString("value", topAsm.Обозначение);
                        myXml.WriteEndElement();

                        // Раздел
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Раздел");
                        myXml.WriteAttributeString("value", topAsm.Раздел);
                        myXml.WriteEndElement();

                        // ERP code
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "ERP code");
                        myXml.WriteAttributeString("value", topAsm.ErpCode);
                        myXml.WriteEndElement();

                        // Код_Материала
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Код_Материала");
                        myXml.WriteAttributeString("value", topAsm.CodeMaterial);
                        myXml.WriteEndElement();

                        // Код документа
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Код документа");
                        myXml.WriteAttributeString("value", "");//topAsm..КодДокумента);
                        myXml.WriteEndElement();

                        // Кол. Материала
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Кол. Материала");
                        myXml.WriteAttributeString("value", topAsm.SummMaterial);
                        myXml.WriteEndElement();

                        // Состояние
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Состояние");
                        myXml.WriteAttributeString("value", topAsm.Состояние);
                        myXml.WriteEndElement();

                        // Подсчет ссылок
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Подсчет ссылок");
                        myXml.WriteAttributeString("value", topAsm.Количество.ToString());
                        myXml.WriteEndElement();

                        // Конфигурация
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Конфигурация");
                        myXml.WriteAttributeString("value", topAsm.Конфигурация);
                        myXml.WriteEndElement();

                        // Идентификатор
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Идентификатор");
                        myXml.WriteAttributeString("value", "");
                        myXml.WriteEndElement();

                        // references
                        myXml.WriteStartElement("references");

                        // document
                        myXml.WriteStartElement("document");
                        myXml.WriteAttributeString("pdmweid", "73592");
                        myXml.WriteAttributeString("aliasset", "Export To ERP");

                        foreach (var topLevel in спецификация.Where(x => x.Уровень == 1))
                        {
                             #region XML

                             // Конфигурация
                             myXml.WriteStartElement("configuration");
                             myXml.WriteAttributeString("name", topLevel.Конфигурация);

                             // Версия
                             myXml.WriteStartElement("attribute");
                             myXml.WriteAttributeString("name", "Версия");
                             myXml.WriteAttributeString("value", topLevel.ПоследняяВерсия.ToString());
                             myXml.WriteEndElement();

                             // Масса
                             myXml.WriteStartElement("attribute");
                             myXml.WriteAttributeString("name", "Масса");
                             myXml.WriteAttributeString("value", topLevel.Weight);
                             myXml.WriteEndElement();

                             // Наименование
                             myXml.WriteStartElement("attribute");
                             myXml.WriteAttributeString("name", "Наименование");
                             myXml.WriteAttributeString("value", topLevel.Наименование);
                             myXml.WriteEndElement();

                             // Обозначение
                             myXml.WriteStartElement("attribute");
                             myXml.WriteAttributeString("name", "Обозначение");
                             myXml.WriteAttributeString("value", topLevel.Обозначение);
                             myXml.WriteEndElement();

                             // Раздел
                             myXml.WriteStartElement("attribute");
                             myXml.WriteAttributeString("name", "Раздел");
                             myXml.WriteAttributeString("value", topLevel.Раздел);
                             myXml.WriteEndElement();

                             // ERP code
                             myXml.WriteStartElement("attribute");
                             myXml.WriteAttributeString("name", "ERP code");
                             myXml.WriteAttributeString("value", topLevel.ErpCode);
                             myXml.WriteEndElement();

                             // Код_Материала
                             myXml.WriteStartElement("attribute");
                             myXml.WriteAttributeString("name", "Код_Материала");
                             myXml.WriteAttributeString("value", topLevel.CodeMaterial);
                             myXml.WriteEndElement();

                             // Код документа
                             myXml.WriteStartElement("attribute");
                             myXml.WriteAttributeString("name", "Код документа");
                             myXml.WriteAttributeString("value", "");// topLevel.КодДокумента);
                             myXml.WriteEndElement();

                             // Кол. Материала
                             myXml.WriteStartElement("attribute");
                             myXml.WriteAttributeString("name", "Кол. Материала");
                             myXml.WriteAttributeString("value", topLevel.SummMaterial);
                             myXml.WriteEndElement();
                            
                             // Состояние
                             myXml.WriteStartElement("attribute");
                             myXml.WriteAttributeString("name", "Состояние");
                             myXml.WriteAttributeString("value", topLevel.Состояние);
                             myXml.WriteEndElement();

                             // Подсчет ссылок
                             myXml.WriteStartElement("attribute");
                             myXml.WriteAttributeString("name", "Подсчет ссылок");
                             myXml.WriteAttributeString("value", topLevel.Количество.ToString());
                             myXml.WriteEndElement();

                             // Конфигурация
                             myXml.WriteStartElement("attribute");
                             myXml.WriteAttributeString("name", "Конфигурация");
                             myXml.WriteAttributeString("value", topLevel.Конфигурация);
                             myXml.WriteEndElement();

                             // Идентификатор
                             myXml.WriteStartElement("attribute");
                             myXml.WriteAttributeString("name", "Идентификатор");
                             myXml.WriteAttributeString("value", "");
                             myXml.WriteEndElement();

                             myXml.WriteEndElement(); //configuration

                            #endregion
                        }

                         myXml.WriteEndElement(); // document

                         myXml.WriteEndElement(); // элемент references

                         myXml.WriteEndElement(); // configuration

                        #endregion
                    }

                   // emdpService2.Close();
                 }

                 myXml.WriteEndElement(); // ' элемент DOCUMENT
                 myXml.WriteEndElement(); // ' элемент TRANSACTION
                 myXml.WriteEndElement(); // ' элемент TRANSACTIONS
                 myXml.WriteEndElement(); // ' элемент XML
                 // заносим данные в myMemoryStream
                 myXml.Flush();

                 Thread.Sleep(1000);

                 myXml.Close();
             }
             catch (Exception exception)
             {
                 MessageBox.Show(exception.Message);
             }
         }

        void ВыгрузкаСборкиВxml(bool включаяПодсборки)
        {
            //MessageBox.Show(_путьКСборке);
            
            var имяСборки = new FileInfo(_путьКСборке).Name.Replace(new FileInfo(_путьКСборке).Extension, "");
            
            var включаяВсеПодсборки = "";

            if (включаяПодсборки)
            {
                включаяВсеПодсборки = "(включая все подсборки)";
            }

            if (имяСборки == "")
            {
                MessageBox.Show("Сборка не найдена!");
                return;
            }

            // LoggerInfo(string.Format("Начало выгрузки в XML установки {0}{1}", имяСборки, включаяВсеПодсборки));
            
            if (включаяПодсборки)
            {
                foreach (var путьКСборке in _specBomCells.Where(x =>
                {
                    var extension = Path.GetExtension(x.FilePath + "\\" + x.FileName);
                    return extension.ToLower() == ".sldasm";
                }).Where(x => x.Раздел == "" || x.Раздел == "Сборочные единицы").Select(x => x.FilePath + "\\" + x.FileName).Distinct())
                {
                    //if (
                    //    MessageBox.Show(путьКСборке, "", MessageBoxButton.YesNo, MessageBoxImage.None,
                    //        MessageBoxResult.Yes, MessageBoxOptions.ServiceNotification) == MessageBoxResult.Yes)
                    //{
                        try
                        {
                        //MessageBox.Show(путьКСборке);
                            ВыгрузитьСборку(путьКСборке);
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show(exception.Message);
                        }
                   // }
                }
            }
            else
            {
                ВыгрузитьСборку(_путьКСборке);
            }


            #region Сборка не найдена

            //var имяСборки = Path.GetFileNameWithoutExtension(_путьКСборке);

            //var включаяВсеПодсборки = "";

            //if (включаяПодсборки)
            //{
            //    включаяВсеПодсборки = "(включая все подсборки)";
            //}

            //if (имяСборки == "")
            //{
            //    MessageBox.Show("Сборка не найдена!");
            //    return;
            //}

            //// LoggerInfo(string.Format("Начало выгрузки в XML установки {0}{1}", имяСборки, включаяВсеПодсборки));


            //if (включаяПодсборки)
            //{
            //    foreach (var путьКСборке in _спецификация.Where(x =>
            //    {
            //        var extension = Path.GetExtension(x.Путь);
            //        return extension != null && extension.ToLower() == ".sldasm";
            //    }).Where(x => x.Раздел == "" || x.Раздел == "Сборочные единицы").Select(x => x.Путь).Distinct())
            //    {
            //        if (
            //            MessageBox.Show(путьКСборке, "", MessageBoxButton.YesNo, MessageBoxImage.None,
            //                MessageBoxResult.Yes, MessageBoxOptions.ServiceNotification) == MessageBoxResult.Yes)
            //        {
            //            ВыгрузитьСборку(путьКСборке);
            //        }
            //    }
            //}
            //else
            //{
            //    ВыгрузитьСборку(_путьКСборке);
            //}

            #endregion

            // LoggerInfo(string.Format("Выгрузка в XML установки {0}{1} завершена", имяСборки, включаяВсеПодсборки));
            MessageBox.Show($"Выгрузка сборки {имяСборки}{включаяВсеПодсборки} выполнена.", "Процесс завершен", MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);

        }
        
         //delegate void ВыгрузитьВXmlClickHandler(object sender, KeyEventArgs e);

         //readonly ВыгрузитьВXmlClickHandler ВыгрузитьВXmlClickHandler = DelegateMethod;

         //void DelegateMethod(object sender, KeyEventArgs e)
         //{
         //    if (e.Key == Key.Enter)
         //    {
         //        ВыгрузитьВXml_Click(this, new RoutedEventArgs());
         //    }
         //}

        #endregion
        
        #region ВыгрузитьСпецификацию

        void ВыгрузитьСпецификацию_Click(object sender, RoutedEventArgs e)
        {
           
        }

        void Конфигурации_LayoutUpdated(object sender, EventArgs e)
        {
            ВыгрузитьСпецификацию.IsEnabled = !Конфигурации.Items.IsEmpty;
        }

        void Поиск_Click(object sender, RoutedEventArgs e)
        {
            НайтиПолучитьСборкуЕеКонфигурацииПоИмени(ИмяСборки1.Text);
        }
        
        void ИмяСборки1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Поиск_Click(this, new RoutedEventArgs());
            }
        }


        private void ИмяСборки1_LayoutUpdated(object sender, EventArgs e)
        {
            if (_путьКСборке != null && ИмяСборки1.Text != new FileInfo(_путьКСборке).Name.Replace(new FileInfo(_путьКСборке).Extension, ""))
            {
                Конфигурации.ItemsSource = null;
            }
        }

        List<СпецификацияДляВыгрузкиСборки.ДанныеДляВыгрузки> _bomPartsList;
        List<СпецификацияДляВыгрузкиСборки.ДанныеДляВыгрузки> _bomPartsListFiltered;
        List<СпецификацияДляВыгрузкиСборки.ДанныеДляВыгрузки> _bomPartsListOriginal;

        readonly List<СпецификацияДляВыгрузкиСборки.ДанныеДляВыгрузки> _bomListSelectedPrts = new List<СпецификацияДляВыгрузкиСборки.ДанныеДляВыгрузки>();

        readonly SolidColorBrush _orangeColorBrush = new SolidColorBrush(Colors.LavenderBlush);
        readonly SolidColorBrush _whiteColorBrush = new SolidColorBrush(Colors.White);
        readonly SolidColorBrush _lightCyanColorBrush = new SolidColorBrush(Colors.LightCyan);

        private void Table_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var bomCells = (СпецификацияДляВыгрузкиСборки.ДанныеДляВыгрузки)e.Row.DataContext;

            if (bomCells.Errors == "")
            {
                e.Row.Background = e.Row.GetIndex() % 2 == 1 ? _lightCyanColorBrush : _whiteColorBrush;
            }
            else
            {
                e.Row.Background = _orangeColorBrush;
            }
        }

        private void TablePrt_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (BomTablePrt.SelectedValue == null) return;
            var row = ((СпецификацияДляВыгрузкиСборки.ДанныеДляВыгрузки)(BomTablePrt.SelectedValue));
            if (_bomListSelectedPrts.Any(bomListSelectedPrt => row.Обозначение == bomListSelectedPrt.Обозначение)) return;
            _bomListSelectedPrts.Add(row);
            BomTableSelectedPrt.ItemsSource = _bomListSelectedPrts.OrderBy(x => x.Обозначение);
        }

        private void TablePrt_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BomTablePrt.SelectedValue == null) return;
            var row = ((СпецификацияДляВыгрузкиСборки.ДанныеДляВыгрузки)(BomTablePrt.SelectedValue));
         //   MaterialsList.SelectedValue = row.Материал;

         //   SelectedPart.ItemsSource = _bomPartsList.Where(x => x.Обозначение == row.ОбозначениеDocMgr);
        }


        private void BeginUpdate1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void УдалитьВыбранную_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ClearList_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MaterialsList_LayoutUpdated(object sender, EventArgs e)
        {

        }

        private void BeginUpdatePartProp_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FilterTextBox_OnSelectionChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void BomTableSelectedPrt_LayoutUpdated(object sender, EventArgs e)
        {

        }

        private void BeginUpdate_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BomTableSelectedPrt_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void UpdateCutList_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AssemblyInfo_LayoutUpdated(object sender, EventArgs e)
        {

        }

        #endregion

        #region Выгрузка eDrawing

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var fileName = PartFile.Text;
            var extension = Path.GetExtension(fileName);
            if (extension == null || extension.ToLower() != ".sldprt") return;
            var cutlistClass = new MakeDxfExportPartDataClass
            {
                PdmBaseName = Settings.Default.PdmBaseName
            };
            string file;
            bool isErrors;
            cutlistClass.CreateFlattPatternUpdateCutlistAndEdrawing(fileName, out file, out isErrors, false, false, false, false);

            //var thread = new Thread(RegistationEdrw);
            //thread.Start();
            
            //cutlistClass2.RegistrationPdm(@"E:\Vents-PDM\Проекты\AirVents\EmptyPart.eprt");
            //ВыгрузкаeDrawing("");
        }

        void ВыгрузкаeDrawing(string filePath)
        {
            var swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");

            var swModel = (IModelDoc2)swApp.IActiveDoc;
            
            swApp.SetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swEdrawingsSaveAsSelectionOption, (int)swEdrawingSaveAsOption_e.swEdrawingSaveAll);
            swModel.Extension.SaveAs("C:\\Temp\\" + swModel.GetTitle() + ".EPRT",
                (int)swSaveAsVersion_e.swSaveAsCurrentVersion,
                (int)swSaveAsOptions_e.swSaveAsOptions_Silent, null, 0, 0);
        }

        #endregion

        void Registration(object sender, RoutedEventArgs e)
        {
            var fileName = PartFile.Text;
            var extension = Path.GetExtension(fileName);
            if (extension == null || extension.ToLower() != ".sldprt") return;
            var cutlistClass = new MakeDxfExportPartDataClass
            {
                PdmBaseName = Settings.Default.PdmBaseName
            };
            cutlistClass.RegistrationPdm(fileName);
        }

        void Выгрузить_PDF(object sender, RoutedEventArgs e)
        {

            //E:\Vents-PDM\Заказы AirVents Frameless\AV02 E:\Vents-PDM\Заказы AirVents Frameless\AV02 001002\AV02 001002 Section A.slddrw
            //var fileName = PartFile.Text;
            //var extension = Path.GetExtension(fileName);
            //if (extension == null || extension.ToLower() != ".slddrw") return;
            
            var cutlistClass = new MakeDxfExportPartDataClass()
            {
                PdmBaseName = Settings.Default.PdmBaseName
            };
            try
            {
                string file;
                cutlistClass.SaveDrwAsPdf(@"E:\Vents-PDM\Заказы AirVents Frameless\AV02 E:\Vents-PDM\Заказы AirVents Frameless\AV02 001002\AV02 001002 Section A.slddrw", out file);
                MessageBox.Show(file);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.StackTrace);
            }
        }

        void AddToPdm(object sender, RoutedEventArgs e)
        {
            //var cutlistClass = new MakeDxfUpdatePartData.MakeDxfUpdatePartDataClass
            //{
            //    PdmBaseName = Settings.Default.PdmBaseName
            //};
            const string file = @"C:\Tets_debag\ВНС-900.98.0087.pdf";
            //cutlistClass.AddToPdmByPath(file, Settings.Default.PdmBaseName);

            AddToPdmByPath(file, Settings.Default.PdmBaseName);
        }

        static void AddToPdmByPath(string path, string pdmBase)
        {
            try
            {
                // LoggerInfo(string.Format("Создание папки по пути {0} для сохранения", path));
                var vault1 = new EdmVault5();
                if (!vault1.IsLoggedIn)
                {
                    vault1.LoginAuto(pdmBase, 0);
                }

                var vault2 = (IEdmVault7)vault1;
                var fileDirectory = new FileInfo(path).DirectoryName;
                var fileFolder = vault2.GetFolderFromPath(fileDirectory);
                var result = fileFolder.AddFile(fileFolder.ID, "", Path.GetFileName(path));

                //var edmFolder6 = (IEdmFolder6)vault2.GetFolderFromPath(fileDirectory);
                //MessageBox.Show(edmFolder6.Name);
                //var edmAddFileInfo = new EdmAddFileInfo
                //{
                //    mbsNewName = "",
                //    mbsPath = path,
                //    mlEdmAddFlags = (int)EdmAddFlag.EdmAdd_Simple,
                //    mlFileID = 0,
                //    mlSrcDocumentID = 0,
                //    mlSrcProjectID = 0
                //};
                //edmFolder6.AddFiles(edmFolder6.ID,ref edmAddFileInfo, null);
                //edmFolder6.AddFile(edmFolder6.ID, "", path);
                
                //directoryInfo = new DirectoryInfo(path);
                //if (directoryInfo.Parent == null) return;
                //var parentFolder = vault2.GetFolderFromPath(directoryInfo.Parent.FullName);
                //parentFolder.AddFolder(0, directoryInfo.Name);
                
                //   LoggerInfo(string.Format("Получение папки по пути {0} в папке {1} завершено. {1}", fileDirectory, fileFolder.Name));
                
                    //parentFolder.AddFolder(0, directoryInfo.Name);
                //     LoggerInfo(string.Format("Создание файла по пути {0} в папке {2} завершено. {1}", path, result, fileFolder.Name));
                //}
            }
            catch (Exception exception)
            {
               // LoggerError(string.Format("Не удалось создать файл по пути {0}. Ошибка {1}", path, exception.StackTrace));
            }
        }

        void ПоискEpdm(object sender, RoutedEventArgs e)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var epdmSearch = new EpdmSearch { VaultName = Settings.Default.PdmBaseName };
            List<EpdmSearch.FindedDocuments> найденныеФайлы;
            epdmSearch.SearchDoc("901.30.1.", EpdmSearch.SwDocType.SwDocPart, out найденныеФайлы);
            if (найденныеФайлы == null) return;

            var newNames = найденныеФайлы.Select(findedDocumentse => findedDocumentse.Path).ToList();

            СписокНайденныхФайлов.ItemsSource = newNames.ConvertAll(FileName);
            stopwatch.Stop();
            MessageBox.Show(string.Format(" Поиск {1} файлов {0} мс ", stopwatch.Elapsed, найденныеФайлы.Count));

            #region Проверка на листовую деталь

            //var swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");
            //var model = swApp.IActiveDoc;
            //MessageBox.Show(IsSheetMetalPart((IPartDoc)model).ToString());

            #endregion
        }

        static string FileName(string filePath)
        {
            var fileName = new FileInfo(filePath).Name;
            //Path.GetFileName(filePath);
            return fileName;
        }
        static string FileNameWithoutExt(string filePath)
        {
            var fileName = new FileInfo(filePath).Name.Replace(new FileInfo(filePath).Extension, "");
            //Path.GetFileNameWithoutExtension(filePath);
            return fileName;
        }

        static bool IsSheetMetalPart(IPartDoc swPart)
        {
            //    LoggerInfo("Проверка на листовую деталь IsSheetMetalPart()");
            try
            {
                var vBodies = swPart.GetBodies2((int)swBodyType_e.swSolidBody, false);

                foreach (Body2 vBody in vBodies)
                {
                    try
                    {
                        var isSheetMetal = vBody.IsSheetMetal();
                        if (!isSheetMetal) continue;
                        //                        LoggerInfo("Проверка завершена");
                        return true;
                    }
                    catch (Exception exception)
                    {
                       //LoggerError(exception.StackTrace);
                        return false;
                    }
                }

                //LoggerInfo("Проверка завершена");
                return false;
            }
            catch (Exception)
            {
   //             LoggerInfo("Проверка завершена. Деталь не из листового материала.");
                // var swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");
                // swApp.ExitApp();
                return false;
            }
        }

        private void Сборка_TextInput(object sender, TextCompositionEventArgs e)
        {
           // MessageBox.Show(" Поиск {1} файлов {0} мс ");

            //var stopwatch = new Stopwatch();
            //stopwatch.Start();
            //var epdmSearch = new EpdmSearch { VaultName = Settings.Default.PdmBaseName };
            //if (Сборка.Text.Length < 9) return;
            //List<string> найденныеФайлы;
            //epdmSearch.SearchDoc(Сборка.Text, EpdmSearch.SwDocType.SwDocPart, out найденныеФайлы);
            //if (найденныеФайлы == null) return;
            //Сборка.ItemsSource = найденныеФайлы.ConvertAll(FileName);
            //stopwatch.Stop();
            //MessageBox.Show(String.Format(" Поиск {1} файлов {0} мс ", stopwatch.Elapsed, найденныеФайлы.Count));
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (AsmsNames!=null)
            {
                //  MessageBox.Show(_AsmsNames.Count.ToString());
                СписокНайденныхФайлов.ItemsSource = AsmsNames;
                
                AutoCompleteTextBox1.ItemsSource = AsmsNames;
               // AutoCompleteTextBox1.SelectedItem = SelectedName;
            }
        }

        string FileType { get; set; }

        private void AutoCompleteTextBox1Reload()
        {
           // if (AsmsNames == null) return;
           // MessageBox.Show(AutoCompleteTextBox1.Text);

            var epdmSearch = new EpdmSearch { VaultName = Settings.Default.PdmBaseName };

            List<EpdmSearch.FindedDocuments> найденныеФайлы;
            epdmSearch.SearchDoc(AutoCompleteTextBox1.Text, EpdmSearch.SwDocType.SwDocAssembly, out найденныеФайлы);
            if (найденныеФайлы == null)
            {
                epdmSearch.SearchDoc(AutoCompleteTextBox1.Text, EpdmSearch.SwDocType.SwDocPart, out найденныеФайлы);
                if (найденныеФайлы == null) return;
                FileType = "prt";
            }
            else FileType = "asm";

            var newNames = найденныеФайлы.Select(findedDocumentse => findedDocumentse.Path).ToList();
            AsmsNames = newNames.ConvertAll(FileNameWithoutExt);
            AutoCompleteTextBox1.ItemsSource = AsmsNames;
            
            //AutoCompleteTextBox1.SelectedItem = SelectedName;

            AutoCompleteTextBox1.FilterMode = AutoCompleteFilterMode.Contains;
        }


        private void AutoCompleteTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Button_Click_3(this, new RoutedEventArgs());
            }
         ////   MessageBox.Show(AutoCompleteTextBox1.Text);
         //   if (AutoCompleteTextBox1.Text.Length > 8)
         //   {
         //       MessageBox.Show(AutoCompleteTextBox1.Text.Length.ToString());
         //       AutoCompleteTextBox1Reload();
         //   }
            
            //if (e.Key == Key.Enter)
            //{
            //  //  MessageBox.Show(AutoCompleteTextBox1.Text);
            //  //  AutoCompleteTextBox1Reload();
            //}
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            AutoCompleteTextBox1Reload();

            // AutoCompleteTextBox1.DropDownOpening += AutoCompleteTextBox1_DropDownOpening;

            //   AutoCompleteTextBox1_KeyDown(sender, new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Down));

            //AutoCompleteTextBox1_SelectionChanged(sender, null);
            //   AutoCompleteTextBox1_KeyDown(sender, new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Down));
            //AutoCompleteTextBox1_TextChanged(sender, new RoutedEventArgs());

            
            //  AutoCompleteTextBox1.Text = AutoCompleteTextBox1.Text + "";
            //  AutoCompleteTextBox1.TextChanged += AutoCompleteTextBox1_TextChanged;
        }
        

        private void AutoCompleteTextBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ВыгрузитьВXml.IsEnabled = false;
          //  MessageBox.Show("AutoCompleteTextBox1_SelectionChanged");
            ВыгрузитьВXml.IsEnabled = AsmsNames.Contains(AutoCompleteTextBox1.Text);
        }

        void AutoCompleteTextBox1_TextChanged(object sender, RoutedEventArgs e)
        {
            Найти.IsEnabled = AutoCompleteTextBox1.Text.Length != 0;
            // AutoCompleteTextBox1.DropDownClosed;
        }

        static void AirVents_AddOrder()
        {
            using (var con = new SqlConnection(Settings.Default.ConnectionToSQL))
            {
                try
                {
                    con.Open();
                    var sqlCommand = new SqlCommand("AddErrorLog", con) { CommandType = CommandType.StoredProcedure };

                    var sqlParameter = sqlCommand.Parameters;

                    sqlParameter.AddWithValue("@userName", "hs878fdg");
                    sqlParameter.AddWithValue("@errorModule", "hsf87987dg");
                    sqlParameter.AddWithValue("@errorMessage", "hsfdg");
                    sqlParameter.AddWithValue("@errorCode", "hsfdg");
                    sqlParameter.AddWithValue("@errorTime", DateTime.Now);
                    sqlParameter.AddWithValue("@errorState", "hsfdg");
                    sqlParameter.AddWithValue("@errorFunction", "hsfdg");
                    

                    //var returnParameter = sqlCommand.Parameters.Add("@ProjectNumber", SqlDbType.Int);
                    //returnParameter.Direction = ParameterDirection.ReturnValue;

                   sqlCommand.ExecuteNonQuery();

                    //var result = Convert.ToInt32(returnParameter.Value);

                    //switch (result)
                    //{
                    //    case 0:
                    //        MessageBox.Show("Подбор №" + Номерподбора.Text + " уже существует!");
                    //        break;
                    //}

                }
                catch (Exception exception)
                {
                    MessageBox.Show("Введите корректные данные! " + exception.Message);
                }
                finally
                {
                    con.Close();
                }
            }
        }

        static void WriteToBase(string описание, string тип, string код, string модуль, string функция)
        {
            using (var con = new SqlConnection(Settings.Default.ConnectionToSQL))
            {
                try
                {
                    con.Open();
                    var sqlCommand = new SqlCommand("AirVents_AddErrorLog", con) { CommandType = CommandType.StoredProcedure };
                    //sqlCommand.Parameters.Add("@userName", SqlDbType.NVarChar).Value = System.Environment.userName + " (" + System.Net.Dns.GetHostName() + ")";
                    //sqlCommand.Parameters.Add("@errorModule", SqlDbType.NVarChar).Value = модуль;
                    //sqlCommand.Parameters.Add("@errorMessage", SqlDbType.NVarChar).Value = функция;
                    //sqlCommand.Parameters.Add("@errorCode", SqlDbType.NVarChar).Value = код;
                    //sqlCommand.Parameters.Add("@errorTime", SqlDbType.DateTime).Value = DateTime.Now;
                    //sqlCommand.Parameters.Add("@errorMessage", SqlDbType.NVarChar).Value = описание;
                    //sqlCommand.Parameters.Add("@errorState", SqlDbType.NVarChar).Value = тип;
                    var myDateTime = DateTime.Now;
                    var sqlFormattedDate = myDateTime.ToString("yyyy-MM-dd HH:mm:ss");

                    sqlCommand.Parameters.Add("@userName", SqlDbType.Char).Value = "asdgh";
                    sqlCommand.Parameters.Add("@errorModule", SqlDbType.Char).Value = "asdgh";
                    sqlCommand.Parameters.Add("@errorFunction", SqlDbType.Char).Value = "asdgh";
                    sqlCommand.Parameters.Add("@errorCode", SqlDbType.Char).Value = "asdgh";
                    sqlCommand.Parameters.Add("@errorTime", SqlDbType.DateTime).Value = DateTime.Now;
                    sqlCommand.Parameters.Add("@errorState", SqlDbType.Char).Value = "asdgh";
                    sqlCommand.Parameters.Add("@errorMessage", SqlDbType.Char).Value = "asdgh";


                    sqlCommand.ExecuteNonQuery();

                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.StackTrace);
                    return;
                }
                finally
                {
                    con.Close();
                }
            }
        }

        void ДобавитьТаблицу_Click(object sender, RoutedEventArgs e)
        {
            var swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");

            var swModel = (IModelDoc2)swApp.IActiveDoc;
            TableAnnotation myTable;
            //InsertTable(swModel, out myTable);

            var обозначения = new[] { "AV 0989897", "-01", "-02", "-03", "-04" };

            InsertTableРис(swModel, обозначения);

            swModel.Extension.SelectByID2("Общая таблица7", "GENERALTABLEFEAT", 0, 0, 0, false, 0, null, 0);
            swModel.SelectedFeatureProperties(0, 0, 0, 0, 0, 0, 0, true, true, "Общая таблица4_15");
      
            var myModelView = ((ModelView)(swModel.ActiveView));
            myModelView.TranslateBy(-0.00040965517241379308, 0.00040965517241379308);

            //boolstatus = swDoc.Extension.SelectByID2("Детальный элемент86@Sheet1", "ANNOTATIONTABLES", 0.42799491570140613, 0.28291848892280747, 0, false, 0, null, 0);
            //boolstatus = swDoc.Extension.SelectByID2("Детальный элемент86@Sheet1", "ANNOTATIONTABLES", 0.43157101326023029, 0.28403601940994005, 0, false, 0, null, 0);
//            swModel.ClearSelection2(true);
        }

        static void InsertTable(IModelDoc2 swModel, out TableAnnotation myTable)
        {
            var swDrawing = ((DrawingDoc) (swModel));
            myTable = swDrawing.InsertTableAnnotation(0.417, 0.292, 2, 4, 2);
            if ((myTable == null)) return;
            myTable.BorderLineWeight = 2;
            myTable.GridLineWeight = 1;
            myTable.Text[0, 0] = "Обозначение";
            myTable.SetColumnWidth(0, 0.05, 0);
            myTable.Text[0, 1] = "Рис.";
            myTable.SetColumnWidth(1, 0.02, 0);
            //myTable.InsertRow();
        }

        static void InsertTableРис(IModelDoc2 swModel, string[] обозначения)
        {
            var swDrawing = ((DrawingDoc)(swModel));
            var enumerable = обозначения as string[] ?? обозначения.ToArray();
            var myTable = swDrawing.InsertTableAnnotation(0.417, 0.292, 2, enumerable.Count()+1, 2);
            if ((myTable == null)) return;
            myTable.Title = "Таблица видов";
            myTable.BorderLineWeight = 2;
            myTable.GridLineWeight = 1;
            myTable.Text[0, 0] = "Обозначение";
           
            myTable.SetColumnWidth(0, 0.05, 0);
            myTable.Text[0, 1] = "Рис.";
            myTable.SetColumnWidth(1, 0.02, 0);
            
            for (var i = 1; i < обозначения.Count()+1; i++)
            {
                myTable.Text[i, 0] = обозначения[i-1];
                if (i>1)
                {
                    myTable.set_CellTextHorizontalJustification(i, 0, (int)swTextJustification_e.swTextJustificationRight);    
                }
                myTable.Text[i, 1] = (i).ToString();
            }
        }

        void AutoCompleteTextBox1_DropDownClosed(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            if (ВыгрузитьВXml.IsEnabled)
            {
                ИмяСборки1.Text = AutoCompleteTextBox1.Text;
             //   Поиск_Click(null, null);
            }
        }

        void ПолучитьПереченьДеталей_Click(object sender, RoutedEventArgs e)
        {
            //if (ПолучитьПереченьДеталей.IsChecked == true)
            //{
            //    _путьКСборке = _работаСоСпецификацией.ПолучениеПутиКСборке(AutoCompleteTextBox1.Text);

            //    if (_путьКСборке != "")
            //    {
            //        Конфигурация.ItemsSource = _работаСоСпецификацией.ПолучениеКонфигураций(_путьКСборке).ToList();
            //        Конфигурация.IsEnabled = true;
            //        Конфигурация.SelectedIndex = 0;
            //    }
            //    else
            //    {
            //        MessageBox.Show("Сборка не найдена!");
            //    }
            //}
            //else
            //{
            //    Конфигурация.Text = "";
            //    Конфигурация.IsEnabled = false;
            //}
        }

        class PartsListXml
        {
            public bool Xml { get; set; }
            public int CurrentVersion { get; set; }
            public string Путь {  get; set; }
            public string Наименование => Path.GetFileNameWithoutExtension(Путь);
        }


        class PartsListXml2
        {
            public bool Xml { get; set; }
            public int CurrentVersion { get; set; }
            public int? IdPmd { get; set; }
            public string Наименование { get; set; }
            public string Путь { get; set; }


            public string НаименованиеБезРасширения { get; set; }

            public string PartNumber { get; set; }
            public string Конфигурация { get; set; }
            public string ЗаготовкаШирина { get; set; }
            public string ЗаготовкаВысота { get; set; }
            public string Гибы { get; set; }
            public string Толщина { get; set; }
            public string ПокараскаX { get; set; }
            public string ПокараскаY { get; set; }
            public string ПокараскаZ { get; set; }
            public string ПлощадьПокрытия { get; set; }


            public string Материал { get; set; }
            

            public string ПлощадьS
            {
                get {
                    try
                    {
                        return ((Math.Round(Convert.ToDouble(ЗаготовкаШирина)*Convert.ToDouble(ЗаготовкаВысота)*2/10000))/100).ToString();
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }


            public string МассаS
            {
                get
                {
                    try
                    {
                        return ((Math.Round(Convert.ToDouble(ПлощадьПокрытия) * Convert.ToDouble(Толщина) * 780)) /100 ).ToString();
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }

        }

        void ВыгрузитьВXml_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ВыгрузитьВXml.IsEnabled)
            {
                ПолучитьПереченьДеталей.IsEnabled = true;
            }
            else
            {
                ПолучитьПереченьДеталей.IsEnabled = false;
                ПолучитьПереченьДеталей.IsChecked = false;
            }
        }

        void Конфигурация_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var partsListXml = new List<PartsListXml>();

            foreach (var данныеДляВыгрузки in _работаСоСпецификацией.Спецификация(_путьКСборке, 10, Конфигурация.Text).Where(x => x.Путь.ToLower().EndsWith("prt")).Where(x => x.Раздел == "Детали" || x.Раздел == "").GroupBy(x => new { x.Путь, x.Версия }))
            {
                partsListXml.Add(new PartsListXml
                {
                    Путь = Convert.ToString(данныеДляВыгрузки.Key.Путь),
                    CurrentVersion = Convert.ToInt32(данныеДляВыгрузки.Key.Версия),
                    //listXml.Xml = ExistLastXml(listXml.Путь, listXml.CurrentVersion);
                });
            }

            foreach (var listXml in partsListXml)
            {
                listXml.Xml = ExistLastXml(listXml.Путь, listXml.CurrentVersion);
            }
         
            PartsList.ItemsSource = partsListXml.OrderBy(x => x.Xml).ThenBy(x => x.Наименование);
        }

        static bool ExistLastXml(string partPath, int currentVersion)
        {
            var xmlPartPath =
                new FileInfo(@"\\srvkb\SolidWorks Admin\XML\" + Path.GetFileNameWithoutExtension(partPath) + ".xml");

            //var xmlPartPath =
            //    new FileInfo(@"\\srvkb\XML\" + Path.GetFileNameWithoutExtension(partPath) + ".xml");

            //MessageBox.Show(partPath);
            //MessageBox.Show(xmlPartPath.FullName);

            if (!xmlPartPath.Exists) return false;

            var xmlPartVersion = Version(xmlPartPath.FullName);

            return Equals(xmlPartVersion, currentVersion);
        }

        void PartsList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var partsListXml = (PartsListXml)e.Row.DataContext;

            if (partsListXml.Xml)
            {
                e.Row.Background = e.Row.GetIndex() % 2 == 1 ? _lightCyanColorBrush : _whiteColorBrush;
            }
            else
            {
                e.Row.Background = _orangeColorBrush;
            }
        }

        void XMLParts_Click(object sender, RoutedEventArgs e)
        {
            var modelSw = new ModelSw();

            var list = PartsListXml2sDataGrid.ItemsSource.OfType<PartsListXml2>().ToList();

            foreach (var newComponent in list)
            {
                if (!newComponent.Xml)
                {
                    MessageBox.Show(newComponent.Путь, newComponent.Наименование);
                    modelSw.PartInfoToXml(newComponent.Путь);
                }
            }

            #region

            //var list = PartsList.ItemsSource.OfType<PartsListXml>().ToList();

            //foreach (var newComponent in list)
            //{
            //    if (!newComponent.Xml)
            //    {
            //        modelSw.PartInfoToXml(newComponent.Путь);
            //    }
            //}

            //Конфигурация_SelectionChanged(null, null);

            #endregion
        }

        void Конфигурация_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Конфигурация.Visibility == Visibility.Visible )
            {
                PartsList.Visibility = Visibility.Visible;
            }
            else
            {
                PartsList.Visibility = Visibility.Hidden;
                PartsList.ItemsSource = null;
            }
        }

        void PartsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            XmlParts.Visibility = PartsList.HasItems ? Visibility.Visible : Visibility.Hidden;
        }

        void ПолучитьПереченьДеталей_Unchecked(object sender, RoutedEventArgs e)
        {
            Конфигурация.Text = "";
            Конфигурация.IsEnabled = false;
            PartsList.ItemsSource = null;
            ПереченьДеталей.Visibility = Visibility.Hidden;


            if (PartsList.HasItems)
            {
                XmlParts.Visibility = Visibility.Visible;
                PartsList.Visibility = Visibility.Visible;
            }
            else
            {
                XmlParts.Visibility = Visibility.Hidden;
                PartsList.Visibility = Visibility.Hidden;
            }
        }
        
        private Epdm.BomCells[] _specBomCells;
        
        void ПолучитьПереченьДеталей_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (FileType == "prt")
                {
                    var emdpService = new Epdm();
                    var path = emdpService.SearchDoc(AutoCompleteTextBox1.Text + ".SLDPRT");

                    var partsListXml2S = new List<PartsListXml2>
                    {
                        new PartsListXml2
                        {
                            Наименование = Path.GetFileNameWithoutExtension(path[0].FilePath),
                            Путь = path[0].FilePath
                        }
                    };

                    PartsListXml2sDataGrid.ItemsSource = null;
                    PartsListXml2sDataGrid.ItemsSource = partsListXml2S;

                    ПереченьДеталей_Copy.Visibility = PartsListXml2sDataGrid.ItemsSource == null
                        ? Visibility.Hidden
                        : Visibility.Visible;

                    PartsListXml2sDataGrid_Copy.Visibility = Visibility.Collapsed;
                    XmlParts1_Copy.Visibility = Visibility.Collapsed;
                }
                else
                {
                    var emdpService = new Epdm(); 
                    var path = emdpService.SearchDoc(AutoCompleteTextBox1.Text + ".SLDASM");
                    var configs = emdpService.GetConfiguration(path[0].FilePath);
                    var itemsSource = configs as IList<string> ?? configs.ToList();
                    Конфигурация.ItemsSource = itemsSource;

                    var bomClass = new Epdm
                    {
                        BomId = 8,
                        AssemblyPath = path[0].FilePath
                    };
                    
                    var спецификация = bomClass.BomList(path[0].FilePath, itemsSource[0]);
                    
                    var partsListXml2S = new List<PartsListXml2>();

                    foreach (var item in спецификация)
                    {
                        if (item.ТипФайла.ToLower() == "sldprt")
                        {
                            if (item.Раздел == "Детали" || item.Раздел == "")
                            {
                                partsListXml2S.Add(new PartsListXml2
                                {
                                    CurrentVersion = Convert.ToInt32(item.ПоследняяВерсия),
                                    IdPmd = item.IdPdm,
                                    Наименование = item.FileName,
                                    Путь = item.FilePath + @"\" + item.FileName,
                                    Конфигурация = item.Конфигурация,
                                    Материал = item.Материал
                                });
                            }
                        }
                    }

                    foreach (var listXml in partsListXml2S)
                    {
                        listXml.Xml = ExistLastXml(listXml.Путь, listXml.CurrentVersion);
                        listXml.НаименованиеБезРасширения = listXml.Наименование.ToUpper().Replace(".SLDPRT", "");
                    }

                    var list = InnerPartsList();


                    var newList = from partsListXml2 in partsListXml2S
                                  join listXml2 in list
                                  on partsListXml2.НаименованиеБезРасширения.ToLower() equals
                                  listXml2.PartNumber.ToLower()
                                  select new
                                  {
                                      partsListXml2.НаименованиеБезРасширения,
                                      listXml2.PartNumber,

                                      listXml2.ЗаготовкаВысота,
                                      listXml2.Толщина,
                                      listXml2.ЗаготовкаШирина,
                                      listXml2.Гибы,
                                      listXml2.ПлощадьПокрытия,
                                      listXml2.Конфигурация,
                                      listXml2.ПлощадьS,
                                      listXml2.МассаS,
                                      partsListXml2.Материал
                                  };


                    var newList2 = newList.Select(variable => new PartsListXml2
                    {
                        НаименованиеБезРасширения = variable.НаименованиеБезРасширения,
                        PartNumber = variable.PartNumber,
                        ЗаготовкаВысота = variable.ЗаготовкаВысота,
                        Толщина = variable.Толщина,
                        ЗаготовкаШирина = variable.ЗаготовкаШирина,
                        Гибы = variable.Гибы,
                        ПлощадьПокрытия = variable.ПлощадьПокрытия,
                        Конфигурация = variable.Конфигурация,
                        Материал = variable.Материал
                    }).ToList();


                    var newListOrder = new List<PartsListXml2>();
                    foreach (var partsListXml2 in partsListXml2S)
                    {
                        try
                        {
                            var selectedItem =
                            newList2.Where(x => string.Equals(x.НаименованиеБезРасширения.ToLower(), partsListXml2.Наименование.Replace(".SLDPRT", "").ToLower(), StringComparison.CurrentCultureIgnoreCase) && x.Конфигурация == partsListXml2.Конфигурация);

                            foreach (var listXml2 in selectedItem)
                            {
                                if (newListOrder.Any(x => string.Equals(x.НаименованиеБезРасширения,
                                                listXml2.НаименованиеБезРасширения,
                                                StringComparison.CurrentCultureIgnoreCase) &&
                                            x.Конфигурация == listXml2.Конфигурация)) continue;

                                newListOrder.Add(new PartsListXml2
                                {
                                    НаименованиеБезРасширения = listXml2.НаименованиеБезРасширения,
                                    PartNumber = listXml2.PartNumber,

                                    ЗаготовкаВысота = listXml2.ЗаготовкаВысота,
                                    Толщина = listXml2.Толщина,
                                    ЗаготовкаШирина = listXml2.ЗаготовкаШирина,
                                    Гибы = listXml2.Гибы,
                                    ПлощадьПокрытия = listXml2.ПлощадьПокрытия,
                                    Конфигурация = listXml2.Конфигурация,

                                    Материал = listXml2.Материал
                                });
                            }
                        }
                        catch (Exception){}
                    }

                    PartsListXml2sDataGrid.ItemsSource = null;
                    PartsListXml2sDataGrid.ItemsSource = partsListXml2S;

                    ПереченьДеталей_Copy.Visibility = PartsListXml2sDataGrid.ItemsSource == null
                        ? Visibility.Hidden
                        : Visibility.Visible;

                    PartsListXml2sDataGrid_Copy.Visibility = Visibility.Visible;
                    XmlParts1_Copy.Visibility = Visibility.Visible;

                    PartsListXml2sDataGrid_Copy.ItemsSource = newListOrder.OrderBy(x=>x.НаименованиеБезРасширения).ThenBy(x => x.Конфигурация); 
                    
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }

        void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (PartsListXml2)PartsList.SelectedItem;
            MessageBox.Show(item?.Путь, item?.PartNumber);
        }

        #region XML File Version

        static int? Version(string xmlPath)
        {
            if (!xmlPath.EndsWith("xml")){return null;}

            int? version = null;

            try
            {
                var coordinates = XDocument.Load(xmlPath);

                var enumerable = coordinates.Descendants("attribute")
                                    .Select(
                                            element =>
                                            new
                                            {
                                                Number = element.FirstAttribute.Value,
                                                Values = element.Attribute("value")
                                            });
                foreach (var obj in enumerable)
                {
                    if (obj.Number != "Версия") continue;

                    version = Convert.ToInt32(obj.Values.Value);
                }
            }
            catch (Exception){}
            
            return version;
        }

        #endregion

        void OpenFile(object sender, RoutedEventArgs e)
        {
            var item = (PartsListXml2)PartsList.SelectedItem;
            try
            {
                Process.Start(@item.Путь);
            }
            catch (Exception){}
        }

        void PartsList_LayoutUpdated(object sender, EventArgs e)
        {
            if (PartsList.ItemsSource == null) return;
            var list = PartsList.ItemsSource.OfType<PartsListXml>().ToList();
            Total.Content = "Всего: " + list.Count();
            Ready.Content = "Выгружено: " + list.Count(x => x.Xml);
        }

        void Button_Click(object sender, RoutedEventArgs e)
        {

            var swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");

            var swModel = (ModelDoc2) swApp.ActiveDoc;

            MessageBox.Show(swModel.GetTitle());

            swModel.EditRebuild3();
            var swPart = (PartDoc)swModel;
            var arrNamesConfig = (string[])swModel.GetConfigurationNames();

            Feature swFeature = swPart.FirstFeature();
            const string strSearch = "FlatPattern";

            while (swFeature != null)
            {
                var nameTypeFeature = swFeature.GetTypeName2();

                if (nameTypeFeature == strSearch)
                {
                    Feature swSubFeature = swFeature.GetFirstSubFeature();
                    while (swSubFeature != null)
                    {
                        var nameTypeSubFeature = swSubFeature.GetTypeName2();
                        
                        if (nameTypeSubFeature == "UiBend")
                        {
                            swSubFeature.SetSuppression2((int)swFeatureSuppressionAction_e.swUnSuppressFeature, (int)swInConfigurationOpts_e.swAllConfiguration,
                                arrNamesConfig);
                        }
                        swSubFeature = swSubFeature.GetNextSubFeature();
                    }
                }
                swFeature = swFeature.GetNextFeature();
            }
            swModel.EditRebuild3();
        }

        void ПолучитьПереченьДеталей_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // throw new NotImplementedException();
        }

        void XmlParts1_Click(object sender, RoutedEventArgs e)
        {
            var modelSw = new ModelSw();

            var list = PartsListXml2sDataGrid.ItemsSource.OfType<PartsListXml2>().ToList();

            foreach (var newComponent in list)
            {
                if (!newComponent.Xml)
                {
                    modelSw.PartInfoToXml(newComponent.Путь);
                }
            }
        }

        static void PartInfoToXml(string путь)
        {
            try
            {
                var modelSw = new ModelSw();
                modelSw.PartInfoToXml(путь);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        EnumerableRowCollection<DataRow> PartNumberData(string partName)
        {
            var sqlBaseData = new SqlBaseData();
            var table = sqlBaseData.PartTechParams();
            var results = from myRow in table.AsEnumerable()
                          where myRow.Field<string>("PartNumber") == partName
                          select myRow;

            return results;
        }


        static IEnumerable<PartsListXml2> InnerPartsList()
        {
            var sqlBaseData = new SqlBaseData();
            var table = sqlBaseData.PartTechParams();
            var list = (from DataRow row in table.Rows
                        select new PartsListXml2
                        {
                            PartNumber = row["PartNumber"].ToString(),
                            Конфигурация = row["Конфигурация"].ToString(),
                            ЗаготовкаШирина = row["Заготовка Ширина"].ToString(),
                            ЗаготовкаВысота = row["Заготовка Высота"].ToString(),
                            Гибы = row["Гибы"].ToString(),
                            Толщина = row["Толщина"].ToString(),
                            ПлощадьПокрытия = row["Площадь покрытия"].ToString()
                        }).ToList();
            return list;
        }


        void XmlParts1_Copy_Click(object sender, RoutedEventArgs e)
        {
            PartsListXml2sDataGrid_Copy.SelectAllCells();
        }
        
        void buttonParttoxml_Click(object sender, RoutedEventArgs e)
        {
            PartInfoToXml(textBoxPath.Text);
        }
        void DxfParts1_Click(object sender, RoutedEventArgs e)
        {
            var modelSw = new ModelSw();
            //  modelSw.CreateFlattPatternUpdateCutlistAndEdrawing(@"E:\Vents-PDM\Проекты\Blauberg\02-01-Panels\02-01-01-1000-1000-50-AZ.sldprt");

            var list = PartsListXml2sDataGrid.ItemsSource.OfType<PartsListXml2>().ToList();

            foreach (var newComponent in list)
            {
                modelSw.CreateFlattPatternUpdateCutlistAndEdrawing(newComponent.Путь, checkBox.IsChecked == true ? newComponent.Конфигурация : null, true);
                //if (!newComponent.Xml)
                //{
                //    //MessageBox.Show(newComponent.Путь);
                //    modelSw.PartInfoToXml(newComponent.Путь);
                //}
            }
        }

        private void button_Click_4(object sender, RoutedEventArgs e)
        {
            var dir = new DirectoryInfo(@"C:\DXF\Parts");
            var modelSw = new ModelSw();

            foreach (var fileInfo in dir.GetFiles())
            {
                try
                {
                    //modelSw.CreateFlattPatternUpdateCutlistAndEdrawing(fileInfo.FullName, null, true);
                    modelSw.CreateEprt(fileInfo.FullName, true);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
            MessageBox.Show("Process finished");
        }
    }
}
