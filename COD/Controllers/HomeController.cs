using COD.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace COD.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Energetics()
        {
            return View();
        }

        public IActionResult Trends()
        {
            return View();
        }

        public string TestAjax(int leagueId)
        {

            List<sandJson> sendArray = new List<sandJson>();
            string json = JsonSerializer.Serialize<List<sandJson>>(sendArray);

            return json;
        }

        public string startData(int leagueId)
        {

            List<sandJson> sendArray = ReadDB.FullData;// .GetFullData();
            string json = JsonSerializer.Serialize<List<sandJson>>(sendArray);

            return json;
        }

        public string refreshData(int leagueId)
        {

            List<sandJson> sendArray = ReadDB.ChangeData;// .GetFullData();
            string json = JsonSerializer.Serialize<List<sandJson>>(sendArray);

            return json;
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public string ChartData(string chek)
        {
            DateTime t = DateTime.Now;
            List<chart> data = new List<chart>();
            try
            {
                List<string> arr = new List<string>();
                if (chek.IndexOf('&') > -1)
                    arr.AddRange(chek.Split('&'));
                else
                    arr.Add(chek);

                foreach (string elem in arr)
                {

                    string sConnect = "Data Source=" + COD.Properties.Resources.DB_Node + "; Initial Catalog = " + COD.Properties.Resources.DB_Catalog + "; Password = " + COD.Properties.Resources.DB_Pwd + "; User ID = " + COD.Properties.Resources.DB_User + ";";
                    string sqlQuery = @"SELECT top 1000 [Tag_name],[Timestamp],[Value] FROM [dbo].[History_float] WHERE Tag_Name = '" + elem + @"' AND Timestamp > dateadd(month, -1, getdate()) order by Timestamp";

                    SqlConnection con = new SqlConnection(sConnect);
                    SqlCommand comm = new SqlCommand(sqlQuery, con);
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(comm);
                        DataSet ds = new DataSet();
                        da.Fill(ds);
                        chart item = new chart();
                        item.label = chek;
                        item.MaxValue = 0;
                        item.MinValue = 0;
                        item.data = new List<point>();
                        double yPrev = 0;
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            try
                            {
                                //if (Math.Abs(Convert.ToDouble(row[2]) - yPrev) > yPrev / 100)
                                
                                    point p = new point();
                                    p.x = Convert.ToDateTime(row[1]).ToString("yyyy-MM-dd HH:mm"); //i+"";//row[1];//.ToString();
                                    p.y = row[2];// Convert.ToDouble(row[2]).ToString("#.##");
                                    item.data.Add(p);
                                    yPrev = Convert.ToDouble(row[2]);
                                    item.MaxValue = Convert.ToInt32(row[2]) > item.MaxValue ? Convert.ToInt32(row[2]) : item.MaxValue;
                                    item.MinValue = Convert.ToInt32(row[2]) < item.MinValue ? Convert.ToInt32(row[2]) : item.MinValue;

                            }
                            catch { }
                        }
                        //TableReference = ds.Tables[0];
                        data.Add(item);


                        con.Close();
                    }
                    catch (Exception ex) { Console.WriteLine("ERROR Ошибка при рабоде с БД: " + ex.Message); }
                }
            }
            catch (Exception ex) { Console.WriteLine("ERROR Ошибка при рабоде с БД: " + ex.Message); }


            Dictionary<string, string> chartData = new Dictionary<string, string>
                {
                    {"T", @"T_MR1&20&30&15&20&55&0&100|T_MR2&20&30&15&20&55&0&100"},
                    {"TDate", "01.10.20 00:00&01.10.20 00:01&01.10.20 00:02&0&0&0&0"},
                    {"P", "20&30&15"},
                    {"PDate", "01.10.20 00:00&01.10.20 00:01&01.10.20 00:02"},
                };
            //string json = JsonSerializer.Serialize(chartData);
            string json = JsonSerializer.Serialize(data);
            TimeSpan dif = DateTime.Now - t;
            //Console.WriteLine("ms: " + dif.TotalMilliseconds);
            return json;
        }


        public string GetChartData(string start, string end)
        {

            ChartPack result = new ChartPack();
            try
            {
                int[] d_arr = GetDateToArray(start);
                DateTime D1 = new DateTime(d_arr[2], d_arr[1], d_arr[0], d_arr[3], d_arr[4], d_arr[5]); 
                d_arr = GetDateToArray(end);
                DateTime D2 = new DateTime(d_arr[2], d_arr[1], d_arr[0], d_arr[3], d_arr[4], d_arr[5]); 


                result.startDate = D1.ToString(@"dd'/'MM'/'yyyy hh:mm:ss");
                result.endDate = D2.ToString(@"dd'/'MM'/'yyyy hh:mm:ss");

                ChartData ArrDt = new ChartData();

                try
                {
                    //получаем данные
                    DataRow[] arr = ReadDB.TableChart.Select("Timestamp > '" + D1 + "' and  Timestamp < '" + D2 + "' ", "Timestamp ASC");
                    //формируем структуру к отправки
                    List<object> query = (from r in arr.AsEnumerable()
                                          select r["Tag_name"]).Distinct().ToList();

                    ArrDt.day = ReadDB.getChart(arr, (int)ChartSize.day);
                    ArrDt.week = ReadDB.getChart(arr, (int)ChartSize.week);
                    ArrDt.month = ReadDB.getChart(arr, (int)ChartSize.month);
                    result.data = ArrDt;



                }
                catch (Exception ex) { Console.WriteLine("ERROR Ошибка при рабоде с БД: " + ex.Message); }

            }catch (Exception ex) { Console.WriteLine("ERROR Ошибка при рабоде с БД: " + ex.Message); }
            //отправка данных
            string json = JsonSerializer.Serialize(result);

            return json;
        }

        private int[] GetDateToArray(string start)
        {
            int[] result = new int[6];

            try 
            {
                var arr = start.Split(' ');
                var a1 = arr[0].Split(@"/");
                var a2 = arr[1].Split(@":");

                for (int i = 0; i < a1.Length; i++)
                    result[i] = Convert.ToInt32(a1[i]);


                for (int i = 0; i < a2.Length; i++)
                    result[3+i] = Convert.ToInt32(a2[i]);
            } 
            catch (Exception ex) { Console.WriteLine("ERROR Ошибка: " + ex.Message); }

            return result;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public string GetDateConvert1(string date)
        {

            string result = "";
            try
            {
                string dateString, format;
                //DateTime result;
                CultureInfo provider = CultureInfo.InvariantCulture;
                format = @"dd'/'MM'/'yyyy hh:mm:ss";

                result = DateTime.ParseExact(date, format, provider).ToString(@"dd'/'MM'/'yyyy hh:mm:ss");

                //result = DateTime.Parse(date, new CultureInfo("fr-FR"));

            }
            catch (Exception ex) { Console.WriteLine("ERROR Ошибка: " + ex.Message); }

            return result;
        }

        public string GetDateConvert2(string date)
        {

            string result = "";
            try
            {
                string dateString, format;
                //DateTime result;
                CultureInfo provider = CultureInfo.InvariantCulture;
                format = @"dd'/'MM'/'yyyy hh:mm:ss";

                result = DateTime.Parse(date, new CultureInfo("fr-FR")).ToString(@"dd'/'MM'/'yyyy hh:mm:ss");

                //result = DateTime.Parse(date, new CultureInfo("fr-FR"));

            }
            catch (Exception ex) { Console.WriteLine("ERROR Ошибка: " + ex.Message); }

            return result;
        }
    }
}
