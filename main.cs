using System;
using System.Collections.Generic;
using System.IO;
using Telegram.Bot;
using System.Drawing;
using Telegram.Bot.Types.InputFiles;

namespace BotTelegramGame
{
    class Program
    {
        static void Main(string[] args)
        {
            string token = ""; // <----token.
            TelegramBotClient bot = new TelegramBotClient(token);
            Console.WriteLine($"@{bot.GetMeAsync().Result.Username} start");

            int max = 5; //maksimums kādu ciparu lietotājs var ievadīt.
            Random rand = new Random();
            Dictionary<long, int> db = new Dictionary<long, int>(); // long unikals id, katram lietotajam savs | int ir skaitlis ko lietotājs ievada.

            bot.OnMessage += (s, arg) =>
            {
                #region var

                string msgText = arg.Message.Text;
                string firstName = arg.Message.Chat.FirstName;
                string replyMsg = String.Empty;
                int msgId = arg.Message.MessageId;
                long chatId = arg.Message.Chat.Id;

                int user = 0;
                string path = $"id_{chatId.ToString().Substring(0, 5).Substring(0, 5)}";
                bool skip = false;

                Console.WriteLine($"{firstName}: {msgText}"); // atbild userim ar viņa vārdu...

                #endregion //visi mainīgie.

                if (!db.ContainsKey(chatId)
                || msgText == "/restart"
                || msgText.StartsWith("start")
                || msgText.ToLower().IndexOf("start") != -1)
                {
                    int startGame = rand.Next(20, 50); //ģenerē random skaitļus, tie kas ir sākumā.
                    db[chatId] = startGame;
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    skip = true;
                    replyMsg = $"ЗАГАДАНО ЧИСЛО: {db[chatId]}";
                }
                else
                {
                    if (db[chatId] <= 0) return;

                    int.TryParse(msgText, out user);
                    if (!(user >= 1 && user <= max))
                    {
                        skip = true;
                        replyMsg = $"обноружено читерство. Число: {db[chatId]}";
                    }
                    if (!skip)
                    {
                        db[chatId] -= user;

                        replyMsg = $"Ход {firstName}: {user}. Число: {db[chatId]}";
                        if (db[chatId] <= 0)
                        {
                            replyMsg = $"Ура! Победа, {firstName}!";
                            skip = true;
                        }
                    }
                }

                if (!skip)
                {
                    int temp = rand.Next(max) + 1; // 1 2 3 4 5

                    db[chatId] -= temp;
                    replyMsg += $"\nХод БОТА: {temp} Число: {db[chatId]}";
                    if (db[chatId] <= 0) replyMsg = $"Ура! Победа БОТА!";
                }

                Bitmap image = new Bitmap(400, 400); // izveido attēlu, ievadot to izķirtspēju.
                Graphics graphics = Graphics.FromImage(image); 

                graphics.DrawString(
                    s: replyMsg, // string: replyMsg.
                    font: new Font("Consolas", 16), // Nosaka rakstīšanas stilu un burtu izmēru.
                    brush: Brushes.Black, // teksta krāsa.
                    x: 10, //atrašanās vieta.
                    y: 200);

                path += $@"\file_{DateTime.Now.Ticks}.bmp"; // saglabāšanas vieta.
                image.Save(path); // saglabā.


                Console.WriteLine($" >>> {replyMsg}"); // ievada man koncolē ko BOTS atbild userim.

                bot.SendPhotoAsync(
                    chatId: chatId,
                    caption: "Правило - загадывай число от 1 до 5 и попробуй победить БОТА, кто первый дойдёт до 0, тот Победил",

                    photo: new InputOnlineFile(new FileStream(path, FileMode.Open)),

                    replyToMessageId: msgId
                    );
            };
            bot.StartReceiving();

            Console.ReadLine();
        }
    }
}
