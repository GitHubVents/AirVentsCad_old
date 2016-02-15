using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
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
using ExportPartData;
using HostingWindowsForms.EPDM;
using MakeDxfUpdatePartData;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using VentsCadLibrary;
using Timer = System.Threading.Timer;

namespace AirVentsCadWpf.DataControls.Specification
{
    /// <summary>
    /// 
    /// </summary>
    public partial class SpecificationUc
    {
        string ПутьКСборке { get; set; }
        string CurrentPath { get; set; }

        #region Конструктор класса

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecificationUc"/> class.
        /// </summary>
        public SpecificationUc()
        {
            InitializeComponent();
            ПереченьДеталей_Copy.Visibility = Visibility.Hidden;
        }

        void НайтиПолучитьСборкуЕеКонфигурацииПоИмени()
        {
            try
            {
                var emdpService = new Epdm();
                var path = emdpService.SearchDoc(AutoCompleteTextBox1.Text);

                if (path == null) return;

                var configs = emdpService.GetConfiguration(path[0].FilePath);

                if (configs == null) return;

                var itemsSource = configs as IList<string> ?? configs.ToList();

                ПутьКСборке = path[0].FilePath;

                var bomClass = new Epdm
                {
                    BomId = 8,
                    AssemblyPath = path[0].FilePath
                };
                Exception exception;

                SpecBomCells = bomClass.BomList(path[0].FilePath, itemsSource[0], true, out exception).ToArray();

                if (exception != null)
                {
                   // MessageBox.Show(exception.StackTrace);
                }
            }
            catch (Exception exception)
            {
              //  MessageBox.Show(exception.StackTrace);

            }
        }

        #endregion

        #region ВыгрузитьВXml

        static void ВыгрузитьСборку(string путьКСборке)
        {
            var имяСборки = new FileInfo(путьКСборке).Name.Replace(new FileInfo(путьКСборке).Extension, "");

            var emdpService = new Epdm();

            //var path = emdpService.SearchDoc(имяСборки + ".SLDASM");

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
                        AssemblyPath = путьКСборке//path[0].FilePath
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
                    if (lastVerOfAsm != null) exist = ExistLastXml(путьКСборке//path[0].FilePath
                        , (int)lastVerOfAsm);
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
                            AssemblyPath = путьКСборке// path[0].FilePath
                        };
                        Exception exception;
                        var спецификация = bomClass.BomList(путьКСборке//  path[0].FilePath
                            , config, false, out exception);
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
                                myXml.WriteAttributeString("value", topAsm.Раздел == "Материалы" ? "" : topAsm.Обозначение);
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

                Thread.Sleep(1000);

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

            var emdpService = new Epdm();
            var path = emdpService.SearchDoc(имяСборки + ".SLDASM");

            IEnumerable<string> списокКонфигурацийСборки = null;

            try
            {
                списокКонфигурацийСборки = emdpService.GetConfiguration(path[0].FilePath);
            }
            catch (Exception)
            {
                списокКонфигурацийСборки = new[] {"00"};
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
                        AssemblyPath = path[0].FilePath
                    };
                    Exception exception;
                    var spec = bomClassGetLastVersion.BomList(path[0].FilePath, "00", false, out exception);
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
                //    if (lastVerOfAsm != null) exist = ExistLastXml(path[0].FilePath, (int)lastVerOfAsm);
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

                foreach (var config in списокКонфигурацийСборки)
                {
                    var bomClass = new Epdm
                    {
                        BomId = 8,
                        AssemblyPath = path[0].FilePath
                    };
                    Exception exception;
                    var спецификация = bomClass.BomList(path[0].FilePath, config, false, out exception);
                    if (exception != null)
                    {
                        MessageBox.Show(exception.StackTrace);
                    }
                    
                    var allParts =
                        спецификация.Where(x => x.ТипФайла.ToLower() == "sldprt")
                            .Where(x => x.Раздел == "Детали" || x.Раздел == "").OrderBy(x=>x.FileName).ToList();

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
                        myXml.WriteAttributeString("value", topAsm.Раздел == "Материалы" ? "" : topAsm.Обозначение);  // 1C Для раздела "материалов" вставляем ПУСТО в обозначение из-за конфликта с 1С 
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
                            myXml.WriteAttributeString("value", "");// topLevel.КодДокумента);
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
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    
        void ВыгрузкаСборкиВXml()
        {
            if (ПутьКСборке == null) return;

            ModelSw.GetAsBuild(ПутьКСборке, Settings.Default.PdmBaseName);

            #region Выгрузка Главной Сборки

            try
            {
                //var parentAsm = SpecBomCells.Where(x => x.Раздел == "" || x.Раздел == "Сборочные единицы").Where(x => x.Уровень == 0)
                //    .Select(x => x.FilePath + "\\" + x.FileName).Distinct().ToList();
                //BomData.ItemsSource = SpecBomCells;
                //MessageBox.Show((parentAsm.Count).ToString(), SpecBomCells.Count().ToString());
        //        MessageBox.Show(ПутьКСборке);
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
                    ВыгрузитьСборку(путьКСборке);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.StackTrace);
                }
            }

