using EdmLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
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

        internal void GetLastVersionAsmPdm(string path, string pdmBase)
        {            
            try
            {
                LoggerInfo($"Получение последней версии по пути {path}\nБаза - {pdmBase}", "", "GetLastVersionPdm");
                //MessageBox.Show($"Получение последней версии по пути {path}\nБаза - {pdmBase}", "GetLastVersionPdm");
                var vaultSource = new EdmVault5();
                IEdmFolder5 oFolder;
                vaultSource.LoginAuto(pdmBase, 0);
                var edmFile5 = vaultSource.GetFileFromPath(path, out oFolder);
                edmFile5.GetFileCopy(0, 0, oFolder.ID, (int)EdmGetFlag.EdmGet_RefsVerLatest);
            }
            catch (Exception exception)
            {
                LoggerError(
                    $"Во время получения последней версии по пути {path} возникла ошибка!\nБаза - {pdmBase}. {exception.Message}", exception.StackTrace, "GetLastVersionPdm");
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
            catch (Exception exception)
            {
                LoggerError($"Удаление уравнения #{index} в модели {swModel.GetPathName()}. {exception.Message}", exception.StackTrace, "DelEquations");
            }
        }

        public class VentsCadFiles
        {
            public int PartIdSql { get; set; }

            public string PartName { get; set; }

            public string PartWithoutExtension => LocalPartFileInfo.Substring(LocalPartFileInfo.LastIndexOf('\\'));

            public int PartIdPdm { get; set; }

            public string LocalPartFileInfo { get; set; }
        }
        
        void CheckInOutPdm(List<VentsCadFiles> filesList, string pdmBase, out List<VentsCadFiles> newFilesList)
        {
            if (!_vault5.IsLoggedIn)
            {
                //MessageBox.Show("Вход в базу - " + pdmBase);
                _vault5.LoginAuto(pdmBase, 0);
                //_vault5.LoginAuto("Tets_debag", 0);
            }

            BatchAddFiles(filesList);            

            try
            {
                BatchUnLock(filesList);
            }
            catch (Exception){}                                   

            newFilesList = new List<VentsCadFiles>();                      

            foreach (var file in filesList)
            {
                try
                {
                    #region Check In Out Pdm

                    //    var vault1 = new EdmVault5();
                    //    IEdmFolder5 oFolder;
                    //    vault1.LoginAuto("Tets_debag", 0);
                    //    var attempts = 1;
                    //m1:
                    //    Thread.Sleep(350);
                    //    var edmFile5 = vault1.GetFileFromPath(file.LocalPartFileInfo.FullName, out oFolder);

                    //   // MessageBox.Show(file.LocalPartFileInfo.FullName);

                    //    if (oFolder == null)
                    //    {
                    //        attempts++;
                    //        if (attempts > 25)
                    //        {
                    //            MessageBox.Show(file.LocalPartFileInfo.FullName, "Не взят");
                    //            goto m2;
                    //        }
                    //        goto m1;
                    //    }
                    //m2:
                    //    // Разрегистрировать
                    //    if (registration == false)
                    //    {
                    //        edmFile5.GetFileCopy(0, 0, oFolder.ID, (int)EdmGetFlag.EdmGet_Simple);
                    //        edmFile5.LockFile(oFolder.ID, 0);
                    //    }
                    //    // Зарегистрировать
                    //    if (registration)
                    //    {
                    //        edmFile5.UnlockFile(oFolder.ID, "");
                    //    }                    
                    //    LoggerInfo(string.Format("В базе PDM - {1}, зарегестрирован документ по пути {0}", file.LocalPartFileInfo.FullName, pdmBase), "", "CheckInOutPdm");

                    #endregion
                }
                catch (Exception exception)
                {
                    LoggerError(
                        $"Во время регистрации документа по пути {file.LocalPartFileInfo} возникла ошибка\nБаза - {pdmBase}. {exception.Message}", "", "CheckInOutPdm");
                }
                finally
                {
                    string fileName;
                    int fileIdPdm;
                    GetIdPdm(file.LocalPartFileInfo, out fileName, out fileIdPdm);
                    newFilesList.Add(new VentsCadFiles
                    {
                        PartName = fileName,
                        PartIdPdm = fileIdPdm,
                        LocalPartFileInfo = file.LocalPartFileInfo,
                        PartIdSql = file.PartIdSql
                    });
                }
            }
        }

        class EpdmSearch
        {
            public string VaultName { private get; set; }

            private IEdmVault5 _edmVault5;

            /// <summary>
            /// Тип документа для поиска. 
            /// </summary>
            public enum SwDocType
            {
                /// <summary>
                /// Точное имя файла для поиска
                /// </summary>
                SwDocNone = 0,
                /// <summary>
                /// Имя файла с расширением .sldprt
                /// </summary>
                SwDocPart = 1,
                /// <summary>
                /// Имя файла с расширением .sldasm
                /// </summary>
                SwDocAssembly = 2,
                /// <summary>
                /// Имя файла с расширением .slddrw
                /// </summary>
                SwDocDrawing = 3,
                /// <summary>
                /// По схожести
                /// </summary>
                SwDocLike = 4,
            }

            public class FindedDocuments
            {
                public string Path { get; set; }

                public int FileId { get; set; }

                public int ProjectId { get; set; }
            }

            public void SearchDoc(string fileName, SwDocType swDocType, out List<FindedDocuments> fileList)
            {
                var files = new List<FindedDocuments>();

                try
                {
                    if (_edmVault5 == null)
                    {
                        _edmVault5 = new EdmVault5();
                    }
                    var edmVault7 = (IEdmVault7)_edmVault5;

                    if (!_edmVault5.IsLoggedIn)
                    {
                        _edmVault5.LoginAuto(VaultName, 0);
                    }

                    //Search for all text files in the edmVault7
                    var edmSearch5 = (IEdmSearch5)edmVault7.CreateUtility(EdmUtility.EdmUtil_Search);

                    var extenison = "";
                    var like = "";

                    switch ((int)swDocType)
                    {
                        case 0:
                            extenison = "";
                            like = "";
                            break;
                        case 1:
                            like = "%";
                            extenison = ".sldprt";
                            break;
                        case 2:
                            like = "%";
                            extenison = "%.sldasm";
                            break;
                        case 3:
                            like = "%";
                            extenison = "%.slddrw";
                            break;
                        case 4:
                            like = "%";
                            extenison = "%.%";
                            break;
                    }
                    edmSearch5.FileName = like + fileName + extenison;

                    var edmSearchResult5 = edmSearch5.GetFirstResult();

                    while (edmSearchResult5 != null)
                    {
                        files.Add(
                            new FindedDocuments
                            {
                                FileId = edmSearchResult5.ID,
                                ProjectId = edmSearchResult5.ParentFolderID,
                                Path = edmSearchResult5.Path
                            });
                        
                        edmSearchResult5 = edmSearch5.GetNextResult();
                    }

                    if (edmSearch5.GetFirstResult() == null)
                    {
                        //LoggerInfo("Файл не найден!");
                        files = null;
                    }
                }

                catch (Exception ex)
                {
                    //LoggerError(ex.Message);
                }
                fileList = files;
            }
        }

        bool GetExistingFile(string fileName, int type)
        {
            var epdmSearch = new EpdmSearch { VaultName = VaultName };
            List<EpdmSearch.FindedDocuments> найденныеФайлы;
            switch (type)
            {
                case 0:
                    epdmSearch.SearchDoc(fileName, EpdmSearch.SwDocType.SwDocAssembly, out найденныеФайлы);
                    if (найденныеФайлы?.Count > 0) goto m1;
                    break;
                case 1:
                    epdmSearch.SearchDoc(fileName, EpdmSearch.SwDocType.SwDocPart, out найденныеФайлы);
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
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString()); return false;
            }
            m2:
            return false;
        }




        bool GetExistingFile(string partName, out string path, out int fileId, out int projectId)
        {
            fileId = 0;
            projectId = 0;
            path = null;
            // MessageBox.Show("Поиск" + partName);
            var epdmSearch = new EpdmSearch { VaultName = VaultName };
            List<EpdmSearch.FindedDocuments> найденныеФайлы;
            epdmSearch.SearchDoc(partName, EpdmSearch.SwDocType.SwDocAssembly, out найденныеФайлы);
            // MessageBox.Show(найденныеФайлы?[0] + " - Сборка");
            if (найденныеФайлы != null) goto m1;

            epdmSearch.SearchDoc(partName, EpdmSearch.SwDocType.SwDocPart, out найденныеФайлы);
            // MessageBox.Show(найденныеФайлы?[0] + " - Деталь");
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
            var status = false;
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

                    //if (fileType == null)
                    //{
                    //    sqlParameter.AddWithValue("@FileType", DBNull.Value);
                    //}
                    //else
                    //{
                    //    sqlParameter.AddWithValue("@FileType", fileType);
                    //}

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

                    //sqlCommand.Parameters.Add("@status", SqlDbType.Bit);
                    //sqlCommand.Parameters["@status"].Direction = ParameterDirection.ReturnValue;
                    sqlCommand.ExecuteNonQuery();

                    //status = Convert.ToBoolean(sqlCommand.Parameters["@status"].Value);

                    //MessageBox.Show(
                    //    $"fileName - {fileName} idPdm - {idPdm} fileType - {fileType} typeOfSpigot - {typeOfSpigot} height - {height} width - {width}");
                    //MessageBox.Show(status ? "Деталь есть в базе" : "Детали нет в базе", fileName);
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
                    List<VentsCadFiles> list;
                    CheckInOutPdm(new List<VentsCadFiles> { new VentsCadFiles { LocalPartFileInfo = newEdrwFileName } }, VaultName, out list);
                    LoggerError("Закончена обработка детали " + Path.GetFileName(filePath) + " с ошибками", "", "PartInfoToXml");
                }
            }
            catch (Exception exception)
            {
                LoggerError("Ошибка: " + exception.StackTrace, GetHashCode().ToString("X"), "OnTaskRun");
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
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }

        //bool ExistFileInPdmBase(string fileName)
        //{
            //var emdpService = new EpdmServiceClient();
            //var path = emdpService.SearchDoc(fileName);
            //MessageBox.Show(path[0].FileName + " " + (path.Length > 0) );
            //return path.Length > 0;
        //}

        readonly EdmVault5 _vault5 = new EdmVault5();        

        public void BatchAddFiles(List<VentsCadFiles> filesList)
        {
            try
            {
                poAdder = (IEdmBatchAdd2)_vault5.CreateUtility(EdmUtility.EdmUtil_BatchAdd);

                foreach (var file in filesList)
                {
                    var directoryName = file.LocalPartFileInfo.Replace(file.PartWithoutExtension, "");
                    poAdder.AddFileFromPathToPath(file.LocalPartFileInfo, directoryName, 0);
                }
                poAdder.CommitAdd(0, null, 0);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString(), "BatchAddFiles");
            }            
        }
                
        public void BatchUnLock(List<VentsCadFiles> filesList)
        {
            batchUnlocker = (IEdmBatchUnlock2)_vault5.CreateUtility(EdmUtility.EdmUtil_BatchUnlock);
            var i = 0;
            var ppoSelection = new EdmSelItem[filesList.Count];

            foreach (var file in filesList)
            {
                aFile = _vault5.GetFileFromPath(file.LocalPartFileInfo, out ppoRetParentFolder);                
                aPos = aFile.GetFirstFolderPosition();
                aFolder = aFile.GetNextFolder(aPos);

                ppoSelection[i] = new EdmSelItem
                {
                    mlDocID = aFile.ID,
                    mlProjID = aFolder.ID
                };
                i++;
            }

            // Add selections to the batch of files to check in
            batchUnlocker.AddSelection(_vault5, ppoSelection);

            if ((batchUnlocker != null))
            {
                batchUnlocker.CreateTree(0, (int)EdmUnlockBuildTreeFlags.Eubtf_ShowCloseAfterCheckinOption + (int)EdmUnlockBuildTreeFlags.Eubtf_MayUnlock);
                fileList = (IEdmSelectionList6)batchUnlocker.GetFileList((int)EdmUnlockFileListFlag.Euflf_GetUnlocked + (int)EdmUnlockFileListFlag.Euflf_GetUndoLocked + (int)EdmUnlockFileListFlag.Euflf_GetUnprocessed);
                aPos = fileList.GetHeadPosition();

                while (!(aPos.IsNull))
                {
                    fileList.GetNext2(aPos, out poSel);
                }
                batchUnlocker.UnlockFiles(0, null);
            }
        }

        internal void GetIdPdm(string path, out string fileName, out int fileIdPdm)
        {
            fileName = null;
            fileIdPdm = 0;
            try
            {                
                //LoggerInfo(string.Format("Получение последней версии по пути {0}\nБаза - {1}", path, pdmBase), "", "GetLastVersionPdm");               
                IEdmFolder5 oFolder;
                var tries = 1;
                m1:
                Thread.Sleep(500);
                var edmFile5 = _vault5.GetFileFromPath(path, out oFolder);
                if (oFolder == null)
                {
                    tries++;
                    if (tries > 20)
                    {
                        return;
                    }
                    goto m1;
                }
                edmFile5.GetFileCopy(0, 0, oFolder.ID, (int)EdmGetFlag.EdmGet_RefsVerLatest);
                fileName = edmFile5.Name;
                fileIdPdm = edmFile5.ID;
            }
            catch (Exception)
            {
                //LoggerError(string.Format("Во время получения последней версии по пути {0} возникла ошибка!\nБаза - {1}. {2}", path, pdmBase, exception.Message), exception.StackTrace, "GetLastVersionPdm");
            }
        }
        

        private void VentsMatdll(IList<string> materialP1, IList<string> покрытие, string newName)
        {
            try
            {
                //MessageBox.Show(newName);                

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
                catch (Exception exception)
                {
                    MessageBox.Show(exception.StackTrace, exception.Message);
                }
            }

            catch (Exception exception)
            {
                MessageBox.Show(exception.StackTrace);
            }

            finally
            {
                try
                {
                    GabaritsForPaintingCamera(_swApp.IActiveDoc2);
                    _swApp.IActiveDoc2.Save();
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.ToString());
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
                    catch (Exception exception)
                    {
                        LoggerDebug(string.Format("Во время изменения детали {4} произошла ошибка при изменении параметров {0}={1}. {2} {3}",
                            newParams[i, 0], newParams[i, 1], exception.TargetSite, exception.Message, Path.GetFileNameWithoutExtension(modName)),
                            "", "SwPartParamsChangeWithNewName");
                    }
                }

                if (newName == "") { return; }
                
                swDoc.EditRebuild3();
                swDoc.ForceRebuild3(false);


                if (!newFuncOfAdding)
                {
                    NewComponents.Add(new VentsCadFiles
                    {
                        LocalPartFileInfo = new FileInfo(newName + ".SLDPRT").FullName
                    });
                }
                
                if (newFuncOfAdding)
                {
                    NewComponents.Add(new VentsCadFiles
                    {
                        LocalPartFileInfo = new FileInfo(newName + ".SLDPRT").FullName,
                        PartIdSql = Convert.ToInt32(newName.Substring(newName.LastIndexOf('-') + 1))
                    });
                }

                swDoc.SaveAs2(newName + ".SLDPRT", (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                _swApp.CloseDoc(newName + ".SLDPRT");
                LoggerInfo($"Деталь {partName} изменена и сохранена по пути {new FileInfo(newName).FullName}", "", "SwPartParamsChangeWithNewName");
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
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
                    catch (Exception exception)
                    {
                        LoggerDebug(string.Format("Во время изменения детали {4} произошла ошибка при изменении параметров {0}={1}. {2} {3}",
                            newParams[i, 0], newParams[i, 1], exception.TargetSite, exception.Message, Path.GetFileNameWithoutExtension(modName)),
                            "", "SwPartParamsChangeWithNewName");
                    }
                }
                if (newName == "") { return; }

                GabaritsForPaintingCamera(swDoc);

                swDoc.EditRebuild3();
                swDoc.ForceRebuild3(false);

                if (!newFuncOfAdding)
                {
                    NewComponents.Add(new VentsCadFiles{LocalPartFileInfo = new FileInfo(newName + ".SLDPRT").FullName});
                }

                if (newFuncOfAdding)
                {
                    NewComponents.Add(new VentsCadFiles
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
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }

    }
}
