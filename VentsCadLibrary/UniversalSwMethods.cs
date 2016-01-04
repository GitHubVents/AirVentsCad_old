using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MakeDxfUpdatePartData;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using VentsMaterials;

namespace VentsCadLibrary
{
    public partial class VentsCad
    {
        static bool IsConvertToInt(IEnumerable<string> newStringParams)
        {
            foreach (var param in newStringParams)
            {
                try
                {
                    // ReSharper disable once UnusedVariable
                    var y = Convert.ToInt32(param);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }

        internal void GetLastVersionAsmPdm(string path, string vaultName)
        {            
            try
            {
                LoggerInfo($"Получение последней версии по пути {path}\nБаза - {vaultName}", "", "GetLastVersionPdm");

                VaultSystem.GetLastVersionOfFile(path, vaultName);

                #region to delete

                //  MessageBox.Show($"Получение последней версии по пути {path}\nБаза - {vaultName}", "GetLastVersionPdm");
                //var vaultSource = new EdmVault5();
                //IEdmFolder5 oFolder;
                //vaultSource.LoginAuto(vaultName, 0);
                //var edmFile5 = vaultSource.GetFileFromPath(path, out oFolder);
                //edmFile5.GetFileCopy(0, 0, oFolder.ID, (int)EdmGetFlag.EdmGet_RefsVerLatest);

                #endregion
            }
            catch (Exception e)
            {
                LoggerError(
                    $"Во время получения последней версии по пути {path} возникла ошибка!\nБаза - {vaultName}. {e.Message}", e.StackTrace, "GetLastVersionPdm");
            }
        }

        internal bool InitializeSw(bool visible)
        {
            try
            {
                _swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");
            }
            catch (Exception)
            {
                _swApp = new SldWorks { Visible = visible };
            }
            return _swApp != null;
        }

        static void DelEquations(int index, IModelDoc2 swModel)
        {
            try
            {
                LoggerInfo($"Удаление уравнения #{index} в модели {swModel.GetPathName()}", "", "DelEquations");
                var myEqu = swModel.GetEquationMgr();
                myEqu.Delete(index);
                swModel.EditRebuild3();                
            }
            catch (Exception e)
            {
                LoggerError($"Удаление уравнения #{index} в модели {swModel.GetPathName()}. {e.Message}", e.StackTrace, "DelEquations");
            }
        }
        
        bool GetExistingFile(string fileName, int type, string vaultName)
        {
            List<SwEpdm.EpdmSearch.FindedDocuments> найденныеФайлы;
            switch (type)
            {
                case 0:
                    VaultSystem.SearchInVault.SearchDoc(fileName, SwEpdm.EpdmSearch.SwDocType.SwDocAssembly, out найденныеФайлы, vaultName);
                    if (найденныеФайлы?.Count > 0) goto m1;
                    break;
                case 1:
                    VaultSystem.SearchInVault.SearchDoc(fileName, SwEpdm.EpdmSearch.SwDocType.SwDocPart, out найденныеФайлы, vaultName);
                    if (найденныеФайлы?.Count > 0) goto m1;
                    break;
            }
            goto m2;
            m1:
            try
            {
                GetLastVersionAsmPdm(найденныеФайлы[0].Path, VaultName);
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(найденныеФайлы[0].Path);
                return fileNameWithoutExtension != null && string.Equals(fileNameWithoutExtension, fileName, StringComparison.CurrentCultureIgnoreCase);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString()); return false;
            }
            m2:
            return false;

        }

        bool GetExistingFile(string partName, out string path, out int fileId, out int projectId)
        {
            fileId = 0;
            projectId = 0;
            path = null;
            
            List<SwEpdm.EpdmSearch.FindedDocuments> найденныеФайлы;
            VaultSystem.SearchInVault.SearchDoc(partName, SwEpdm.EpdmSearch.SwDocType.SwDocAssembly, out найденныеФайлы, VaultName);
            
            if (найденныеФайлы != null) goto m1;

            VaultSystem.SearchInVault.SearchDoc(partName, SwEpdm.EpdmSearch.SwDocType.SwDocPart, out найденныеФайлы, VaultName);
            if (найденныеФайлы != null) goto m1;

            return false;
            m1:
            try
            {
                GetLastVersionAsmPdm(найденныеФайлы[0].Path, VaultName);
                fileId = найденныеФайлы[0].FileId;
                projectId = найденныеФайлы[0].ProjectId;
                return true;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
                return false;
            }
        }
        
        void AddInSqlBaseSpigot(string fileName, int? idPdm, int? typeOfSpigot, int? height, int? width)
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

                    #region to delete
                    //if (fileType == null)
                    //{
                    //    sqlParameter.AddWithValue("@FileType", DBNull.Value);
                    //}
                    //else
                    //{
                    //    sqlParameter.AddWithValue("@FileType", fileType);
                    //}
                    #endregion

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

                    #region ReturnValue

                    //sqlCommand.Parameters.Add("@status", SqlDbType.Bit);
                    //sqlCommand.Parameters["@status"].Direction = ParameterDirection.ReturnValue;

                    #endregion

                    sqlCommand.ExecuteNonQuery();

                    #region ReturnValue

                    //status = Convert.ToBoolean(sqlCommand.Parameters["@status"].Value);

                    //MessageBox.Show(
                    //    $"fileName - {fileName} idPdm - {idPdm} fileType - {fileType} typeOfSpigot - {typeOfSpigot} height - {height} width - {width}");
                    //MessageBox.Show(status ? "Деталь есть в базе" : "Детали нет в базе", fileName);

                    #endregion
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
            // todo
            //return status;
        }

