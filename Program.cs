using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ApiGeoYaMap
{
    class Program
    {
        public static string appRoot;
        static void Main(string[] args)
        {
            try
            {
                appRoot = AppDomain.CurrentDomain.BaseDirectory;
                Console.WriteLine("Программа запущена");
                EventLog("************************");
                EventLog("Программа запущена");

                System.AppDomain.CurrentDomain.UnhandledException += UnhandeldExceptionTrapper;

                string api_url = ConfigurationManager.AppSettings["api_url"];
                string api_key = ConfigurationManager.AppSettings["api_key"];
                string sco = ConfigurationManager.AppSettings["sco"];

                var list = Get();
                int count = list.Count();
                if (count == 0) throw new Exception("В документе нет данных для проверки");
                EventLog("Прочитаны строки из файла. Общее количество: " + count);
                int index = 1;

                foreach (var i in list)
                {

                    try
                    {
                        string geo = i.ToString();

                        string url = api_url + api_key + "&geocode=" + geo + "&format=json&sco=" + sco;

                        string result = GetJsonGeo(url).Result;

                        JObject o2 = JObject.Parse(result);
                        var json = o2.Value<JObject>("response").Value<JObject>("GeoObjectCollection").Value<JArray>("featureMember");
                        var first = json[0].Value<JObject>("GeoObject").Value<JObject>("metaDataProperty").Value<JObject>("GeocoderMetaData");

                        string address = first.Value<string>("text");

                        StatusLine(i.ToString() + " : " + index + " : 1");
                        Write(i.ToString() + ";" + address);
                        Console.WriteLine("Обработана строка " + index + " из " + count + " успешно");
                    }

                    catch
                    {
                        Write(i.ToString() + ";" + "Ошибка при получении данных");
                        StatusLine(i.ToString() + " : " + index + " : 1");
                        Console.WriteLine("Обработана строка " + index + " из " + count + " с ошибкой");
                    }

                    finally
                    {
                        index++;
                    }
 
                }
                EventLog("Программа отработала без ошибок");
            }

            catch(Exception ex)
            {
                ErrorLog(ex.Message);
                EventLog("Программа отработана с ошибкой");
            }

            finally
            {
                CloseApp();
            }
        }



        public static async Task<string> GetJsonGeo(string url)
        {
            Console.WriteLine("Получаем значение из яндекс апи....");
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await client.GetAsync(url);
            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Значение получено");
            return responseBody;
        }

        static void UnhandeldExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                ErrorLog(e.ExceptionObject.ToString());
            }
            catch
            {

            }

            finally
            {
               CloseApp();
            }
        }


        public static List<object> Get()
        {
            var list = new List<object>();
            using (StreamReader streamReader = new StreamReader(appRoot + "координаты на проверку.txt", encoding: Encoding.GetEncoding(1251)))
            {
                string line;
                while (!streamReader.EndOfStream)
                {
                    line = streamReader.ReadLine();
                    list.Add(line);
                }
            }

            return list;
        }
        public static void Write(string t)
        {
            using (StreamWriter streamWriter = new StreamWriter(appRoot + "результат проверки.txt", append: true, encoding: Encoding.GetEncoding(1251)))
            {
                streamWriter.WriteLine(t);
            }
        }

        public static void EventLog(string eventLog)
        {
            using (StreamWriter streamWriter = new StreamWriter(appRoot + "события.txt", append: true, encoding: Encoding.GetEncoding(1251)))
            {
                streamWriter.WriteLine(DateTime.Now + " : " + eventLog);
            }
        }
        public static void StatusLine(string statusLine)
        {
            using (StreamWriter streamWriter = new StreamWriter(appRoot + "статуслайны.txt", append: true, encoding: Encoding.GetEncoding(1251)))
            {
                streamWriter.WriteLine(statusLine);
            }
        }
        public static void ErrorLog(string Exception)
        {
            using (StreamWriter streamWriter = new StreamWriter(appRoot + "ошибки.txt", append: true, encoding: Encoding.GetEncoding(1251)))
            {
                streamWriter.WriteLine(DateTime.Now + " : " + Exception);
                streamWriter.WriteLine();
            }
        }

        public static void KillProccess(string proccessName)
        {
            Process[] processList = Process.GetProcessesByName(proccessName);

            foreach (var process in processList)
            {
                process.Kill();
            }
        }

        public static void CloseApp()
        {
            Environment.Exit(1);
        }
    }
}
