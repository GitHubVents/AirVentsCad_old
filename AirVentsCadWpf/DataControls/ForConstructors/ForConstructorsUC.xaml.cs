using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using AirVentsCadWpf.Properties;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using ModelSw = AirVentsCadWpf.AirVentsClasses.UnitsBuilding.ModelSw;

namespace AirVentsCadWpf.DataControls.ForConstructors
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ForConstructorsUc
    {


        /// <summary>
        /// Initializes a new instance of the <see cref="ForConstructorsUc"/> class.
        /// </summary>
        public ForConstructorsUc()
        {
            InitializeComponent();
            Temporary.Visibility = Visibility.Collapsed;
        }

        private void BuildDamper_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sw = new ModelSw();
                sw.Dumper(TypeOfDumper.Text, WidthDamper.Text, HeightDamper.Text, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Grid_Loaded_2(object sender, RoutedEventArgs e)
        {
           
        }
        
        private void WidthDamper_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuildDamper_Click(this, new RoutedEventArgs());
            }
        }

        private void HeightDamper_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuildDamper_Click(this, new RoutedEventArgs());
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ДобавитьТаблицуВидов_Click(object sender, RoutedEventArgs e)
        {
           // MessageBox.Show("Откройте чертеж и выделете основной вид модели!\n Затем нажмите 'OK'");
            
            try
            {
                var swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");
                var swModel = (IModelDoc2)swApp.IActiveDoc;

                var swDraw = (DrawingDoc) swModel;
                
                //Select View
                swModel.ClearSelection2(true);
                var getFirstView = swDraw.GetCurrentSheet().GetViews()[0];

                //swDraw.GetFirstView();
                //var getFirstView = swDraw.IActiveDrawingView;

                var referencedDocument = (IModelDoc2)getFirstView.ReferencedDocument;


                #region From swApp

                var docName = referencedDocument.GetTitle();
                var configNames = referencedDocument.GetConfigurationNames();
                var обозначения = new List<string>();
                //foreach (var configName in configNames)
                //{
                //    if (configName == "00")
                //    {
                //        обозначения.Add(docName);
                //    }
                //    else
                //    {
                //        обозначения.Add("-" + configName);
                //    }
                //}

                #endregion

                #region VentsMaterials dll

                var vm = new VentsMaterials.SetMaterials();
                var tosql = new VentsMaterials.ToSQL();
                VentsMaterials.ToSQL.Conn = Settings.Default.ConnectionToSQL;

                swApp.ActivateDoc(referencedDocument.GetTitle());
                var configNamesDll = vm.GetConfigurationNames();
                swApp.CloseDoc(Path.GetFileName(referencedDocument.GetPathName()));


                foreach (var configName in configNamesDll)
                {
                    if (configName == "00")
                    {
                        обозначения.Add(docName);
                    }
                    else
                    {
                        обозначения.Add("-" + configName);
                    }
                }

                #endregion
                
                InsertTableImg(swModel, обозначения);

              //  MessageBox.Show("Готово.");
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void GetDataByBaseView(IModelDoc2 swModel)
        {
            var swDraw = (DrawingDoc)swModel;

            swDraw.ResolveOutOfDateLightWeightComponents();
            swDraw.ForceRebuild();

            // Движение по листам
            var vSheetName = (string[])swDraw.GetSheetNames();

            foreach (var name in vSheetName)
            {
                swDraw.ResolveOutOfDateLightWeightComponents();
                var swSheet = swDraw.Sheet[name];
                swDraw.ActivateSheet(swSheet.GetName());

                if ((swSheet.IsLoaded()))
                {
                    try
                    {
                        var sheetviews = (object[])swSheet.GetViews();
                        var firstView = (View)sheetviews[0];
                        firstView.SetLightweightToResolved();

                        var baseView = firstView.IGetBaseView();
                        var dispData = (IModelDoc2)baseView.ReferencedDocument;

                        MessageBox.Show(dispData.GetTitle());
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

        private void GetData(DrawingDoc swDraw)
        {
           // var swDraw = (DrawingDoc) swModel;

          //  var ModelDoc

            var getFirstView = (View)swDraw.GetFirstView();
            MessageBox.Show(getFirstView.ReferencedDocument.GetTitle());


            swDraw.ResolveOutOfDateLightWeightComponents();
            swDraw.ForceRebuild();

            // Движение по листам
            var vSheetName = (string[]) swDraw.GetSheetNames();

            foreach (var name in vSheetName)
            {
                swDraw.ResolveOutOfDateLightWeightComponents();
                var swSheet = swDraw.Sheet[name];
                swDraw.ActivateSheet(swSheet.GetName());

                if ((!swSheet.IsLoaded())) continue;

                var sheetviews1 = (object[])swSheet.GetViews();
                var firstView1 = (View)sheetviews1[0];
                var baseView1 = firstView1.IGetBaseView();
                var dispData1 = (IModelDoc2)baseView1.ReferencedDocument;
                MessageBox.Show(dispData1.GetTitle());

                try
                {
                    var sheetviews = (object[]) swSheet.GetViews();
                    var firstView = (View) sheetviews[0];
                    firstView.SetLightweightToResolved();

                    var baseView = firstView.IGetBaseView();
                    var dispData = (IModelDoc2) baseView.ReferencedDocument;

                    MessageBox.Show(dispData.GetTitle());
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
        }

        private static
            void InsertTableImg(IModelDoc2 swModel, IList<string> обозначения)
        {
            var swDrawing = ((DrawingDoc)(swModel));
            var enumerable = обозначения as string[] ?? обозначения.ToArray();
            var myTable = swDrawing.InsertTableAnnotation(0.07, 0.07, 1, enumerable.Count() + 1, 2);
            if ((myTable == null)) return;
            myTable.Title = "Таблица видов";
            myTable.BorderLineWeight = 2;
            myTable.GridLineWeight = 1;
            myTable.Text[0, 0] = "Обозначение";

            myTable.SetColumnWidth(0, 0.05, 0);
            myTable.Text[0, 1] = "Рис.";
            myTable.SetColumnWidth(1, 0.02, 0);

            for (var i = 1; i < обозначения.Count() + 1; i++)
            {
                myTable.Text[i, 0] = обозначения[i - 1];
                if (i > 1)
                {
                    myTable.set_CellTextHorizontalJustification(i, 0, (int)swTextJustification_e.swTextJustificationRight);
                }
                myTable.Text[i, 1] = (i).ToString();
            }
        }







        private static
            void InsertTableSizes(IModelDoc2 swModel, IList<string> обозначения)
        {
            var swDrawing = ((DrawingDoc)(swModel));
            var enumerable = обозначения as string[] ?? обозначения.ToArray();
            var myTable = swDrawing.InsertTableAnnotation(0.07, 0.07, 1, enumerable.Count() + 1, 2);
            if ((myTable == null)) return;
            myTable.Title = "Таблица видов";
            myTable.BorderLineWeight = 2;
            myTable.GridLineWeight = 1;
            myTable.Text[0, 0] = "Обозначение";

            myTable.SetColumnWidth(0, 0.05, 0);
            myTable.Text[0, 1] = "Рис.";
            myTable.SetColumnWidth(1, 0.02, 0);

            for (var i = 1; i < обозначения.Count() + 1; i++)
            {
                myTable.Text[i, 0] = обозначения[i - 1];
                if (i > 1)
                {
                    myTable.set_CellTextHorizontalJustification(i, 0, (int)swTextJustification_e.swTextJustificationRight);
                }
                myTable.Text[i, 1] = (i).ToString();
            }
        }




        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                var swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");
                var swModel = (IModelDoc2)swApp.IActiveDoc;

                var info = String.Format(" swModel.GetPathName - {0} swModel.GetTitle - {1} ", swModel.GetPathName(), swModel.GetTitle());
                MessageBox.Show(info);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                var swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");
                swApp.ExitApp();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                var swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");
                var swModel = (IModelDoc2)swApp.IActiveDoc;

               swApp.CloseDoc(Path.GetFileName(swModel.GetPathName()));
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            try
            {
                var processes = Process.GetProcessesByName("SLDWORKS");
                foreach (var process in processes)
                {
                    process.CloseMainWindow();
                    process.Kill();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }





        private void ДобавитьТаблицуРазмеров_Click(object sender, RoutedEventArgs e)
        {
            var classf = new SizesClass();




            ТаблицаВидов.ItemsSource = classf.SheetSizes("DRW1");

            return;


            var swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");
            var swModel = (IModelDoc2)swApp.IActiveDoc;

            var swDraw = (DrawingDoc)swModel;

            //Select View
            swModel.ClearSelection2(true);

            var getFirstView = (View)swDraw.GetCurrentSheet().GetViews()[0];

            var views = swDraw.GetCurrentSheet().GetViews();

            // Получить виды

            var getFirstView2 = (View)swDraw.get_Sheet("DRW1").GetViews()[Convert.ToInt32(getFirstView.get_IPosition())];

            MessageBox.Show(getFirstView2.Name);




            foreach (View view in views)
            {
                MessageBox.Show(view.Name + " - " + view.GetDimensionCount());

                var dimensionsForView = (string[]) view.GetDimensionIds4();
                foreach (var d in dimensionsForView)
                {
                    MessageBox.Show(d);
                }

                //var dimensionsForView = (string[])view.GetDimensionDisplayString4();
                //foreach (var d in dimensionsForView)
                //{
                //    MessageBox.Show(d);
                //}

                //var dimensionsForView = (double[])view.GetDimensionDisplayInfo5();
                //foreach (var d in dimensionsForView)
                //{
                //    MessageBox.Show(d.ToString());
                //}
                
            }
            
            // ТаблицаВидов.ItemsSource = (View[])swDraw.GetCurrentSheet().GetViews();

            // Выделить размер
            swModel.Extension.SelectByID2("RD2@Чертежный вид11", "DIMENSION", 0, 0, 0, false, 0, null, 0);
            
            // Проставить букву
            swModel.EditDimensionProperties2(0, 0, 0, "", "", true, 9, 2, true, 12, 12, "M", "", false, "", "", false);
            
            // Проставить размер
            swModel.EditDimensionProperties2(0, 0, 0, "", "", false, 0, 2, true, 12, 12, "", "", true, "", "", false);
        }


        /// <summary>
        /// 
        /// </summary>
        public class SizesClass
        {
             
            SldWorks _swApp;// = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");
            IModelDoc2 _swModel;
            DrawingDoc _swDraw;

            /// <summary>
            /// Initializes a new instance of the <see cref="SizesClass"/> class.
            /// </summary>
            public SizesClass()
            {
                _swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");
                _swModel = (IModelDoc2)_swApp.IActiveDoc;
                _swDraw = (DrawingDoc)_swModel;   
            }

            /// <summary>
            /// Initializes this instance.
            /// </summary>
            public void Init()
            {
                _swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");
                _swModel = (IModelDoc2)_swApp.IActiveDoc;
                _swDraw = (DrawingDoc)_swModel;
            }

            /// <summary>
            /// Gets or sets the dim identifier.
            /// </summary>
            /// <value>
            /// The dim identifier.
            /// </value>
            public string DimId { get; set; }
            /// <summary>
            /// Gets or sets the dim string.
            /// </summary>
            /// <value>
            /// The dim string.
            /// </value>
            public string DimString { get; set; }
            /// <summary>
            /// Gets or sets the name of the view.
            /// </summary>
            /// <value>
            /// The name of the view.
            /// </value>
            public string ViewName { get; set; }
            //public string SheetName { get; set; }

            /// <summary>
            /// Размеры листа
            /// </summary>
            /// <param name="sheetName">Name of the sheet.</param>
            /// <returns></returns>
            public List<SizesClass> SheetSizes(string sheetName)
            {
                var list = new List<SizesClass>();
                var размеры = new SizesClass();
                Init();
                foreach (View view in _swDraw.get_Sheet(sheetName).GetViews())
                {
                   // MessageBox.Show(view.Name + " - " + view.GetDimensionCount());

                    string[] dimensionIds4 = view.GetDimensionIds4();

                    //swDocExt.SelectByID2("D1@Расстояние1@02-11-40-1.SLDASM", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                    //var myDimension = ((Dimension)(swDoc.Parameter("D1@Расстояние1")));
                    //myDimension.SystemValue = 0; // p1Deep = 19.2;


                    foreach (string dimensoinId in dimensionIds4)
                    {
                       // MessageBox.Show(d);
                        размеры.DimId = dimensoinId;
                        размеры.ViewName = view.Name;
                        list.Add(размеры);
                    }

                    //var dimensionsForView = (string[])view.GetDimensionDisplayString4();
                    //foreach (var d in dimensionsForView)
                    //{
                    //    MessageBox.Show(d);
                    //}

                    //var dimensionsForView = (double[])view.GetDimensionDisplayInfo5();
                    //foreach (var d in dimensionsForView)
                    //{
                    //    MessageBox.Show(d.ToString());
                    //}
                }
                return list;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sw = new ModelSw();sw.Lono();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            
        }
    }
}
