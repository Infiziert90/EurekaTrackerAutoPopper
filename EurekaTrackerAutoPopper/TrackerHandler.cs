// ReSharper disable ExplicitCallerInfoArgument

using System;
using System.IO;
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
using Serilog;

namespace EurekaTrackerAutoPopper;

public class TrackerHandler
{
    private const string TableName = "OccultTrackerV3";
    private const string BaseUrl = "https://infi.ovh/api/";
    private const string AnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJyb2xlIjoiYW5vbiJ9.Ur6wgi_rD4dr3uLLvbLoaEvfLCu4QFWdrF-uHRtbl_s";

    private readonly Plugin Plugin;

    private readonly HttpClient Client = new();

    private readonly CancellationTokenSource TokenSource = new();

    public bool IsConnected;
    public string ConnectedTo = string.Empty;
    public string TrackerPassword = string.Empty;

    public int FailedCounter;

    public NewTracker? UpcomingTracker;
    public ExistingTracker? CurrentTracker;

    public TrackerHandler(Plugin plugin)
    {
        Plugin = plugin;

        Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AnonKey}");
        Client.DefaultRequestHeaders.Add("Prefer", "return=representation");
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
        TrackerPassword = string.Empty;
        FailedCounter = 0;

        UpcomingTracker = null;
        CurrentTracker = null;
    }

    public class Upload
    {
        [JsonIgnore]
        public string Table;

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
        [JsonProperty("last_fate")]
        public string LastFateHash;

        [JsonProperty("tracker_type")]
        public byte TrackerType;

        [JsonProperty("encounter_history")]
        public string EncounterHistory;

        [JsonProperty("fate_history")]
        public string FateHistory;

        [JsonProperty("pot_history")]
        public string PotHistory;

        [JsonConstructor]
        public NewTracker() {}

        public NewTracker(uint dcId, uint fateId, int timestamp, Fates fateManager) : base(TableName)
        {
            TrackerType = 1;

            EncounterHistory = JsonConvert.SerializeObject(fateManager.OccultCriticalEncounters.Select(f => new ShareableFate(f)));
            FateHistory = JsonConvert.SerializeObject(fateManager.OccultFates.Select(f => new ShareableFate(f)));
            PotHistory = JsonConvert.SerializeObject(fateManager.BunnyFates.TakeLast(2).Select(f => new ShareableFate(f)));

            using var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(dcId);
                writer.Write(fateId);
                writer.Write(timestamp);
            }

            stream.Position = 0;
            using (var hash = SHA256.Create())
            {
                var result = hash.ComputeHash(stream);
                LastFateHash = string.Join("", result.Select(b => $"{b:X2}"));
            }
        }
    }

    public class ExistingTracker : Upload
    {
        [JsonProperty("id")]
        public long Id;

        [JsonProperty("last_update")]
        public long LastUpdate;

        [JsonProperty("tracker_id")]
        public string TrackerId;

        [JsonProperty("password")]
        public string TrackerPassword;

        [JsonProperty("tracker_type")]
        public byte TrackerType;

        [JsonProperty("last_fate")]
        public string LastFateHash;

        [JsonProperty("encounter_history")]
        public string EncounterHistory;

        [JsonProperty("fate_history")]
        public string FateHistory;

        [JsonProperty("pot_history")]
        public string PotHistory;

        [JsonIgnore]
        public ShareableFate[] Encounters;

        [JsonIgnore]
        public ShareableFate[] Fates;

        [JsonIgnore]
        public ShareableFate[] Pots;

        [JsonConstructor]
        public ExistingTracker() {}

        [OnDeserialized]
        internal void Init(StreamingContext _)
        {
            Encounters = JsonConvert.DeserializeObject<ShareableFate[]>(EncounterHistory) ?? [];
            Fates = JsonConvert.DeserializeObject<ShareableFate[]>(FateHistory) ?? [];
            Pots = JsonConvert.DeserializeObject<ShareableFate[]>(PotHistory) ?? [];

            LastUpdate = DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        public void Update(Fates fateManager)
        {
            EncounterHistory = JsonConvert.SerializeObject(fateManager.OccultCriticalEncounters.Select(f => new ShareableFate(f)));
            FateHistory = JsonConvert.SerializeObject(fateManager.OccultFates.Select(f => new ShareableFate(f)));
            PotHistory = JsonConvert.SerializeObject(fateManager.BunnyFates.TakeLast(2).Select(f => new ShareableFate(f)));

            LastUpdate = DateTimeOffset.Now.ToUnixTimeSeconds();
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
        public long[] PreviousRespawnTimes = fate.PreviousRespawnTimes.ToArray();

        [JsonProperty("killed_fates")]
        public int KilledFates = fate.KilledFates;

        [JsonProperty("killed_ces")]
        public int KilledCEs = fate.KilledCEs;
    }

    public void InstanceCheckAsync(IFate fate, IPlayerCharacter localPlayer)
    {
        if (Plugin.ClientState.TerritoryType != (uint)Territory.SouthHorn)
            return;

        // Check upload permission
        if (!Plugin.Configuration.UploadPermission)
            return;

        var dcId = localPlayer.CurrentWorld.Value.DataCenter.RowId;
        UpcomingTracker = new NewTracker(dcId, fate.FateId, fate.StartTimeEpoch, Plugin.Fates);
        Task.Run(async () => await DelayedInstanceCheck());
    }

    public void UpdateRunningTracker()
    {
        if (Plugin.ClientState.TerritoryType != (uint)Territory.SouthHorn)
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
                CurrentTracker.LastFateHash = UpcomingTracker.LastFateHash;

                CurrentTracker.EncounterHistory = UpcomingTracker.EncounterHistory;
                CurrentTracker.FateHistory = UpcomingTracker.FateHistory;
                CurrentTracker.PotHistory = UpcomingTracker.PotHistory;

                CurrentTracker.Init(new StreamingContext());

                await UploadExistingTracker(CurrentTracker);
                return;
            }

            await Task.Delay(10_000, TokenSource.Token);

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
            TrackerPassword = CurrentTracker.TrackerPassword;

            // Write back critical encounters fetched from tracker
            foreach (var sharedFate in CurrentTracker.Encounters)
            {
                var localFate = Plugin.Fates.OccultCriticalEncounters.First(f => f.FateId == sharedFate.FateId);

                localFate.LastSeenAlive = sharedFate.LastSeenAlive;
                localFate.SpawnTime = sharedFate.SpawnTime;
                localFate.DeathTime = sharedFate.DeathTime;
                localFate.PreviousRespawnTimes = sharedFate.PreviousRespawnTimes.ToList();

                // Only important for forked tower
                if (localFate.FateId != 48)
                    continue;

                localFate.KilledFates = sharedFate.KilledFates;
                localFate.KilledCEs = sharedFate.KilledCEs;
            }

            // Write back fates fetched from tracker
            foreach (var sharedFate in CurrentTracker.Fates)
            {
                var localFate = Plugin.Fates.OccultFates.First(f => f.FateId == sharedFate.FateId);

                localFate.LastSeenAlive = sharedFate.LastSeenAlive;
                localFate.SpawnTime = sharedFate.SpawnTime;
                localFate.DeathTime = sharedFate.DeathTime;
                localFate.PreviousRespawnTimes = sharedFate.PreviousRespawnTimes.ToList();
            }

            // Write back pot fates fetched from tracker
            foreach (var sharedFate in CurrentTracker.Pots)
            {
                var localFate = Plugin.Fates.BunnyFates.First(f => f.FateId == sharedFate.FateId);

                localFate.LastSeenAlive = sharedFate.LastSeenAlive;
                localFate.SpawnTime = sharedFate.SpawnTime;
                localFate.DeathTime = sharedFate.DeathTime;
                localFate.PreviousRespawnTimes = sharedFate.PreviousRespawnTimes.ToList();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to instance check after time delay.");

            Reset();
        }
    }

    private async Task<ExistingTracker[]?> TryFindInstance()
    {
        try
        {
            if (UpcomingTracker == null)
                return null;

            var response = await Client.GetAsync($"{BaseUrl}{TableName}?last_fate=eq.{UpcomingTracker.LastFateHash}");
            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ExistingTracker[]>(content);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to find instance.");
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

            var response = await Client.PutAsync($"{BaseUrl}{entry.Table}?id=eq.{entry.Id}", content);
            Plugin.Log.Debug($"Table {entry.Table} ({response.StatusCode}) | Content: {response.Content.ReadAsStringAsync().Result}");
        }
        catch (Exception e)
        {
            Plugin.Log.Error(e, "Upload failed");
        }
    }
}
