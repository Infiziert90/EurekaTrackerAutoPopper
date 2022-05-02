using ImGuiScene;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Net.WebSockets;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Linq;

namespace UIDev
{
    internal class UITest : IPluginUIMock
    {
        public static void Main()
        {
            UIBootstrap.Inititalize(new UITest());
        }

        private SimpleImGuiScene? scene;
        public void Initialize(SimpleImGuiScene scene)
        {
            // scene is a little different from what you have access to in dalamud
            // but it can accomplish the same things, and is really only used for initial setup here

            // eg, to load an image resource for use with ImGui 
            scene.OnBuildUI += Draw;

            MainWindowVisible = true;
            ResultsWindowVisible = false;

            // saving thi22 only so we can kill the test application by closing the window
            // (instead of just by hitting escape)
            this.scene = scene;
        }

        public void Dispose()
        {
        }

        // You COULD go all out here and make your UI generic and work on interfaces etc, and then
        // mock dependencies and conceivably use exactly the same class in this testbed and the actual plugin
        // That is, however, a bit excessive in general - it could easily be done for this sample, but I
        // don't want to imply that is easy or the best way to go usually, so it's not done here either
        private void Draw()
        {
            DrawMainWindow();

            if (!MainWindowVisible)
            {
                scene!.ShouldQuit = true;
            }
        }

        private bool mainWindowVisible = false;
        private bool resultsWindowVisible = false;
        private string instance = "";
        private string password = "";
        public bool MainWindowVisible
        {
            get => mainWindowVisible;
            set => mainWindowVisible = value;
        }

        public bool ResultsWindowVisible
        {
            get => resultsWindowVisible;
            set => resultsWindowVisible = value;
        }

        private class NM
        {
            public NM(string name, int id)
            {
                this.name = name;
                this.id = id;
            }
            public string name;
            public int id;
        }

        private List<NM> HydatosNms = new()
        {
            new NM("Khalamari", 55),
            new NM("Stegodon", 56),
            new NM("Molech", 57),
            new NM("Piasa", 58),
            new NM("Frostmane", 59),
            new NM("Daphne", 60),
            new NM("Goldemar", 61),
            new NM("Leuke", 62),
            new NM("Barong", 63),
            new NM("Ceto", 64),
            new NM("Watcher", 65)
        };

        // this is where you'd have to start mocking objects if you really want to match
        // but for simple UI creation purposes, just hardcoding values works        
        public void DrawMainWindow()
        {
            if (!MainWindowVisible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(375, 330), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(375, 330), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("Test", ref mainWindowVisible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoCollapse))
            {
                _ = ImGui.InputText("Instance", ref instance, 50);
                _ = ImGui.InputText("Password", ref password, 50);
                foreach (NM nm in HydatosNms)
                {
                    if (ImGui.Button($"Pop {nm.name}"))
                    {
                        popNM(nm.id);
                    }
                }
                if (ImGui.Button("TEST"))
                {
                    findTracker();
                }
                ImGui.End();
            }
        }


        private async void popNM(int nmId)
        {
            ClientWebSocket socket = new();
            await socket.ConnectAsync(new Uri("wss://ffxiv-eureka.com/socket/websocket?vsn=2.0.0"), System.Threading.CancellationToken.None);
            await socket.SendAsync(Encoding.UTF8.GetBytes($"[\"1\",\"1\",\"instance:{instance}\",\"phx_join\",{{\"password\":\"{password}\"}}]"), WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);
            await socket.SendAsync(Encoding.UTF8.GetBytes($"[\"2\",\"2\",\"instance:{instance}\",\"set_kill_time\",{{\"id\":{nmId},\"time\":{getEpochTime()}}}]"), WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, System.Threading.CancellationToken.None);
        }

        private long getEpochTime()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        private async void findTracker()
        {
            ClientWebSocket socket = new();
            await socket.ConnectAsync(new Uri("wss://ffxiv-eureka.com/socket/websocket?vsn=2.0.0"), System.Threading.CancellationToken.None);
            await socket.SendAsync(Encoding.UTF8.GetBytes($"[\"1\",\"1\",\"datacenter:4\",\"phx_join\",{{}}]"), WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);
            byte[]? buffer = new byte[60000];
            ArraySegment<byte> segment = new(buffer);
            _ = await socket.ReceiveAsync(segment, System.Threading.CancellationToken.None);
            _ = await socket.ReceiveAsync(segment, System.Threading.CancellationToken.None);
            string result = Encoding.UTF8.GetString(buffer);
            try
            {
                dynamic deserializedResult = JsonConvert.DeserializeObject(result)!;
                IEnumerable<dynamic> data = ((IEnumerable<dynamic>)deserializedResult[4].data).Where(i => i.relationships.zone.data.id == 1);
                if (data.Count() == 0)
                {
                    return;
                }

                var instance = ((Newtonsoft.Json.Linq.JValue)data.First().id).Value;
            }
            catch (Newtonsoft.Json.JsonReaderException)
            {
                return;
            }

            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, System.Threading.CancellationToken.None);
        }
    }
}
