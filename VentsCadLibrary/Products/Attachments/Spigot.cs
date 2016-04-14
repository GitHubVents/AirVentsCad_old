using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace VentsCadLibrary
{
    partial class VentsCad
    { 
        public class Spigot : WorkWithProduct
        {
            private const string SpigotFolder = @"\Библиотека проектирования\DriveWorks\12 - Spigot";            
            private const string SpigotDestinationFolder = @"Проекты\Blauberg\12 - Вибровставка";

            internal string Type;
            internal string Width;
            internal string Height;

            internal string modelName;

            internal string newSpigotName;
            internal string newSpigotPath;

            /// <summary>
            /// Вибровставка прямоугольная
            /// </summary>
            /// <param name="type">Тип вибровставки: 20, 30</param>
            /// <param name="width">200-2000</param>
            /// <param name="height">200-2000</param>            
            /// <returns></returns>
            public Spigot(string type, string width, string height)
            {
                Type = type;
                if (!ConvertToInt(new[] { width, height })) throw new Exception("Недопустимі розміри");
                Width = width;
                Height = height;

                modelName = null;
                switch (type)
                {
                    case "20":
                        modelName = "12-20";
                        break;
                    case "30":
                        modelName = "12-30";
                        break;
                }
                if (string.IsNullOrEmpty(modelName)) throw new Exception("Недопустимий тип");

                newSpigotName = modelName + "-" + width + "-" + height;
                newSpigotPath = $@"{destRootFolder}\{SpigotDestinationFolder}\{newSpigotName}";

                GetPlace();
            }

            public void Build()
            {
                if (Exist) return;

                var drawing = "12-00";
                if (modelName == "12-30")
                {
                    drawing = modelName;
                }

                Dimension myDimension;
                var modelSpigotDrw = $@"{sourceRootFolder}{SpigotFolder}\{drawing}.SLDDRW";

                GetLastVersionAsmPdm(modelSpigotDrw, VaultName);

                if (!InitializeSw(true)) return;

                var swDrwSpigot = _swApp.OpenDoc6(modelSpigotDrw, (int)swDocumentTypes_e.swDocDRAWING,
                    (int)swOpenDocOptions_e.swOpenDocOptions_LoadModel, "", 0, 0);

                if (swDrwSpigot == null) return;

                ModelDoc2 swDoc = _swApp.ActivateDoc2("12-00", false, 0);
                var swAsm = (AssemblyDoc)swDoc;
                swAsm.ResolveAllLightWeightComponents(false);

                switch (modelName)
                {
                    case "12-20":
                        DelEquations(5, swDoc);
                        DelEquations(4, swDoc);
                        DelEquations(3, swDoc);
                        break;
                    case "12-30":
                        DelEquations(0, swDoc);
                        DelEquations(0, swDoc);
                        DelEquations(0, swDoc);
                        break;
                }
                swDoc.ForceRebuild3(true);

                string newPartName;
                string newPartPath;
                IModelDoc2 swPartDoc;

                #region Удаление ненужного

                string[] itemsToDelete = null; 

                switch (Type)
                {
                    case "20":
                        itemsToDelete = new[] { "12-30-001-1", "12-30-001-2", "12-30-002-1", "12-30-002-2",
                                            "ВНС-96.61.002-1", "ВНС-96.61.002-2", "ВНС-96.61.002-3", "ВНС-96.61.002-4",
                                            "ВНС-96.61.002-5", "ВНС-96.61.002-6", "ВНС-96.61.002-7", "ВНС-96.61.002-8",
                                            "12-30-001-3", "12-30-001-4", "12-30-002-3", "12-30-002-4",
                                            "12-003-2", "Клей-2" };
                        break;
                    case "30":
                        itemsToDelete = new[] { "12-20-001-1", "12-20-001-2", "12-20-002-1", "12-20-002-2",
                                            "ВНС-96.61.001-1", "ВНС-96.61.001-2", "ВНС-96.61.001-3", "ВНС-96.61.001-4",
                                            "ВНС-96.61.001-5", "ВНС-96.61.001-6", "ВНС-96.61.001-7", "ВНС-96.61.001-8",
                                            "12-20-001-3", "12-20-001-4", "12-20-002-3", "12-20-002-4",
                                            "12-003-1", "Клей-1"};
                        break;                
                }

                foreach (var item in itemsToDelete)
                {
                    DoWithSwDoc(_swApp, CompType.COMPONENT, item, Act.DeletWithOption);
                }             

                DoWithSwDoc(_swApp, CompType.FTRFOLDER, "30", Act.Delete);
                DoWithSwDoc(_swApp, CompType.FTRFOLDER, "20", Act.Delete);
                #endregion

                #region Сохранение и изменение элементов

                string path;
                int fileId;
                int projectId;

                var addDimH = 1;
                if (modelName == "12-30")
                {
                    addDimH = 10;
                }

                var w = (Convert.ToDouble(Width) - 1) / 1000;
                var h = Convert.ToDouble((Convert.ToDouble(Height) + addDimH) / 1000);
                const double step = 50;
                var weldW = Convert.ToDouble((Math.Truncate(Convert.ToDouble(Width) / step) + 1));
                var weldH = Convert.ToDouble((Math.Truncate(Convert.ToDouble(Height) / step) + 1));

                if (modelName == "12-20")
                {
                    //12-20-001
                    _swApp.IActivateDoc2("12-20-001", false, 0);
                    swPartDoc = _swApp.IActiveDoc2;
                    newPartName = $"12-20-{Height}.SLDPRT";
                    newPartPath = $@"{destRootFolder}\{SpigotDestinationFolder}\{newPartName}";                    

                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), out path, out fileId, out projectId))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2("12-00.SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("12-20-001-1@12-00", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", true, true);
                        _swApp.CloseDoc("12-20-001.SLDPRT");
                    }
                    else
                    {
                        swDoc.Extension.SelectByID2("D1@Вытянуть1@12-20-001-1@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                        myDimension = ((Dimension)(swDoc.Parameter("D1@Вытянуть1@12-20-001.Part")));
                        myDimension.SystemValue = h - 0.031;
                        swDoc.Extension.SelectByID2("D1@Кривая1@12-20-001-1@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                        myDimension = ((Dimension)(swDoc.Parameter("D1@Кривая1@12-20-001.Part")));
                        myDimension.SystemValue = weldH;
                        swPartDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                        NewComponents.Add(new VaultSystem.VentsCadFiles
                        {
                            LocalPartFileInfo = newPartPath
                        });
                        _swApp.CloseDoc(newPartName);
                    }

                    //12-20-002
                    _swApp.IActivateDoc2("12-20-002", false, 0);
                    swPartDoc = _swApp.IActiveDoc2;
                    newPartName = $"12-20-{Width}.SLDPRT";
                    newPartPath = $@"{destRootFolder}\{SpigotDestinationFolder}\{newPartName}";
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), out path, out fileId, out projectId))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2("12-00.SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("12-20-002-1@12-00", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", true, true);
                        _swApp.CloseDoc("12-20-002.SLDPRT");
                    }
                    else
                    {
                        swDoc.Extension.SelectByID2("D1@Вытянуть1@12-20-002-1@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                        myDimension = ((Dimension)(swDoc.Parameter("D1@Вытянуть1@12-20-002.Part")));
                        myDimension.SystemValue = w - 0.031;
                        swDoc.Extension.SelectByID2("D1@Кривая1@12-20-002-1@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                        myDimension = ((Dimension)(swDoc.Parameter("D1@Кривая1@12-20-002.Part")));
                        myDimension.SystemValue = weldW;
                        swPartDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                        NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newPartPath });
                        _swApp.CloseDoc(newPartName);
                    }

                    //12-003
                    _swApp.IActivateDoc2("12-003", false, 0);
                    swPartDoc = _swApp.IActiveDoc2;
                    newPartName = $"12-03-{Width}-{Height}.SLDPRT";
                    newPartPath = $@"{destRootFolder}\{SpigotDestinationFolder}\{newPartName}";
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), out path, out fileId, out projectId))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2("12-00.SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("12-003-1@12-00", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", true, true);
                        _swApp.CloseDoc("12-003.SLDPRT");
                    }
                    else
                    {
                        swDoc.Extension.SelectByID2("D3@Эскиз1@12-003-1@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                        myDimension = ((Dimension)(swDoc.Parameter("D3@Эскиз1@12-003.Part")));
                        myDimension.SystemValue = w;
                        swDoc.Extension.SelectByID2("D2@Эскиз1@12-003-1@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                        myDimension = ((Dimension)(swDoc.Parameter("D2@Эскиз1@12-003.Part")));
                        myDimension.SystemValue = h;
                        swDoc.EditRebuild3();
                        swPartDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                        NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newPartPath });
                        _swApp.CloseDoc(newPartName);
                    }
                }

                if (modelName == "12-30")
                {
                    //12-30-001
                    _swApp.IActivateDoc2("12-30-001", false, 0);
                    swPartDoc = _swApp.IActiveDoc2;
                    newPartName = $"12-30-01-{Height}.SLDPRT";
                    newPartPath = $@"{destRootFolder}\{SpigotDestinationFolder}\{newPartName}";
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), out path, out fileId, out projectId))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2("12-00.SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("12-30-001-1@12-00", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", true, true);
                        _swApp.CloseDoc("12-30-001.SLDPRT");
                    }
                    else
                    {
                        swDoc.Extension.SelectByID2("D1@Вытянуть1@12-30-001-1@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                        myDimension = ((Dimension)(swDoc.Parameter("D1@Вытянуть1@12-30-001.Part")));
                        myDimension.SystemValue = h - 0.031;
                        swDoc.Extension.SelectByID2("D1@Кривая1@12-30-001-1@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                        myDimension = ((Dimension)(swDoc.Parameter("D1@Кривая1@12-30-001.Part")));
                        myDimension.SystemValue = weldH;
                        swPartDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                        NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newPartPath });
                        _swApp.CloseDoc(newPartName);
                    }

                    //12-30-002

                    _swApp.IActivateDoc2("12-30-002", false, 0);
                    swPartDoc = _swApp.IActiveDoc2;
                    newPartName = $"12-30-02-{Width}.SLDPRT";
                    newPartPath = $@"{destRootFolder}\{SpigotDestinationFolder}\{newPartName}";
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), out path, out fileId, out projectId))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2("12-00.SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("12-30-002-1@12-00", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", true, true);
                        _swApp.CloseDoc("12-30-002.SLDPRT");
                    }
                    else
                    {
                        swDoc.Extension.SelectByID2("D1@Вытянуть1@12-30-002-1@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                        myDimension = ((Dimension)(swDoc.Parameter("D1@Вытянуть1@12-30-002.Part")));
                        myDimension.SystemValue = w - 0.031;
                        swDoc.Extension.SelectByID2("D1@Кривая1@12-30-002-1@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                        myDimension = ((Dimension)(swDoc.Parameter("D1@Кривая1@12-30-002.Part")));
                        myDimension.SystemValue = weldH;
                        swPartDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                        NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newPartPath });
                        _swApp.CloseDoc(newPartName);
                    }

                    //12-003

                    _swApp.IActivateDoc2("12-003", false, 0);
                    swPartDoc = _swApp.IActiveDoc2;
                    newPartName = $"12-03-{Width}-{Height}.SLDPRT";
                    newPartPath = $@"{destRootFolder}\{SpigotDestinationFolder}\{newPartName}";
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), out path, out fileId, out projectId))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2("12-00.SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("12-003-2@12-00", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", true, true);
                        _swApp.CloseDoc("12-003.SLDPRT");
                    }
                    else
                    {
                        swDoc.Extension.SelectByID2("D3@Эскиз1@12-003-2@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                        myDimension = ((Dimension)(swDoc.Parameter("D3@Эскиз1@12-003.Part")));
                        myDimension.SystemValue = w;
                        swDoc.Extension.SelectByID2("D2@Эскиз1@12-003-2@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                        myDimension = ((Dimension)(swDoc.Parameter("D2@Эскиз1@12-003.Part")));
                        myDimension.SystemValue = h;
                        swDoc.EditRebuild3();
                        swPartDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                        NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newPartPath });
                        _swApp.CloseDoc(newPartName);
                    }
                }

                #endregion

                GabaritsForPaintingCamera(swDoc);

                swDoc.ForceRebuild3(true);
                swDoc.SaveAs2(newSpigotPath + ".SLDASM", (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                _swApp.CloseDoc(newSpigotName + ".SLDASM");
                NewComponents.Add(new VaultSystem.VentsCadFiles
                {
                    LocalPartFileInfo = newSpigotPath + ".SLDASM"
                });
                swDrwSpigot.Extension.SelectByID2("DRW1", "SHEET", 0, 0, 0, false, 0, null, 0);
                var drw = (DrawingDoc)(_swApp.IActivateDoc3(drawing + ".SLDDRW", true, 0));
                drw.ActivateSheet("DRW1");
                var m = 5;
                if (Convert.ToInt32(Width) > 500 || Convert.ToInt32(Height) > 500) { m = 10; }
                if (Convert.ToInt32(Width) > 850 || Convert.ToInt32(Height) > 850) { m = 15; }
                if (Convert.ToInt32(Width) > 1250 || Convert.ToInt32(Height) > 1250) { m = 20; }
                drw.SetupSheet5("DRW1", 12, 12, 1, m, true, destRootFolder + @"\Vents-PDM\\Библиотека проектирования\\Templates\\Основные надписи\\A3-A-1.slddrt", 0.42, 0.297, "По умолчанию", false);
                var errors = 0; var warnings = 0;

                swDrwSpigot.SaveAs4(newSpigotPath + ".SLDDRW", (int)swSaveAsVersion_e.swSaveAsCurrentVersion, (int)swSaveAsOptions_e.swSaveAsOptions_Silent, ref errors, ref warnings);

                NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newSpigotPath + ".SLDDRW" });

                _swApp.CloseDoc(newSpigotPath);
                _swApp.ExitApp();
                _swApp = null;

                List<VaultSystem.VentsCadFiles> newFilesList;
                VaultSystem.CheckInOutPdmNew(NewComponents, true, DestVaultName, out newFilesList);
                
                foreach (var item in newFilesList)
                {
                    if (item.LocalPartFileInfo.ToUpper().Contains(".SLDASM"))
                    {
                        AddInSqlBaseSpigot(item.PartName.Remove(item.PartName.LastIndexOf('.')), item.PartIdPdm,
                       Convert.ToInt32(Type), Convert.ToInt32(Height), Convert.ToInt32(Width));
                    }
                }

                foreach (var newComponent in NewComponents)
                {
                    PartInfoToXml(newComponent.LocalPartFileInfo);
                }

                GetPlace();
            }

            public string Name()
            {
                return newSpigotName;
            }

            public string DestinationFolder()
            {
                return newSpigotPath;
            }          

            public bool Exist { get; set; }

            internal ProductPlace place{ get; set;}

            public ProductPlace GetPlace()
            {
                if (place != null) return place;                

                string path;
                int fileId;
                int projectId;
                Exist = GetExistingFile(newSpigotName, out path, out fileId, out projectId);
                if (string.IsNullOrEmpty(path))
                {
                    place = null;
                }
                else
                {
                    place = new ProductPlace(path, fileId, projectId);
                }

                return place;
            }            

            static void AddInSqlBaseSpigot(string fileName, int? idPdm, int? typeOfSpigot, int? height, int? width)
            {
                using (var con = new SqlConnection(ConnectionToSql))
                {
                    try
                    {
                        con.Open();

                        var sqlCommand = new SqlCommand("AirVents.Spigot", con) { CommandType = CommandType.StoredProcedure };
                        var sqlParameter = sqlCommand.Parameters;

                        if (fileName == null)
                        {
                            sqlParameter.AddWithValue("@Filename", DBNull.Value);
                        }
                        else
                        {
                            sqlParameter.AddWithValue("@Filename", fileName);
                        }

                        if (idPdm == null)
                        {
                            sqlParameter.AddWithValue("@IDPDM", DBNull.Value);
                        }
                        else
                        {
                            sqlParameter.AddWithValue("@IDPDM", idPdm);
                        }

                        if (typeOfSpigot == null)
                        {
                            sqlParameter.AddWithValue("@Type", DBNull.Value);
                        }
                        else
                        {
                            sqlParameter.AddWithValue("@Type", typeOfSpigot);
                        }

                        if (height == null)
                        {
                            sqlParameter.AddWithValue("@Hight", DBNull.Value);
                        }
                        else
                        {
                            sqlParameter.AddWithValue("@Hight", height);
                        }

                        if (width == null)
                        {
                            sqlParameter.AddWithValue("@Width", DBNull.Value);
                        }
                        else
                        {
                            sqlParameter.AddWithValue("@Width", width);
                        }                      
                        sqlCommand.ExecuteNonQuery();
                   
                    }
                    catch (Exception exception)
                    {
                        Логгер.Информация("Введите корректные данные! " + exception.Message + $"\nIDPDM - {idPdm} Type - {typeOfSpigot}", null, "", "AddInSqlBaseSpigot");                        
                    }
                    finally
                    {
                        con.Close();
                    }
                }
            }

        }
    }
}