            #endregion

        }

        #endregion
      
        static string FileName(string filePath)
        {
            var fileName = new FileInfo(filePath).Name;
            return fileName;
        }
    
        private void AutoCompleteTextBox1Reload()
        {
            List< SwEpdm.EpdmSearch.FindedDocuments> найденныеФайлы;
            List<SwEpdm.EpdmSearch.FindedDocuments> найденныеФайлы1;

            VaultSystem.SearchInVault.SearchDoc(
                AutoCompleteTextBox1.Text,
                SwEpdm.EpdmSearch.SwDocType.SwDocAssembly,
                out найденныеФайлы, 
                Settings.Default.PdmBaseName);

            //FileType = "asm";

            VaultSystem.SearchInVault.SearchDoc(
                AutoCompleteTextBox1.Text,
                SwEpdm.EpdmSearch.SwDocType.SwDocPart,
                out найденныеФайлы1,
                Settings.Default.PdmBaseName);

           // FileType = "prt";

            if (найденныеФайлы == null & найденныеФайлы1 == null) return;

            if (найденныеФайлы1 != null & найденныеФайлы != null) найденныеФайлы.AddRange(найденныеФайлы1);
            if (найденныеФайлы1 != null & найденныеФайлы == null) найденныеФайлы = найденныеФайлы1;

            var newNames = найденныеФайлы.Select(findedDocumentse => findedDocumentse.Path).ToList();
                FilePath = newNames.ConvertAll(FileName);
            
            AutoCompleteTextBox1.ItemsSource = FilePath;
           
            AutoCompleteTextBox1.FilterMode = AutoCompleteFilterMode.Contains;
        }

