using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BitMinistry.OpenAi
{
    public class ChatClient
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly string _model;

        public string BaseUrl { get; private set; }

        public ChatClient(string apiKey, string baseUrl, string model)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API key is required.", nameof(apiKey));
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("Base URL is required.", nameof(baseUrl));
            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Model name is required.", nameof(model));

            _http = new HttpClient(LocalNetwork.CreateHandler())
            {
                Timeout = TimeSpan.FromMinutes(5)
            };

            _apiKey = apiKey;
            BaseUrl = baseUrl.TrimEnd('/');
            _model = model;
        }

        public async Task<string> AskAsync(IEnumerable<ChatMessage> messages, double temperature = 0.2)
        {
            var payload = new
            {
                model = _model,
                temperature,
                messages = messages.Select(m => new { role = m.Role, content = m.Content })
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _http.DefaultRequestHeaders.Add("User-Agent", "BitMinistry-Client/1.0");
            _http.DefaultRequestHeaders.Add("Accept", "application/json");

            var response = await _http.PostAsync($"{BaseUrl}/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();

            string raw;
            using (var doc = JsonDocument.Parse(responseJson))
            {
                raw = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? string.Empty;
            }

            if (raw.StartsWith("```"))
            {
                int first = raw.IndexOf('\n') + 1;
                int last = raw.LastIndexOf("```");
                if (first > 0 && last > first)
                    raw = raw.Substring(first, last - first);
            }

            if (raw.StartsWith("\"") && raw.EndsWith("\""))
            {
                try { raw = JsonSerializer.Deserialize<string>(raw) ?? raw; } catch { }
            }

            return raw.Trim();
        }

        public async Task<string> Ask(string command, string data)
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(command),
                new UserChatMessage(data)
            };

            return await AskAsync(messages);
        }
    }
}
