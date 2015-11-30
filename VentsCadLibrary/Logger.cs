using System;
using System.Data;
using System.Data.SqlClient;


namespace VentsCadLibrary
{
    public partial class VentsCad
    {
        #region Logger

        static void LoggerDebug(string logText, string код, string функция)
        {
            LoggerMine.Debug(logText, код, функция);
        }

        static void LoggerError(string logText, string код, string функция)
        {
            LoggerMine.Error(logText, код, функция);
        }

        static void LoggerInfo(string logText, string код, string функция)
        {
            LoggerMine.Info(logText, код, функция);
        }
        
        static class LoggerMine
        {              

            private const string ConnectionString = "Data Source=192.168.14.11;Initial Catalog=SWPlusDB;User ID=sa;Password=PDMadmin";
            private const string ClassName = "VentsCadLibrary";

            public static void Debug(string message, string код, string функция)
            {
                //using (var streamWriter = new StreamWriter("C:\\log.txt", true))
                //{
                //    streamWriter.WriteLine("{0,-20}  {2,-7} {3,-20} {1}", DateTime.Now + ":", message, "Error", ClassName);
                //}
                WriteToBase(message, "Debug", код, ClassName, функция);
            }

            public static void Error(string message, string код, string функция)
            {
                //using (var streamWriter = new StreamWriter("C:\\log.txt", true))
                //{
                //    streamWriter.WriteLine("{0,-20}  {2,-7} {3,-20} {1}", DateTime.Now + ":", message, "Error", ClassName);
                //}
                WriteToBase(message, "Error", код, ClassName, функция);
            }

            public static void Info(string message, string код, string функция)
            {
                //using (var streamWriter = new StreamWriter("C:\\log.txt", true))
                //{
                //    streamWriter.WriteLine("{0,-20}  {2,-7} {3,-20} {1}", DateTime.Now + ":", message, "Info", ClassName);
                //}
                WriteToBase(message, "Info", код, ClassName, функция);
            }

            static void WriteToBase(string описание, string тип, string код, string модуль, string функция)
            {
                using (var con = new SqlConnection(ConnectionString))
                {
                    try
                    {
                        con.Open();
                        var sqlCommand = new SqlCommand("AddErrorLog", con) { CommandType = CommandType.StoredProcedure };

                        var sqlParameter = sqlCommand.Parameters;

                        sqlParameter.AddWithValue("@UserName", Environment.UserName + " (" + System.Net.Dns.GetHostName() + ")");
                        sqlParameter.AddWithValue("@ErrorModule", модуль);
                        sqlParameter.AddWithValue("@ErrorMessage", описание);
                        sqlParameter.AddWithValue("@ErrorCode", код);
                        sqlParameter.AddWithValue("@ErrorTime", DateTime.Now);
                        sqlParameter.AddWithValue("@ErrorState", тип);
                        sqlParameter.AddWithValue("@ErrorFunction", функция);
                        
                        sqlCommand.ExecuteNonQuery();                        
                    }
                    catch (Exception exception)
                    {
                        //MessageBox.Show("Введите корректные данные! " + exception.Message);
                    }
                    finally
                    {
                        con.Close();
                    }
                }
            }
        }

        #endregion

    }    
}
