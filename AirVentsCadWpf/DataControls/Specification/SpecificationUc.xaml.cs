using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using AirVentsCadWpf.AirVentsClasses;
using AirVentsCadWpf.AirVentsClasses.UnitsBuilding;
using AirVentsCadWpf.Properties;
using ExportPartData;
using HostingWindowsForms.EPDM;
using VentsCadLibrary;

namespace AirVentsCadWpf.DataControls.Specification
{
    /// <summary>
    /// 
    /// </summary>
    public partial class SpecificationUc
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecificationUc"/> class.
        /// </summary>
        public SpecificationUc()
        {
            InitializeComponent();
            ПереченьДеталей_Copy.Visibility = Visibility.Hidden;
            dataGridAfterGet.Visibility = Visibility.Hidden;

            checkBoxOnlyAsms.Visibility = Visibility.Hidden;
            //checkBox.Visibility = Visibility.Hidden;
            COnf.Visibility = Visibility.Hidden;
        }
        
        string ПутьКСборке { get; set; }

        List<string> AssemblyConfigs { get; set; }
        
        string CurrentModel { get; set; }

        void НайтиПолучитьСборкуЕеКонфигурацииПоИмени()
        {
            try
            {
                var emdpService = new Epdm();
                var configs = emdpService.GetConfiguration(ПутьКСборке);
                if (configs == null) return;

                var itemsSource = configs as IList<string> ?? configs.ToList();

                var bomClass = new Epdm
                {
                    BomId = 8,
                    AssemblyPath = ПутьКСборке
                };
                Exception exception;

                SpecBomCells = bomClass.BomList(ПутьКСборке, itemsSource[0], false, out exception).ToArray();

                if (exception != null)
                {
                    MessageBox.Show(exception.StackTrace);
                }
            }
            catch (Exception exception)
            {
                  MessageBox.Show(exception.StackTrace);
            }
        }
        