        void PartInfoToXml(string filePath)
        {
            try
            {
                if (filePath == "") return;
                var extension = Path.GetExtension(filePath);
                if (extension == null) return;
                if (extension.ToLower() != ".sldprt") return;
                var @class = new MakeDxfExportPartDataClass
                {
                    PdmBaseName = VaultName
                };
                bool isErrors;
                string newEdrwFileName;
                @class.CreateFlattPatternUpdateCutlistAndEdrawing(filePath, out newEdrwFileName, out isErrors, false, false, true, true);
                if (!isErrors)
                {
                    LoggerInfo("Закончена обработка " + Path.GetFileName(filePath), "", "PartInfoToXml");
                }
                else
                {
                    List<VaultSystem.VentsCadFiles> list;
                    VaultSystem.CheckInOutPdmNew(new List<VaultSystem.VentsCadFiles> { new VaultSystem.VentsCadFiles { LocalPartFileInfo = newEdrwFileName } }, true, VaultName, out list);
                    LoggerError("Закончена обработка детали " + Path.GetFileName(filePath) + " с ошибками", "", "PartInfoToXml");
                }
            }
            catch (Exception e)
            {
                LoggerError("Ошибка: " + e.StackTrace, GetHashCode().ToString("X"), "OnTaskRun");
            }
        }