        private List<string> FilePath { get; set; }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            AutoCompleteTextBox1Reload();
        }        

        void EnabledExport()
        {
            //var currentPath = AutoCompleteTextBox1.Text == CurrentPath;
            //ПолучитьПереченьДеталей.IsChecked = currentPath;
            //XmlParts1.IsEnabled = currentPath;
            //PartsListXml2sDataGrid.IsEnabled = currentPath;

            Найти.IsEnabled = AutoCompleteTextBox1.Text.Length != 0;
        }
        
        class PartsListXml2
        {
            public bool Dxf { get; set; }

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

            //public string ПокараскаX { get; set; }
            //public string ПокараскаY { get; set; }
            //public string ПокараскаZ { get; set; }

            public string ПлощадьПокрытия { get; set; }

            public string Материал { get; set; }

            public string ПлощадьS
            {
                get {
                    try
                    {
                        return
                            ((Math.Round(Convert.ToDouble(ЗаготовкаШирина)*Convert.ToDouble(ЗаготовкаВысота)*2/10000))/
                             100).ToString();
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
        
        Epdm.BomCells[] SpecBomCells { get; set; }

        void ПолучитьПереченьДеталей()
        {
            try
            {
                if (AutoCompleteTextBox1.Text.ToLower().EndsWith("prt"))
                {
                    var emdpService = new Epdm();
                    var path = emdpService.SearchDoc(AutoCompleteTextBox1.Text);
                    var filePath = path[0].FilePath;
                    var lastVersion = SwEpdm.GetVersionOfFile(filePath, Settings.Default.PdmBaseName);
                    var partsListXml2S = new List<PartsListXml2>
                    {
                        new PartsListXml2
                        {
                            Наименование = Path.GetFileNameWithoutExtension(filePath),
                            Путь = filePath,
                            Xml = ExistLastXml(filePath, lastVersion),
                            Dxf = ExistLastDxf(filePath, lastVersion)
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
                    var path = emdpService.SearchDoc(AutoCompleteTextBox1.Text);
                    var configs = emdpService.GetConfiguration(path[0].FilePath);
                    var itemsSource = configs as IList<string> ?? configs.ToList();

                    var bomClass = new Epdm
                    {
                        BomId = 8,
                        AssemblyPath = path[0].FilePath
                    };
                    Exception exception;
                    var спецификация = bomClass.BomList(path[0].FilePath, itemsSource[0], false, out exception);
                    if (exception != null)
                    {
                        MessageBox.Show(exception.StackTrace);
                    }

                    //MessageBox.Show(спецификация.Where(x=>x.ТипФайла.ToLower() == "sldprt").Where(x => x.Раздел == "Детали" || x.Раздел == "").Count().ToString());

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
                        listXml.Dxf = ExistLastXml(listXml.Путь, listXml.CurrentVersion);
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

                                    Материал = listXml2.Материал
                                });
                            }
                        }
                        catch (Exception exceptio)
                        {
                            MessageBox.Show(exceptio.StackTrace);
                        }
                    }

                    PartsListXml2sDataGrid.ItemsSource = null;
                    PartsListXml2sDataGrid.ItemsSource = partsListXml2S;

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
                CurrentPath = AutoCompleteTextBox1.Text;
                ПереченьДеталей_Copy.Header = CurrentPath.ToLower().EndsWith("prt") ? $"Деталь {CurrentPath}" : $"Перечень деталей для сборки {CurrentPath} (Всего деталей: {PartsListXml2sDataGrid.Items.Count}шт.)";
            }
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

                    goto m1;
                }
            }
            catch (Exception)
            {
                return 0;
            }
            m1:
            return version;
        }

        static bool ExistLastXml(string partPath, int currentVersion)
        {
            try
            {
                var xmlPartPath =
                new FileInfo(@"\\srvkb\SolidWorks Admin\XML\" + Path.GetFileNameWithoutExtension(partPath) + ".xml");

                if (!xmlPartPath.Exists) return false;

                var xmlPartVersion = Version(xmlPartPath.FullName);

                return Equals(xmlPartVersion, currentVersion);
            }
            catch (Exception)
            {
                return false;
            }
        }

        static bool ExistLastDxf(string partPath, int currentVersion)
        {
            return true;
            try
            {
                var xmlPartPath =
                new FileInfo(@"\\srvkb\SolidWorks Admin\XML\" + Path.GetFileNameWithoutExtension(partPath) + ".xml");

                if (!xmlPartPath.Exists) return false;

                var xmlPartVersion = Version(xmlPartPath.FullName);

                return Equals(xmlPartVersion, currentVersion);
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
        
        void XmlParts1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                НайтиПолучитьСборкуЕеКонфигурацииПоИмени();
                ВыгрузкаСборкиВXml();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.StackTrace);
            }

            ListToRun = PartsListXml2sDataGrid.ItemsSource.OfType<PartsListXml2>().ToList();
            ExportTask();
        }

        private List<PartsListXml2> ListToRun { get; set; }

        private Task Export { get; set; }

        void ExportTask()
        {
            Export = new Task(ExportPartsToXml);
            Export.Start();
        }

        void ExportPartsToXml()
        {
            CurrentPath = Path.GetFileNameWithoutExtension(ПутьКСборке);
            try
            {
                foreach (var newComponent in ListToRun.Where(newComponent => !newComponent.Xml))
                {
                    using (var modelSw = new ModelSw())
                    {
                        ExportXmlSql.Export(modelSw.GetSwWithPart(newComponent.Путь), newComponent.CurrentVersion, (int)newComponent.IdPmd);
                        modelSw.ExitSw();
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.StackTrace);
            }

            MessageBox.Show($"Выгрузка {Path.GetFileNameWithoutExtension(ПутьКСборке)} завершена");
            CurrentPath = null;
            
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

        void DxfParts1_Click(object sender, RoutedEventArgs e)
        {

            var list = PartsListXml2sDataGrid.ItemsSource.OfType<PartsListXml2>().ToList();
            
            if (list.Count == 0) return;

            НайтиПолучитьСборкуЕеКонфигурацииПоИмени();

            ModelSw.GetAsBuild(ПутьКСборке, Settings.Default.PdmBaseName);

            var listOfFiles = "";

            foreach (var newComponent in list)
            {
                    try
                    {
                        Dxf.Save(Path.GetFullPath(newComponent.Путь), checkBox.IsChecked == true ? newComponent.Конфигурация : null);
                     
                    }
                    catch (Exception)
                    {
                        //
                    }
            
                listOfFiles = listOfFiles + newComponent .Наименование + "-" +  "\n";
            }

           // MessageBox.Show(listOfFiles);
        }
        
        private void AutoCompleteTextBox1_TextChanged(object sender, RoutedEventArgs e)
        {
            EnabledExport();

            var items = AutoCompleteTextBox1.ItemsSource as List<string>;
            if (items == null) return;

            GetList.IsEnabled = items.Contains(AutoCompleteTextBox1.Text);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
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
    }
}
