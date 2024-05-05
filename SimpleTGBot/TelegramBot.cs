using System.Reflection.Metadata.Ecma335;

namespace SimpleTGBot;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class TelegramBot
{
    
    private const string BotToken = "7124254960:AAGz4WZrvt5yQuR640QcD8Vk3UvXyYrLoRs";
    
    public async Task Run()
    {
        static string filter = "";
        static Random r = new Random();
        var botClient = new TelegramBotClient(BotToken);
        
        using CancellationTokenSource cts = new CancellationTokenSource();

        ReceiverOptions receiverOptions = new ReceiverOptions()
        {
            AllowedUpdates = new[] { };
        };

        botClient.StartReceiving(
            updateHandler: OnMessageReceived,
            pollingErrorHandler: OnErrorOccured,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

        var me = await botClient.GetMeAsync(cancellationToken: cts.Token);
        Console.WriteLine($"Бот @{me.Username} запущен.\nДля остановки нажмите клавишу Esc...");
        while (Console.ReadKey().Key != ConsoleKey.Escape){}
        cts.Cancel();
    }
    
    /// <summary>
    /// Обработчик события получения сообщения.
    /// </summary>
    /// <param name="botClient">Клиент, который получил сообщение</param>
    /// <param name="update">Событие, произошедшее в чате. Новое сообщение, голос в опросе, исключение из чата и т. д.</param>
    /// <param name="cancellationToken">Служебный токен для работы с многопоточностью</param>
    async Task OnMessageReceived(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
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
                                            InlineKeyboardButton.WithCallbackData("Не сейчас", "Не сейчас"),
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
    }

    /// <summary>
    /// Обработчик исключений, возникших при работе бота
    /// </summary>
    /// <param name="botClient">Клиент, для которого возникло исключение</param>
    /// <param name="exception">Возникшее исключение</param>
    /// <param name="cancellationToken">Служебный токен для работы с многопоточностью</param>
    /// <returns></returns>
    Task OnErrorOccured(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        // В зависимости от типа исключения печатаем различные сообщения об ошибке
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        
        // Завершаем работу
        return Task.CompletedTask;
    }
}