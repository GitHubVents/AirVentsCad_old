using AirVentsCadWpf.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;


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

        public static bool OpenIfExist(string assemblyName)
        {
            var exist = false;

            if (string.IsNullOrEmpty(assemblyName)) return exist;            

            string asmPath;
            int fileId;
            int projectId;
            var asmName = Path.GetFileNameWithoutExtension(assemblyName);
            
            if (GetExistingFile(asmName, out asmPath, out fileId, out projectId, Settings.Default.PdmBaseName))
            {
                if (MessageBox.Show(asmName + " уже есть в базе. Открыть?",
                    "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Process.Start("conisio://" + Settings.Default.TestPdmBaseName + "/open?projectid=" + projectId +
                                  "&documentid=" + fileId + "&objecttype=1");
                    exist = true;
                }
                else exist = false;
                
            }
            return exist;
        }
  
    }
}
