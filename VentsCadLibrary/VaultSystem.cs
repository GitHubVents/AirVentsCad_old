using System.Collections.Generic;
using System.IO;

namespace VentsCadLibrary
{
    /// <summary>
    /// 
    /// </summary>
    public class VaultSystem
    {
        public class VentsCadFiles
        {
            public int PartIdSql { get; set; }

            public string PartName { get; set; }

            public string PartWithoutExtension => LocalPartFileInfo.Substring(LocalPartFileInfo.LastIndexOf('\\'));

            public int PartIdPdm { get; set; }

            public string LocalPartFileInfo { get; set; }
        }

        public static void GoToFile(string path, string vaultName)
        {
            SwEpdm.GoToFile(path, vaultName);
        }

        public static void BatchUnLock1(List<string> filesPathesList, string vaultName)
        {
            SwEpdm.BatchUnLock1(filesPathesList, vaultName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vaultName"></param>
        /// <returns></returns>
        public static string GetSwEpdRootFolderPath(string vaultName)
        {
          return SwEpdm.GetSwEpdRootFolderPath(vaultName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="vaultName"></param>
        public static void CreateDistDirectory(string path, string vaultName)
        {
            SwEpdm.CreateDistDirectory(path, vaultName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="vaultName"></param>
        public static void GetLastVersionOfFile(string path, string vaultName)
        {
            SwEpdm.GetLastVersionOfFile(path, vaultName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filesList"></param>
        /// <param name="registration"></param>
        /// <param name="vaultName"></param>
        public static void CheckInOutPdm(List<FileInfo> filesList, bool registration, string vaultName)
        {
            SwEpdm.CheckInOutPdm(filesList, registration, vaultName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="registration"></param>
        /// <param name="vaultName"></param>
        public static void CheckInOutPdm(string filePath, bool registration, string vaultName)
        {
            SwEpdm.CheckInOutPdm(filePath, registration, vaultName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        /// <param name="fileIdPdm"></param>
        /// <param name="vaultName"></param>
        public static void GetIdPdm(string path, out string fileName, out int fileIdPdm, string vaultName)
        {
            SwEpdm.GetIdPdm(path, out fileName, out fileIdPdm, vaultName);
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
        public static void CheckInOutPdmNew(List<VentsCadFiles> filesList, bool registration, string vaultName,
            out List<VentsCadFiles> newFilesList)
        {
            SwEpdm.CheckInOutPdmNew(filesList, registration, vaultName, out newFilesList);
        }
    }
}
