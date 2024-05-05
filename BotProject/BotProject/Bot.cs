using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace BotProject
{
    public class Bot
    {
        private const string BotToken = "6709752938:AAHUc1k5Je77f3sbJ_oR-ni5koaRU6L8cYI";
        static string filter = "";
        static Random r = new Random();
        public async Task Run()
        {
            var botClient = new TelegramBotClient(BotToken);

            using CancellationTokenSource cts = new CancellationTokenSource();

            ReceiverOptions receiverOptions = new ReceiverOptions()
            {
                AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery }, 
            };


            botClient.StartReceiving(
                updateHandler: OnUpdateReceived,
                pollingErrorHandler: OnErrorOccured,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

        var me = await botClient.GetMeAsync(cancellationToken: cts.Token);
        Console.WriteLine($"Бот @{me.Username} запущен.\nДля остановки нажмите клавишу Esc...");
            Console.ReadLine();
        cts.Cancel();
    }
        public static async Task OnUpdateReceived(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            if (update.Type == UpdateType.Message)
            {
                var message = update.Message;
                if (message.Text.ToLower() == "/start")
                {
                    var ans1 = new string[] { "Да", "Начнем", "Хорошо" };
                    var ans2 = new string[] { "Не сейчас", "Позже", "Потом" };
                    await botClient.SendTextMessageAsync(message.Chat, "Привет! Я помогу Вам подобрать интересные места для посещения в России.", cancellationToken: cancellationToken);
                    var inlineKeyboard = new InlineKeyboardMarkup(
                        new List<InlineKeyboardButton[]>()
                        {
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData($"{ans1[r.Next(0,2)]}", "Да"),
                                            InlineKeyboardButton.WithCallbackData($"{ans2[r.Next(0,2)]}", "Нет"),
                                        },
                        });
                    await botClient.SendTextMessageAsync(
                        message.Chat.Id,
                        "Начнем? Вы всегда можете воспользоваться упрощенным поиском, сразу введя название нтересующего Вас региона, в этом случае будет использоваться последний установленный фильтр по типу достопримечательностей, либо фильтрация будет отсутствовать, в случае, если данный запрос является первым.",
                        replyMarkup: inlineKeyboard);
                    return;
                }

                else
                {

                    string msg = message.Text.ToString();
                    if (msg.StartsWith("/newsight"))
                    {
                        msg = msg.Remove(0, 10);
                        using var fs = new FileStream("../../../photos.txt", FileMode.Append);
                        using var sw = new StreamWriter(fs);
                        sw.WriteLine(msg);
                    }
                    else
                    {
                        string url = "";
                        using (var fs = new FileStream("../../../photos.txt", FileMode.Open))
                        using (var sr = new StreamReader(fs))
                        {
                            while (!sr.EndOfStream)
                            {
                                var l = sr.ReadLine().Split("|");
                                if (l[0] == msg && (filter == l[3] || filter == ""))
                                {
                                    url = l[1];
                                    await botClient.SendPhotoAsync(
                                        chatId: message.Chat.Id,
                                        photo: InputFile.FromUri(url),
                                        caption: $"<a href=\"{l[1]}\">{l[2]}</a>",
                                        parseMode: ParseMode.Html,
                                        cancellationToken: cancellationToken);
                                }
                            }
                        }
                        if (url == "") await botClient.SendTextMessageAsync(message.Chat.Id, "Нет подходящего места. Возможно, Вы неверно указали название региона, либо в базе данных нет мест, удовлетворяющих запросу.");
                        var inlineKeyboard = new InlineKeyboardMarkup(
                        new List<InlineKeyboardButton[]>()
                        {
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Добавить достопримечательности", "Добавить достопримечательности"),
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Не сейчас", "Нет"),
                                        },
                        });
                        await botClient.SendTextMessageAsync(
                            message.Chat.Id,
                            "Не нашли подходящего места? Вы можете пополнить нашу базу данных.",
                            replyMarkup: inlineKeyboard);
                        return;
                    }


                }

            }
            if (update.Type == UpdateType.CallbackQuery)
            {
                var callbackQuery = update.CallbackQuery;
                var chat = callbackQuery.Message.Chat;
                switch (callbackQuery.Data)
                {
                    case "Да":
                        {
                            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: cancellationToken);

                            var inlineKeyboard = new InlineKeyboardMarkup(
                            new List<InlineKeyboardButton[]>()
                            {
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Музей", "Музей"),
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Природный объект", "Природный объект"),
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Все равно", "Все равно"),
                                        },
                            });
                            await botClient.SendTextMessageAsync(chat.Id, "Какой тип достопримечательностей Вас интересует?", replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
                            return;
                        }

                    case "Нет":
                        {
                            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: cancellationToken);

                            await botClient.SendTextMessageAsync(
                                chat.Id,
                                "Как скажете", cancellationToken: cancellationToken);
                            return;
                        }
                    case "Добавить достопримечательности":
                        {
                            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: cancellationToken);
                            await botClient.SendTextMessageAsync(chat.Id, "Введите ответ в следующем виде:", cancellationToken: cancellationToken);
                            await botClient.SendTextMessageAsync(chat.Id, "/newsight|название региона|ссылка на фотографию|название достопримечательности|тип(Музей/Природный объект)", cancellationToken: cancellationToken);
                            return;
                        }
                    case "Музей":
                        {
                            filter = "Музей";
                            await botClient.SendTextMessageAsync(chat.Id, "Какой регион Вас интересует?", cancellationToken: cancellationToken);
                            return;
                        }
                    case "Природный объект":
                        {
                            filter = "Природный объект";
                            await botClient.SendTextMessageAsync(chat.Id, "Какой регион Вас интересует?", cancellationToken: cancellationToken);
                            return;
                        }
                    case "Все равно":
                        {
                            filter = "Природный объект";
                            await botClient.SendTextMessageAsync(chat.Id, "Какой регион Вас интересует?", cancellationToken: cancellationToken);
                            return;
                        }
                }
                return;
            }
        }

        public static async Task OnErrorOccured(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }
    }
}
