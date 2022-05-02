using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.Reflection;
using Dalamud.Game.Gui;
using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using System;
using System.Linq;
using XivCommon.Functions.Tooltips;
using System.Threading.Tasks;
using Dalamud.DrunkenToad;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Game.ClientState;
using System.Net.WebSockets;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Dalamud.Game;
using Dalamud.Game.Text.SeStringHandling;
using System.Threading;
using ImGuiNET;

namespace ItemVendorLocation
{
    public class Plugin : IDalamudPlugin
    {
        public string Name => "Eureka Tracker Auto Popper";

        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        private Configuration Configuration { get; init; }
        private PluginUI PluginUi { get; init; }

        private List<Fate> lastPolledFates = new();
        private ushort lastPolledTerritory;

        public static Dictionary<ushort, ushort> anemosFates = new()
        {
            { 1332, 1 },   // Unsafety Dance                     => Sabotender Corrido
            { 1348, 2 },   // The Shadow over Anemos             => The Lord of Anemos
            { 1333, 3 },   // Teles House                        => Teles
            { 1328, 4 },   // The Swarm Never Sets               => The Emperor of Anemos
            { 1344, 5 },   // One Missed Callisto                => Callisto
            { 1347, 6 },   // By Numbers                         => Number
            { 1345, 7 },   // Disinherit the Wind                => Jahannam
            { 1334, 8 },   // Prove Your Amemettle               => Amemet
            { 1335, 9 },   // Caym What May                      => Caym
            { 1336, 10 },  // The Killing of a Sacred Bombardier => Bombadeel
            { 1339, 11 },  // Short Serket 2                     => Serket
            { 1346, 12 },  // Don't Judge Me, Morbol             => Judgmental Julika
            { 1343, 13 },  // When You Ride Alone                => The White Rider
            { 1337, 14 },  // Sing, Muse                         => Polyphemus
            { 1342, 15 },  // Simurghasbord                      => Simurgh's Strider
            { 1341, 16 },  // To the Mat                         => King Hazmat
            { 1331, 17 },  // Wine and Honey                     => Fafnir
            { 1340, 18 },  // I Amarok                           => Amarok
            { 1338, 19 },  // Drama Lamashtu                     => Lamashtu
            { 1329, 20 },  // Wail in the Willows                => Pazuzu
        };

        public static Dictionary<ushort, ushort> pagosFates = new()
        {
            { 1351, 21 },  // Eternity                       => The Snow Queen
            { 1369, 22 },  // Cairn Blight 451               => Taxim
            { 1353, 23 },  // Ash the Magic Dragon           => Ash Dragon
            { 1354, 24 },  // Conqueror Worm                 => Glavoid
            { 1355, 25 },  // Melting Point                  => Anapos
            { 1366, 26 },  // The Wobbler in Darkness        => Hakutaku
            { 1357, 27 },  // Does It Have to Be a Snowman   => King Igloo
            { 1356, 28 },  // Disorder in the Court          => Asag
            { 1352, 29 },  // Cows for Concern               => Surabhi
            { 1360, 30 },  // Morte Arthro                   => King Arthro
            { 1358, 31 },  // Brothers                       => Mindertaur/Eldertaur
            { 1361, 32 },  // Apocalypse Cow                 => Holy Cow
            { 1362, 33 },  // Third Impact                   => Hadhayosh
            { 1359, 34 },  // Eye of Horus                   => Horus
            { 1363, 35 },  // Eye Scream for Ice Cream       => Arch Angra Mainyu
            { 1365, 36 },  // Cassie and the Copycats        => Copycat Cassie
            { 1364, 37 },  // Louhi on Ice                   => Louhi
        };

        public static Dictionary<ushort, ushort> pyrosFates = new()
        {
            { 1388, 38 },  // Medias Res             => Leucosia
            { 1389, 39 },  // High Voltage           => Flauros
            { 1390, 40 },  // On the Nonexistent     => The Sophist
            { 1391, 41 },  // Creepy Doll            => Graffiacane
            { 1392, 42 },  // Quiet, Please          => Askalaphos
            { 1393, 43 },  // Up and Batym           => Grand Duke Batym
            { 1394, 44 },  // Rondo Aetolus          => Aetolus
            { 1395, 45 },  // Scorchpion King        => Lesath
            { 1396, 46 },  // Burning Hunger         => Eldthurs
            { 1397, 47 },  // Dry Iris               => Iris
            { 1398, 48 },  // Thirty Whacks          => Lamebrix Strikebocks
            { 1399, 49 },  // Put Up Your Dux        => Dux
            { 1400, 50 },  // You Do Know Jack       => Lumber Jack
            { 1401, 51 },  // Mister Bright-eyes     => Glaukopis
            { 1402, 52 },  // Haunter of the Dark    => Ying-Yang
            { 1403, 53 },  // Heavens' Warg          => Skoll
            { 1404, 54 }   // Lost Epic              => Penthesilea
        };

