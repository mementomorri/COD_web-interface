using COD.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace COD
{
    public static class ReadDB
    {
        #region variable
        public static DataTable History;// = new DataTable();

        public static DataTable TableChart;

        private static DateTime ChartLastPoint = DateTime.Parse("2020-01-01 00:00");

        private static DateTime ChartPointUpdate;

        private static int ChartCycleUpdate = 60;

        public static DataTable TableReference;

        public static DataTable TableAction;

        public static List<DataTable> TableValues;

        public static List<sandJson> FullData;

        public static List<sandJson> ChangeData;

        public static List<chart> DataChart;

        public static string DataChartString;

        #endregion

        public static void ThreadDB() 
        {
            bool InitStatus = false;
            // запрашиваем конфигурационные таблицы
            while (!InitStatus)
            {
                try
                {
                    string sConnect = "Data Source=" + COD.Properties.Resources.DB_Node + "; Initial Catalog = " + COD.Properties.Resources.DB_Catalog + "; Password = " + COD.Properties.Resources.DB_Pwd + "; User ID = " + COD.Properties.Resources.DB_User + ";";
                    string sqlQuery = @"SELECT [TagID],[TagName] FROM [dbo].[Reference] WHERE TagName is not null and TagName <> ''";
                   
                    SqlConnection con = new SqlConnection(sConnect);
                    SqlCommand comm = new SqlCommand(sqlQuery, con);
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(comm);
                        DataSet ds = new DataSet();
                        da.Fill(ds);
                        TableReference = ds.Tables[0];

                        sqlQuery = "SELECT [TagID],[Feature],[Attribute],[Value] FROM [dbo].[Action]";
                        comm = new SqlCommand(sqlQuery, con);
                        da = new SqlDataAdapter(comm);
                        ds = null;
                        ds = new DataSet();
                        da.Fill(ds);
                        TableAction = ds.Tables[0];

                        sqlQuery = "SELECT [Tag_name],[Value],[Timestamp],[quality],[type] FROM [cod_db].[dbo].[Chart]";
                        comm = new SqlCommand(sqlQuery, con);
                        da = new SqlDataAdapter(comm);
                        ds = null;
                        ds = new DataSet();
                        da.Fill(ds);
                        TableChart = ds.Tables[0];

                        con.Close();

                        if (TableChart != null)
                            if(TableChart.Rows.Count > 0)
                                ChartLastPoint = TableChart.AsEnumerable().Max(r => r.Field<DateTime>(TableChart.Columns[2]));

                    }
                    catch (Exception ex) { Console.WriteLine("ERROR Ошибка при рабоде с БД: " + ex.Message); }
                }
                catch (Exception ex) { Console.WriteLine("ERROR Ошибка при рабоде с БД: " + ex.Message); }

                if (TableReference != null && TableAction != null && TableChart != null) 
                    InitStatus = true;
                else 
                    Thread.Sleep(1000); 
                
            }

            while (true) 
            {
                // обновляем данные в таблице истории
                try
                {
                    string sConnect = "Data Source=" + COD.Properties.Resources.DB_Node + "; Initial Catalog = " + COD.Properties.Resources.DB_Catalog + "; Password = " + COD.Properties.Resources.DB_Pwd + "; User ID = " + COD.Properties.Resources.DB_User + ";";
                    string sqlQuery = @"SELECT  [Tag_name],[Value],[Timestamp],[quality] FROM [dbo].[live_float] " +
                            @" union " +
                            @" SELECT  [Tag_name],[Value],[Timestamp],[quality] FROM [dbo].[live_int] " +
                            @" union " +
                            @" SELECT  [Tag_name],[Value],[Timestamp],[quality] FROM [dbo].[live_bool]";

                    SqlConnection con = new SqlConnection(sConnect);
                    SqlCommand comm = new SqlCommand(sqlQuery, con);
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(comm);
                        DataSet ds = new DataSet();
                        da.Fill(ds);

                        //формируем таблицу
                        History = ds.Tables[0];
                        con.Close();
                    }
                    catch(Exception ex) { Console.WriteLine("ERROR Ошибка при рабоде с БД: " + ex.Message); }

                    //обновляем JSON
                    if (History != null) 
                        FullData = GetFullData();

                    //обновляем данные в таблице графиков для веба от последнего значения до текущей даты
                    TimeSpan interval = DateTime.Now - ChartPointUpdate;
                    if (interval.TotalMinutes > ChartCycleUpdate) 
                    {
                        // get date from SQL 
                        sqlQuery = @"SELECT * FROM [dbo].[History_float] where ("+
                            @"Tag_name = '[q1]SHUK/AI/AI3/Out' or " +
                            @"Tag_name = '[q1]SHUKH/AI/AI1/Out' or Tag_name = '[q1]SHUK/AI/AI1/Out' or Tag_name = '[q1]SHUK/AI/AI2/Out' or " +
                            @"Tag_name = '[q1]SHUK/AI/AI4/Out' or Tag_name = '[q1]SHUK/AI/AI5/Out' or Tag_name = '[q1]SHUKH/AI/AI2/Out' or " +
                            @"Tag_name = '[q1]SHUKH/AI/AI3/Out' or Tag_name = '[q1]SHUKH/AI/AI6/Out' or Tag_name = '[q1]SHUKH/AI/AI7/Out' or "+
                            @"Tag_name = '[q1]SHUKH/AI/AI4/Out' or Tag_name = '[q1]SHUKH/AI/AI5/Out' or Tag_name = '[q1]SHUKH/AI/AI10/Out' or "+
                            @"Tag_name = '[q1]SHUKH/AI/AI11/Out') and Timestamp >  '"+ChartLastPoint.ToString( "yyyy-MM-dd HH:mm:ss" )+@"'"+ 
                            @"order by Timestamp desc";

                        try 
                        {
                            //con.ConnectionTimeout = 60000;
                            con.Open();
                            
                            comm = new SqlCommand(sqlQuery, con);
                            comm.CommandTimeout = 6000;
                            SqlDataAdapter da = new SqlDataAdapter(comm);
                            DataSet ds = new DataSet();
                            da.Fill(ds);

                            //выполняем фильтраци данных
                            DataTable FilterTable = filterData(ds.Tables[0]);
                            //добавляем данные в таблицу
                            for (int i = 0; i < FilterTable.Rows.Count; i++)
                            {
                                TableChart.Rows.Add(FilterTable.Rows[i][0], FilterTable.Rows[i][1], FilterTable.Rows[i][2], FilterTable.Rows[i][3], FilterTable.Rows[i][4]);
                            }

                            // сохраняем данные в SQL
                            try {
                                // Get a reference to a single row in the table. 
                                DataRow[] rowArray = FilterTable.Select();

                                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(con))
                                {
                                    bulkCopy.DestinationTableName = "dbo.Chart";
                                    bulkCopy.BulkCopyTimeout = 6600;

                                    try
                                    {
                                        //Write the array of rows to the destination.
                                        //bulkCopy.WriteToServer(rowArray);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                    }

                                }

                            } catch (Exception ex) { Console.WriteLine("ERROR Ошибка при рабоде с БД: " + ex.Message); }

                            con.Close();
                        }
                        catch (Exception ex) { Console.WriteLine("ERROR Ошибка при рабоде с БД: " + ex.Message); } //баг при смене формата данных

                        if (TableChart != null)
                            if (TableChart.Rows.Count > 0)
                                ChartLastPoint = TableChart.AsEnumerable().Max(r => r.Field<DateTime>(TableChart.Columns[2]));

                        // update data
                        ChartPointUpdate = DateTime.Now;
                    }
                }
                catch (Exception ex) { Console.WriteLine("ERROR Ошибка при рабоде с БД: " + ex.Message); }
                Thread.Sleep(1000);
            }
        }

        internal static List<FullPoint> getChart(DataRow[] arr, int size)
        {
            List<FullPoint> result = new List<FullPoint>();

            try
            {
                // определяем массив тэгов
                var query = (from r in arr.AsEnumerable()
                             select r["Tag_name"]).Distinct().ToList();
                foreach (string tag in query) 
                {
                    List<point> dt = new List<point>();

                    var max = arr.AsEnumerable().Where(p => p.Field<string>("Tag_name") == tag && p.Field<int>("type") == size).
                        Max(s => s["Value"]); 
                    var min = arr.AsEnumerable().Where(p => p.Field<string>("Tag_name") == tag && p.Field<int>("type") == size).
                        Min(s => s["Value"]);

                    var list = arr.AsEnumerable().Where(p => p.Field<string>("Tag_name") == tag && p.Field<int>("type") == size).ToList();
                    foreach (DataRow row in list)
                    {
                        try
                        {
                            point p = new point();
                            p.x = Convert.ToDateTime(row[2]).ToString("yyyy-MM-dd HH:mm");
                            p.y = row[1];
                            dt.Add(p);
                            

                        }
                        catch (Exception ex) { Console.WriteLine("ERROR Ошибка при рабоде с БД : " + ex.Message); }
                    }

                    result.Add(new FullPoint { label = tag, data = dt , MaxValue = Convert.ToInt16(max), MinValue = Convert.ToInt16(min) });
                }


            }catch (Exception ex) { Console.WriteLine("ERROR Ошибка при рабоде с БД: " + ex.Message); }

            return result;
        }

        private static DataTable filterData(DataTable dataTable)
        {
            DataTable result = new DataTable();
            //init table
            result.Columns.Add("Tag_name", typeof(string));
            result.Columns.Add("Value", typeof(float));
            result.Columns.Add("Timestamp", typeof(DateTime));
            result.Columns.Add("quality", typeof(string));
            result.Columns.Add("type", typeof(int));

            var arr = (from r in dataTable.AsEnumerable()
                       select r["Tag_name"]).Distinct().ToList();


            int a = 5;

            a = 6;
            // обработка массива данных
            foreach (string item in arr) 
            {
                //получаем набор данных
                DataRow[] row = dataTable.Select("Tag_name = '" + item + "'", "Timestamp ASC");
                //выполняем сортировку Small
                List<iChart> fData = PercolateData(row, 0);
                foreach (iChart dt in fData) 
                {
                    DataRow rw = result.NewRow();
                    rw["Tag_name"] = dt.Tag_name;
                    rw["Value"] = dt.Value;
                    rw["Timestamp"] = dt.Timestamp;
                    rw["quality"] = dt.quality;
                    rw["type"] = dt.type;
                    result.Rows.Add(rw);
                }
                //выполняем сортировку Medium
                fData = PercolateData(row, 1);
                foreach (iChart dt in fData)
                {
                    DataRow rw = result.NewRow();
                    rw["Tag_name"] = dt.Tag_name;
                    rw["Value"] = dt.Value;
                    rw["Timestamp"] = dt.Timestamp;
                    rw["quality"] = dt.quality;
                    rw["type"] = dt.type;
                    result.Rows.Add(rw);
                }
                //выполняем сортировку Large
                fData = PercolateData(row, 2);
                foreach (iChart dt in fData)
                {
                    DataRow rw = result.NewRow();
                    rw["Tag_name"] = dt.Tag_name;
                    rw["Value"] = dt.Value;
                    rw["Timestamp"] = dt.Timestamp;
                    rw["quality"] = dt.quality;
                    rw["type"] = dt.type;
                    result.Rows.Add(rw);
                }
            }
            
            return result;
        }

        #region GetChangeData
        private static List<sandJson> GetChangeData()
        {
            List<sandJson> result = new List<sandJson>();

            try
            {
                foreach (DataRow RowHis in History.Select("FlagUpdate = 1"))
                {
                    DataRow[] FoundR  = TableReference.Select("Tag_name like '%" + RowHis["Tag_name"].ToString() + "%'");

                    foreach (DataRow RowRwf in FoundR)
                    {
                        int count = RowRwf["TagName"].ToString().IndexOf("&") == -1 ? 1 : RowRwf["TagName"].ToString().Split("&").Count();
                        if (RowRwf["TagName"].ToString() != null && RowRwf["TagName"].ToString() != "")
                        {
                            string query = "";

                            if (RowRwf["TagName"].ToString().IndexOf("&") == -1)
                                query = @"Tag_name = '" + RowRwf["TagName"].ToString() + @"'";
                            else
                                foreach (string tag in RowRwf["TagName"].ToString().Split("&"))
                                    query += (query == "") ? (@"Tag_name = '" + tag + @"'") : (@" or Tag_name = '" + tag + @"'");

                            //query = @"Tag_name = '" + RowRwf["TagName"].ToString() + @"'";
                            DataRow[] FoundId;
                            FoundId = History.Select(query);
                            HistData[] hd = new HistData[count];
                            if (FoundId != null)
                            {
                                if (FoundId.Count() > 0)
                                {
                                    foreach (DataRow row in FoundId)
                                    {
                                        //hd.Add(new HistData
                                        //{
                                        hd[RowRwf["TagName"].ToString().Split("&").ToList().IndexOf(row["Tag_Name"].ToString())].value = row["Value"];
                                        hd[RowRwf["TagName"].ToString().Split("&").ToList().IndexOf(row["Tag_Name"].ToString())].tag = row["Tag_Name"].ToString();
                                        hd[RowRwf["TagName"].ToString().Split("&").ToList().IndexOf(row["Tag_Name"].ToString())].id = RowRwf["TagID"].ToString();
                                        hd[RowRwf["TagName"].ToString().Split("&").ToList().IndexOf(row["Tag_Name"].ToString())].quality = row["Quality"].ToString();
                                        hd[RowRwf["TagName"].ToString().Split("&").ToList().IndexOf(row["Tag_Name"].ToString())].timestamp = (DateTime)row["Timestamp"];
                                        //});
                                    }
                                    //перенос значений



                                    // находим действие
                                    DataRow[] FoundAct;
                                    FoundAct = TableAction.Select("TagID = '" + hd[0].id + "'");

                                    foreach (DataRow row in FoundAct)
                                    {
                                        object val = getValue(hd, row[3].ToString());

                                        result.Add(new sandJson { id = hd[0].id, feature = row[1].ToString(), atribute = row[2].ToString(), value = val });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine(ex); }
            return result;
        }
        #endregion

        #region GetFullData
        internal static List<sandJson> GetFullData()
        {
            List<sandJson> result = new List<sandJson>();

            try 
            {
                DateTime maxData = Convert.ToDateTime( History.AsEnumerable().Max(row => row["Timestamp"]).ToString());
                TimeSpan ts = DateTime.Now - maxData;

                result.Add(new sandJson { id = "TS_2020", value = maxData.ToString("dd.MM.yyyyy HH:mm"), feature = "TS_2020", atribute = ts.TotalMinutes > 10 ? "red":"green" });

                foreach (DataRow RowRwf in TableReference.Rows) 
                {
                     //if (RowRwf["TagID"].ToString() == "Count2_value")
                     //   Console.WriteLine("the tag 'Count2_value' exists!");
                    int count = RowRwf["TagName"].ToString().IndexOf("&") == -1 ? 1 : RowRwf["TagName"].ToString().Split("&").Count();
                    if (RowRwf["TagName"].ToString() != null && RowRwf["TagName"].ToString() != "")
                    {
                        string query = "";

                        if (RowRwf["TagName"].ToString().IndexOf("&") == -1)
                            query = @"Tag_name = '" + RowRwf["TagName"].ToString() + @"'";
                        else
                            foreach (string tag in RowRwf["TagName"].ToString().Split("&")) 
                                query += (query == "") ? (@"Tag_name = '" + tag + @"'") : (@" or Tag_name = '" + tag + @"'");

                        //query = @"Tag_name = '" + RowRwf["TagName"].ToString() + @"'";
                        DataRow[] FoundId;
                        FoundId = History.Select(query);
                        HistData[] hd = new HistData[count];

                        if (FoundId != null)
                        {
                            if (FoundId.Count() > 0)
                            {
                                foreach (DataRow row in FoundId)
                                {
                                    //hd.Add(new HistData
                                    if (count > 1)
                                    {
                                        hd[RowRwf["TagName"].ToString().Split("&").ToList().IndexOf(row["Tag_Name"].ToString())].value = row["Value"];
                                        hd[RowRwf["TagName"].ToString().Split("&").ToList().IndexOf(row["Tag_Name"].ToString())].tag = row["Tag_Name"].ToString();
                                        hd[RowRwf["TagName"].ToString().Split("&").ToList().IndexOf(row["Tag_Name"].ToString())].id = RowRwf["TagID"].ToString();
                                        hd[RowRwf["TagName"].ToString().Split("&").ToList().IndexOf(row["Tag_Name"].ToString())].quality = row["Quality"].ToString();
                                        hd[RowRwf["TagName"].ToString().Split("&").ToList().IndexOf(row["Tag_Name"].ToString())].timestamp = (DateTime)row["Timestamp"];
                                    }
                                    else 
                                    {
                                        hd[0].value = row["Value"];
                                        hd[0].tag = row["Tag_Name"].ToString();
                                        hd[0].id = RowRwf["TagID"].ToString();
                                        hd[0].quality = row["Quality"].ToString();
                                        hd[0].timestamp = (DateTime)row["Timestamp"];
                                    }
                                }
                                //перенос значений



                                // находим действие
                                DataRow[] FoundAct;
                                FoundAct = TableAction.Select("TagID = '" + RowRwf["TagID"].ToString() + "'");

                                foreach (DataRow row in FoundAct)
                                {
                                    object val = getValue(hd, row[3].ToString());
                                    
                                    if(val != null)
                                        result.Add(new sandJson { id = RowRwf["TagID"].ToString(), feature = row[1].ToString(), atribute = row[2].ToString(), value = val });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex){ Console.WriteLine(ex); }

            return result;
        }


        private static object getValue(HistData[] hd, string v)
        {
            object result = null;
            switch (v) 
            {
                case "auto_man": 
                    { 
                        try
                        {
                            if ((double)hd[0].value == 1)
                            {
                                result = "основной";
                            }
                            else
                            {
                                result = "резервный";
                            }
                        }
                        catch
                        {
                            result = "Err";
                        }
                    } break;
                case "color_7": {
                        try
                        {                          
                            if (Convert.ToInt16(hd[0].value) == 3)
                            {
                                result = "#F03115";
                            }
                            else if (Convert.ToInt16(hd[0].value) == 5)
                            {
                                result = "#FFFFFF";
                            }
                            else
                            {
                                result = "#5F639D";
                            }

                        }
                        catch
                        {
                            result = "#5F639D";
                        }
                    } break;
                case "display": {
                        try
                        {
                            if ((double)hd[0].value > 0)
                            {
                                result = "block";
                            }
                            else
                            {
                                result = "none";
                            }
                        } catch
                        {
                            result = "none";
                        }
                    } break;

                case "fill_1": {
                        try
                            
                        {
                            if ((Convert.ToInt16(hd[0].value) & 0x04) > 0)
                            {
                                result = "#FF0A0A";
                            }
                            else if ((Convert.ToInt16(hd[0].value) & 0x02) > 0)
                            {
                                result = "#838483";
                            }
                            else if ((Convert.ToInt16(hd[0].value) & 0x01) > 0)
                            {
                                result = "#76BB81";
                            }
                            else
                            {
                                result = "#838483";
                            }
                        }
                        catch
                        {
                            result = "grey";
                        }
                    }
                    break;

                case "fill_1a":
                    {
                        try
                        {
                            result = (double)hd[0].value == 1 ? "#76BB81" : "#838483";
                        }
                        catch
                        {
                            result = "grey";
                        }
                    }



                    break;
                case "fill_2": 
                    {
                        try
                        {
                            if ((double)hd[0].value == 1)
                            {
                                result = "#76BB81";
                            }
                            else
                            {
                                result = "#838483";
                            }
                        }
                        catch
                        {
                            result = "#838483";
                        }
                    } break;
                case "fill_3": {
                        try
                        {
                            if (((double)hd[0].value) == 1)
                            {
                                result = "#FF9D0A";
                            }
                            else
                            {
                                result = "#838483";
                            }
                        }
                        catch
                        {
                            result = "#838483";
                        }

                    } break;
                case "fill_5_1": {
                        try
                        {                        
                            if ((Convert.ToInt16(hd[0].value) & 0x04) > 0 )
                            {
                                result = "#FF0A0A";
                            } 
                            else if ((Convert.ToInt16(hd[0].value) & 0x02) > 0)
                            {
                                result = "#76BB81";
                            }
                            else if ((Convert.ToInt16(hd[0].value) & 0x01) > 0)
                            {
                                result = "#76BB81";
                            }
                            else
                            {
                                result = "#9D9D9D";
                            }
                        }
                        catch
                        {
                            result = "grey";
                        }

                    } break;
                case "fill_5_2": {
                        try
                        {
                            if ((Convert.ToInt16(hd[0].value) & 0x04) > 0)
                            {
                                result = "#FF0A0A";
                            }
                            else if ((Convert.ToInt16(hd[0].value) & 0x02) > 0)
                            {
                                result = "#9D9D9D";
                            }
                            else if ((Convert.ToInt16(hd[0].value) & 0x01) > 0)
                            {
                                result = "#76BB81";
                            }
                            else
                            {
                                result = "#9D9D9D";
                            }
                        }
                        catch
                        {
                            result = "grey";
                        }
                    } break;
                case "fill_8": 
                    {
                        try
                        {
                            if ((Convert.ToInt16(hd[0].value) & 0x04) > 0)
                            {
                                result = "#F03115";
                            }
                            else if ((Convert.ToInt16(hd[0].value) & 0x02) > 0)
                            {
                                result = "#5F639D";
                            }
                            else if ((Convert.ToInt16(hd[0].value) & 0x01) > 0)
                            {
                                result = "#29D54F";
                            }
                            else
                            {
                                result = "#3D53BD";
                            }
                        }
                        catch
                        {
                            result = "#3D53BD";
                        }
                    } break;
                     
                case "fill_9": {
                        try
                        {
                            if (Convert.ToInt16(hd[0].value) == 1)
                            {
                                result = "#FF0A0A";
                            }
                            else if (Convert.ToInt16(hd[1].value) == 1)
                            {
                                result = "#FFB740";
                            }
                            else
                            {
                                result = "#8799C8";
                            }
                        }
                        catch
                        {
                            result = "#8799C8";
                        }
                    } break;

                case "height": {
                        try
                        {
                            if ((Convert.ToInt16(hd[0].value) == 0) && !(hd[0].value is null)) 
                            {
                                result = 10;
                            }
                            else if (Convert.ToInt16(hd[0].value) == 1)
                            {
                                result = 20;
                            }
                            else if (Convert.ToInt16(hd[1].value) == 1)
                            {
                                result = 25;
                            }
                            else if (Convert.ToInt16(hd[2].value) == 1)
                            {
                                result = 30;
                            
                            }
                            else if (Convert.ToInt16(hd[3].value) == 1)
                            {
                                result = 40;

                            }
                            else
                            {
                                result = 0;
                            }
                        }
                        catch
                        {
                            result = 0;
                        }

                    } break;
                case "shift": {
                        try
                        {
                            if ((Convert.ToInt16(hd[0].value) == 0) && !(hd[0].value is null)) 
                            {
                                result = 285;
                            }
                            else if (Convert.ToInt16(hd[0].value) == 1 )
                            {
                                result = 275;
                            }
                            else if (Convert.ToInt16(hd[1].value) == 1)
                            {
                                result = 270;
                            }
                            else if (Convert.ToInt16(hd[2].value) == 1)
                            {
                                result = 265;

                            }
                            else if (Convert.ToInt16(hd[3].value) ==1)
                            {
                                result = 255;

                            }
                            else
                            {
                                result = 295;
                            }
                        }
                        catch
                        {
                            result = 295;
                        }
                    } break;


                case "fire_alarm": {
                        try
                        {
                            if ((Convert.ToInt16(hd[0].value) & 0x01) > 0)
                            {
                                result = "addClass&fire_alarm";
                            }
                            else
                            {
                                result = "removeClass&fire_alarm";
                            }
                        }
                        catch
                        {
                            result = "removeClass&fire_alarm";
                        }
                    } break;
                case "heating": {
                        try
                        {
                            if (((double)hd[0].value) == 1)
                            {
                                result = "обогр";
                            }
                            else
                            {
                                result = "";
                            }
                        }
                        catch
                        {
                            result = "Err";
                        }

                    } break;
                case "num_avt": {
                        try
                        {
                            if(hd.Length == 7)
                            {
                                result = ((Convert.ToInt16(hd[0].value)) & 0x01) + (Convert.ToInt16(hd[1].value) & 0x01) + (Convert.ToInt16(hd[2].value) & 0x01)
                                        + (Convert.ToInt16(hd[3].value) & 0x01) + (Convert.ToInt16(hd[4].value) & 0x01) + (Convert.ToInt16(hd[5].value) & 0x01)
                                        + (Convert.ToInt16(hd[6].value) & 0x01);
                            }
                        }
                        catch
                        {
                            result = 0;
                        }                    
                    
                    } break;
                case "num_on": {
                        try
                        {
                            if (hd.Length == 7)
                            {
                                result = ((Convert.ToInt16(hd[0].value)) & 0x01) + (Convert.ToInt16(hd[1].value) & 0x01) + (Convert.ToInt16(hd[2].value) & 0x01)
                                        + (Convert.ToInt16(hd[3].value) & 0x01) + (Convert.ToInt16(hd[4].value) & 0x01) + (Convert.ToInt16(hd[5].value) & 0x01)
                                        + (Convert.ToInt16(hd[6].value) & 0x01);
                            }
                        }
                        catch
                        {
                            result = 0;
                        }

                    } break;
                case "on_off": {
                        try
                        {
                            if ((Convert.ToInt16(hd[0].value) & 0x04) > 0)
                            {
                                result = "авар";
                            }
                            else if ((Convert.ToInt16(hd[0].value) & 0x01) > 0)
                            {
                                result = "вкл";
                            }
                            else
                            {
                                result = "откл";
                            }
                        }
                        catch
                        {
                            result = "Err";
                        }
                    } break;
                case "open_close": {
                        try
                        {
                            if ((Convert.ToInt16(hd[0].value) & 0x01) > 0)
                            {
                                result = "откр";
                            }                           
                            else
                            {
                                result = "закр";
                            }
                        }
                        catch
                        {
                            result = "Err";
                        }

                    } break;
                case "percent": {
                        try
                        {
                            result = (((double)hd[0].value - (double)hd[1].value) / ((double)hd[2].value - (double)hd[1].value)) * 100.0;
                            if (result == null || Double.IsNaN((Double)result) || Double.IsInfinity((Double)result)) result = 0;
                        }
                        catch { result = 0; }
                    } break;
                
                case "rotate": {

                        try
                        {
                            double rot = 0;
                            double activeValue = 0;

                            if (hd.Length == 8) 
                            {activeValue = ((double)hd[0].value > (double)hd[7].value) ? (double)hd[0].value : (double)hd[7].value;}
                            else {activeValue = (double)hd[0].value;}

                            rot = (int)((activeValue - (double)hd[1].value) / ((double)hd[2].value - (double)hd[1].value) * 270.0 - 135.0);                            
                            rot = rot < -135 ? -135 : rot > 135 ? 135 : rot;
                            result = "rotate(" + rot + "deg)";
                        }
                        catch { result = "rotate(" + -135 + "deg)";}
                } break;
                case "rotation": {
                        try
                        {
                            result = (double)hd[0].value == 1 ? "addClass&rotation" : "removeClass&rotation";
                        }
                        catch{ result = "removeClass&rotation"; }
                    
                    } break;
 
                case "stroke_10": {
                        try
                        {
                            if ((double)hd[0].value > (double)hd[3].value || (double)hd[0].value > (double)hd[4].value || (double)hd[0].value < (double)hd[5].value || (double)hd[0].value < (double)hd[6].value)
                            {
                                result = "#FF0A0A";
                            }
                            else
                            {
                                result = "#76BB81";
                            }
                        }
                        catch
                        {
                            result = "#76BB81";
                        }

                    } break;

                case "fill_11":
                    {
                        try

                        {
                            if (Convert.ToInt16(hd[0].value) == 3)
                            {
                                result = "#FF0A0A";
                            }
                            else if (Convert.ToInt16(hd[0].value) == 5)
                            {
                                result = "#76BB81";
                            }
                            else
                            {
                                result = "#838483";
                            }
                        }
                        catch
                        {
                            result = "grey";
                        }
                    }
                    break;

                case "fill_11a":
                    {
                        try

                        {
                            if (Convert.ToInt16(hd[0].value) == 0)
                            {
                                result = "#838483";
                            }
                            else if (Convert.ToInt16(hd[0].value) == 1)
                            {
                                result = "#76BB81";
                            }
                            else
                            {
                                result = "#838483";
                            }
                        }
                        catch
                        {
                            result = "grey";
                        }
                    }
                    break;

                case "on_off_2":
                    {
                        try

                        {
                            if (Convert.ToInt16(hd[0].value) == 3)
                            {
                                result = "авар";
                            }
                            else if (Convert.ToInt16(hd[0].value) == 5)
                            {
                                result = "вкл";
                            }
                            else
                            {
                                result = "откл";
                            }
                        }
                        catch
                        {
                            result = "Err";
                        }
                    }
                    break;


                case "value": {

                            try
                            {
                            if (hd.Length == 1)
                                result = ((double)hd[0].value >= 100) ? Math.Round((double)hd[0].value, 0) : Math.Round((double)hd[0].value, 1);
                            else
                                result = ((double)hd[0].value >= 100 || (double)hd[1].value >= 100) ?
                                            ((double)hd[0].value > (double)hd[1].value) ? Math.Round((double)hd[0].value, 0) : Math.Round((double)hd[1].value, 0)
                                    :
                                            ((double)hd[0].value > (double)hd[1].value) ? Math.Round((double)hd[0].value, 1) : Math.Round((double)hd[1].value, 1);

                            }
                            catch {
                                result = "Err";
                            }                          
                                              
                        
                    } break;
                case "vent_alarm": {

                        try
                        {
                            if ((Convert.ToInt16(hd[0].value) & 0x00) > 0)
                            {
                                result = "addClass&vent_alarm";
                            }
                            else
                            {
                                result = "removeClass&vent_alarm";
                            }
                        }
                        catch
                        {
                            result = "removeClass&vent_alarm";
                        }

                    } break;
                case "vent_on": {
                        {
                            try
                            {
                                if (hd.Length == 1)
                                {
                                    if ((Convert.ToInt16(hd[0].value) & 0x01) > 0)
                                    {
                                        result = "addClass&vent_on";
                                    }
                                    else
                                    {
                                        result = "removeClass&vent_on";
                                    }
                                }
                                else
                                {
                                    if (((Convert.ToInt16(hd[0].value) & 0x01) > 0) || (Convert.ToInt16(hd[1].value) & 0x01) > 0)
                                    {
                                        result = "addClass&vent_on";
                                    }
                                    else
                                    {
                                        result = "removeClass&vent_on";
                                    }
                                }
                                   
                            }
                            catch
                            {
                                result = "removeClass&vent_on";
                            }
                        }
                   
                    } break;               
                case "absent":
                    {
                        {
                            try { result =  (Convert.ToInt16(hd[0].value) == 1) ? "block" : "none"; }
                            catch { result = "none"; }
                        }
                    }
                    break;

                case "auto_man_text_color":
                    {
                        {
                            try{
                                if (Convert.ToBoolean(hd[0].value)) {
                                    result = "A&#4362B4";
                                } 
                                else
                                {
                                    result = "P&#4362B4";
                                }                                
                            
                            }
                            catch{ result = "Err&#838483"; }
                        }
                    }
                    break;

                case "discIndic":
                    {
                        try
                        {
                            if ((double)hd[0].value == 0)
                            {
                                result = 0;
                            }
                            else if ((double)hd[0].value > 400)
                            {
                                result = 1;
                            }
                            else if ((double)hd[0].value > 725)
                            {
                                result = 2;
                            }
                            else if ((double)hd[0].value > 950)
                            {
                                result = 3;
                            }
                            else if ((double)hd[0].value > 1175 && (double)hd[0].value < 3000)
                            {
                                result = 4;
                            }
                            else
                            {
                                result = 0;
                            }
                        }
                        catch { result = 0; }
                    }
                    break;

                    case "timestamp":
                    {
                        try
                        {
                            result = (double)hd[0].value;  
                        }
                        catch { result = 0; }
                    }
                    break;

            }
            return result; 
        }
        #endregion


        #region add row action table
        private static void AddAction()
        {
            AddRowAct("T_street", "bar", "", "percent&-50&50");
            AddRowAct("T_street", "css", "stroke", "color&#76BB81&grey");
            AddRowAct("T_street_v", "text", "", "value&");

            AddRowAct("T_street_v_3", "text", "", "value&");
            AddRowAct("T_street_v_f_3", "text", "", "value&");
            AddRowAct("T_street_rot3", "css", "transform", "rotate&per&deg&-50&50");
        }
        private static void AddRowAct(string id, string feature,string attr, string value)
        {
            DataRow row = TableAction.NewRow();
            row["Tag_ID"] = id;
            row["feature"] = feature;
            row["attribute"] = attr;
            row["value"] = value;
           
            TableAction.Rows.Add(row);
        }
        #endregion


        #region Add reference table
        private static void AddRef()
        {
            AddRowRef("[edge]SHUKH/AI/AI1/Out","T_street");
            AddRowRef("[edge]SHUKH/AI/AI1/Out", "T_street_v");

            AddRowRef("[edge]SHUKH/AI/AI1/Out", "T_street_v_3");
            AddRowRef("[edge]SHUKH/AI/AI1/Out", "T_street_v_f_3");
            AddRowRef("[edge]SHUKH/AI/AI1/Out", "T_street_rot3");
        }

        private static void AddRowRef(string tag, string id) 
        {
            DataRow row = TableReference.NewRow();
            row["Tag_name"] = tag;
            row["Tag_ID"] = id;
            TableReference.Rows.Add(row);
        }
        #endregion


        internal static List<iChart> PercolateData(DataRow[] row, int type) 
        {
            List<iChart> result = new List<iChart>();
            if (row is null) return result;
            if (row.Length == 0) return result;
            int interval = 32, deadband = 4;
            float lastVal;
            DateTime lastTime;
            TimeSpan difTime;

            try 
            {
                // в соответствии с типом выбираем дедбенд и интервал
                switch (type)
                {
                    case 0: { interval = 10; deadband = 1; } break;
                    case 1: { interval = 30; deadband = 2; } break;
                    case 2: { interval = 60; deadband = 4; } break;
                }
                // фильтрация данных
                result.Add(new iChart
                {
                    Tag_name = row[0].ItemArray[0].ToString(),
                    Value = (float)Convert.ToDouble(row[0].ItemArray[1].ToString()),
                    Timestamp = Convert.ToDateTime(row[0].ItemArray[2].ToString()),
                    quality = row[0].ItemArray[3].ToString(),
                    type = type
                });

                lastVal = result[0].Value;
                lastTime = result[0].Timestamp;

                if (lastVal > 100) deadband *= 3;

                for (int i = 1; i < row.Length; i++) 
                {
                    //проверяем на интервал и отклонение
                    difTime = Convert.ToDateTime(row[i].ItemArray[2].ToString()) - lastTime;
                    if (Math.Abs((float)Convert.ToDouble(row[i].ItemArray[1].ToString()) - lastVal) >= deadband || difTime.TotalMinutes >= interval) 
                    {
                        result.Add(new iChart
                        {
                            Tag_name = row[i].ItemArray[0].ToString(),
                            Value = (float)Convert.ToDouble(row[i].ItemArray[1].ToString()),
                            Timestamp = Convert.ToDateTime(row[i].ItemArray[2].ToString()),
                            quality = row[i].ItemArray[3].ToString(),
                            type = type
                        });
                        lastVal = (float)Convert.ToDouble(row[i].ItemArray[1].ToString());
                        lastTime = Convert.ToDateTime(row[i].ItemArray[2].ToString());
                    }
                }

            } 
            catch (Exception ex) { Console.WriteLine("ERROR Ошибка при рабоде с БД: " + ex.Message); }


            return result;
        }
    }
}
