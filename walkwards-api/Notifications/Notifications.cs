using System.Text;
using Newtonsoft.Json;

namespace walkwards_api.Notifications
{
    public class Notifications
    {
        private readonly string _appId;
        private readonly string _apiUrl;
        private static readonly HttpClient Client = new HttpClient();
        
        public Notifications(string appId, string apiKey, string apiUrl)
        {
            _appId = appId;
            _apiUrl = apiUrl;
            
            if(Client.DefaultRequestHeaders.Authorization == null)
            {
                Client.DefaultRequestHeaders.Add("Authorization", "Basic " + apiKey);
            }
        }

        private async Task<object[]> POSTData(object json, string url)
        {
            try
            {
                string ajson = JsonConvert.SerializeObject(json).Replace(@"\", "");
                StringContent content = new (ajson, Encoding.UTF8, "application/json");

                HttpResponseMessage result = await Client.PostAsync(url, content);
                string response = await result.Content.ReadAsStringAsync();
                return new object[] { true, response };
            }
            catch (Exception e)
            {
                string response = e.Message;
                return new object[] { false, response };
            }
        }

        public async Task<object?> SendToSpecificDevices(string title, string message, string appUrl, string[] ids)
        {

            var success = await POSTData(new {
                app_id = _appId,
                include_external_user_ids = ids,
                channel_for_external_user_ids = "push",
                contents = new { en = message },
                headings = new { en = title },
                app_url = appUrl
            }, _apiUrl);
            
            return (bool) success[0] ? success[1] : null;
        }
        
    }
}