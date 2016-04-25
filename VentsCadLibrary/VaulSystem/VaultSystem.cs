using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VentsCadLibrary
{
    /// <summary>
    /// 
    /// </summary>
    public class VaultSystem
    {
        public static void SetPmdVaultName(string vaultName)
        {
            SwEpdm.VaultName = vaultName;
        }       

        public static void AddToPdmByPath(string path)//, string vaultName)
        {
            SwEpdm.AddToPdmByPath(path);//, vaultName);
        }

        public class SearchInVault : SwEpdm.EpdmSearch
        {
            public new static void SearchDoc(string fileName, SwDocType swDocType, out List<FindedDocuments> fileList, string vaultName)
            {
                SwEpdm.EpdmSearch.SearchDoc(fileName, swDocType, out fileList, vaultName);
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

        public static void GoToFile(string path)//, string vaultName)
        {
            SwEpdm.OpenContainingFolder(path);//, vaultName);
        }

        public static void BatchUnLock1(List<string> filesPathesList)//, string vaultName)
        {
            SwEpdm.BatchUnLock1(filesPathesList);//, vaultName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vaultName"></param>
        /// <returns></returns>
        public static string GetSwEpdRootFolderPath(string vaultName)
        {
            return SwEpdm.GetSwEpdRootFolderPath();//vaultName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="vaultName"></param>
        public static void CreateDistDirectory(string path, string vaultName)
        {
            SwEpdm.CreateDistDirectory(path);//, vaultName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="vaultName"></param>
        public static void GetLastVersionOfFile(string path)//, string vaultName)
        {
            SwEpdm.GetLastVersionOfFile(path);//, vaultName);
        }       
     

        public static void GetAsmFilesAsBuild(string path, string vaultName)
        {
            SwEpdm.GetAsmFilesAsBuild(path);//, vaultName);
        }

        public class BatchGetParams
        {
            public string FilePath { get; set; }
            public int IdPdm { get; set; }
            public int CurrentVersion { get; set; }

        }

        public static void BatchGet(string vaultName, List<BatchGetParams> list, out List<PdmFilesAfterGet> pdmFilesAfterGets)
        {
            var taskParams = list.Select(batchGetParamse => new AddinConvertTo.Classes.FilesData.TaskParam
            {
                CurrentVersion = batchGetParamse.CurrentVersion,
                FullFilePath = batchGetParamse.FilePath,
                IdPDM  = batchGetParamse.IdPdm
            }).ToList();

            pdmFilesAfterGets = taskParams.Select(batchGetParamse => new PdmFilesAfterGet
            {
                VersionToGet = batchGetParamse.CurrentVersion,
                FilePath = batchGetParamse.FullFilePath,                
            }).ToList();

            SwEpdm.BatchGet(vaultName, taskParams);
        }

        public class PdmFilesAfterGet
        {
            public string FilePath { get; set; }

            public string FileName { get; set; }

            public int VersionBeforeGet { get; set; }

            public int VersionToGet { get; set; }

            public int VersionAfterGet { get; set; }

            public bool Equal => VersionToGet == VersionAfterGet;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filesList"></param>
        /// <param name="registration"></param>
        /// <param name="vaultName"></param>
        public static void CheckInOutPdm(List<FileInfo> filesList, bool registration, string vaultName)
        {
            SwEpdm.CheckInOutPdm(filesList, registration);//, vaultName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="registration"></param>
        /// <param name="vaultName"></param>
        public static void CheckInOutPdm(string filePath, bool registration, string vaultName)
        {
            SwEpdm.CheckInOutPdm(filePath, registration);//, vaultName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        /// <param name="fileIdPdm"></param>
        /// <param name="currentVerison"></param>
        /// <param name="getFileCopy"></param>
        /// <param name="vaultName"></param>
        public static void GetIdPdm(string path, out string fileName, out int fileIdPdm, out int currentVerison, out List<string> configurations, bool getFileCopy, string vaultName)
        {
            SwEpdm.GetIdPdm(path, out fileName, out fileIdPdm, out currentVerison, out configurations, getFileCopy);//, vaultName);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<KeyValuePair<string, int>> GetSwEpdmVaults()
        {
            return SwEpdm.GetSwEpdmVaults();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filesList"></param>
        /// <param name="registration"></param>
        /// <param name="vaultName"></param>
        /// <param name="newFilesList"></param>
        public static void CheckInOutPdmNew(List<VentsCadFiles> filesList, bool registration, //string vaultName,
            out List<VentsCadFiles> newFilesList)
        {
            SwEpdm.CheckInOutPdmNew(filesList, registration, 
                //vaultName,
                out newFilesList);
        }
    }
}
