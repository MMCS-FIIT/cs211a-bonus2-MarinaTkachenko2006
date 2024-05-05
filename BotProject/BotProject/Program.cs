using System;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotProject
{
    public class Program 
    { 
        static async Task Main(string[] args)
        {
            Bot telegramBot = new Bot();
            await telegramBot.Run();
        }
  
    }
}