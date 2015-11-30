using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using AirVentsCadWpf.Properties;


namespace AirVentsCadWpf.Логирование
{
    static class Логгер
   {
       #region Логгер

        public static void Отладка(string logText, string код, string функция, string className)
        {
            Лог.Debug(logText, код, функция, className);
        }

        public static void Ошибка(string logText, string код, string функция, string className)
        {
            Лог.Error(logText, код, функция, className);
        }

        public static void Информация(string logText, string код, string функция, string className)
        {
            Лог.Info(logText, код, функция, className);
        }

        static class Лог
        {
            //private const string ConnectionString = "Data Source=192.168.14.11;Initial Catalog=SWPlusDB;User ID=sa;Password=PDMadmin";//Settings.Default.ConnectionToSQL;//
            //private const string ClassName = "ModelSw";
            //----------------------------------------------------------
            // Статический метод записи строки в файл лога без переноса
            //----------------------------------------------------------
            public static void Write(string text)
            {
                //using (var streamWriter = new StreamWriter("C:\\log.txt", true))  //Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + 
                //{
                //    streamWriter.Write(text);
                //}
            }

            //---------------------------------------------------------
            // Статический метод записи строки в файл лога с переносом
            //---------------------------------------------------------
            public static void WriteLine(string message)
            {
                //using (var streamWriter = new StreamWriter("C:\\log.txt", true))
                //{
                //    streamWriter.WriteLine("{0,-23} {1}", DateTime.Now + ":", message);
                //}
            }


            public static void Debug(string message, string код, string функция, string className)
            {
                //using (var streamWriter = new StreamWriter("C:\\log.txt", true))
                //{
                //    streamWriter.WriteLine("{0,-20}  {2,-7} {3,-20} {1}", DateTime.Now + ":", message, "Error", className);
                //}
                WriteToBase(message, "Debug", код, className, функция);
            }


            public static void Error(string message, string код, string функция, string className)
            {
                //using (var streamWriter = new StreamWriter("C:\\log.txt", true))
                //{
                //    streamWriter.WriteLine("{0,-20}  {2,-7} {3,-20} {1}", DateTime.Now + ":", message, "Error", className);
                //}
                WriteToBase(message, "Error", код, className, функция);
            }

            public static void Info(string message, string код, string функция, string className)
            {
                //using (var streamWriter = new StreamWriter("C:\\log.txt", true))
                //{
                //    streamWriter.WriteLine("{0,-20}  {2,-7} {3,-20} {1}", DateTime.Now + ":", message, "Info", className);
                //}
                WriteToBase(message, "Info", код, className, функция);
            }

            static void WriteToBase(string описание, string тип, string код, string модуль, string функция)
            {
                using (var con = new SqlConnection(Settings.Default.ConnectionToSQL))
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
                        
                        //var returnParameter = sqlCommand.Parameters.Add("@ProjectNumber", SqlDbType.Int);
                        //returnParameter.Direction = ParameterDirection.ReturnValue;

                        sqlCommand.ExecuteNonQuery();

                        //var result = Convert.ToInt32(returnParameter.Value);

                        //switch (result)
                        //{
                        //    case 0:
                        //        MessageBox.Show("Подбор №" + Номерподбора.Text + " уже существует!");
                        //        break;
                        //}

                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show("Введите корректные данные! " + exception.Message);
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
