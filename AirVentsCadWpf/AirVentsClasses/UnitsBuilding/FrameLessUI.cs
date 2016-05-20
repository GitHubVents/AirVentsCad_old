using AirVentsCadWpf.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using VentsCadLibrary;

// TODO Добавить при длине от 1200 мм в варианте с ножками третьи ножки посредине

namespace AirVentsCadWpf.AirVentsClasses.UnitsBuilding
{
    public partial class ModelSw
    {
        public static bool RemPanWidthCheck(TextBox combo, int panelNumber)
        {
            var check = true;
            if (combo.Visibility == Visibility.Visible)
            {
                if (combo.Text == "")
                {
                    check = false;
                    MessageBox.Show($"Введите ширину {panelNumber}-й съемной панели");  return check;
                }
                if (combo.Text.Contains("-"))
                {
                    check = false;
                    MessageBox.Show($"Ширина {panelNumber}-й съемной панели не может быть отрицательной!"); return check;
                }
                if (Convert.ToInt32(combo.Text) < 100)
                {
                    check = false;
                    MessageBox.Show($"Ширина {panelNumber}-й съемной панели не может быть меньше 100 мм!"); return check;
                }
            }
            return check;
        }

        public static Replaced replaced = Replaced.Instance;

        public class Replaced
        {
            public static Replaced Instance => instance;

            private static readonly Replaced instance = new Replaced();
            static Replaced() { }
            private Replaced()
            {
                List = new List<OldFile>();
            }

            public List<OldFile> List { get; set; }

            public void Add(OldFile file)
            {
                List.Add(file);
            }

            public void Clear()
            {
                List.Clear();
            }

            public class OldFile
            {
                public string FilePath { get; set; }
                public DateTime? LastChanged { get; set; }
                public DateTime? LastWriteTime { get; set; }
                public bool NeedReplace { get; set; }
                public bool NeedToReplaced { get; set; }
            }

            public static OldFile IsOldVersion(string filePath, DateTime? lastChanged, DateTime fileTime)
            {
                OldFile oldFile = null;
                if (lastChanged != null)
                {
                    var info = new FileInfo(filePath);                    
                    var compare = DateTime.Compare((DateTime)lastChanged, fileTime);

                    if (compare > 0)
                    {
                        oldFile = new OldFile
                        {
                            LastChanged = lastChanged,
                            LastWriteTime = fileTime,
                            FilePath = new FileInfo(filePath)?.FullName
                        };
                        replaced.Add(oldFile);
                    }

                    MessageBox.Show($"Name - {filePath}\nChanged - {lastChanged}\nWriteTime - {fileTime}\nCompare - /{compare}/");
                }

                return oldFile;
            }
        }

        public static bool OpenIfExist(string FilePath, VaultSystem.VentsCadFile.Type type, DateTime? lastChange) //, out VaultSystem.VentsCadFile cadFile) 
        {
            if (string.IsNullOrEmpty(FilePath)) return true;
            var fileName = Path.GetFileNameWithoutExtension(FilePath);
            var cadFiles = VaultSystem.VentsCadFile.Get(fileName, type, Settings.Default.PdmBaseName);
            if (cadFiles == null) return true;

            var findedFile = cadFiles[0];

            VaultSystem.CheckInOutPdm(new List<FileInfo> { new FileInfo(findedFile.Path)}, false);
            var oldFile = Replaced.IsOldVersion(findedFile.Path, lastChange, findedFile.Time);

            MessageBox.Show($"IdPdm - {findedFile.PartIdPdm}\nName - {findedFile.PartName}\nTime - {findedFile.Time}");


            //VaultSystem.CheckInOutPdm(new List<FileInfo> { new FileInfo(newDamperPath), new FileInfo(newDamperAsmPath) }, false);

            //if (GetExistingFile(fileName, out asmPath, out fileId, out projectId, out time, Settings.Default.PdmBaseName))
            //{
            //    var oldFile = Replaced.IsOldVersion(asmPath, lastChanged, time);

            //    var result = MessageBox.Show(fileName + " уже есть в базе. Открыть? Нажмите \"Нет\" для перезаписи ",
            //        "", MessageBoxButton.YesNoCancel);
            //    if (result == MessageBoxResult.No)
            //    {
            //        exist = false;
            //    }
            //    else if (result != MessageBoxResult.No)
            //    {
            //        exist = true;
            //        if (result == MessageBoxResult.Yes)
            //        {
            //            var processes = Process.GetProcessesByName("SLDWORKS");
            //            if (processes?.Length > 0)
            //            {                            
            //                VentsCad.OpenSwDoc(asmPath);
            //            }
            //            else
            //            {
            //                Process.Start("conisio://" + Settings.Default.TestPdmBaseName + "/open?projectid=" + projectId +
            //                      "&documentid=" + fileId + "&objecttype=1");
            //            }
            //        }
            //    }                
            //}
            //return exist;

            return false;
        }  
    }
}
