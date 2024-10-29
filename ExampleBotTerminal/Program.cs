using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Timers;

namespace AzureBotConsoleApp
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly string directLineSecret = Environment.GetEnvironmentVariable("DIRECT_LINE_SECRET");
        private const string directLineEndpoint = "https://directline.botframework.com/v3/directline/conversations";
        private static string conversationId = "";
        private static string watermark = "";
        private static System.Timers.Timer messagePollingTimer; // Fully qualify Timer class

        static async Task Main(string[] args)
        {
            Console.WriteLine("Start chatting with the bot. Type 'exit' to end the conversation.");

            // Initialize Direct Line conversation
            await StartConversationAsync();

            // Set up timer to poll for bot responses
            messagePollingTimer = new System.Timers.Timer(500); // Use System.Timers.Timer
            messagePollingTimer.Elapsed += async (sender, e) => await PollForMessagesAsync();
            messagePollingTimer.Start();

            await SendMessageToBotAsync("initial");
            await Task.Delay(1000);

            // Chat loop for sending messages
            while (true)
            {
                Console.Write("You: ");
                string userMessage = Console.ReadLine();

                if (userMessage.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                await SendMessageToBotAsync(userMessage);
                await Task.Delay(500);

            }

            messagePollingTimer.Stop(); // Stop the timer when exiting
        }

        private static async Task StartConversationAsync()
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", directLineSecret);

            var response = await client.PostAsync(directLineEndpoint, null);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var conversationResponse = JsonSerializer.Deserialize<Conversation>(responseContent);
                conversationId = conversationResponse.ConversationId;
                Console.WriteLine("Conversation started successfully.");
            }
            else
            {
                Console.WriteLine("Failed to start conversation.");
            }
        }

        private static async Task SendMessageToBotAsync(string message)
        {
            var messagePayload = new
            {
                type = "message",
                from = new { id = "user1" },
                text = message
            };

            string messageUrl = $"{directLineEndpoint}/{conversationId}/activities";
            var content = new StringContent(JsonSerializer.Serialize(messagePayload), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(messageUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Error posting message to bot.");
            }
        }

        private static async Task PollForMessagesAsync()
        {
            if (string.IsNullOrEmpty(conversationId)) return;

            try{
                string messageUrl = $"{directLineEndpoint}/{conversationId}/activities?watermark={watermark}";

                var response = await client.GetAsync(messageUrl);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var activitySet = JsonSerializer.Deserialize<ActivitySet>(responseContent);

                    // Update watermark to avoid processing the same messages repeatedly
                    watermark = activitySet.Watermark;

                    foreach (var activity in activitySet.Activities)
                    {
                        if (activity.From.Id != "user1" && !string.IsNullOrEmpty(activity.Text))
                        {
                            Console.WriteLine($"Bot: {activity.Text}");
                        }
                    }
                }else
                {
                    Console.WriteLine($"Error polling messages. Status Code: {response.StatusCode}, Reason: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in PollForMessagesAsync: {ex.Message}");
            }
        }

    }

    public class Conversation
    {
        [JsonPropertyName("conversationId")]
        public string ConversationId { get; set; }
    }

    public class ActivitySet
    {
        [JsonPropertyName("activities")]
        public List<Activity> Activities { get; set; }

        [JsonPropertyName("watermark")]
        public string Watermark { get; set; }
    }

    public class Activity
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("from")]
        public From From { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

    public class From
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
}