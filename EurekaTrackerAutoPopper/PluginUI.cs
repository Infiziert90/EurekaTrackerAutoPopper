using Dalamud.Game.Text.SeStringHandling.Payloads;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.DrunkenToad;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace ItemVendorLocation
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    internal class PluginUI : IDisposable
    {
        private readonly Configuration configuration;

        private bool settingsVisible = false;
        private string instance = "";
        private string password = "";

        public bool SettingsVisible
        {
            get => settingsVisible;
            set => settingsVisible = value;
        }

        public string Instance
        {
            get => instance;
            set => instance = value;
        }

        public string Password
        {
            get => password;
            set => password = value;
        }

        public PluginUI(Configuration configuration)
        {
            this.configuration = configuration;
        }

        public void Dispose()
        {
        }

        public void Draw()
        {
            // This is our only draw handler attached to UIBuilder, so it needs to be
            // able to draw any windows we might have open.
            // Each method checks its own visibility/state to ensure it only draws when
            // it actually makes sense.
            // There are other ways to do this, but it is generally best to keep the number of
            // draw delegates as low as possible.
            DrawSettingsWindow();
        }

        public void DrawSettingsWindow()
        {
            if (!SettingsVisible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(375, 330), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(375, 330), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("Test", ref settingsVisible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoCollapse))
            {
                _ = ImGui.InputText("Instance", ref instance, 50);
                _ = ImGui.InputText("Password", ref password, 50);
                if (ImGui.Button("Start New Tracker"))
                {
                    Task.Run(() =>
                    {
                        SetupNewTracker();
                    });
                }
                if (Instance.Length > 0)
                {
                    if (ImGui.Button("Open Tracker in Browser"))
                    {
                        _ = Process.Start(new ProcessStartInfo()
                        {
                            FileName = $"https://ffxiv-eureka.com/{instance}",
                            UseShellExecute = true
                        });
                    }
                }
                ImGui.End();
            }
        }

        private void SetupNewTracker()
        {
            ushort zoneId = 0;
            switch (Plugin.ClientState.TerritoryType)
            {
                case 827:  // Hydatos
                    zoneId = 4;
                    break;
                case 795:  // Pyros
                    zoneId = 3;
                    break;
                case 763:  // Pagos
                    zoneId = 2;
                    break;
                case 732:  // Anemos
                    zoneId = 1;
                    break;
            }
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://ffxiv-eureka.com/api/instances");
            request.Method = "POST";
            request.Headers.Add("Content-Type:application/json");
            request.GetRequestStream().Write(Encoding.UTF8.GetBytes($"{{\"data\":{{\"attributes\":{{\"zone-id\":\"{zoneId}\"}},\"type\":\"instances\"}}}}"));
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                StreamReader reader = new(response.GetResponseStream());
                string result = reader.ReadToEnd();
                dynamic deserializedResult = JsonConvert.DeserializeObject(result)!;
                string instance = deserializedResult.data.id;
                string password = deserializedResult.data.attributes.password;
                Instance = instance;
                Password = password;
                SetDataCenter();
            }
        }

        private async void SetDataCenter()
        {
            uint? dataCenterId = Plugin.ClientState.LocalPlayer?.CurrentWorld.GameData?.DataCenter.Row;
            // keep trying this method until we get the local player
            if (dataCenterId == null)
            {
                Thread.Sleep(500);
                SetDataCenter();
                return;
            }
            ClientWebSocket socket = new();
            await socket.ConnectAsync(new Uri("wss://ffxiv-eureka.com/socket/websocket?vsn=2.0.0"), CancellationToken.None);
            await socket.SendAsync(Encoding.UTF8.GetBytes($"[\"1\",\"1\",\"instance:{Instance}\",\"phx_join\",{{\"password\":\"{Password}\"}}]"), WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);
            await socket.SendAsync(Encoding.UTF8.GetBytes($"[\"2\",\"2\",\"instance:{Instance}\",\"set_instance_information\",{{\"data_center_id\":{dataCenterId}}}]"), WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
        }

    }
}
