using System.Net;

namespace FNSBot
{
    public class GetRequest
    {
        HttpWebRequest _request;
        private string _address;

        public string Response { get; set;}

        public GetRequest(string address)
        {
            _address = address;
        }

        public void Run()
        {
            try
            {
                _request = (HttpWebRequest)WebRequest.Create(_address);
                _request.Method = "Get";

                HttpWebResponse response = (HttpWebResponse)_request.GetResponse();
                var stream = response.GetResponseStream();
                if (stream != null)
                    Response = new StreamReader(stream).ReadToEnd();
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);                
            }
        }
    }
}