using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace walkwards_api.Notifications
{
    public class Devices
    {
        private string APP_ID;
        private string API_KEY;
        private string API_URL;
        
        public Devices(string appId, string apiKey, string apiUrl)
        {
            APP_ID = appId;
            API_KEY = apiKey;
            API_URL = apiUrl;
        }
        
        public async Task<JObject> GetDevices(int offset)
        {
            string url = API_URL + "?app_id=" + APP_ID + "&limit=300" + "&offset=" + offset;
            
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Basic " + API_KEY);
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            return JObject.Parse(content);
        }
    }
}