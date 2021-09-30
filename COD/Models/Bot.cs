using COD.Models.Commands;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace COD.Models
{
    public class Bot
    {

        public Bot() { }
        private bool noti = false;
        private static  DataTable TableUser;

        private static Dictionary<string, bool> trig = new Dictionary<string, bool>();
        private static Dictionary<string, string> reference = new Dictionary<string, string>();
        private static Dictionary<string, string> message = new Dictionary<string, string>();

        public static TelegramBotClient botClient;
        private List<Command> commandsList;


        //read DB
        Thread DBStream = new Thread(NotificationTelegram);

        private static void NotificationTelegram(object obj)
        {
            // добавляем тригиры автоматического оповещения
            trig.Add("Fire", false);    reference.Add(@"[q1]Fire","Fire");                         message.Add("Fire",    @"Внимание! Пожар в Центре Обработки Данных!");
            trig.Add("M1_HiHi", false); reference.Add(@"[q1]SHUK/AI/AI1/HiHi_trig", "M1_HiHi"); message.Add("M1_HiHi", @"Внимание! В Машзале №1 аварийно высокая температура.");
            trig.Add("M2_HiHi", false); reference.Add(@"[q1]SHUK/AI/AI2/HiHi_trig", "M2_HiHi"); message.Add("M2_HiHi", @"Внимание! В Машзале №2 аварийно высокая температура.");
            trig.Add("M3_HiHi", false); reference.Add(@"[q1]SHUK/AI/AI3/HiHi_trig", "M3_HiHi"); message.Add("M3_HiHi", @"Внимание! В Машзале №3 аварийно высокая температура.");
            trig.Add("M4_HiHi", false); reference.Add(@"[q1]SHUK/AI/AI4/HiHi_trig", "M4_HiHi"); message.Add("M4_HiHi", @"Внимание! В Машзале №4 аварийно высокая температура.");
            trig.Add("ME_HiHi", false); reference.Add(@"[q1]SHUK/AI/AI5/HiHi_trig", "ME_HiHi"); message.Add("ME_HiHi", @"Внимание! В Электрощитовой аварийно высокая температура.");

            bool InitStatus = false;
            
            // запрашиваем конфигурационные таблицы
            while (!InitStatus)
            {
                if (TableUser == null)
                {
                    TableUser = getTgUser();
                }

                if (TableUser != null)
                    InitStatus = true;
                else
                    Thread.Sleep(3000);
            }

            while (true)
            {
                // обновляем данные в таблице истории
                try
                {
                    List<string> mes = new List<string>();
                    //формируем запрос для получения последних данных
                    string query = "";

                    for (int i = 0; i < reference.Count; i++) 
                    {
                        if (query.Length != 0) { query += " or "; }
                        query += @"Tag_name = '" + reference.ElementAt(i).Key + @"'";
                    }
                    //запрашиваем данные из таблицы истории
                    DataRow[] dataRows = ReadDB.History.Select(query);
                    DataRow[] selData = dataRows;
                    //if (selData.Length > 0)
                    //    Console.WriteLine(@"Ну наконец то бля...");
                    //проверяем наличие изменений
                    foreach (DataRow row in selData) 
                    {
                        //сравниваем показатели
                        bool his_trig = Convert.ToBoolean(row["Value"]);
                        if (his_trig ^ trig[reference[row["Tag_name"].ToString()]]) 
                        {
                            if (his_trig) 
                            {
                                //добавляем информационное сообщение для отправки
                                mes.Add(message[reference[row["Tag_name"].ToString()]]);
                            }
                            //обновляем значение в словаре
                            trig[reference[row["Tag_name"].ToString()]] = his_trig;
                        }
                    }
                    //инициализируем отправку сообщений
                    if (mes.Count > 0)
                    {
                        DataRow[] selUser = TableUser.Select(@"notifications = True");
                        foreach (DataRow user in selUser)
                        {
                            MarkupOff.OneTimeKeyboard = true;
                            botClient.SendTextMessageAsync(Convert.ToInt64(user["id"]), string.Join(", ", mes), replyMarkup: MarkupOff);
                        }
                    }

                    }
                catch (Exception ex) { Console.WriteLine("ERROR Ошибка при рабоде с БД: " + ex.Message); }
                Thread.Sleep(1000);
            }




        }

        public IReadOnlyList<Command> Commands => commandsList.AsReadOnly();


        private static ReplyKeyboardMarkup MarkupOn = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton("Температуры"),
                new KeyboardButton("Давления"),
                //new KeyboardButton("Оборудование"),
                new KeyboardButton("Подписаться на уведомления"),
            });
        private static ReplyKeyboardMarkup MarkupOff = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton("Температуры"),
                new KeyboardButton("Давления"),
                //new KeyboardButton("Оборудование"),
                new KeyboardButton("Отключить уведомления"),
            });

        public System.Int64 chatId = 979885439;

        public void GetBotClient()
        {
            if (botClient != null)
            {
                return;// botClient;
            }

            commandsList = new List<Command>();
            commandsList.Add(new StartCommand());
            //TODO: Add more commands

            //botClient = new TelegramBotClient("1313345463:AAGeek--uRsx-muZvc6KKzjIKq8gAC9o9mE");
            botClient = new TelegramBotClient(AppSettings.Key);
            botClient.OnMessage += BotClient_OnMessage;
            botClient.StartReceiving();
            // запуск уведомлений
            DBStream.Start();
        }

        private void BotClient_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            TgUser tgUser = new TgUser();
            chatId = e.Message.Chat.Id;
            //подписка на уведомления
            if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
            {
                noti = (e.Message.Text == "Подписаться на уведомления");
            }
            //первое чтение данных из ДБ
            if (TableUser == null) 
            {
                TableUser = getTgUser();
            }
            if (TableUser != null)
            {
                //проверка наличия пользователя в базе
                DataRow[] FoundRow = TableUser.Select("id = '" + e.Message.Chat.Id + "'");
                if (FoundRow.Length == 0)
                {
                    //данных в базе нет, записываем их
                    WriteUserDB(chatId, noti, e.Message.Chat.FirstName + " " + e.Message.Chat.LastName);
                    tgUser = new TgUser { id = chatId, Name = e.Message.Chat.FirstName + " " + e.Message.Chat.LastName, notifications = noti };
                }
                else
                {
                    //получаем данные из ДБ
                    tgUser = new TgUser { id = Convert.ToInt64(FoundRow[0][0]), Name = FoundRow[0][1].ToString(), notifications = FoundRow[0][2].ToString() == "True" };
                }

                //if (noti.ToString() != tgUser.notifications.ToString())
                if (e.Message.Text == "Подписаться на уведомления" || e.Message.Text == "Отключить уведомления")
                {
                    //данных в базе нет, записываем их
                    UpdateUserDB(chatId, noti);
                    tgUser.notifications = noti;
                }

            }

            ReplyKeyboardMarkup markup = new ReplyKeyboardMarkup();
            markup = tgUser.notifications ? MarkupOff : MarkupOn;
            markup.OneTimeKeyboard = true;

            if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
            {
                switch (e.Message.Text) 
                {
                    case "Температуры": 
                        {
                            botClient.SendTextMessageAsync(chatId, GetTemperature(), replyMarkup: markup);
                        }
                        break;
                    case "Давления":
                        {
                            botClient.SendTextMessageAsync(chatId, GetPressure(), replyMarkup: markup);
                        }
                        break;
                    case "Подписаться на уведомления":
                        {
                            botClient.SendTextMessageAsync(chatId, "Подписка прошла успешна", replyMarkup: markup);
                        }
                        break;
                    case "Отключить уведомления":
                        {
                            botClient.SendTextMessageAsync(chatId, "Уведомления отключены", replyMarkup: markup);
                        }
                        break;
                    default:
                        {
                            botClient.SendTextMessageAsync(chatId, "Команда не распознана", replyMarkup: markup);
                        }
                        break;
                }
            }

                //botClient.SendTextMessageAsync(chatId, "Hello => " + e.Message.Text, replyMarkup: markup);




            //    if (update == null) return Ok();

            //    var commands = Bot.Commands;
            //    var message = update.Message;
            //    var botClient = await Bot.GetBotClientAsync();

            //    foreach (var command in commands)
            //    {
            //        if (command.Contains(message))
            //        {
            //            await command.Execute(message, botClient);
            //            break;
            //        }
            //    }
            //    return Ok();
        }

        private string GetTemperature()
        {
            string result = "";

            if (ReadDB.History != null)
            {
                string query = @"Tag_name = '[q1]SHUK/AI/AI1/Out' or " +
                               @"Tag_name = '[q1]SHUK/AI/AI2/Out' or " +
                               @"Tag_name = '[q1]SHUK/AI/AI3/Out' or " +
                               @"Tag_name = '[q1]SHUK/AI/AI4/Out' or " +
                               @"Tag_name = '[q1]SHUK/AI/AI5/Out'";

                //запрашиваем данные из таблицы истории
                DataRow[] selData = ReadDB.History.Select(query);

                foreach (DataRow row in selData)
                {
                    switch (row["Tag_name"].ToString())
                    {
                        case @"[q1]SHUK/AI/AI1/Out":
                            {
                                result += result.Length > 0 ?
                                    Environment.NewLine + "Машзал 1: " + Convert.ToDouble(row["Value"]).ToString("#.#") + @"гр.С" :
                                    "Машзал 1: " + Convert.ToDouble(row["Value"]).ToString("#.#") + @"гр.С";
                            }
                            break;

                        case @"[q1]SHUK/AI/AI2/Out":
                            {
                                result += result.Length > 0 ?
                                    Environment.NewLine + "Машзал 2: " + Convert.ToDouble(row["Value"]).ToString("#.#") + @"гр.С" :
                                    "Машзал 2: " + Convert.ToDouble(row["Value"]).ToString("#.#") + @"гр.С";
                            }
                            break;
                        case @"[q1]SHUK/AI/AI3/Out":
                            {
                                result += result.Length > 0 ?
                                    Environment.NewLine + "Машзал 3: " + Convert.ToDouble(row["Value"]).ToString("#.#") + @"гр.С" :
                                    "Машзал 3: " + Convert.ToDouble(row["Value"]).ToString("#.#") + @"гр.С";
                            }
                            break;
                        case @"[q1]SHUK/AI/AI4/Out":
                            {
                                result += result.Length > 0 ?
                                    Environment.NewLine + "Машзал 4: " + Convert.ToDouble(row["Value"]).ToString("#.#") + @"гр.С" :
                                    "Машзал 4: " + Convert.ToDouble(row["Value"]).ToString("#.#") + @"гр.С";
                            }
                            break;
                        case @"[q1]SHUK/AI/AI5/Out":
                            {
                                result += result.Length > 0 ?
                                    Environment.NewLine + "Эл.щит.: " + Convert.ToDouble(row["Value"]).ToString("#.#") + @"гр.С" :
                                    "Эл.щит.: " + Convert.ToDouble(row["Value"]).ToString("#.#") + @"гр.С";
                            }
                            break;
                    }

                }

            }
            else 
            {
                result = "Ошибка получения данных от сервера";
            }
            return result;
        }

        private string GetPressure()
        {
            string result = "";

            if (ReadDB.History != null) {
                string query = @"Tag_name = '[q1]SHUKH/AI/AI7/Out' or " +
                               @"Tag_name = '[q1]SHUKH/AI/AI11/Out'";

                //запрашиваем данные из таблицы истории
                DataRow[] selData = ReadDB.History.Select(query);

                foreach (DataRow row in selData)
                {
                    switch (row["Tag_name"].ToString())
                    {
                        case "[q1]SHUKH/AI/AI7/Out":
                            {
                                result += result.Length > 0 ?
                                    Environment.NewLine + "На вх. насосов: " + Convert.ToDouble(row["Value"]).ToString("#.#") + @"кПа" :
                                    "На вх. насосов: " + Convert.ToDouble(row["Value"]).ToString("#.#") + @"кПа";
                            }
                            break;

                        case "[q1]SHUKH/AI/AI11/Out":
                            {
                                result += result.Length > 0 ?
                                    Environment.NewLine + "На вых. насосов: " + Convert.ToDouble(row["Value"]).ToString("#.#") + @"кПа" :
                                    "На вых. насосов: " + Convert.ToDouble(row["Value"]).ToString("#.#") + @"кПа";
                            }
                            break;
                    }

                }
            }
            else
            {
                result = "Ошибка получения данных от сервера";
            }
            return result;
        }

        private void UpdateUserDB(long chatId, bool noti)
        {
            try
            {
                string sConnect = "Data Source=" + COD.Properties.Resources.DB_Node + "; Initial Catalog = " + COD.Properties.Resources.DB_Catalog + "; Password = " + COD.Properties.Resources.DB_Pwd + "; User ID = " + COD.Properties.Resources.DB_User + ";";
                //string sConnect = "Data Source=127.0.0.1; Initial Catalog = Site; Password = sa; User ID = sa;";
                string sqlQuery = @"UPDATE [dbo].[Telegram] SET [notifications] = '"+ noti + @"' WHERE [id] = " + chatId + @" ";

                SqlConnection con = new SqlConnection(sConnect);
                SqlCommand comm = new SqlCommand(sqlQuery, con);
                try
                {
                    con.Open();
                    comm.ExecuteNonQuery();

                    con.Close();

                    DataRow row = TableUser.Select("id = "+ chatId).FirstOrDefault();

                    row["notifications"] = noti.ToString();
                }
                catch (Exception ex) { Console.WriteLine("ERROR Ошибка при рабоде с БД: " + ex.Message); }
            }
            catch (Exception ex) { Console.WriteLine("ERROR Ошибка при рабоде с БД: " + ex.Message); }
        }

        private void WriteUserDB(long id, bool noti, string v)
        {
            try
            {
                string sConnect = "Data Source=" + COD.Properties.Resources.DB_Node + "; Initial Catalog = " + COD.Properties.Resources.DB_Catalog + "; Password = " + COD.Properties.Resources.DB_Pwd + "; User ID = " + COD.Properties.Resources.DB_User + ";";
                //string sConnect = "Data Source=127.0.0.1; Initial Catalog = Site; Password = sa; User ID = sa;";
                string sqlQuery = @"INSERT INTO [dbo].[Telegram]([id],[name],[notifications])VALUES(" + id + @",'"+v+@"','"+noti+@"')";

                SqlConnection con = new SqlConnection(sConnect);
                SqlCommand comm = new SqlCommand(sqlQuery, con);
                try
                {
                    con.Open();
                    comm.ExecuteNonQuery();

                    con.Close();

                   DataRow dataRow =  TableUser.NewRow();
                    dataRow["id"] = id;
                    dataRow["name"] = v;
                    dataRow["notifications"] = noti.ToString();
                    TableUser.Rows.Add(dataRow);
                }
                catch (Exception ex) { Console.WriteLine("ERROR Ошибка при рабоде с БД: " + ex.Message); }
            }
            catch (Exception ex) { Console.WriteLine("ERROR Ошибка при рабоде с БД: " + ex.Message); }
        }

        private static DataTable getTgUser() 
        {
            DataTable result = null;

            try
            {
                string sConnect = "Data Source=" + COD.Properties.Resources.DB_Node + "; Initial Catalog = " + COD.Properties.Resources.DB_Catalog + "; Password = " + COD.Properties.Resources.DB_Pwd + "; User ID = " + COD.Properties.Resources.DB_User + ";";
                //string sConnect = "Data Source=127.0.0.1; Initial Catalog = Site; Password = sa; User ID = sa;";
                string sqlQuery = @"SELECT  [id],[name],[notifications] FROM [dbo].[Telegram]";

                SqlConnection con = new SqlConnection(sConnect);
                SqlCommand comm = new SqlCommand(sqlQuery, con);
                try
                {
                    con.Open();
                    SqlDataAdapter da = new SqlDataAdapter(comm);
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    result = ds.Tables[0];

                    con.Close();
                }
                catch (Exception ex) { Console.WriteLine("ERROR Ошибка при рабоде с БД: " + ex.Message); }
            }
            catch (Exception ex) { Console.WriteLine("ERROR Ошибка при рабоде с БД: " + ex.Message); }

            return result;
        }

    }
}
