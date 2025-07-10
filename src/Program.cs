using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Polling;
using Telegram.Bot.Exceptions;

namespace FNSBot
{
    class Program
    {
    private static ITelegramBotClient _botClient;
    

    private static ReceiverOptions _receiverOptions;
    // API ключи
    public static string TG_BOT_API_KEY = Environment.GetEnvironmentVariable("TG_BOT_API_KEY");
    public static string FNS_API_KEY    = Environment.GetEnvironmentVariable("FNS_API_KEY");
    

        // Запуск бота
        static async Task Main()
        {
            
            _botClient = new TelegramBotClient(TG_BOT_API_KEY); 
            _receiverOptions = new ReceiverOptions 
            {
                AllowedUpdates = new[]
                {
                    UpdateType.Message, 
                },
                DropPendingUpdates = true, 
            };
            
            using var cts = new CancellationTokenSource();
            
            _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token); 
            
            var me = await _botClient.GetMe(); 
            Console.WriteLine($"{me.FirstName} запущен");
            
            await Task.Delay(-1);
        }

        static string lastMsg = "Выполните операцию";
        // Основная логика бота
        private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                    {
                        switch (update.Message.Text)
                        {
                            case "/start":
                            {
                                string start_msg = $"Я выдаю адрес и наименованиее компаний по ИНН. Используйте \"/inn [инн компаний/ип через пробел]\"\n" 
                                                   +"Или используйте \"/help\" для полного списка команд";
                                lastMsg = start_msg;
                                await botClient.SendMessage(update.Message.Chat, start_msg);

                                return;
                            }
                            case "/help":
                            {
                                string helpMsg = 
                                "/start – начать общение с ботом.\n"
                                +"/help – вывести справку о доступных командах.\n"
                                +"/hello – вывод справки о разработчике.\n"
                                +"/inn – получить наименования и адреса компаний по ИНН (/inn [инн компаний/ип через пробел]).\n"
                                +"/last – повторить последнее действие бота.";

                                lastMsg = helpMsg;
                                await botClient.SendMessage(update.Message.Chat, helpMsg);
                                return;
                            }
                            case "/hello":
                            {
                                string helloMsg = 
                                "Разработал Макаров Владимир\n"
                                +"email - mve45@mail.ru\n"
                                +"github проекта - https://github.com/drpdrpdrp/CRMPark_FNSAgent_bot\n"
                                +"резюме hh.ru - https://hh.ru/resume/1fbd22afff0f0c5b330039ed1f7a56385a7436";

                                lastMsg = helloMsg;
                                await botClient.SendMessage(update.Message.Chat, helloMsg);
                                return;

                            }
                            case "/inn":
                            {
                                string innMsg = $"Формат: \"/inn [инн компаний/ип через пробел]";
                                lastMsg = innMsg;
                                await botClient.SendMessage(update.Message.Chat, innMsg);
                                return;
                            }
                            case "/last":
                            {
                                await botClient.SendMessage(update.Message.Chat, lastMsg);
                                return;
                            }
                            default:
                            {
                                if (update.Message.Text.Substring(0,5) == "/inn ")
                                {
                                    string inn = update.Message.Text.Substring(5);
                                    string dataMsg = "";
                                    var agentsList = GetData.Run(inn);
                                    if (agentsList == null)
                                    {
                                        dataMsg = "Неккоректный ИНН";
                                    }
                                    else
                                    {
                                        foreach (var item in agentsList)
                                        {
                                            dataMsg += $"Название: {item.Item1}\nИНН: {item.Item2}\nАдрес: {item.Item3}\n\n";
                                        }
                                    }

                                    await botClient.SendMessage(update.Message.Chat, dataMsg);

                                }
                                else
                                {
                                    string wrongMsg = "Неверный формат, введите /help";
                                    lastMsg = wrongMsg;
                                    await botClient.SendMessage(update.Message.Chat, wrongMsg);
                                }

                                return;
                            }
                        }

                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
            {
                var ErrorMessage = error switch
                {
                    ApiRequestException apiRequestException
                        => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                    _ => error.ToString()
                };

                Console.WriteLine(ErrorMessage);
                return Task.CompletedTask;
            }
    }
}