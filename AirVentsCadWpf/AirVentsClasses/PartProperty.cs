using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using EdmLib;

namespace AirVentsCadWpf.AirVentsClasses
{
    /// <summary>
    /// 
    /// </summary>
    public class PartProperty
    {

        #region Поля 

        /// <summary>
        /// Gets or sets the name of the PDM base. For example: "Vents-PDM"
        /// </summary>
        public string PdmBaseName { get; set; }

        /// <summary>
        /// Gets or sets the name of the user. For example: "kb64"
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the user password. For example: "1"
        /// </summary>
        public string UserPassword { get; set; }
        
        /// <summary>
        /// The message
        /// </summary>
        public string Message;
        
        /// <summary>
        /// 
        /// </summary>
        public string AssemblyInfoLabel;

        /// <summary>
        /// Gets or sets the assembly path.
        /// </summary>
        /// <value>
        /// The assembly path.
        /// </value>
        public string AssemblyPath { get; set; }
        
        /// <summary>
        /// Boms the list.
        /// </summary>
        /// <returns></returns>
        public List<PartPropBomCells> BomList()
        {
            try
            {
                Login();
            }
            catch (Exception exception)
            {
                Message = "Ошибка входа в хранилище " + exception.Message;
            }
            
            if (GetFile(AssemblyPath))
            {
                Getbom();
            }
            return _bomList;
        }



        readonly IEdmVault10 _mVault = new EdmVault5Class();
        IEdmFile7 _edmFile7;

        public List<PartPropBomCells> BomListAsm()
        {
          return  BomList().OrderBy(x => x.Количество).ToList();
        }

        public List<PartPropBomCells> BomListPrt()
        {
            return BomList().OrderBy(x => x.Количество).ToList();
        }
        
        void Login()
        {
            if (_mVault == null) return;
            try
            {
                try
                {
                    _mVault.LoginAuto(PdmBaseName, 0);
                }
                catch (Exception)
                {
                    _mVault.Login(UserName, UserPassword, PdmBaseName);
                }
               
                _mVault.CreateSearch();
                if (_mVault.IsLoggedIn)
                {
                    Message = "Logged in " + PdmBaseName;
                }
            }
            catch (COMException ex)
            {
                Message = "Failed to connect to " + PdmBaseName + " - " + ex.Message;
            }
        }
        
        bool GetFile(string assemblyPath)
        {
            if (assemblyPath == null)
            {
                Message = "Введите полный путь к сборке";
                return false;
            }
            if (!assemblyPath.ToLower().EndsWith("sldasm"))
            {
                Message = "Выбрана не сборка! Выберете файл с расширением .sldasm";
                return false;
            }
            var filename = assemblyPath;
            try
            {
                IEdmFolder5 folder;
                _edmFile7 = (IEdmFile7)_mVault.GetFileFromPath(filename, out folder);
            }
            catch (Exception exception)
            {
                Message = exception.Message;
                return false;
            }
            return true;
        }



        void Getbom()
        {
            if (_edmFile7 == null) return;

            var bomView = _edmFile7.GetComputedBOM(Convert.ToInt32(7), Convert.ToInt32(-1), "",
                (int) EdmBomFlag.EdmBf_ShowSelected);

            if (bomView == null) return;
            Array bomRows;
            Array bomColumns;
            bomView.GetRows(out bomRows);
            bomView.GetColumns(out bomColumns);

            var bomTable = new DataTable();

            foreach (EdmBomColumn bomColumn in bomColumns)
            {
                bomTable.Columns.Add(new DataColumn {ColumnName = bomColumn.mbsCaption});
            }
            
            
            bomTable.Columns.Add(new DataColumn{ ColumnName = "Уровень" });
            //bomTable.Columns.Add(new DataColumn { ColumnName = "Путь" });
            //bomTable.Columns.Add(new DataColumn { ColumnName = "ItemID" });

            for (var i = 0; i < bomRows.Length; i++)
            {
                var bomCell = (IEdmBomCell) bomRows.GetValue(i);

                bomTable.Rows.Add();
                for (var j = 0; j < bomColumns.Length; j++)
                {
                    var column = (EdmBomColumn) bomColumns.GetValue(j);
                    object value;
                    object computedValue;
                    String config;
                    bool readOnly;
                    bomCell.GetVar(column.mlVariableID, column.meType, out value, out computedValue, out config,
                        out readOnly);
                    if (value != null)
                    {
                        bomTable.Rows[i][j] = value;
                    }

                    bomTable.Rows[i][j + 1] = bomCell.GetTreeLevel();
                    //bomTable.Rows[i][j + 2] = bomCell.GetPathName();
                    //bomTable.Rows[i][j + 3] = bomCell.GetItemID();
                }
            }

            _bomList = PdmBomTableToBomList(bomTable);
            #region LabelContent
            var labelContentList = _bomList.Where(x => x.Уровень == "0").ToList();
            
            foreach (var bomCells in labelContentList)
            {
                AssemblyInfoLabel = " Сборка - " + Path.GetFileNameWithoutExtension(bomCells.Путь) +
                    " (конфигурация - " + bomCells.Конфигурация + ")";
            }
            #endregion

        }
        