        public static Dictionary<ushort, ushort> hydatosFates = new()
        {
            { 1412, 55 },  // I Ink, Therefore I Am          => Khalamari
            { 1413, 56 },  // From Tusk till Dawn            => Stegodon
            { 1414, 57 },  // Bullheaded Berserker           => Molech
            { 1415, 58 },  // Mad, Bad, and Fabulous to Know => Piasa
            { 1416, 59 },  // Fearful Symmetry               => Frostmane
            { 1417, 60 },  // Crawling Chaos                 => Daphne
            { 1418, 61 },  // Duty-free                      => King Goldemar
            { 1419, 62 },  // Leukewarm Reception            => Leuke
            { 1420, 63 },  // Robber Barong                  => Barong
            { 1421, 64 },  // Stone-cold Killer              => Ceto
            { 1423, 65 }   // Crystalline Provenance         => Provenance Watcher
        };

        private static List<ushort> relevantTerritories = new()
        {
            827,  // Hydatos
            795,  // Pyros
            763,  // Pagos
            732   // Anemos
        };

        [PluginService] public static ChatGui Chat { get; private set; } = null!;
        [PluginService] public static GameGui GameGui { get; private set; } = null!;
        [PluginService] public static Dalamud.Data.DataManager DataManager { get; private set; } = null!;
        [PluginService] public static FateTable FateTable { get; private set; } = null!;
        [PluginService] public static Framework Framework { get; private set; } = null!;
        [PluginService] public static ClientState ClientState { get; private set; } = null!;
        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager,
            [RequiredVersion("1.0")] ClientState clientState)
        {
            PluginInterface = pluginInterface;
            CommandManager = commandManager;
            ClientState = clientState;

            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(PluginInterface);

            // you might normally want to embed resources and load them from the manifest stream
            string? assemblyLocation = Assembly.GetExecutingAssembly().Location;
            PluginUi = new PluginUI(Configuration);

            CommandManager.AddHandler("/xleureka", new CommandInfo(OnEurekaCommand)
            {
                HelpMessage = "Opens the config window",
                ShowInHelp = true
            });

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            ClientState.TerritoryChanged += pollForPlayerGoingToEureka;
        }

        private void OnEurekaCommand(string command, string arguments)
        {
            DrawConfigUI();
        }

        private void pollForPlayerGoingToEureka(object? sender, ushort territoryId)
        {
            if (playerInRelevantTerritory())
            {
                Task.Run(() =>
                {
                    findTracker();
                });
                Framework.Update += pollForFateChange;
            }
            else
            {
                PluginUi.Instance = "";
                PluginUi.Password = "";
                Framework.Update -= pollForFateChange;
            }
        }

        private bool playerInRelevantTerritory()
        {
            return relevantTerritories.Contains(ClientState.TerritoryType);
        }

        private bool noFatesHaveChangedSinceLastChecked()
        {
            return FateTable.SequenceEqual(lastPolledFates);
        }

        private void checkForRelevantFates(ushort currentTerritory)
        {
            List<Fate> newFates = FateTable.Except(lastPolledFates).ToList();
            Dictionary<ushort, ushort> relevantFateDictionary = new();
            switch (currentTerritory)
            {
                case 827:  // Hydatos
                    relevantFateDictionary = hydatosFates;
                    break;
                case 795:  // Pyros
                    relevantFateDictionary = pyrosFates;
                    break;
                case 763:  // Pagos
                    relevantFateDictionary = pagosFates;
                    break;
                case 732:  // Anemos
                    relevantFateDictionary = anemosFates;
                    break;
            }
            List<ushort> newRelevantFateIds = newFates.Select(i => i.FateId).Intersect(relevantFateDictionary.Keys.ToList()).ToList();
            foreach (ushort fateId in newRelevantFateIds)
            {
                popNM(relevantFateDictionary[fateId]);
            }
        }

