using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using AirVentsCadWpf.Properties;
//using EdmLib;

namespace AirVentsCadWpf.AirVentsClasses
{
    //class EpdmSearch
    //{
    //    public string VaultName { get; set; }

    //    private IEdmVault5 _edmVault5;

    //    /// <summary>
    //    /// Тип документа для поиска. 
    //    /// </summary>
    //    public enum SwDocType
    //    {
    //        /// <summary>
    //        /// Точное имя файла для поиска
    //        /// </summary>
    //        SwDocNone = 0,
    //        /// <summary>
    //        /// Имя файла с расширением .sldprt
    //        /// </summary>
    //        SwDocPart = 1,
    //        /// <summary>
    //        /// Имя файла с расширением .sldasm
    //        /// </summary>
    //        SwDocAssembly = 2,
    //        /// <summary>
    //        /// Имя файла с расширением .slddrw
    //        /// </summary>
    //        SwDocDrawing = 3,
    //        /// <summary>
    //        /// По схожести
    //        /// </summary>
    //        SwDocLike = 4,
    //    }

    //    public class FindedDocuments
    //    {
    //        public string Path { get; set; }

    //        public int FileId { get; set; }

    //        public int ProjectId { get; set; }
    //    }

    //    /// <summary>
    //    /// Searches the document.
    //    /// </summary>
    //    /// <param name="fileName">The file path.</param>
    //    /// <param name="swDocType">Type of the sw document.</param>
    //    /// <param name="fileList">The file list.</param>
    //    /// <returns></returns>
    //    public void SearchDoc(string fileName, SwDocType swDocType, out List<FindedDocuments> fileList)
    //    {
    //        var files = new List<FindedDocuments>();

    //        try
    //        {
    //            if (_edmVault5 == null)
    //            {
    //                _edmVault5 = new EdmVault5();
    //            }
    //            var edmVault7 = (IEdmVault7)_edmVault5;

    //            if (!_edmVault5.IsLoggedIn)
    //            {
    //                _edmVault5.LoginAuto(VaultName, 0);
    //            }

    //            //Search for all text files in the edmVault7
    //            var edmSearch5 = (IEdmSearch5)edmVault7.CreateUtility(EdmUtility.EdmUtil_Search);

    //            var extenison = "";
    //            var like = "";

    //            switch ((int)swDocType)
    //            {
    //                case 0:
    //                    extenison = "";
    //                    like = "";
    //                    break;
    //                case 1:
    //                    like = "%";
    //                    extenison = ".sldprt";
    //                    break;
    //                case 2:
    //                    like = "%";
    //                    extenison = "%.sldasm";
    //                    break;
    //                case 3:
    //                    like = "%";
    //                    extenison = ".slddrw";
    //                    break;
    //                case 4:
    //                    like = "%";
    //                    extenison = "%.%";
    //                    break;
    //            }
    //            edmSearch5.FileName = like + fileName + extenison;

    //            var edmSearchResult5 = edmSearch5.GetFirstResult();

    //            while (edmSearchResult5 != null)
    //            {
    //                files.Add(
    //                     new FindedDocuments
    //                     {
    //                         FileId = edmSearchResult5.ID,
    //                         ProjectId = edmSearchResult5.ParentFolderID,
    //                         Path = edmSearchResult5.Path
    //                     });
    //                edmSearchResult5 = edmSearch5.GetNextResult();
    //            }

    //            if (edmSearch5.GetFirstResult() == null)
    //            {
    //                //LoggerInfo("Файл не найден!");
    //                files = null;
    //            }
    //        }
    //        catch (Exception){}
    //        fileList = files;
    //    }

    //    void ShowReferences(IEdmVault7 edmVault7, string filePath)
    //    {
    //        string projName = null;
    //        IEdmFolder5 edmFolder5;
    //        var edmFile5 = edmVault7.GetFileFromPath(filePath, out edmFolder5);
    //        var edmReference5 = edmFile5.GetReferenceTree(edmFolder5.ID);
    //        AddReferences(edmReference5, 0, ref projName, false);
    //    }

    //    string AddReferences(IEdmReference5 file, long indent, ref string projName, bool isTop)
    //    {
    //        var fileName = file.Name;

    //        if (_edmVault5 == null)
    //        {
    //            _edmVault5 = new EdmVault5();
    //        }
    //        if (!_edmVault5.IsLoggedIn)
    //        {
    //            _edmVault5.LoginAuto(VaultName, 0);
    //        }
    //        var edmPos5 = file.GetFirstChildPosition(projName, isTop, true);
    //        while (!(edmPos5.IsNull))
    //        {
    //            var edmReference5 = file.GetNextChild(edmPos5);
    //            IEdmFolder5 edmFolder5;
    //            var edmFile5 = _edmVault5.GetFileFromPath(edmReference5.FoundPath, out edmFolder5);
    //            fileName = fileName + AddReferences(edmReference5, indent, ref projName, isTop);
    //            edmFile5.GetFileCopy(0, "", edmReference5.FoundPath);
    //        }
    //        return fileName;
    //    }
        

    //    private void Button_Click_1(object sender, RoutedEventArgs e)
    //    {
    //        try
    //        {

    //            var vault1 = new EdmVault5();
    //            vault1.LoginAuto(Settings.Default.PdmBaseName, 0);

    //            var search = vault1.CreateSearch();
    //            search.FindFiles = true;
    //            search.FindFolders = false;

    //            var edmFile5 = vault1.GetObject(EdmObjectType.EdmObject_File, 125);

    //            search.FileName = edmFile5.Name;
    //            var result = search.GetFirstResult();
    //            MessageBox.Show(result.Path);

    //            var searchDir = new DirectoryInfo(@"E:\Vents-PDM");
    //            var files = searchDir.GetFiles("*.sldasm", SearchOption.AllDirectories);

    //            var allfiles = files.Aggregate("", (current, fileInfo) => " " + current + fileInfo.Name);

    //            MessageBox.Show(allfiles);

    //            //   var edmFile5 = (IEdmFile5)edmVault5.GetObject(EdmObjectType.EdmObject_ItemFolder, 125);
    //            //edmFile5.GetFileCopy();
    //            //            MessageBox.Show(edmFile5.Name + "  " + edmFile5.ID + "  " +  edmFile5.Vault.);
    //            //    var edmFile5 = edmVault5.GetFileFromPath(path, out oFolder);
    //            //   edmFile5.GetFileCopy(1, 0, oFolder.ID, (int)EdmGetFlag.EdmGet_Simple);

    //        }
    //        catch (Exception){}
    //    }

    //}
}
