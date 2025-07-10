using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace FNSBot
{
    public class GetData
    {
        // Логика обработки json, возвращенного через API FNS
        public static List<(string, string, string)> Run(string inn)
        {
            // Проверка строки 
            if (Regex.IsMatch(inn, @"^(?:\d{10}|\d{12})(?: (?:\d{10}|\d{12}))*$"))
            {
                inn = inn.Replace(" ", ",");
            }
            else
            {
                return null;
            }

            // Получение json
            var request = new GetRequest($"https://api-fns.ru/api/multinfo?req={inn}&key={Program.FNS_API_KEY}");
            request.Run();
            var response = request.Response;



            var root = JObject.Parse(response);
            var agents = root["items"];

            if (!agents.HasValues)
                return null;

            // Парсинг json
            List<(string, string, string)> fnsData = new List<(string, string, string)>();
            foreach (var agent in agents)
            {   

                string fullName = null;
                string fullAddress = null;
                string agentInn = null;
                if (agent["ЮЛ"] != null)
                {
                    fullName = agent["ЮЛ"]["НаимПолнЮЛ"].ToString();
                    agentInn = agent["ЮЛ"]["ИНН"].ToString();
                    if (agent["ЮЛ"]["Адрес"] == null || 
                        agent["ЮЛ"]["Адрес"].Type == JTokenType.Null || 
                        agent["ЮЛ"]["Адрес"]["АдресПолн"] == null)
                    {
                        fullAddress = "Адрес не указан";
                    }
                    else
                    {
                        fullAddress = agent["ЮЛ"]["Адрес"]["АдресПолн"].ToString();
                    }
                }
                else if (agent["ИП"] != null)
                {
                    fullName = "ИП " + agent["ИП"]["ФИОПолн"].ToString();
                    agentInn = agent["ИП"]["ИННФЛ"].ToString();
                    if (agent["ИП"]["Адрес"] == null || 
                        agent["ИП"]["Адрес"].Type == JTokenType.Null || 
                        agent["ИП"]["Адрес"]["АдресПолн"] == null)
                    {
                        fullAddress = "Адрес не указан";
                    }             
                    else
                    {
                        fullAddress = agent["ИП"]["Адрес"]["АдресПолн"].ToString();
                    }

                    
                }
                else
                {
                    throw new Exception("После запроса были получены некоректные данные.");
                }

                fnsData.Add((fullName,
                             agentInn,
                             fullAddress));
            }

            return fnsData.OrderBy(x => x.Item1).ToList();
        }
    }
}