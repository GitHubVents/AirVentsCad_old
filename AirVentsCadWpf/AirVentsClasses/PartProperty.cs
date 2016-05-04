using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;


namespace AirVentsCadWpf.AirVentsClasses
{
    /// <summary>
    /// 
    /// </summary>
    public class PartProperty
    {

    

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

            #region  tO DELETE

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

            #endregion

        }

        #endregion
    }

}