        static void ВыгрузитьСборку(string путьКСборке)
        {
            var имяСборки = new FileInfo(путьКСборке).Name.Replace(new FileInfo(путьКСборке).Extension, "");

            var emdpService = new Epdm();

            IEnumerable<string> списокКонфигурацийСборки = null;

            try
            {
                списокКонфигурацийСборки = emdpService.GetConfiguration(путьКСборке);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }

            try
            {
                const string xmlPath = @"\\srvkb\SolidWorks Admin\XML\";

                int? lastVerOfAsm = null;

                try
                {
                    var bomClassGetLastVersion = new Epdm
                    {
                        BomId = 8,
                        AssemblyPath = путьКСборке
                    };
                    Exception exception;
                    var spec = bomClassGetLastVersion.BomList(путьКСборке, "00", false, out exception);
                    lastVerOfAsm = spec[0].ПоследняяВерсия;
                    if (exception != null)
                    {
                        MessageBox.Show(exception.StackTrace);
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.StackTrace);
                }

                var exist = false;

                try
                {
                    if (lastVerOfAsm != null) exist =
                            ExportXmlSql.ExistXml(путьКСборке, (int)lastVerOfAsm);
                            //ExistLastXml(путьКСборке, (int)lastVerOfAsm, false);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }

                if (exist) return;

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

                if (списокКонфигурацийСборки != null)
                    foreach (var config in списокКонфигурацийСборки)
                    {
                        var bomClass = new Epdm
                        {
                            BomId = 8,
                            AssemblyPath = путьКСборке
                        };
                        Exception exception;
                        var спецификация = bomClass.BomList(путьКСборке, config, false, out exception);
                        if (exception != null)
                        {
                            MessageBox.Show(exception.StackTrace);
                        }

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
                            myXml.WriteAttributeString("value", topAsm.Раздел == "Материалы" ? "" : topAsm.Обозначение);
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
                                myXml.WriteAttributeString("value", topLevel.Раздел == "Материалы" ? "" : topLevel.Обозначение);
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
                                myXml.WriteAttributeString("value", "");
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
                    }

                myXml.WriteEndElement(); // ' элемент DOCUMENT
                myXml.WriteEndElement(); // ' элемент TRANSACTION
                myXml.WriteEndElement(); // ' элемент TRANSACTIONS
                myXml.WriteEndElement(); // ' элемент XML
                                         
                myXml.Flush();

                myXml.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        static void ВыгрузитьСборкуПеречень(string путьКСборке)
        {

            var имяСборки = new FileInfo(путьКСборке).Name.Replace(new FileInfo(путьКСборке).Extension, "");
            var specification = new List<Epdm.BomCells>() ;

            try
            {
                const string xmlPath = @"\\srvkb\SolidWorks Admin\XML\";

                int? lastVerOfAsm = null;

                try
                {
                    var bomClassGetLastVersion = new Epdm
                    {
                        BomId = 8,
                        AssemblyPath = путьКСборке
                    };
                    Exception exception;
                    var spec = bomClassGetLastVersion.BomList(путьКСборке, "00", false, out exception);
                    specification = spec;
                    if (exception != null)
                    {
                        MessageBox.Show(exception.StackTrace);
                    }
                    lastVerOfAsm = spec[0].ПоследняяВерсия;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.StackTrace);
                }

                //var exist = false;
                //try
                //{
                //    if (lastVerOfAsm != null) exist = ExistLastXml(путьКСборке, (int)lastVerOfAsm, true);
                //}
                //catch (Exception e)
                //{
                //    MessageBox.Show(e.ToString(), "Try to find if exist XML");
                //}
                //if (!exist) return;

                var myXml = new XmlTextWriter(xmlPath + имяСборки + " Parts.xml", Encoding.UTF8);

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

                var спецификация = specification;

                var allParts =
                    спецификация.Where(x => x.ТипФайла.ToLower() == "sldprt")
                        .Where(x => x.Раздел == "Детали" || x.Раздел == "").OrderBy(x => x.FileName).ToList();

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
                    myXml.WriteAttributeString("value", topAsm.Раздел == "Материалы" ? "" : topAsm.Обозначение);
                        // 1C Для раздела "материалов" вставляем ПУСТО в обозначение из-за конфликта с 1С 
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
                    myXml.WriteAttributeString("value", ""); //topAsm..КодДокумента);
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

                    foreach (var part in allParts)
                    {
                        #region XML

                        // Конфигурация
                        myXml.WriteStartElement("configuration");
                        myXml.WriteAttributeString("name", part.Конфигурация);

                        // Версия
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Версия");
                        myXml.WriteAttributeString("value", part.ПоследняяВерсия.ToString());
                        myXml.WriteEndElement();

                        // Масса
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Масса");
                        myXml.WriteAttributeString("value", part.Weight);
                        myXml.WriteEndElement();

                        // Наименование
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Наименование");
                        myXml.WriteAttributeString("value", part.Наименование);
                        myXml.WriteEndElement();

                        // Обозначение
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Обозначение");
                        myXml.WriteAttributeString("value", part.Раздел == "Материалы" ? "" : part.Обозначение);
                        myXml.WriteEndElement();

                        // Раздел
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Раздел");
                        myXml.WriteAttributeString("value", part.Раздел);
                        myXml.WriteEndElement();

                        // ERP code
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "ERP code");
                        myXml.WriteAttributeString("value", part.ErpCode);
                        myXml.WriteEndElement();

                        // Код_Материала
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Код_Материала");
                        myXml.WriteAttributeString("value", part.CodeMaterial);
                        myXml.WriteEndElement();

                        // Код документа
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Код документа");
                        myXml.WriteAttributeString("value", ""); // topLevel.КодДокумента);
                        myXml.WriteEndElement();

                        // Кол. Материала
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Кол. Материала");
                        myXml.WriteAttributeString("value", part.SummMaterial);
                        myXml.WriteEndElement();

                        // Состояние
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Состояние");
                        myXml.WriteAttributeString("value", part.Состояние);
                        myXml.WriteEndElement();

                        // Подсчет ссылок
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Подсчет ссылок");
                        myXml.WriteAttributeString("value", part.Количество.ToString());
                        myXml.WriteEndElement();

                        // Конфигурация
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Конфигурация");
                        myXml.WriteAttributeString("value", part.Конфигурация);
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

                myXml.WriteEndElement(); // ' элемент DOCUMENT
                myXml.WriteEndElement(); // ' элемент TRANSACTION
                myXml.WriteEndElement(); // ' элемент TRANSACTIONS
                myXml.WriteEndElement(); // ' элемент XML
                                         // заносим данные в myMemoryStream
                myXml.Flush();

                myXml.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    
        void ВыгрузкаСборкиВXml()
        {
            if (ПутьКСборке == null) return;
         
            #region Выгрузка Главной Сборки

            try
            {
                ВыгрузитьСборкуПеречень(ПутьКСборке);
                ВыгрузитьСборку(ПутьКСборке);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace);
            }          

            #endregion

            #region Выгрузка подсборок

            foreach (var путьКСборке in SpecBomCells.Where(x =>
            {
                var extension = Path.GetExtension(x.FilePath + "\\" + x.FileName);
                return extension.ToLower() == ".sldasm";
            })
                .Where(x => x.Раздел == "" || x.Раздел == "Сборочные единицы")
                .Select(x => x.FilePath + "\\" + x.FileName)
                .Distinct())
            {
                try
                {
                   // MessageBox.Show(путьКСборке);
                    ВыгрузитьСборку(путьКСборке);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.StackTrace);
                }
            }

            #endregion
        }

        static string FileName(string filePath)
        {
            var fileName = new FileInfo(filePath).Name;
            return fileName;
        }
    
        void AutoCompleteTextBox1Reload()
        {
            List< SwEpdm.EpdmSearch.FindedDocuments> найденныеФайлы;
            List<SwEpdm.EpdmSearch.FindedDocuments> найденныеФайлы1;

            VaultSystem.SearchInVault.SearchDoc(
                AutoCompleteTextBox1.Text,
                SwEpdm.EpdmSearch.SwDocType.SwDocAssembly,
                out найденныеФайлы, 
                Settings.Default.PdmBaseName);
            
            VaultSystem.SearchInVault.SearchDoc(
                AutoCompleteTextBox1.Text,
                SwEpdm.EpdmSearch.SwDocType.SwDocPart,
                out найденныеФайлы1,
                Settings.Default.PdmBaseName);
            
            if (найденныеФайлы == null & найденныеФайлы1 == null) return;

            if (найденныеФайлы1 != null & найденныеФайлы != null) найденныеФайлы.AddRange(найденныеФайлы1);
            if (найденныеФайлы1 != null & найденныеФайлы == null) найденныеФайлы = найденныеФайлы1;

            var newNames = найденныеФайлы.Select(findedDocuments => findedDocuments.Path).ToList();
                FilePath = newNames.ConvertAll(FileName);
            
            AutoCompleteTextBox1.ItemsSource = FilePath;
            AutoCompleteTextBox1.FilterMode = AutoCompleteFilterMode.Contains;
        }

        List<string> FilePath { get; set; }

        void Button_Click_3(object sender, RoutedEventArgs e)
        {
            AutoCompleteTextBox1Reload();
        }
        
        void EnabledExport()
        {
            #region To Delete

            //var currentPath = AutoCompleteTextBox1.Text == CurrentModel;
            //ПолучитьПереченьДеталей.IsChecked = currentPath;
            //XmlParts1.IsEnabled = currentPath;
            //PartsListXml2sDataGrid.IsEnabled = currentPath;    

            #endregion

            Найти.IsEnabled = AutoCompleteTextBox1.Text.Length != 0;
            GetList.Content = AutoCompleteTextBox1.Text.ToLower().EndsWith("prt") ? "Получить данные" : "Получить перечень деталей";
        }

        class PartsListXml2
        {
            public string Hash { get; set; }

            public bool Dxf { get; set; }
            public bool Xml { get; set; }
            public int CurrentVersion { get; set; }
            public int IdPmd { get; set; }
            public string Наименование { get; set; }
            public string Путь { get; set; }
            public string НаименованиеБезРасширения { get; set; }
            public string PartNumber { get; set; }
            public string Конфигурация { get; set; }
            public string ЗаготовкаШирина { get; set; }
            public string ЗаготовкаВысота { get; set; }
            public string Гибы { get; set; }
            public string Толщина { get; set; }     
            public string ImageSrc { get; set; }
            public string ПлощадьПокрытия { get; set; }
            public string Материал { get; set; }
            public string ПлощадьS { get; set; }
            public string ТипОбъекта { get; set; }
            public string МассаS { get; set; }
        }

        string GetMass(string ПлощадьПокрытия, string Толщина)
        {
            try
            {
                return ((Math.Round(Convert.ToDouble(ПлощадьПокрытия) * Convert.ToDouble(Толщина) * 780)) / 100).ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }

        class Files
        {
            public string Путь { get; set; }

            public string Имя => Path.GetFileNameWithoutExtension(Путь);

            static List<Files> GetFiles(string path)
            {
                var files = Directory.GetFiles(path);
                var list = files.Select(file => new Files
                {
                    Путь = file
                }).ToList();

                return list;
            }
        }
        
        Epdm.BomCells[] SpecBomCells { get; set; }

        void ПолучитьПереченьДеталей()
        {
            try
            {
                if (AutoCompleteTextBox1.Text.ToLower().EndsWith("dprt"))
                {
                    var emdpService = new Epdm();
                    var path = emdpService.SearchDoc(AutoCompleteTextBox1.Text);
                    var filePath = path[0].FilePath;
                    ПутьКСборке = filePath;
                    var lastVersion = SwEpdm.GetVersionOfFile(filePath, Settings.Default.PdmBaseName);
                    int pdmId;
                    int curVer;
                    string name;
                    List<string> configurations;
                    SwEpdm.GetIdPdm(filePath, out name, out pdmId, out curVer, out configurations, true); ;//, Settings.Default.PdmBaseName);

                    //MessageBox.Show(configurations.Count.ToString());

                    var partsListXml2S = new List<PartsListXml2>();

                    var listOfExc = new List<Exception>();

                    foreach (var configname in configurations)
                    {
                        Exception exc;
                        var existDxf = ExistLastDxf(pdmId, curVer, configname, out exc);
                        if (exc != null)
                        {
                            listOfExc.Add(exc);
                        } 

                        partsListXml2S.Add(new PartsListXml2
                        {
                            Наименование = Path.GetFileNameWithoutExtension(filePath),
                            Путь = filePath,
                            Xml = ExportXmlSql.ExistXml(filePath, lastVersion),
                        //ExistLastXml(filePath, lastVersion, false),
                            CurrentVersion = lastVersion,
                            Конфигурация = configname,
                            IdPmd = pdmId,
                            Dxf = existDxf,
                            ImageSrc = existDxf ?
                                        @"\DataControls\Pictures\cancel.jpg" : @"\DataControls\Pictures\empty.jpg",
                            НаименованиеБезРасширения = Path.GetFileNameWithoutExtension(filePath)
                        });
                    }                  

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
                    if (CurrentModel != AutoCompleteTextBox1.Text)
                    {
                        var emdpService = new Epdm();
                        var path = emdpService.SearchDoc(AutoCompleteTextBox1.Text);                         
                        ПутьКСборке = path[0].FilePath;
                        var configs = emdpService.GetConfiguration(path[0].FilePath);
                        AssemblyConfigs = configs.ToList();
                        ConfigsCombo.ItemsSource = AssemblyConfigs;
                        COnf.Visibility = Visibility.Visible;
                        ConfigsCombo.Text = AssemblyConfigs[0];
                    }

                    var bomClass = new Epdm
                    {
                        BomId = 8,
                        AssemblyPath = ПутьКСборке
                    };
                    Exception exception;
                    //string.IsNullOrEmpty(ConfigsCombo.Text) ? AssemblyConfigs[0] : 
                    var спецификация = bomClass.BomList(ПутьКСборке, ConfigsCombo.Text, false, out exception);
                    if (exception != null)
                    {
                        MessageBox.Show(exception.StackTrace);
                    }                    

                    //MessageBox.Show(спецификация.Where(x=>x.ТипФайла.ToLower() == "sldprt").Where(x => x.Раздел == "Детали" || x.Раздел == "").Count().ToString());

                    var partsListXml2S = new List<PartsListXml2>();
                    //var excList = new List<Exception>();

                    foreach (var item in спецификация)
                    {
                        if (!item.ТипФайла.ToLower().Contains("dprt")) continue;
                        //Exception exc;
                        //var existDxf = ExistLastDxf((int)item.IdPdm, (int)item.ПоследняяВерсия, item.Конфигурация, out exc);
                        //if (exc != null){ excList.Add(exc); } 
                        if (item.Раздел == "Детали" || item.Раздел == "")
                        {
                            partsListXml2S.Add(new PartsListXml2
                            {
                                CurrentVersion = Convert.ToInt32(item.ПоследняяВерсия),
                                IdPmd = (int)item.IdPdm,
                                Наименование = item.FileName,
                                Путь = item.FilePath + @"\" + item.FileName,
                                Конфигурация = item.Конфигурация,
                                Материал = item.Материал,
                                //Dxf = existDxf,
                                //ImageSrc = existDxf ?
                                //        @"\DataControls\Pictures\cancel.jpg" : @"\DataControls\Pictures\empty.jpg"  
                            });
                        }
                    }

                    #region to delete
                    //var message = "";
                    //foreach (var item in excList)
                    //{
                    //    message = message + "\n_________\n" + item.Message + "\n" + item.StackTrace;
                    //}
                    //if (!string.IsNullOrEmpty(message))
                    //{
                    //    MessageBox.Show(message);
                    //} 
                    #endregion

                    foreach (var listXml in partsListXml2S)
                    {
                        listXml.Xml =
                            ExportXmlSql.ExistXml(listXml.Наименование, listXml.CurrentVersion);
                            //ExistLastXml(listXml.Путь, listXml.CurrentVersion, false);
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
                                newList2.Where(
                                    x =>
                                        string.Equals(x.НаименованиеБезРасширения.ToLower(),
                                            partsListXml2.Наименование.Replace(".SLDPRT", "").ToLower(),
                                            StringComparison.CurrentCultureIgnoreCase) &&
                                        x.Конфигурация == partsListXml2.Конфигурация);

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

                                    Материал = listXml2.Материал,
                                    МассаS = GetMass(listXml2.ПлощадьПокрытия, listXml2.Толщина)
                                });
                            }
                        }
                        catch (Exception exceptio)
                        {
                            MessageBox.Show(exceptio.StackTrace);
                        }
                    }

                    var listXml2S = new List<PartsListXml2>();

                    partsListXml2S = partsListXml2S.OrderBy(x => x.Наименование).ThenBy(x=>x.Конфигурация).ToList();
                    listXml2S.Add(partsListXml2S[0]);

                    for (var i = 0; i < partsListXml2S.Count - 1; i++)
                    {
                        var currentItem = partsListXml2S[i];
                        var newItem = partsListXml2S[i + 1];
                        if (!(currentItem.Наименование == newItem.Наименование && currentItem.Конфигурация == newItem.Конфигурация))
                        {                            
                            listXml2S.Add(newItem);
                        }
                    }

                    foreach (var item in listXml2S)
                    {
                        Exception exc;
                        var existDxf = ExistLastDxf((int)item.IdPmd, (int)item.CurrentVersion, item.Конфигурация, out exc);
                        item.Dxf = existDxf;
                        item.ImageSrc = existDxf ?
                                        @"\DataControls\Pictures\cancel.jpg" : @"\DataControls\Pictures\empty.jpg";
                    }



                    PartsListXml2sDataGrid.ItemsSource = null;
                    PartsListXml2sDataGrid.ItemsSource = listXml2S;

                    ПереченьДеталей_Copy.Visibility = PartsListXml2sDataGrid.ItemsSource == null
                        ? Visibility.Hidden
                        : Visibility.Visible;

                    PartsListXml2sDataGrid_Copy.Visibility = Visibility.Visible;
                    XmlParts1_Copy.Visibility = Visibility.Visible;

                    PartsListXml2sDataGrid_Copy.ItemsSource =
                        newListOrder.OrderBy(x => x.НаименованиеБезРасширения).ThenBy(x => x.Конфигурация);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
            finally
            {
                CurrentModel = AutoCompleteTextBox1.Text;
                var sdgv = (List<PartsListXml2>)PartsListXml2sDataGrid.ItemsSource;
                ПереченьДеталей_Copy.Header = CurrentModel.ToLower().EndsWith("prt") ? $"Деталь {CurrentModel}" : 
                    $"Перечень деталей для сборки {CurrentModel.ToUpper().Replace(".SLDASM","")}-{ConfigsCombo.Text} (Всего деталей: {PartsListXml2sDataGrid.Items.Count}шт. Разверток - {sdgv.Count(x=>x.Dxf)} 1С - {sdgv.Count(x => x.Xml)})";
            }
        }
        
        #region XML File Version

        //static int? Version(string xmlPath)
        //{
        //    if (!xmlPath.EndsWith("xml")){return null;}

        //    int? version = null;

        //    try
        //    {
        //        var coordinates = XDocument.Load(xmlPath);

        //        var enumerable = coordinates.Descendants("attribute")
        //            .Select(
        //                element =>
        //                    new
        //                    {
        //                        Number = element.FirstAttribute.Value,
        //                        Values = element.Attribute("value")
        //                    });
        //        foreach (var obj in enumerable)
        //        {
        //            if (obj.Number != "Версия") continue;

        //            version = Convert.ToInt32(obj.Values.Value);

        //            goto m1;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return 0;
        //    }
        //    m1:
        //    return version;
        //}

        //const string XmlPAth = @"C:\Temp\"; // @"\\srvkb\SolidWorks Admin\XML\";

        //static bool ExistLastXml(string partPath, int currentVersion, bool partsXml)
        //{
        //    try
        //    {
        //        var xmlPartPath =
        //        new FileInfo(XmlPAth + Path.GetFileNameWithoutExtension(partPath) + (partsXml ? " Parts" : null) + ".xml");

        //        if (!xmlPartPath.Exists) return false;

        //        var xmlPartVersion = Version(xmlPartPath.FullName);

        //        return Equals(xmlPartVersion, currentVersion);
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}

