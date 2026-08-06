// ReSharper disable ExplicitCallerInfoArgument

using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Newtonsoft.Json;

namespace EurekaTrackerAutoPopper;

public class TrackerHandler
{
    private const string TableName = "OccultTrackerV3";
    private const string BaseUrl = "https://infi.ovh/api/";
    private const string AnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJyb2xlIjoiYW5vbiJ9.Ur6wgi_rD4dr3uLLvbLoaEvfLCu4QFWdrF-uHRtbl_s";

    private readonly Plugin Plugin;

    private readonly HttpClient Client = new();
    private readonly Random Random = new();

    private readonly CancellationTokenSource TokenSource = new();

    public bool IsConnected;
    public string ConnectedTo = string.Empty;

    public int FailedCounter;

    public NewTracker? UpcomingTracker;
    public ExistingTracker? CurrentTracker;

    public TrackerHandler(Plugin plugin)
    {
        Plugin = plugin;

        Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AnonKey}");
        Client.DefaultRequestHeaders.Add("Prefer", "return=representation, resolution=ignore-duplicates, on_conflict=last_fate");

        Client.DefaultRequestHeaders.Add("User-Agent", $"Eureka Linker {Plugin.PluginInterface.Manifest.AssemblyVersion}");

        // Task.Run(async () => await GetEntry());
    }

    public void Dispose()
    {
        Client.Dispose();

        TokenSource.Cancel();
        TokenSource.Dispose();
    }

    public void Reset()
    {
        IsConnected = false;
        ConnectedTo = string.Empty;
        FailedCounter = 0;

        UpcomingTracker = null;
        CurrentTracker = null;
    }

    public class Upload
    {
        [JsonIgnore]
        public string Table = string.Empty;

        [JsonProperty("version")]
        public string Version = Plugin.PluginInterface.Manifest.AssemblyVersion.ToString();

        [JsonConstructor]
        public Upload() {}

        public Upload(string table)
        {
            Table = table;
        }
    }

    public class NewTracker : Upload
    {
        [JsonProperty("territory")]
        public uint Territory;

        [JsonProperty("last_fate")]
        public string LastFateHash = string.Empty;

        [JsonProperty("tracker_type")]
        public byte TrackerType;

        [JsonProperty("datacenter")]
        public ushort Datacenter;

        [JsonProperty("encounter_history")]
        public string EncounterHistory = string.Empty;

        [JsonProperty("fate_history")]
        public string FateHistory = string.Empty;

        [JsonProperty("pot_history")]
        public string PotHistory = string.Empty;

        [JsonProperty("server")]
        public uint Server;

        [JsonProperty("fate_timestamp")]
        public int FateTimestamp;

        [JsonProperty("fate")]
        public uint Fate;

        [JsonConstructor]
        public NewTracker() {}

        public NewTracker(uint dcId, uint fateId, int timestamp, Fates fateManager, uint server) : base(TableName)
        {
            Territory = Plugin.ClientState.TerritoryType;
            TrackerType = 1;
            Datacenter = (ushort)dcId;
            FateTimestamp = timestamp;
            Server = server;
            Fate = fateId;

            EncounterHistory = JsonConvert.SerializeObject(fateManager.GetCEsSkipExtremeForTerritory().Select(f => new ShareableFate(f)));
            FateHistory = JsonConvert.SerializeObject(fateManager.GetFatesForTerritory().Select(f => new ShareableFate(f)));
            PotHistory = JsonConvert.SerializeObject(fateManager.GetBunnyForTerritory().Select(f => new ShareableFate(f)));

            Span<byte> buffer = stackalloc byte[12]; // 3 ints * 4 bytes each
            BitConverter.TryWriteBytes(buffer[0..4], dcId);
            BitConverter.TryWriteBytes(buffer[4..8], fateId);
            BitConverter.TryWriteBytes(buffer[8..12], timestamp);
            LastFateHash = string.Join("", SHA256.HashData(buffer).Select(b => $"{b:X2}"));
        }
    }

    public class ExistingTracker : Upload
    {
        [JsonProperty("id")]
        public long Id;

        [JsonProperty("territory")]
        public uint Territory;

        [JsonProperty("last_update")]
        public long LastUpdate;

        [JsonProperty("tracker_id")]
        public string TrackerId = string.Empty;

        [JsonProperty("tracker_type")]
        public byte TrackerType;

        [JsonProperty("datacenter")]
        public ushort Datacenter;

        [JsonProperty("last_fate")]
        public string LastFateHash = string.Empty;

        [JsonProperty("encounter_history")]
        public string EncounterHistory = string.Empty;

        [JsonProperty("fate_history")]
        public string FateHistory = string.Empty;

        [JsonProperty("pot_history")]
        public string PotHistory = string.Empty;

        [JsonProperty("server")]
        public uint Server;

        [JsonProperty("fate_timestamp")]
        public int FateTimestamp;

        [JsonProperty("fate")]
        public uint Fate;

        [JsonIgnore]
        public ShareableFate[] Encounters = [];

        [JsonIgnore]
        public ShareableFate[] Fates = [];

        [JsonIgnore]
        public ShareableFate[] Pots = [];

        [JsonConstructor]
        public ExistingTracker() {}

        [OnDeserialized]
        internal void Init(StreamingContext _)
        {
            Encounters = JsonConvert.DeserializeObject<ShareableFate[]>(EncounterHistory) ?? [];
            Fates = JsonConvert.DeserializeObject<ShareableFate[]>(FateHistory) ?? [];
            Pots = JsonConvert.DeserializeObject<ShareableFate[]>(PotHistory) ?? [];
        }

        public void Update(Fates fateManager)
        {
            EncounterHistory = JsonConvert.SerializeObject(fateManager.GetCEsSkipExtremeForTerritory().Select(f => new ShareableFate(f)));
            FateHistory = JsonConvert.SerializeObject(fateManager.GetFatesForTerritory().Select(f => new ShareableFate(f)));
            PotHistory = JsonConvert.SerializeObject(fateManager.GetBunnyForTerritory().Select(f => new ShareableFate(f)));

            LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }

    [Serializable]
    public struct ShareableFate(Fate fate)
    {
        [JsonProperty("fate_id")]
        public uint FateId = fate.FateId;

        [JsonProperty("spawn_time")]
        public long SpawnTime = fate.SpawnTime;

        [JsonProperty("death_time")]
        public long DeathTime = fate.DeathTime;

        [JsonProperty("last_seen")]
        public long LastSeenAlive = fate.LastSeenAlive;

        [JsonProperty("respawn_times")]
        public long[] PreviousRespawnTimes = [];

        [JsonProperty("killed_fates")]
        public int KilledFates = fate.KilledFates;

        [JsonProperty("killed_ces")]
        public int KilledCEs = fate.KilledCEs;

        [JsonProperty("state")]
        public byte State = (byte)fate.State;
    }

    public void InstanceCheckAsync(IFate fate, IPlayerCharacter localPlayer)
    {
        if (!TerritoryHelper.PlayerInOccult())
            return;

        // Check upload permission
        if (!Plugin.Configuration.UploadPermission)
            return;

        var dcId = localPlayer.CurrentWorld.Value.DataCenter.RowId;
        UpcomingTracker = new NewTracker(dcId, fate.FateId, fate.StartTimeEpoch, Plugin.Fates, Plugin.HookManager.ServerId);
        Task.Run(async () => await DelayedInstanceCheck());
    }

    public void UpdateRunningTracker()
    {
        if (!TerritoryHelper.PlayerInOccult())
            return;

        // Check upload permission
        if (!Plugin.Configuration.UploadPermission)
            return;

        if (CurrentTracker == null || !IsConnected)
            return;

        CurrentTracker.Table = TableName;
        CurrentTracker.Update(Plugin.Fates);
        Task.Run(async () => await UploadExistingTracker(CurrentTracker));
    }

    private async Task DelayedInstanceCheck()
    {
        try
        {
            if (CurrentTracker != null && UpcomingTracker != null)
            {
                CurrentTracker.Table = TableName;
                CurrentTracker.Version = Plugin.PluginInterface.Manifest.AssemblyVersion.ToString();

                CurrentTracker.LastFateHash = UpcomingTracker.LastFateHash;
                CurrentTracker.Server = UpcomingTracker.Server;
                CurrentTracker.FateTimestamp = UpcomingTracker.FateTimestamp;
                CurrentTracker.Fate = UpcomingTracker.Fate;

                CurrentTracker.EncounterHistory = UpcomingTracker.EncounterHistory;
                CurrentTracker.FateHistory = UpcomingTracker.FateHistory;
                CurrentTracker.PotHistory = UpcomingTracker.PotHistory;

                CurrentTracker.Init(new StreamingContext());

                await UploadExistingTracker(CurrentTracker);
                return;
            }

            await Task.Delay(Random.Next(2_500, 4_000), TokenSource.Token);

            var trackers = await TryFindInstance();
            if (trackers == null || trackers.Length == 0)
            {
                FailedCounter++;

                if (FailedCounter < 2)
                    return;

                if (UpcomingTracker == null)
                    return;

                await UploadNewTracker(UpcomingTracker);
                trackers = await TryFindInstance();
                if (trackers == null || trackers.Length == 0)
                    return;
            }

            CurrentTracker = trackers[0];

            IsConnected = true;
            ConnectedTo = CurrentTracker.TrackerId;

            // Write back critical encounters fetched from tracker
            foreach (var sharedFate in CurrentTracker.Encounters)
            {
                if (sharedFate.FateId == 65)
                    continue;

                var localFate = Plugin.Fates.GetCEsSkipExtremeForTerritory().First(f => f.FateId == sharedFate.FateId);

                localFate.LastSeenAlive = sharedFate.LastSeenAlive;
                localFate.SpawnTime = sharedFate.SpawnTime;
                localFate.DeathTime = sharedFate.DeathTime;

                // Only important for forked tower
                if (localFate.FateId is 48 or 64)
                {
                    localFate.KilledFates = sharedFate.KilledFates;
                    localFate.KilledCEs = sharedFate.KilledCEs;
                }
            }

            // Write back fates fetched from tracker
            foreach (var sharedFate in CurrentTracker.Fates)
            {
                var localFate = Plugin.Fates.GetFatesForTerritory().First(f => f.FateId == sharedFate.FateId);

                localFate.LastSeenAlive = sharedFate.LastSeenAlive;
                localFate.SpawnTime = sharedFate.SpawnTime;
                localFate.DeathTime = sharedFate.DeathTime;
            }

            // Write back pot fates fetched from tracker
            foreach (var sharedFate in CurrentTracker.Pots)
            {
                var localFate = Plugin.Fates.GetBunnyForTerritory().First(f => f.FateId == sharedFate.FateId);

                localFate.LastSeenAlive = sharedFate.LastSeenAlive;
                localFate.SpawnTime = sharedFate.SpawnTime;
                localFate.DeathTime = sharedFate.DeathTime;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, "Failed to instance check after time delay.");

            Reset();
        }
    }

    private async Task<ExistingTracker[]?> TryFindInstance()
    {
        try
        {
            if (UpcomingTracker == null)
                return null;

            var response = await Client.GetAsync($"{BaseUrl}{TableName}?last_fate=eq.{UpcomingTracker.LastFateHash}&territory=eq.{UpcomingTracker.Territory}");
            var content = await response.Content.ReadAsStringAsync();
            Plugin.Log.Debug($"Instance Search ({response.StatusCode}) | Hash: {UpcomingTracker.LastFateHash} | Timestamp: {UpcomingTracker.FateTimestamp} | Content: {content}");

            return JsonConvert.DeserializeObject<ExistingTracker[]>(content);
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, "Failed to find instance.");
            return null;
        }
    }

    private async Task UploadNewTracker(NewTracker entry)
    {
        try
        {
            var content = new StringContent(JsonConvert.SerializeObject(entry), Encoding.UTF8, "application/json");
            var response = await Client.PostAsync($"{BaseUrl}{entry.Table}", content);
            Plugin.Log.Debug($"Table {entry.Table} ({response.StatusCode}) | Content: {response.Content.ReadAsStringAsync().Result}");
        }
        catch (Exception e)
        {
            Plugin.Log.Error(e, "Upload failed");
        }
    }

    private async Task UploadExistingTracker(ExistingTracker entry)
    {
        try
        {
            var content = new StringContent(JsonConvert.SerializeObject(entry), Encoding.UTF8, "application/json");
            var response = await Client.PatchAsync($"{BaseUrl}{entry.Table}?id=eq.{entry.Id}", content);
            Plugin.Log.Debug($"Table {entry.Table} ({response.StatusCode}) | Content: {response.Content.ReadAsStringAsync().Result}");
        }
        catch (Exception e)
        {
            Plugin.Log.Error(e, "Upload failed");
        }
    }

    // private long LastUpdate;
    // private async Task GetEntry()
    // {
    //     try
    //     {
    //         while (true)
    //         {
    //             await Task.Delay(100);
    //
    //             var response = await Client.GetAsync($"{BaseUrl}OccultTrackerV3?id=eq.195016");
    //             var content = await response.Content.ReadAsStringAsync();
    //             var trackers = JsonConvert.DeserializeObject<ExistingTracker[]>(content);
    //             var tracker = trackers[0];
    //
    //             Plugin.Log.Debug(content);
    //             if (tracker.LastUpdate > LastUpdate)
    //             {
    //                 LastUpdate = tracker.LastUpdate;
    //
    //                 Plugin.Log.Information($"\nServer: {tracker.Server}Hash: {tracker.LastFateHash}\nTimestamp: {tracker.FateTimestamp}\nDC: {tracker.Datacenter}\nFate: {tracker.Fate}\nLast Update: {tracker.LastUpdate}\nVersion: {tracker.Version}");
    //             }
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         Plugin.Log.Error(ex, $"Error");
    //     }
    // }
}
