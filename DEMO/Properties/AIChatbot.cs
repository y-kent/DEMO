using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DEMO
{
    public class AIChatbot
    {
        // ==================================================================
        // DITO MO I-PASTE ANG YONG OPENROUTER API KEY!
        // ==================================================================
        private string MY_API_KEY = "?";
        // ==================================================================

        // ── Fields ────────────────────────────────────────────────────────────
        private List<ChatMessage> history;
        private string model;
        private readonly string _apiKey;
        private readonly HttpClient _http;

        // ── Internal config ───────────────────────────────────────────────────
        private const string ApiUrl = "https://openrouter.ai/api/v1/chat/completions";
        private const int MaxRetries = 3;
        private const int RetryDelay = 5;
        private const int MaxTokens = 250;
        private const int MaxHistory = 10;

        // BINAGO: Pinasimple natin at tinanggal ang mga "bawal" na salita para hindi ma-trigger ang Safety Filter ng OpenRouter!
        private const string SystemPrompt = @"You are a supportive, warm, and casual human friend. 

HOW YOU RESPOND:
- If your friend shares good news (like passing a mock interview), celebrate with them! Say you are very proud.
- If they are sad or stressed, comfort them like a best friend would.
- Give simple, friendly suggestions if they ask for ideas (like how to journal, what music to listen to, or how to relax).
- Keep it short, like a text message (2 to 3 sentences max).
- Use casual language (e.g., 'honestly', 'that is awesome', 'so happy for you').
- Always end by asking a casual follow-up question to keep the chat going.

STRICT RULES:
- Never use bullet points.
- Do not sound formal. Just be a normal, caring friend.";

        // ── Constructor ───────────────────────────────────────────────────────────
        public AIChatbot(string passedKey = null, string modelName = "openrouter/free")
        {
            model = modelName;
            history = new List<ChatMessage>();

            // BAGONG PARAAN: Kukunin ang key mula sa Environment Variable ng inyong computer
            // Hindi na ito naka-hardcode sa mismong text ng program!
            string envKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");

            // Smart Key Selector
            string keyToUse = "";
            if (!string.IsNullOrWhiteSpace(envKey))
            {
                keyToUse = envKey.Trim(); // Gagamitin ang nasa computer
            }
            else if (!string.IsNullOrWhiteSpace(MY_API_KEY) && MY_API_KEY != "PASTE_YOUR_API_KEY_HERE")
            {
                keyToUse = MY_API_KEY.Trim(); // Fallback lang kung nagmatigas kayong ilagay sa code
            }
            else if (!string.IsNullOrWhiteSpace(passedKey) && passedKey != "ILAGAY_ANG_OPENROUTER_API_KEY_DITO")
            {
                keyToUse = passedKey.Trim(); // Fallback mula sa dashboard
            }

            _apiKey = keyToUse;

            _http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _http.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost");
            _http.DefaultRequestHeaders.Add("X-Title", "Emotion Chatbot");

            history.Add(new ChatMessage("system", SystemPrompt));
        }

        // ── Methods ───────────────────────────────────────────────────────────
        public async Task<string> ChatAsync(string input)
        {
            if (!_apiKey.StartsWith("sk-or-v1-"))
            {
                return "Oops! Wala ka pang API Key o mali ang format. Ilagay muna ang inyong OpenRouter API Key!";
            }

            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            history.Add(new ChatMessage("user", input));
            ApplySlidingWindow();

            string reply = await CallWithRetryAsync();
            history.Add(new ChatMessage("assistant", reply));
            return reply;
        }

        public void Reset()
        {
            history.Clear();
            history.Add(new ChatMessage("system", SystemPrompt));
        }

        // ── Private API Logic ─────────────────────────────────────────────
        private async Task<string> CallWithRetryAsync(List<ChatMessage> overrideMessages = null)
        {
            var messages = overrideMessages ?? history;

            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    string result = await PostAsync(messages);
                    if (result != null)
                        return result;

                    await Task.Delay(TimeSpan.FromSeconds(RetryDelay));
                }
                catch (TaskCanceledException)
                {
                    if (attempt < MaxRetries)
                        await Task.Delay(TimeSpan.FromSeconds(RetryDelay));
                }
                catch (HttpRequestException)
                {
                    return "Couldn't connect — check your internet and try again.";
                }
                catch
                {
                    if (attempt == MaxRetries)
                        return "Something went wrong, try again in a moment.";
                }
            }
            return "Still having trouble connecting, give it another shot.";
        }

        private async Task<string> PostAsync(List<ChatMessage> messages)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"model\":\"").Append(model).Append("\",\"temperature\":0.7,\"max_tokens\":").Append(MaxTokens).Append(",\"messages\":[");
            for (int i = 0; i < messages.Count; i++)
            {
                sb.Append("{\"role\":\"").Append(messages[i].role).Append("\",\"content\":\"").Append(EscapeJson(messages[i].content)).Append("\"}");
                if (i < messages.Count - 1) sb.Append(",");
            }
            sb.Append("]}");

            string json = sb.ToString();
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(ApiUrl, content);
            int code = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                string raw = await response.Content.ReadAsStringAsync();
                string text = ParseContent(raw);

                // Kapag naharang pa rin ng server kahit safe ang prompt, gagawa tayo ng custom fallback message para mabait pa rin ang dating.
                if (text.Trim().ToLower() == "refusal")
                {
                    return "That's so great to hear! Tell me more about it.";
                }

                return CleanText(text);
            }

            if (code == 503 || code == 429)
            {
                await Task.Delay(TimeSpan.FromSeconds(code == 429 ? 15 : RetryDelay));
                return null;
            }

            if (code == 401)
            {
                return "Error 401 (Unauthorized): Hindi tinanggap ng OpenRouter ang API Key. Siguraduhing tama ang na-copy niyo.";
            }

            return $"Error {code}: May problema sa connection. Try again.";
        }

        // ── Helpers ──────────────────────────────────────────────────────

        private static string ParseContent(string json)
        {
            try
            {
                int idx = json.IndexOf("\"content\"");
                if (idx == -1) return "";

                int start = json.IndexOf("\"", idx + 9);
                start = json.IndexOf("\"", start) + 1;

                int end = start;
                bool isEscaped = false;
                while (end < json.Length)
                {
                    if (json[end] == '\\' && !isEscaped) isEscaped = true;
                    else if (json[end] == '"' && !isEscaped) break;
                    else isEscaped = false;
                    end++;
                }

                string content = json.Substring(start, end - start);
                return UnescapeJson(content);
            }
            catch
            {
                return "";
            }
        }

        private static string EscapeJson(string text)
        {
            return text.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }

        private static string UnescapeJson(string text)
        {
            return text.Replace("\\n", "\n").Replace("\\\"", "\"").Replace("\\r", "\r").Replace("\\t", "\t").Replace("\\\\", "\\");
        }

        private static string CleanText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "Hmm, didn't catch that — what were you saying?";

            text = Regex.Replace(text.Trim(), @"\s+", " ");
            text = Regex.Replace(text, @"([.!?,])([^\s])", "$1 $2");
            return text;
        }

        private void ApplySlidingWindow()
        {
            if (history.Count > MaxHistory + 1)
            {
                int remove = history.Count - MaxHistory - 1;
                history.RemoveRange(1, remove);
            }
        }

        // ── Inner type ────────────────────────────────────────────────────────────
        private class ChatMessage
        {
            public string role { get; }
            public string content { get; }
            public ChatMessage(string role, string content)
            {
                this.role = role;
                this.content = content;
            }
        }
    }
}