        #endregion

        #region BomCells - Поля с х-ками деталей
        
        public class PartPropBomCells
        {
            // Pdm
            public string Количество { get; set; }
            public string ТипФайла { get; set; }
            public string Конфигурация { get; set; }
            public string ПоследняяВерсия { get; set; }
            public string Идентификатор { get; set; }

            // DocManager
            public string МатериалЦми { get; set; }
            public string ТолщинаЛиста { get; set; }
            public string Обозначение { get; set; }
            public string Наименование { get; set; }
            public string Материал { get; set; }
            public string Раздел { get; set; }

            // Auto
            public string Уровень { get; set; }
            public string АссоциированныйОбъект { get; set; }
            public string Путь { get; set; }
            public string Errors { get; set; }
        }

        List<PartPropBomCells> _bomList = new List<PartPropBomCells>();

        private static List<PartPropBomCells> PdmBomTableToBomList(DataTable table)
        {
            var bomList = new List<PartPropBomCells>(table.Rows.Count);
            bomList.AddRange(from DataRow row in table.Rows
                select row.ItemArray
                into values
                select new PartPropBomCells
                {
                    Количество = values[0].ToString(),
                    ТипФайла = values[1].ToString(),
                    Конфигурация = values[2].ToString(),
                    ПоследняяВерсия = values[3].ToString(),
                    Идентификатор = values[4].ToString(),
                });
            foreach (var bomCells in bomList)
            {
                bomCells.Errors = ErrorMessageForParts(bomCells);
            }
            return bomList;
        }



        static string ErrorMessageForParts(PartPropBomCells partPropBomCells)
        {
            return "";
            //var обозначениеErr = "";
            //var материалЦмиErr = "";
            //var наименованиеErr = "";
            //var разделErr = "";
            //var толщинаЛистаErr = "";
            //var конфигурацияErr = "";

            //var messageErr = String.Format("Необходимо заполнить: {0}{1}{2}{3}{4}{5}",
            //   обозначениеErr,
            //   материалЦмиErr,
            //   наименованиеErr,
            //   разделErr,
            //   толщинаЛистаErr,
            //   конфигурацияErr);

            //if (partPropBomCells.Обозначение == "")
            //{
            //    обозначениеErr = "\n Обозначение";
            //}

            //var regex = new Regex("[^0-9]");
            //if (regex.IsMatch(partPropBomCells.Конфигурация))
            //{
            //    конфигурацияErr = "\n Изменить имя конфигурации на численное значение";
            //}

            //if (partPropBomCells.Наименование == "")
            //{
            //    наименованиеErr = "\n Наименование";
            //}

            //if (partPropBomCells.Раздел == "")
            //{
            //    разделErr = "\n Раздел";
            //}


            //if (partPropBomCells.ТипФайла == "sldprt")
            //{

            //    if (partPropBomCells.ТолщинаЛиста == "")
            //    {
            //        толщинаЛистаErr = "\n ТолщинаЛиста";
            //    }
            //}

            //var message = partPropBomCells.Errors = String.Format("Необходимо заполнить: {0}{1}{2}{3}{4}{5}",
            //    обозначениеErr,
            //    материалЦмиErr,
            //    наименованиеErr,
            //    разделErr,
            //    толщинаЛистаErr,
            //    конфигурацияErr);

            //return partPropBomCells.Errors == messageErr ? "" : message;
        }

        #endregion
    }
}
