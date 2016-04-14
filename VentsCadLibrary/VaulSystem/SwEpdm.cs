using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using EdmLib;

namespace VentsCadLibrary
{
    public class SwEpdm
    {
        public static void AddToPdmByPath(string path, string vaultName)
        {
            try
            {
                var vault1 = new EdmVault5Class();
                if (!vault1.IsLoggedIn)
                {
                    vault1.LoginAuto(vaultName, 0);
                }
                var fileDirectory = new FileInfo(path).DirectoryName;

                var fileFolder = vault1.GetFolderFromPath(fileDirectory);
                fileFolder.AddFile(fileFolder.ID, "", Path.GetFileName(path));
            }
            catch (Exception e)
            {
                Логгер.Ошибка("Ошибка:" + e.Message, e.StackTrace, "SwEpdm", "AddToPdmByPath");
            }
        }

        public static void GoToFile(string path, string vaultName)
        {
            try
            {
                IEdmVault8 edmVault8 = new EdmVault5Class();
                edmVault8.LoginAuto(vaultName, 0);
                if (string.IsNullOrEmpty(path)) return;
                edmVault8.OpenContainingFolder(path);
            }
            catch (Exception)
            {
                //
            }
        }

        public static string GetSwEpdRootFolderPath(string vaultName)
        {
            try
            {
                var edmVault5 = new EdmVault5();
                edmVault5.LoginAuto(vaultName, 0);
                return edmVault5.RootFolderPath;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void CreateDistDirectory(string path, string vaultName)
        {
            try
            {
                //if (Directory.Exists(path)){return;}
                var vault1 = new EdmVault5();

                if (!vault1.IsLoggedIn)
                {
                    vault1.LoginAuto(vaultName, 0);
                }
                var vault2 = (IEdmVault7)vault1;
                var directoryInfo = new DirectoryInfo(path);
                if (directoryInfo.Parent == null) return;
                var parentFolder = vault2.GetFolderFromPath(directoryInfo.Parent.FullName);
                parentFolder.AddFolder(0, directoryInfo.Name);
            }
            catch (Exception)
            {
                // 
            }
        }

        public static void CheckInOutPdm(string filePath, bool registration, string vaultName)
        {
            var retryCount = 2;
            var success = false;
            while (!success && retryCount > 0)
            {
                try
                {
                    var vault1 = new EdmVault5();
                    if (!vault1.IsLoggedIn) { vault1.LoginAuto(vaultName, 0); }
                    var trys = 1;
                    m1:

                    try
                    {
                        IEdmFolder5 oFolder;
                        var edmFile5 = vault1.GetFileFromPath(filePath, out oFolder);
                        if (edmFile5 == null)
                        {
                            if (trys > 5)
                            {
                                return;
                            }
                            Thread.Sleep(1000);
                            trys++;
                            goto m1;
                        }

                        // Разрегистрировать
                        if (registration == false) { edmFile5.LockFile(oFolder.ID, 0); }

                        // Зарегистрировать
                        if (registration) { edmFile5.UnlockFile(oFolder.ID, ""); }
                        success = true;
                    }
                    catch (Exception)
                    {
                        goto m1;
                    }

                }
                catch (Exception)
                {
                    retryCount--;
                    Thread.Sleep(200);
                    if (retryCount == 0)
                    {
                        //
                    }
                }
            }
        }

        public static void CheckInOutPdm(List<FileInfo> filesList, bool registration, string vaultName)
        {
            foreach (var file in filesList)
            {
                var retryCount = 2;
                var success = false;
                while (!success && retryCount > 0)
                {
                    try
                    {
                        var vault1 = new EdmVault5();
                        IEdmFolder5 oFolder;
                        vault1.LoginAuto(vaultName, 0);
                        var edmFile5 = vault1.GetFileFromPath(file.FullName, out oFolder);

                        // Разрегистрировать
                        if (registration == false)
                        {
                            edmFile5.GetFileCopy(0, 0, oFolder.ID, (int)EdmGetFlag.EdmGet_Simple);
                            m1:
                            edmFile5.LockFile(oFolder.ID, 0);
                            //MessageBox.Show(edmFile5.Name);
                            Thread.Sleep(50);
                            var j = 0;
                            if (!edmFile5.IsLocked)
                            {
                                j++;
                                if (j > 5)
                                {
                                    goto m3;
                                }
                                goto m1;
                            }
                        }
                        // Зарегистрировать
                        if (registration)
                        {
                            m2:
                            edmFile5.UnlockFile(oFolder.ID, "");
                            Thread.Sleep(50);
                            var i = 0;
                            if (edmFile5.IsLocked)
                            {
                                i++;
                                if (i > 5)
                                {
                                    goto m4;
                                }
                                goto m2;
                            }
                        }
                        m3:
                        m4:
                        //LoggerInfo(string.Format("В базе PDM - {1}, зарегестрирован документ по пути {0}", file.FullName, vaultName), "", "CheckInOutPdm");
                        success = true;
                    }
                    catch (Exception)
                    {
                        retryCount--;
                        Thread.Sleep(200);
                        if (retryCount == 0)
                        {
                            //
                        }
                    }
                }
                if (!success)
                {
                    //LoggerError($"Во время регистрации документа по пути {file.FullName} возникла ошибка\nБаза - {vaultName}. {ex.Message}", "", "CheckInOutPdm");
                }
            }
        }

        public static void GetAsmFilesAsBuild(string path, string vaultName)
        {
            try
            {
                var vaultSource = new EdmVault5();
                IEdmFolder5 oFolder;
                if (!vaultSource.IsLoggedIn)
                {
                    vaultSource.LoginAuto(vaultName, 0);
                }
                var edmFile5 = vaultSource.GetFileFromPath(path, out oFolder);
                MessageBox.Show(path, vaultName);
                edmFile5.GetFileCopy(0, 0, oFolder.ID, (int)EdmGetFlag.EdmGet_Refs + (int)EdmGetFlag.EdmGet_RefsOnlyMissing + (int)EdmGetFlag.EdmGet_RefsOverwriteLocked);
                MessageBox.Show("Get");
                //(int)EdmGetFlag.EdmGet_Refs + (int)EdmGetFlag.EdmGet_RefsOnlyMissing);

            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Не удалось получить последнюю версию файлов из хранилища");
            }
        }

        public void GetObjectType(int idPdm, string vaultName)
        {
            try
            {
                var vaultSource = new EdmVault5();
                IEdmFolder5 oFolder;
                if (!vaultSource.IsLoggedIn)
                {
                    vaultSource.LoginAuto(vaultName, 0);
                }

  //              var objectType = vaultSource..GetObject(EdmObjectType., idPdm);

                //File = (IEdmFile5)vault1.GetObject(EdmObjectType.EdmObject_File, curListFile.mlFileID);


           //     var edmFile5 = vaultSource.GetFileFromPath(path, out oFolder);
       //         edmFile5.GetFileCopy(0, 0, oFolder.ID, (int)EdmGetFlag.EdmGet_RefsVerLatest);
                //edmFile5.GetFileCopy(0, 0, oFolder.ID, (int)EdmGetFlag.EdmGet_Simple);
            }
            catch (Exception excepiton)
            {
                MessageBox.Show(excepiton.Message);
            }
        }


        public static void GetLastVersionOfFile(string path, string vaultName)
        {
            try
            {
                var vaultSource = new EdmVault5();
                IEdmFolder5 oFolder;
                if (!vaultSource.IsLoggedIn)
                {
                    vaultSource.LoginAuto(vaultName, 0);
                }
                var edmFile5 = vaultSource.GetFileFromPath(path, out oFolder);
                edmFile5.GetFileCopy(0, 0, oFolder.ID, (int)EdmGetFlag.EdmGet_RefsVerLatest);
                //edmFile5.GetFileCopy(0, 0, oFolder.ID, (int)EdmGetFlag.EdmGet_Simple);
            }
            catch (Exception)
            {
                //
            }
        }

        public static int GetVersionOfFile(string path, string vaultName)
        {
            try
            {
                var vaultSource = new EdmVault5();
                IEdmFolder5 oFolder;
                if (!vaultSource.IsLoggedIn)
                {
                    vaultSource.LoginAuto(vaultName, 0);
                }
                var edmFile5 = vaultSource.GetFileFromPath(path, out oFolder);
                edmFile5.GetFileCopy(0, 0, oFolder.ID, (int)EdmGetFlag.EdmGet_RefsVerLatest);
                //edmFile5.GetFileCopy(0, 0, oFolder.ID, (int)EdmGetFlag.EdmGet_Simple);

                return edmFile5.CurrentVersion;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static List<KeyValuePair<string, int>> GetSwEpdmVaults() 
        {
            var vaults = new List<KeyValuePair<string, int>>();
            try
            {
                var edmVault5 = new EdmVault5();
                var vault = edmVault5 as IEdmVault8;
                Array views;
                Debug.Assert(vault != null, "vault != null");
                vault.GetVaultViews(out views, false);

                foreach (EdmViewInfo view in views)
                {
                    vaults.Add(new KeyValuePair<string, int>(view.mbsVaultName, 1));
                }
            }
            catch (Exception)
            {
                vaults.Add(new KeyValuePair<string, int>("Не найдено доступных хранилищ", 9));
            }
            return vaults;
        }

        public static void GetIdPdm(string path, out string fileName, out int fileIdPdm, out int currentVerison, out List<string> configs, bool getFileCopy, string vaultName)
        {
            fileName = null;
            fileIdPdm = 0;
            currentVerison = 0;
            configs = new List<string>();
            try
            {
                IEdmFolder5 oFolder;
                var tries = 1;
                m1:
                Thread.Sleep(500);
                path = new FileInfo(path).FullName;
                var vault1 = new EdmVault5();

                if (!vault1.IsLoggedIn)
                {
                    vault1.LoginAuto(vaultName, 0);
                }

                var edmFile5 = vault1.GetFileFromPath(path, out oFolder);
                
                if (oFolder == null)
                {
                    tries++;
                    if (tries > 10)
                    {
                        return;
                    }
                    goto m1;
                }

                if (getFileCopy)
                {
                    try
                    {
                        edmFile5.GetFileCopy(0, 0, oFolder.ID, (int)EdmGetFlag.EdmGet_RefsVerLatest);
                    }
                    catch (Exception)
                    {
                        // 
                    }
                }

                fileName = edmFile5.Name;
                fileIdPdm = edmFile5.ID;
                currentVerison = edmFile5.CurrentVersion;
                EdmStrLst5 list = edmFile5.GetConfigurations();

                IEdmPos5 pos = default(IEdmPos5);
                pos = list.GetHeadPosition();
                string cfgName = null;
                while (!pos.IsNull)
                {
                    cfgName = list.GetNext(pos);
                    if (cfgName == "@") continue;                    
                    configs.Add(cfgName);
                }                
            }
            catch (Exception ex)
            {
               // MessageBox.Show(ex.StackTrace);
            }
        }

        internal static void GetIdPdm(string path, out string fileName, out int fileIdPdm, EdmVault5 edmVault5)
        {
            fileName = null;
            fileIdPdm = 0;
            try
            {
                IEdmFolder5 oFolder;
                var tries = 1;
                m1:
                Thread.Sleep(500);
                path = new FileInfo(path).FullName;

                var edmFile5 = edmVault5.GetFileFromPath(path, out oFolder);

                if (oFolder == null)
                {
                    tries++;
                    if (tries > 10)
                    {
                        return;
                    }
                    goto m1;
                }

                try
                {
                    edmFile5.GetFileCopy(0, 0, oFolder.ID, (int)EdmGetFlag.EdmGet_RefsVerLatest);
                }
                catch (Exception)
                {
                    // 
                }

                fileName = edmFile5.Name;
                fileIdPdm = edmFile5.ID;
            }
            catch (Exception)
            {
                //
            }
        }
       
        public static void CheckInOutPdmNew(List<VaultSystem.VentsCadFiles> filesList, bool registration, string vaultName, out List<VaultSystem.VentsCadFiles> newFilesList)
        {
            var edmVault5 = new EdmVault5();
            if (!edmVault5.IsLoggedIn)
            {
                edmVault5.LoginAuto(vaultName, 0);
            }            

            BatchAddFiles(filesList, edmVault5);            

            try
            {
                BatchUnLock(filesList, edmVault5);
            }
            catch (Exception e)
            {
                // MessageBox.Show(e.Message, " BatchAddFiles(filesList) ");
            }
            
            newFilesList = new List<VaultSystem.VentsCadFiles>();

            foreach (var file in filesList)
            {
                string fileName;
                int fileIdPdm;
                GetIdPdm(file.LocalPartFileInfo, out fileName, out fileIdPdm, edmVault5);

                newFilesList.Add(new VaultSystem.VentsCadFiles
                {
                    PartName = fileName,
                    PartIdPdm = fileIdPdm,
                    LocalPartFileInfo = file.LocalPartFileInfo,
                    PartIdSql = file.PartIdSql
                });
            }
        }
        
        public static void BatchUnLock1(List<string> filesPathesList, string vaultName)
        {
            var edmVault5 = new EdmVault5();
            if (!edmVault5.IsLoggedIn)
            {
                edmVault5.LoginAuto(vaultName, 0);
            }

            IEdmPos5 aPos;

            var batchUnlocker = (IEdmBatchUnlock2)edmVault5.CreateUtility(EdmUtility.EdmUtil_BatchUnlock);
            var i = 0;
            var ppoSelection = new EdmSelItem[filesPathesList.Count];
            foreach (var file in filesPathesList)
            {
                //var path = new FileInfo(file.LocalPartFileInfo).FullName;
                //MessageBox.Show(vaultB.Name + " - " + vaultB.IsLoggedIn + "\nLocalPartFileInfo - " + file.LocalPartFileInfo, "BatchUnLock");

                IEdmFolder5 ppoRetParentFolder;
                var aFile = edmVault5.GetFileFromPath(file, out ppoRetParentFolder);
                aPos = aFile.GetFirstFolderPosition();
                var aFolder = aFile.GetNextFolder(aPos);

                ppoSelection[i] = new EdmSelItem
                {
                    mlDocID = aFile.ID,
                    mlProjID = aFolder.ID
                };
                i++;
            }

            // Add selections to the batch of files to check in
            batchUnlocker.AddSelection(edmVault5, ppoSelection);

            batchUnlocker.CreateTree(0, (int)EdmUnlockBuildTreeFlags.Eubtf_ShowCloseAfterCheckinOption + (int)EdmUnlockBuildTreeFlags.Eubtf_MayUnlock);
            var fileList = (IEdmSelectionList6)batchUnlocker.GetFileList((int)EdmUnlockFileListFlag.Euflf_GetUnlocked + (int)EdmUnlockFileListFlag.Euflf_GetUndoLocked + (int)EdmUnlockFileListFlag.Euflf_GetUnprocessed);
            aPos = fileList.GetHeadPosition();

            while (!(aPos.IsNull))
            {
                EdmSelectionObject poSel;
                fileList.GetNext2(aPos, out poSel);
            }
            batchUnlocker.UnlockFiles(0);
        }

        internal static void BatchAddFiles(List<VaultSystem.VentsCadFiles> filesList, EdmVault5 edmVault5)
        {
            try
            {
                var  poAdder = (IEdmBatchAdd2)edmVault5.CreateUtility(EdmUtility.EdmUtil_BatchAdd);

                var list = "Всего файлов - "  + filesList.Count;
                var i = 0;
                foreach (var file in filesList)
                {
                    //MessageBox.Show(file.LocalPartFileInfo);

                    i++;

                    var stringMess = list + "\n" + i + ". " + file.LocalPartFileInfo;
                    
                    var directoryName = file.LocalPartFileInfo.Replace(file.PartWithoutExtension, "");

                    list = stringMess + " -|- " + directoryName;
                    
                    //MessageBox.Show(directoryName + "\nLocalPartFileInfo - " + file.LocalPartFileInfo, "BatchAddFiles");

                    if (!string.IsNullOrEmpty(Path.GetExtension(file.LocalPartFileInfo)))
                    {
                        try
                        {
                            poAdder.AddFileFromPathToPath(new FileInfo(file.LocalPartFileInfo).FullName, directoryName);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.StackTrace, "1");
                        }
                    }
                    else
                    {
                        MessageBox.Show(file.LocalPartFileInfo);
                    }
                }
                
                //MessageBox.Show(list);
                try
                {
                    //MessageBox.Show(poAdder.CommitAdd(0, null).ToString());
                    poAdder.CommitAdd(-1, null);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.StackTrace, "2");
                }
            }
            catch (Exception e)
            {
             //   MessageBox.Show(e.StackTrace, "BatchAddFiles");
            }
        }
        
        internal static void BatchUnLock(List<VaultSystem.VentsCadFiles> filesList, EdmVault5 edmVault5)
        {
            IEdmPos5 aPos;

            var batchUnlocker = (IEdmBatchUnlock2)edmVault5.CreateUtility(EdmUtility.EdmUtil_BatchUnlock);
            var i = 0;
            var ppoSelection = new EdmSelItem[filesList.Count];
            foreach (var file in filesList)
            {
              //  MessageBox.Show( "\nLocalPartFileInfo - " + file.LocalPartFileInfo, "BatchUnLock");

                if (string.IsNullOrEmpty(Path.GetExtension(file.LocalPartFileInfo))) continue;
              
                IEdmFolder5 ppoRetParentFolder;
                var aFile = edmVault5.GetFileFromPath(file.LocalPartFileInfo, out ppoRetParentFolder);
                aPos = aFile.GetFirstFolderPosition();
                var aFolder = aFile.GetNextFolder(aPos);

                ppoSelection[i] = new EdmSelItem
                {
                    mlDocID = aFile.ID,
                    mlProjID = aFolder.ID
                };
                i++;
            }

            // Add selections to the batch of files to check in
            batchUnlocker.AddSelection(edmVault5, ppoSelection);

            batchUnlocker.CreateTree(0, (int)EdmUnlockBuildTreeFlags.Eubtf_ShowCloseAfterCheckinOption + (int)EdmUnlockBuildTreeFlags.Eubtf_MayUnlock);
            var fileList = (IEdmSelectionList6)batchUnlocker.GetFileList((int)EdmUnlockFileListFlag.Euflf_GetUnlocked + (int)EdmUnlockFileListFlag.Euflf_GetUndoLocked + (int)EdmUnlockFileListFlag.Euflf_GetUnprocessed);
            aPos = fileList.GetHeadPosition();

            while (!(aPos.IsNull))
            {
                EdmSelectionObject poSel;
                fileList.GetNext2(aPos, out poSel);
            }
            batchUnlocker.UnlockFiles(0);
        }

        public static void BatchGet(string vaultName, List<BatchDLL.Batches.TaskParam> list)
        {
            var edmVault5 = new EdmVault5();
            if (!edmVault5.IsLoggedIn) { edmVault5.LoginAuto(vaultName, 0); }
            var edmVault7 = (IEdmVault7)edmVault5;
            BathGet(edmVault7, list);
        }

        internal static void BathGet(IEdmVault7 vault, List<BatchDLL.Batches.TaskParam> list)
        {
            List<KeyValuePair<Exception, string>> exception;
            BatchDLL.Batches.BatchGet(vault, list, out exception);

            foreach (var item in exception)
            {
                MessageBox.Show(item.Key.InnerException + "\n" + item.Value);// + "\n" + item.Data + "\n" + item.Message + "\n" + item.StackTrace);
            } 
        }

        public class EpdmSearch
        {

            private static IEdmVault5 _edmVault5;

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

            public static void SearchDoc(string fileName, SwDocType swDocType, out List<FindedDocuments> fileList, string vaultName)
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
                        _edmVault5.LoginAuto(vaultName, 0);
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

                catch (Exception)
                {
                    //LoggerError(ex.Message);
                }
                fileList = files;
            }
        }

    }
}