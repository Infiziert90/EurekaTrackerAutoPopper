using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EurekaTrackerAutoPopper.EurekaTrackerWrapper
{
    internal class WebRequests
    {
        public static async void PopNM(ushort trackerId, string instance, string password)
        {
            ClientWebSocket socket = new();
            await socket.ConnectAsync(new Uri("wss://ffxiv-eureka.com/socket/websocket?vsn=2.0.0"), CancellationToken.None);
            await socket.SendAsync(Encoding.UTF8.GetBytes($"[\"1\",\"1\",\"instance:{instance}\",\"phx_join\",{{\"password\":\"{password}\"}}]"), WebSocketMessageType.Text, true, CancellationToken.None);
            await socket.SendAsync(Encoding.UTF8.GetBytes($"[\"2\",\"2\",\"instance:{instance}\",\"set_kill_time\",{{\"id\":{trackerId},\"time\":{GetEpochTime()}}}]"), WebSocketMessageType.Text, true, CancellationToken.None);
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
        }

        public static (string instance, string response) CreateNewTracker(short trackerZoneId)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://ffxiv-eureka.com/api/instances");
            request.Method = "POST";
            request.Headers.Add("Content-Type:application/json");
            request.GetRequestStream().Write(Encoding.UTF8.GetBytes($"{{\"data\":{{\"attributes\":{{\"zone-id\":\"{trackerZoneId}\"}},\"type\":\"instances\"}}}}"));
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                StreamReader reader = new(response.GetResponseStream());
                string result = reader.ReadToEnd();
                dynamic deserializedResult = JsonConvert.DeserializeObject(result)!;
                string instance = deserializedResult.data.id;
                string password = deserializedResult.data.attributes.password;
                return (instance, password);
            }
            return ("", "");
        }

        public static async Task<string?> FindTracker(uint dataCenterId, short zoneId)
        {
            ClientWebSocket socket = new();
            try {
                await socket.ConnectAsync(new Uri("wss://ffxiv-eureka.com/socket/websocket?vsn=2.0.0"), CancellationToken.None);
                await socket.SendAsync(Encoding.UTF8.GetBytes($"[\"1\",\"1\",\"datacenter:{dataCenterId}\",\"phx_join\",{{}}]"), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch
            {
                return null;
            }
            byte[]? buffer = new byte[60000];
            ArraySegment<byte> segment = new(buffer);
            _ = await socket.ReceiveAsync(segment, CancellationToken.None);
            _ = await socket.ReceiveAsync(segment, CancellationToken.None);
            string result = Encoding.UTF8.GetString(buffer);
            try
            {
                dynamic deserializedResult = JsonConvert.DeserializeObject(result)!;
                IEnumerable<dynamic> data = ((IEnumerable<dynamic>)deserializedResult[4].data).Where(i => i.relationships.zone.data.id == zoneId);
                if (!data.Any())
                {
                    // SetupNewTracker(); let's not set up a new tracker by default for now
                    return null;
                }
                else
                {
                    string? instance = (string?)((Newtonsoft.Json.Linq.JValue)data.First().id).Value;
                    return instance ?? null;
                }
            }
            catch (JsonReaderException)
            {
                return null;
            }
            finally
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
            }
        }

        private static long GetEpochTime()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }
    }
}
