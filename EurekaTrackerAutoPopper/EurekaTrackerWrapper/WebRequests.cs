using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EurekaTrackerAutoPopper.EurekaTrackerWrapper
{
    internal static class WebRequests
    {
        public static async void PopNM(ushort trackerId, string instance, string password)
        {
            try
            {
                ClientWebSocket socket = new();
                await socket.ConnectAsync(new Uri("wss://ffxiv-eureka.com/socket/websocket?vsn=2.0.0"), CancellationToken.None);
                await socket.SendAsync(Encoding.UTF8.GetBytes($"[\"1\",\"1\",\"instance:{instance}\",\"phx_join\",{{\"password\":\"{password}\"}}]"), WebSocketMessageType.Text, true, CancellationToken.None);
                await socket.SendAsync(Encoding.UTF8.GetBytes($"[\"2\",\"2\",\"instance:{instance}\",\"set_kill_time\",{{\"id\":{trackerId},\"time\":{GetEpochTime()}}}]"), WebSocketMessageType.Text, true, CancellationToken.None);
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
            }
            catch (Exception e)
            {
                Plugin.Log.Error("Error while popping NM.");
                Plugin.Log.Error(e.Message);
            }
        }

        public static async Task<(string instance, string password)> CreateNewTracker(short trackerZoneId)
        {
            try
            {
                HttpClient httpClient = new();
                HttpResponseMessage response = await httpClient.PostAsync("https://ffxiv-eureka.com/api/instances",
                    new StringContent(
                        $"{{\"data\":{{\"attributes\":{{\"zone-id\":\"{trackerZoneId}\"}},\"type\":\"instances\"}}}}",
                        Encoding.UTF8, "application/json"));

                httpClient.Dispose();

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    dynamic jsonResult = JsonConvert.DeserializeObject(result)!;
                    string instance = jsonResult.data.id;
                    string password = jsonResult.data.attributes.password;

                    return ($"https://ffxiv-eureka.com/{instance}", password);
                }
            }
            catch (Exception e)
            {
                Plugin.Log.Error("Error while creating new tracker.");
                Plugin.Log.Error(e.Message);
            }

            return ("", "");
        }

        private static long GetEpochTime()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }
    }
}