        static void GabaritsForPaintingCamera(IModelDoc2 swmodel)
        {
            try
            {
                const long valueset = 1000;
                const int swDocPart = 1;
                const int swDocAssembly = 2;

                for (var i = 0; i < swmodel.GetConfigurationCount(); i++)
                {
                    i = i + 1;
                    var configname = swmodel.IGetConfigurationNames(ref i);

                    Configuration swConf = swmodel.GetConfigurationByName(configname);
                    if (swConf.IsDerived()) continue;
                    swmodel.EditRebuild3();

                    switch (swmodel.GetType())
                    {
                        case swDocPart:
                            {
                                var part = (IPartDoc)swmodel;

                            var box = part.GetPartBox(true);

                            swmodel.AddCustomInfo3(configname, "Длина", 30, "");
                            swmodel.AddCustomInfo3(configname, "Ширина", 30, "");
                            swmodel.AddCustomInfo3(configname, "Высота", 30, "");

                            swmodel.CustomInfo2[configname, "Длина"] =
                                Convert.ToString(
                                    Math.Round(Convert.ToDecimal((long) (Math.Abs(box[0] - box[3])*valueset)), 0));
                            swmodel.CustomInfo2[configname, "Ширина"] =
                                Convert.ToString(
                                    Math.Round(Convert.ToDecimal((long) (Math.Abs(box[1] - box[4])*valueset)), 0));
                            swmodel.CustomInfo2[configname, "Высота"] =
                                Convert.ToString(
                                    Math.Round(Convert.ToDecimal((long) (Math.Abs(box[2] - box[5])*valueset)), 0));
                            }
                            break;

                        case swDocAssembly:
                            {
                                var swAssy = (AssemblyDoc) swmodel;
                                var boxAss = swAssy.GetBox((int) swBoundingBoxOptions_e.swBoundingBoxIncludeRefPlanes);

                                swmodel.AddCustomInfo3(configname, "Длина", 30, "");
                                swmodel.AddCustomInfo3(configname, "Ширина", 30, "");
                                swmodel.AddCustomInfo3(configname, "Высота", 30, "");

                                swmodel.CustomInfo2[configname, "Длина"] =
                                    Convert.ToString(
                                        Math.Round(Convert.ToDecimal((long) (Math.Abs(boxAss[0] - boxAss[3])*valueset)), 0));
                                swmodel.CustomInfo2[configname, "Ширина"] =
                                    Convert.ToString(
                                        Math.Round(Convert.ToDecimal((long) (Math.Abs(boxAss[1] - boxAss[4])*valueset)), 0));
                                swmodel.CustomInfo2[configname, "Высота"] =
                                    Convert.ToString(
                                        Math.Round(Convert.ToDecimal((long) (Math.Abs(boxAss[2] - boxAss[5])*valueset)), 0));
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        internal void GetIdPdm(string path, out string fileName, out int fileIdPdm)
        {
            fileName = null;
            fileIdPdm = 0;
            try
            {             
                VaultSystem.GetIdPdm(path, out fileName, out fileIdPdm, VaultName);
            }
            catch (Exception)
            {
                //LoggerError(string.Format("Во время получения последней версии по пути {0} возникла ошибка!\nБаза - {1}. {2}", path, vaultName, e.Message), e.StackTrace, "GetLastVersionPdm");
            }
        }
        
        private void VentsMatdll(IList<string> materialP1, IList<string> покрытие, string newName)
        {
            try
            {
                _swApp.ActivateDoc2(newName, true, 0);
                var setMaterials = new SetMaterials();
                ToSQL.Conn = ConnectionToSql;
                var toSql = new ToSQL();
                setMaterials.ApplyMaterial("", "00", Convert.ToInt32(materialP1[0]), _swApp);
                _swApp.IActiveDoc2.Save();

                foreach (var confname in setMaterials.GetConfigurationNames(_swApp))
                {
                    foreach (var matname in setMaterials.GetCustomProperty(confname, _swApp))
                    {
                        toSql.AddCustomProperty(confname, matname.Name, _swApp);
                    }
                }

                if (покрытие[1] != "0")
                {
                    setMaterials.SetColor("00", покрытие[0], покрытие[1], покрытие[2], _swApp);
                }

                _swApp.IActiveDoc2.Save();

                try
                {
                    string message;
                    setMaterials.CheckSheetMetalProperty("00", _swApp, out message);
                    if (message != null)
                    {
                        MessageBox.Show(message, newName);
                    }
                }
                catch (Exception e)
                {
                 //   MessageBox.Show(e.StackTrace, e.Message);
                }
            }

            catch (Exception e)
            {
               // MessageBox.Show(e.StackTrace);
            }

            finally
            {
                try
                {
                    GabaritsForPaintingCamera(_swApp.IActiveDoc2);
                    _swApp.IActiveDoc2.Save();
                }
                catch (Exception e)
                {
                 //   MessageBox.Show(e.ToString());
                }    
            }
        }

        //Todo using

        // Данный класс реализует интерейс IDisposable
        class FinalizeObject : IDisposable
        {
            private int Id { get; set; }

            public FinalizeObject(int id)
            {
                this.Id = id;
            }

            // Реализуем метод Dispose()
            public void Dispose()
            {
                Console.WriteLine("Высвобождение объекта!");
            }
        }

        class Program
        {
            static void Main(string[] args)
            {
                FinalizeObject obj = new FinalizeObject(4);
                obj.Dispose();

                Console.Read();

                using (FinalizeObject objusing = new FinalizeObject(4))
                {
                    // Необходимые действия
                }

            }
        }
    
      







void SwPartParamsChangeWithNewName(string partName, string newName, string[,] newParams, bool newFuncOfAdding)
        {
            try
            {
                LoggerDebug($"Начало изменения детали {partName}", "", "SwPartParamsChangeWithNewName");
                var swDoc = _swApp.OpenDoc6(partName + ".SLDPRT", (int)swDocumentTypes_e.swDocPART, (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);
                
                var modName = swDoc.GetPathName();
                for (var i = 0; i < newParams.Length / 2; i++)
                {
                    try
                    {
                        var myDimension = ((Dimension)(swDoc.Parameter(newParams[i, 0] + "@" + partName + ".SLDPRT")));
                        var param = Convert.ToDouble(newParams[i, 1]); var swParametr = param;
                        myDimension.SystemValue = swParametr / 1000;
                        swDoc.EditRebuild3();
                    }
                    catch (Exception e)
                    {
                        LoggerDebug(string.Format("Во время изменения детали {4} произошла ошибка при изменении параметров {0}={1}. {2} {3}",
                            newParams[i, 0], newParams[i, 1], e.TargetSite, e.Message, Path.GetFileNameWithoutExtension(modName)),
                            "", "SwPartParamsChangeWithNewName");
                    }
                }

                if (newName == "") { return; }
                
                swDoc.EditRebuild3();
                swDoc.ForceRebuild3(false);


                if (!newFuncOfAdding)
                {
                    NewComponents.Add(new VaultSystem.VentsCadFiles
                    {
                        LocalPartFileInfo = new FileInfo(newName + ".SLDPRT").FullName
                    });
                }
                
                if (newFuncOfAdding)
                {
                    NewComponents.Add(new VaultSystem.VentsCadFiles
                    {
                        LocalPartFileInfo = new FileInfo(newName + ".SLDPRT").FullName,
                        PartIdSql = Convert.ToInt32(newName.Substring(newName.LastIndexOf('-') + 1))
                    });
                }

                swDoc.SaveAs2(newName + ".SLDPRT", (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                _swApp.CloseDoc(newName + ".SLDPRT");
                LoggerInfo($"Деталь {partName} изменена и сохранена по пути {new FileInfo(newName).FullName}", "", "SwPartParamsChangeWithNewName");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }


        void SwPartParamsChangeWithNewName(string partName, string newName, string[,] newParams, bool newFuncOfAdding, IList<string> copies)
        {
            try
            {
                LoggerDebug($"Начало изменения детали {partName}", "", "SwPartParamsChangeWithNewName");
                var swDoc = _swApp.OpenDoc6(partName + ".SLDPRT", (int)swDocumentTypes_e.swDocPART, (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);
                var modName = swDoc.GetPathName();
                for (var i = 0; i < newParams.Length / 2; i++)
                {
                    try
                    {
                        var myDimension = ((Dimension)(swDoc.Parameter(newParams[i, 0] + "@" + partName + ".SLDPRT")));
                        var param = Convert.ToDouble(newParams[i, 1]); var swParametr = param;
                        myDimension.SystemValue = swParametr / 1000;
                        swDoc.EditRebuild3();
                    }
                    catch (Exception e)
                    {
                        LoggerDebug(string.Format("Во время изменения детали {4} произошла ошибка при изменении параметров {0}={1}. {2} {3}",
                            newParams[i, 0], newParams[i, 1], e.TargetSite, e.Message, Path.GetFileNameWithoutExtension(modName)),
                            "", "SwPartParamsChangeWithNewName");
                    }
                }
                if (newName == "") { return; }

                GabaritsForPaintingCamera(swDoc);

                swDoc.EditRebuild3();
                swDoc.ForceRebuild3(false);

                if (!newFuncOfAdding)
                {
                    NewComponents.Add(new VaultSystem.VentsCadFiles{LocalPartFileInfo = new FileInfo(newName + ".SLDPRT").FullName});
                }

                if (newFuncOfAdding)
                {
                    NewComponents.Add(new VaultSystem.VentsCadFiles
                    {
                        LocalPartFileInfo = new FileInfo(newName + ".SLDPRT").FullName,
                        PartIdSql = Convert.ToInt32(newName.Substring(newName.LastIndexOf('-') + 1))
                    });
                }

             
                swDoc.SaveAs2(new FileInfo(newName + ".SLDPRT").FullName, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);

                if (copies != null)
                {
                    swDoc.SaveAs2(new FileInfo(copies[0] + ".SLDPRT").FullName, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, true, true);
                    swDoc.SaveAs2(new FileInfo(copies[1] + ".SLDPRT").FullName, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, true, true);
                }
              
                _swApp.CloseDoc(newName + ".SLDPRT");
                LoggerInfo($"Деталь {partName} изменена и сохранена по пути {new FileInfo(newName).FullName}", "", "SwPartParamsChangeWithNewName");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

    }
}