        private async void popNM(int nmId)
        {
            ClientWebSocket socket = new();
            await socket.ConnectAsync(new Uri("wss://ffxiv-eureka.com/socket/websocket?vsn=2.0.0"), CancellationToken.None);
            await socket.SendAsync(Encoding.UTF8.GetBytes($"[\"1\",\"1\",\"instance:{PluginUi.Instance}\",\"phx_join\",{{\"password\":\"{PluginUi.Password}\"}}]"), WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);
            await socket.SendAsync(Encoding.UTF8.GetBytes($"[\"2\",\"2\",\"instance:{PluginUi.Instance}\",\"set_kill_time\",{{\"id\":{nmId},\"time\":{getEpochTime()}}}]"), WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
        }

        private async void SetDataCenter()
        {
            // not going to set data center until I can figure somet things out
            return;
            uint? dataCenterId = ClientState.LocalPlayer?.CurrentWorld.GameData?.DataCenter.Row;
            // keep trying this method until we get the local player
            if (dataCenterId == null)
            {
                Thread.Sleep(500);
                SetDataCenter();
                return;
            }
            ClientWebSocket socket = new();
            await socket.ConnectAsync(new Uri("wss://ffxiv-eureka.com/socket/websocket?vsn=2.0.0"), CancellationToken.None);
            await socket.SendAsync(Encoding.UTF8.GetBytes($"[\"1\",\"1\",\"instance:{PluginUi.Instance}\",\"phx_join\",{{\"password\":\"{PluginUi.Password}\"}}]"), WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);
            await socket.SendAsync(Encoding.UTF8.GetBytes($"[\"2\",\"2\",\"instance:{PluginUi.Instance}\",\"set_instance_information\",{{\"data_center_id\":{dataCenterId}}}]"), WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
        }

        private long getEpochTime()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        private void SetupNewTracker()
        {
            ushort zoneId = 0;
            switch (ClientState.TerritoryType)
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
                PluginUi.Instance = instance;
                PluginUi.Password = password;
                Chat.PrintChat(new Dalamud.Game.Text.XivChatEntry { Message = $"[{Name}] New tracker: https://ffxiv-eureka.com/{instance}" });
                Chat.PrintChat(new Dalamud.Game.Text.XivChatEntry { Message = $"[{Name}] Password: {password}" });
                ImGui.SetClipboardText($"https://ffxiv-eureka.com/{instance}");
                SetDataCenter();
            }
        }

        private void pollForFateChange(Framework framework)
        {
            if (noFatesHaveChangedSinceLastChecked())
            {
                return;
            }

            checkForRelevantFates(ClientState.TerritoryType);
            lastPolledFates = FateTable.ToList();
            lastPolledTerritory = ClientState.TerritoryType;
        }
        public void Dispose()
        {
            PluginUi.Dispose();
            Framework.Update -= pollForFateChange;
            ClientState.TerritoryChanged -= pollForPlayerGoingToEureka;
            CommandManager.RemoveHandler("/xleureka");
        }

        private void DrawUI()
        {
            PluginUi.Draw();
        }

        private void DrawConfigUI()
        {
            PluginUi.SettingsVisible = true;
        }

        private async void findTracker()
        {
            uint? dataCenterId = ClientState.LocalPlayer?.CurrentWorld.GameData?.DataCenter.Row;
            // keep trying this method until we get the local player
            if (dataCenterId == null)
            {
                Thread.Sleep(500);
                findTracker();
                return;
            }

            ushort zoneId = 0;
            switch (ClientState.TerritoryType)
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

            ClientWebSocket socket = new();
            await socket.ConnectAsync(new Uri("wss://ffxiv-eureka.com/socket/websocket?vsn=2.0.0"), CancellationToken.None);
            await socket.SendAsync(Encoding.UTF8.GetBytes($"[\"1\",\"1\",\"datacenter:{dataCenterId}\",\"phx_join\",{{}}]"), WebSocketMessageType.Text, true, CancellationToken.None);
            byte[]? buffer = new byte[60000];
            ArraySegment<byte> segment = new(buffer);
            _ = await socket.ReceiveAsync(segment, CancellationToken.None);
            _ = await socket.ReceiveAsync(segment, CancellationToken.None);
            string result = Encoding.UTF8.GetString(buffer);
            try
            {
                dynamic deserializedResult = JsonConvert.DeserializeObject(result)!;
                IEnumerable<dynamic> data = ((IEnumerable<dynamic>)deserializedResult[4].data).Where(i => i.relationships.zone.data.id == zoneId);
                if (data.Count() == 0)
                {
                    // SetupNewTracker(); let's not set up a new tracker by default for now
                    return;
                }
                else
                {
                    Logger.LogDebug("Found existing tracker");
                    string? instance = (string?)((Newtonsoft.Json.Linq.JValue)data.First().id).Value;
                    if (instance == null)
                    {
                        Logger.LogError("Problem getting Eureka instance");
                        return;
                    }
                    Chat.PrintChat(new Dalamud.Game.Text.XivChatEntry { Message = $"[{Name}] Join existing tracker: https://ffxiv-eureka.com/{instance}" });
                    PluginUi.Instance = instance;
                    ImGui.SetClipboardText($"https://ffxiv-eureka.com/{instance}");
                }
            }
            catch (JsonReaderException)
            {
            }
            finally
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
            }
        }
    }
}
