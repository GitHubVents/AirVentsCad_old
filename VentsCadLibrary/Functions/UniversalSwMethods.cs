using System;
using System.Collections.Generic;
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
        public static string ConnectionToSql { get; set; }

        public string VaultName { get; set; }

        public string DestVaultName { get; set; }

        static string LocalPath(string vault)
        {
            try
            {
                return VaultSystem.GetSwEpdRootFolderPath(vault);

                #region to delete

                //var edmVault5 = new EdmVault5();
                //edmVault5.LoginAuto(vault, 0);
                //return edmVault5.RootFolder.LocalPath;

                #endregion
            }
            catch (Exception e)
            {
                Логгер.Ошибка(string.Format("В базе - {1}, не удалось получить корнувую папку ({0})", e.Message, vault), e.StackTrace, "Add", "LocalPath(string Vault)");
                return null;
            }
        }

        SldWorks _swApp;

        public List<VaultSystem.VentsCadFiles> NewComponents = new List<VaultSystem.VentsCadFiles>();

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
                Логгер.Информация($"Получение последней версии по пути {path}\nБаза - {vaultName}", null, "", "GetLastVersionPdm");

                VaultSystem.GetLastVersionOfFile(path, vaultName);
            }
            catch (Exception e)
            {
                Логгер.Ошибка($"Во время получения последней версии по пути {path} возникла ошибка!\nБаза - {vaultName}. {e.Message}", e.StackTrace, null, "GetLastVersionPdm");
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
                Логгер.Информация($"Удаление уравнения #{index} в модели {swModel.GetPathName()}", null, "", "DelEquations");
                var myEqu = swModel.GetEquationMgr();
                myEqu.Delete(index);
                swModel.EditRebuild3();                
            }
            catch (Exception e)
            {
                Логгер.Ошибка($"Удаление уравнения #{index} в модели {swModel.GetPathName()}. {e.Message}", null, e.StackTrace, "DelEquations");
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
                    Логгер.Информация("Закончена обработка детали " + Path.GetFileName(filePath), null, "", "PartInfoToXml");
                }
                else
                {
                    List<VaultSystem.VentsCadFiles> list;
                    VaultSystem.CheckInOutPdmNew(new List<VaultSystem.VentsCadFiles> { new VaultSystem.VentsCadFiles { LocalPartFileInfo = newEdrwFileName } }, true, VaultName, out list);
                    Логгер.Информация("Закончена обработка детали " + Path.GetFileName(filePath) + " с ошибками", null, "", "PartInfoToXml");
                }
            }
            catch (Exception e)
            {
                Логгер.Ошибка("Ошибка:" + e.Message, e.StackTrace, "SwEpdm", "AddToPdmByPath");
                
            }
        }

        static void GabaritsForPaintingCamera(IModelDoc2 swmodel)
        {
            try
            {
                const long valueset = 1000;
                const int swDocPart = 1;
                const int swDocAssembly = 2;

                var configNames = swmodel.GetConfigurationNames();

                foreach (var configname in configNames)
                {
                    try
                    {
                        IConfiguration swConf = swmodel.GetConfigurationByName(configname);
                        if (swConf.IsDerived()) continue;
                    }
                    catch (Exception)
                    {
                       //
                    }

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
                Логгер.Ошибка("Ошибка: " + e.StackTrace, e.GetHashCode().ToString("X"), null, "GabaritsForPaintingCamera");
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
            catch (Exception e)
            {
                Логгер.Ошибка("Ошибка: " + e.StackTrace, e.GetHashCode().ToString("X"), path, "GetIdPdm");
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
                       // MessageBox.Show(message, newName);
                    }
                }
                catch (Exception e)
                {
                    Логгер.Ошибка("Ошибка: " + e.StackTrace, e.GetHashCode().ToString("X"), newName, "VentsMatdll");
                }
            }

            catch (Exception e)
            {
                Логгер.Ошибка("Ошибка: " + e.StackTrace, e.GetHashCode().ToString("X"), newName, "VentsMatdll");
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
                    Логгер.Ошибка("Ошибка: " + e.StackTrace, e.GetHashCode().ToString("X"), newName, "VentsMatdll");
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
                Логгер.Информация($"Начало изменения детали {partName}", newName, null, "SwPartParamsChangeWithNewName");
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
                        Логгер.Ошибка(string.Format("Во время изменения детали {4} произошла ошибка при изменении параметров {0}={1}. {2} {3}",
                            newParams[i, 0], newParams[i, 1], e.TargetSite, e.Message, Path.GetFileNameWithoutExtension(modName)), newName, null, "SwPartParamsChangeWithNewName");
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
                Логгер.Информация($"Деталь {partName} изменена и сохранена по пути {new FileInfo(newName).FullName}", null, newName, "SwPartParamsChangeWithNewName");
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
                Логгер.Информация($"Начало изменения детали {partName}", null, newName, "SwPartParamsChangeWithNewName");
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
                        Логгер.Ошибка(string.Format("Во время изменения детали {4} произошла ошибка при изменении параметров {0}={1}. {2} {3}",
                            newParams[i, 0], newParams[i, 1], e.TargetSite, e.Message, Path.GetFileNameWithoutExtension(modName)), null,
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

                Логгер.Информация($"Деталь {partName} изменена и сохранена по пути {new FileInfo(newName).FullName}", null, "", "SwPartParamsChangeWithNewName");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

    }
}