        static bool ExistLastDxf(int idPdm, int currentVersion, string configuration, out Exception exc)
        {
            exc = null;
            bool exist;
            try
            {
                exist = Dxf.ExistDxf(idPdm, currentVersion, configuration, out exc);
            }
            catch (Exception)
            {
                exist = false;
            }
            return exist;

            #region

            //return true;
            //try
            //{
            //    var xmlPartPath =
            //    new FileInfo(@"\\srvkb\SolidWorks Admin\XML\" + Path.GetFileNameWithoutExtension(partPath) + ".xml");

            //    if (!xmlPartPath.Exists) return false;

            //    var xmlPartVersion = Version(xmlPartPath.FullName);

            //    return Equals(xmlPartVersion, currentVersion);
            //}
            //catch (Exception)
            //{
            //    return false;
            //}

            #endregion
        }

        void DeleteDxf(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
           var item = PartsListXml2sDataGrid.SelectedItem as PartsListXml2;
            if (item == null) return;
            if (!item.Dxf) return;

            try
            {
                if (MessageBox.Show(
                    $"Удалить развертку для\n Детали - {item.НаименованиеБезРасширения}\n Конфигурация - {item.Конфигурация}\n Версия - {item.CurrentVersion}",
                    "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Exception exc;
                    Database.DeleteDxf(item.IdPmd, item.Конфигурация, item.CurrentVersion, out exc);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        #endregion
        
        string Busy { get; set; }

        bool OnlyParts { get; set; }

        void XmlParts1_Click(object sender, RoutedEventArgs e)
        {
            CollectParams();
            ExportTaskRun(ExportToXml);
        }

        void CollectParams()
        {
            OnlyParts = checkBoxOnlyParts.IsChecked == true;
            OnlyInAsmConfigs = checkBox.IsChecked == true;
            ListToRun = PartsListXml2sDataGrid.ItemsSource.OfType<PartsListXml2>().ToList();
        }

        private List<PartsListXml2> ListToRun { get; set; }

        private Task Export { get; set; }

        void ExportTaskRun(Action action)
        {
            Export = new Task(action);
            Export.Start();
        }

        List<VaultSystem.PdmFilesAfterGet> _pdmFilesAfterGet;

        void ExportToXml()
        {
            if (IsBusy) return;

            try
            {
                Busy = $"Выгрузка {CurrentModel}";
                //НайтиПолучитьСборкуЕеКонфигурацииПоИмени();
                if (!OnlyParts)
                {
                    if (!ПутьКСборке.ToLower().EndsWith("dprt"))
                    // ВыгрузкаСборкиВXml();
                    {
                        List<Exception> excptions;
                        PdmAsmBomToXml.AsmBomToXml.ВыгрузкаСборкиВXml(ПутьКСборке, out excptions);
                    }
                }
            }
            catch (Exception)
            {
                //MessageBox.Show(exception.StackTrace);
            }            

            GetFiles(ListToRun, out _pdmFilesAfterGet);
            //MessageBox.Show(ListToRun.Count(newComponent => !newComponent.Xml).ToString());

            try
            {
                foreach (var newComponent in ListToRun.Where(newComponent => !newComponent.Xml))
                {
                    using (var modelSw = new ModelSw())
                    {
                        try
                        {
                            Exception exception;
                            ExportXmlSql.Export(modelSw.GetSwWithPart(newComponent.Путь), newComponent.CurrentVersion, (int)newComponent.IdPmd, out exception);
                            if (exception != null)
                            {
                                MessageBox.Show(exception.Message);
                            }

                            //modelSw.ExitSw();
                        }
                        catch (Exception ex )
                        {
                            MessageBox.Show(ex.Message);
                        }                        
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.StackTrace);
            }

            MessageBox.Show($"Выгрузка {Path.GetFileNameWithoutExtension(ПутьКСборке)} завершена");
            Busy = null;
        }

        bool IsBusy
        {
            get
            {
                if (string.IsNullOrEmpty(Busy)) return false;
                MessageBox.Show(Busy, "Система занята", MessageBoxButton.OK, MessageBoxImage.Warning);
                return true;
            }
        }

        static void GetFiles(IEnumerable<PartsListXml2> list, out List<VaultSystem.PdmFilesAfterGet> pdmFilesAfterGet)
        {
            ModelSw.BatchGet(Settings.Default.PdmBaseName, list.Select(x => new VaultSystem.BatchGetParams
            {
                CurrentVersion = x.CurrentVersion,
                FilePath = x.Путь,
                IdPdm = x.IdPmd
            }).ToList(),
            out pdmFilesAfterGet);
        }

        void DxfParts1_Click(object sender, RoutedEventArgs e)
        {
            CollectParams();            
            ExportTaskRun(ExportToDxf);
        }       

        private string FolderToSaveDxf { get; set; }
        private string ParentFolderToSaveDxf { get; set; } = @"\\srvkb\SolidWorks Admin\DXF\"; // @"C:\DXF\";    

        void ExportToDxf()
        {
            if (IsBusy) return;
            try
            {
                Busy = $"Выгрузка разверток для {CurrentModel}";

                FolderToSaveDxf = CurrentModel.ToLower().EndsWith("dprt") ? "детали" :
                CurrentModel.ToLower().Replace(".sldasm", "");
                var pathToSave = Path.Combine(ParentFolderToSaveDxf, FolderToSaveDxf);
                Directory.CreateDirectory(pathToSave.ToUpper());

                var listToExportLocal = new List<PartsListXml2>();
                var files = "";

                foreach (var item in ListToRun.Where(x=>x.Dxf))
                {
                    try
                    {
                        Exception exc;
                        string matId;
                        double? thikness;
                        var blob = Database.DatabaseFileRead(item.IdPmd, item.CurrentVersion, item.Конфигурация, out matId, out thikness, out exc);
                        if (exc != null) { MessageBox.Show(exc.StackTrace); }
                        if (blob != null)
                        {
                            Dxf.SaveByteToFile(blob, Path.Combine(pathToSave, item.НаименованиеБезРасширения.ToUpper().Replace("ВНС-", "")
                                + "-" + item.Конфигурация + (string.IsNullOrEmpty(matId) ? "" : "-" + matId) 
                                + ((thikness == null || thikness == 0) ? "" : "-" + string.Format("{0:0.0}", thikness)).Replace(",", ".") + ".dxf"));
                        }
                        else
                        {
                            files = files + "\n" + item.Наименование + " idPdm - " + item.IdPmd + " Ver - " + item.CurrentVersion + " Conf - " + item.Конфигурация;
                            //MessageBox.Show(exc.Message + "\n" + item.Наименование + "\n" + item.IdPmd + "\n" + item.CurrentVersion + "\n" + item.Конфигурация);
                            listToExportLocal.Add(item);
                        }
                    }
                    catch (Exception exc)
                    {
                        listToExportLocal.Add(item);
                        //MessageBox.Show(exc.StackTrace + "\n - " + exc.Message + "\n" + item.Наименование);
                        //   MessageBox.Show(exc.Message + "\n" + item.Наименование + "\n" + item.IdPmd + "\n" + item.CurrentVersion + "\n" + item.Конфигурация);
                    }                
                }
                
                //MessageBox.Show(listToExportLocal.Count.ToString());

                listToExportLocal.AddRange(ListToRun.Where(x => !x.Dxf).ToList());

                GetFiles(listToExportLocal, out _pdmFilesAfterGet);

                //MessageBox.Show(listToExportLocal.Count.ToString());


                var exeptions = "";

                foreach (var part in listToExportLocal)
                { 
                    Exception exception;
                    List<Dxf.ResultList> resultList;
                    List<Dxf.DxfFile> dxfFiles;

                    //if (part.Наименование.ToLower().Contains("902.01.112"))
                    //{
                    //    MessageBox.Show(part.CurrentVersion.ToString(),  part.IdPmd.ToString());
                    //}                    

                    Dxf.Save(Path.GetFullPath(part.Путь), pathToSave, OnlyInAsmConfigs ? part.Конфигурация : null, out exception, part.IdPmd, part.CurrentVersion, out dxfFiles, true, true);
                    if (exception != null)
                    {
                        try
                        {
                            MessageBox.Show(exception.Message + "\n" + exception.StackTrace, "Exception");
                        }
                        catch (Exception) { }                      
                    }
                    Dxf.AddToSql(dxfFiles, false, out resultList);
                    foreach (var item in resultList)
                    {
                        if (item != null)
                        {
                            try
                            {
                                exeptions = exeptions + $"\nfile - {item.dxfFile.FilePath}\n(id - {item.dxfFile.IdPdm} Ver - {item.dxfFile.Version} Conf - {item.dxfFile.Configuration})";
                            }
                            catch (Exception) { }
                        }
                    }
                }

                MessageBox.Show(exeptions);

                foreach (var process in Process.GetProcessesByName("SLDWORKS"))
                {
                    process.Kill();
                }
            }
            catch (Exception exception)
            {
                //MessageBox.Show(exception.StackTrace);
            }

            MessageBox.Show($"Выгрузка разверток {Path.GetFileNameWithoutExtension(ПутьКСборке)} завершена");
            Busy = null;
        }


        void Loading()
        {
            //   App.ElementVisibility.SetImage(@"\DataControls\Pictures\loading.gif", Status1);
        }

        bool OnlyInAsmConfigs { get; set; }

        void AutoCompleteTextBox1_TextChanged(object sender, RoutedEventArgs e)
        {
            EnabledExport();

            var items = AutoCompleteTextBox1.ItemsSource as List<string>;
            if (items == null) return;

            GetList.IsEnabled = items.Contains(AutoCompleteTextBox1.Text);
        }

        void Button_Click(object sender, RoutedEventArgs e)
        {
            ПолучитьПереченьДеталей();

            if (PartsListXml2sDataGrid_Copy.Items.Count == 0)
            {
                PartsListXml2sDataGrid_Copy.Visibility = Visibility.Collapsed;
                XmlParts1_Copy.Visibility = Visibility.Collapsed;
            }
            else
            {
                PartsListXml2sDataGrid_Copy.Visibility = Visibility.Visible;
                XmlParts1_Copy.Visibility = Visibility.Visible;
            }
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

        void button_Click_1(object sender, RoutedEventArgs e)
        {
            Loading();

            dataGridAfterGet.ItemsSource = null;
            dataGridAfterGet.ItemsSource = _pdmFilesAfterGet;

            dataGridAfterGet.Visibility = dataGridAfterGet.Items.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            
        }
    }
}
