using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace TapasApi
{
    public class Tapas
    {
        private string deviceId;
        private string authToken;
        private readonly HttpClient httpClient;
        private readonly string apiUrl = "https://api.tapas.io";
        private readonly string storyApiUrl = "https://story-api.tapas.io/cosmos/api/v1";
        public Tapas()
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };
            httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("okhttp/4.9.2 (Android 9; Device SM-N9860; Build 32220; App 7.11.1)");
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            deviceId = GenerateDeviceId();
            httpClient.DefaultRequestHeaders.Add("X-Device-Type", "ANDROID");
            httpClient.DefaultRequestHeaders.Add("X-Device-Uuid", deviceId);
            httpClient.DefaultRequestHeaders.Add("X-LANG-CODE", "ru");
            httpClient.DefaultRequestHeaders.Add("X-Offset-Time", "180");
        }

        private static string GenerateDeviceId() => Guid.NewGuid().ToString("N")[..16];

        public async Task<string> Register(
            string email, string password, string birthDate = "2001-09-11")
        {
            var data = JsonContent.Create(new
            {
                email = email,
                password = password,
                birth_date = birthDate
            });
            var response = await httpClient.PostAsync($"{apiUrl}/auth/join", data);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string?> Login(string email, string password)
        {
            var data = JsonContent.Create(new
            {
                email = email,
                password = password,
                offset_time = 180
            });
            var response = await httpClient.PostAsync($"{apiUrl}/auth/login", data);
            if (response.StatusCode == System.Net.HttpStatusCode.Found &&
                response.Headers.Location != null)
            {
                var location = response.Headers.Location.ToString();
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/panda+json"));
                var authResponse = await httpClient.GetAsync(location);
                var content = await authResponse.Content.ReadAsStringAsync();
                var document = JsonDocument.Parse(content);
                if (document.RootElement.TryGetProperty("auth_token", out var tokenElement))
                {
                    authToken = tokenElement.GetString();
                    httpClient.DefaultRequestHeaders.Add("X-Auth-Token", authToken);
                }
                return content;
            }
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetAccountCoins()
        {
            var response = await httpClient.GetAsync($"{apiUrl}/v3/user/coins");
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetFeed(string genreType, string categoryType, int page = 0, int size = 25)
        {
            var response = await httpClient.GetAsync($"{storyApiUrl}/landing/ranking?genre_type={genreType}&category_type={categoryType}&page={page}&size={size}");
            return await response.Content.ReadAsStringAsync();
        }
    }
}
