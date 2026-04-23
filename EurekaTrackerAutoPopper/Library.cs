using System;
using Dalamud.Game.Text.SeStringHandling;
using System.Collections.Generic;
using System.Numerics;

namespace EurekaTrackerAutoPopper;

public class Library
{
    private Configuration Configuration;

    public Library(Configuration configuration)
    {
        Configuration = configuration;
    }

    public sealed record EurekaFate(ushort FateId, ushort TrackerId, uint TerritoryId, uint MapId, Vector3 WorldPos, string Name, string ShortName)
    {
        public readonly SeString MapLink = TerritoryId != 0 ? Utils.CreateMapLink(TerritoryId, MapId, WorldPos.X, WorldPos.Z) : "";

        public static EurekaFate Empty => new(0, 0, 0, 0, Vector3.Zero, "", "");

        public bool Equals(EurekaFate? other) => other != null && FateId == other.FateId;
        public override int GetHashCode() => HashCode.Combine(FateId, TrackerId, TerritoryId, MapId, WorldPos, Name, ShortName);
    }

    public record LocationMemory
    {
        public readonly uint EntityId;
        public readonly Vector3 WorldPos;
        public readonly SeString MapLink;
        public readonly string MapDataLink;

        public readonly string Type = string.Empty;

        public long LastSeen = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public LocationMemory(uint entityId, Vector3 worldPos, uint? extraInfo = null)
        {
            EntityId = entityId;
            WorldPos = worldPos;

            MapLink = Utils.CreateMapLink(Plugin.ClientState.TerritoryType, Plugin.ClientState.MapId, worldPos.X, worldPos.Z);
            MapDataLink = Utils.CreateMapDataLink(Plugin.ClientState.TerritoryType, Plugin.ClientState.MapId, worldPos.X, worldPos.Z);

            if (extraInfo != null)
                Type = extraInfo == 1597 ? "Silver" : "Bronze";
        }

        public override int GetHashCode()
        {
            return EntityId.GetHashCode() + WorldPos.GetHashCode();
        }

        public virtual bool Equals(LocationMemory? other)
        {
            if (other == null)
                return false;

            return EntityId == other.EntityId && WorldPos.Equals(other.WorldPos);
        }
    }

    // represents a elemental/fairy seen by the user
    public readonly List<LocationMemory> ExistingFairies = [];

    // represents a treasure or bunny carrot that the user has already spotted
    public readonly List<LocationMemory> ExistingOccultLocations = [];

    public static readonly List<uint> Fairies =
    [
        7184, // Anemos
        7567, // Pagos
        7764, // Pyros
        8131 // Hydatos
    ];

    public void CleanCaches()
    {
        ExistingFairies.Clear();
        ExistingOccultLocations.Clear();
    }

    public static readonly Dictionary<ushort, short> TerritoryToTrackerDictionary = new()
    {
        { 827, 4 },
        { 795, 3 },
        { 763, 2 },
        { 732, 1 },
    };

    public IEnumerable<EurekaFate> TerritoryToFateDictionary(ushort territoryId)
    {
        return territoryId switch
        {
            732 => AnemosFates,
            763 => PagosFates,
            795 => PyrosFates,
            827 => HydatosFates,
            _ => AnemosFates
        };
    }

    public List<EurekaFate> AnemosFates { get; private set; } = null!;
    public List<EurekaFate> PagosFates { get; private set; } = null!;
    public List<EurekaFate> PyrosFates { get; private set; } = null!;
    public List<EurekaFate> HydatosFates { get; private set; } = null!;

    public void Initialize()
    {
        AnemosFates = InitializeAnemosFates();
        PagosFates = InitializePagosFates();
        PyrosFates = InitializePyrosFates();
        HydatosFates = InitializeHydatosFates();
    }

#pragma warning disable format
    private List<EurekaFate> InitializeAnemosFates()
    {
        return
        [
            new(1332, 1, 732, 414, new Vector3(-374.272f, 0f, 046.065f), "Sabotender Corrido", "Sabo"), // Unsafety Dance
            new(1348, 2, 732, 414, new Vector3(422.198f, 0f, 295.295f), "The Lord of Anemos", "Lord"), // The Shadow over Anemos
            new(1333, 3, 732, 414, new Vector3(219.879f, 0f, 278.616f), "Teles", "Teles"), // Teles House
            new(1328, 4, 732, 414, new Vector3(-216.584f, 0f, 036.581f), "The Emperor of Anemos", "Emperor"), // The Swarm Never Sets
            new(1344, 5, 732, 414, new Vector3(217.419f, 0f, 069.252f), "Callisto", "Callisto"), // One Missed Callisto
            new(1347, 6, 732, 414, new Vector3(092.410f, 0f, 042.252f), "Number", "Number"), // By Numbers
            new(1345, 7, 732, 414, new Vector3(-116.856f, 0f, -092.852f), "Jahannam", "Jaha"), // Disinherit the Wind
            new(1334, 8, 732, 414, new Vector3(-339.417f, 0f, -304.344f), "Amemet", "Amemet"), // Prove Your Amemettle
            new(1335, 9, 732, 414, new Vector3(-425.393f, 0f, -409.680f), "Caym", "Caym"), // Caym What May
            new(1336, 10, 732, 414, new Vector3(339.000f, 0f, -059.000f), "Bombadeel", "Bomba"), // The Killing of a Sacred Bombardier
            new(1339, 11, 732, 414, new Vector3(174.274f, 0f, -161.008f), "Serket", "Serket"), // Short Serket 2
            new(1346, 12, 732, 414, new Vector3(022.311f, 0f, -346.764f), "Judgmental Julika", "Julika"), // Don't Judge Me, Morbol
            new(1343, 13, 732, 414, new Vector3(-082.823f, 0f, -410.886f), "The White Rider", "Rider"), // When You Ride Alone
            new(1337, 14, 732, 414, new Vector3(254.101f, 0f, -336.717f), "Polyphemus", "Poly"), // Sing, Muse
            new(1342, 15, 732, 414, new Vector3(381.183f, 0f, -420.454f), "Simurgh's Strider", "Strider"), // Simurghasbord
            new(1341, 16, 732, 414, new Vector3(672.856f, 0f, -118.538f), "King Hazmat", "Hazmat"), // To the Mat
            new(1331, 17, 732, 414, new Vector3(695.816f, 0f, -002.495f), "Fafnir", "Fafnir"), // Wine and Honey
            new(1340, 18, 732, 414, new Vector3(-685.638f, 0f, -179.588f), "Amarok", "Amarok"), // I Amarok
            new(1338, 19, 732, 414, new Vector3(-693.540f, 0f, 256.086f), "Lamashtu", "Lamashtu"), // Drama Lamashtu
            new(1329, 20, 732, 414, new Vector3(-703.714f, 0f, 008.446f), "Pazuzu", "Paz") // Wail in the Willows
        ];
    }

    private List<EurekaFate> InitializePagosFates()
    {
        return
        [
            new(1351, 21, 763, 467, new Vector3(011.382f, 0f, 241.963f), "The Snow Queen", "Queen"), // Eternity
            new(1369, 22, 763, 467, new Vector3(225.633f, 0f, 296.361f), "Taxim", "Taxim"), // Cairn Blight 451
            new(1353, 23, 763, 467, new Vector3(430.406f, 0f, 417.229f), "Ash Dragon", "Dragon"), // Ash the Magic Dragon
            new(1354, 24, 763, 467, new Vector3(569.550f, 0f, 290.306f), "Glavoid", "Glavoid"), // Conqueror Worm
            new(1355, 25, 763, 467, new Vector3(574.972f, 0f, 003.143f), "Anapos", "Anapos"), // Melting Point
            new(1366, 26, 763, 467, new Vector3(374.767f, 0f, 043.833f), "Hakutaku", "Haku"), // The Wobbler in Darkness
            new(1357, 27, 763, 467, new Vector3(-213.825f, 0f, -266.344f), "King Igloo", "Igloo"), // Does It Have to Be a Snowman
            new(1356, 28, 763, 467, new Vector3(-548.321f, 0f, -521.281f), "Asag", "Asag"), // Disorder in the Court
            new(1352, 29, 763, 467, new Vector3(-580.272f, 0f, -122.945f), "Surabhi", "Surabhi"), // Cows for Concern
            new(1360, 30, 763, 467, new Vector3(-634.173f, 0f, -301.533f), "King Arthro", "Arthro"), // Morte Arthro
            new(1358, 31, 763, 467, new Vector3(-379.075f, 0f, -136.462f), "Mindertaur/Eldertaur", "Brothers"), // Brothers
            new(1361, 32, 763, 467, new Vector3(251.451f, 0f, -225.843f), "Holy Cow", "Holy Cow"), // Apocalypse Cow
            new(1362, 33, 763, 467, new Vector3(478.674f, 0f, -149.159f), "Hadhayosh", "Behe"), // Third Impact
            new(1359, 34, 763, 467, new Vector3(226.311f, 0f, -063.235f), "Horus", "Horus"), // Eye of Horus
            new(1363, 35, 763, 467, new Vector3(112.608f, 0f, 178.729f), "Arch Angra Mainyu", "Mainyu"), // Eye Scream for Ice Cream
            new(1365, 36, 763, 467, new Vector3(039.758f, 0f, -359.487f), "Copycat Cassie", "Cassie"), // Cassie and the Copycats
            new(1364, 37, 763, 467, new Vector3(722.135f, 0f, -136.944f), "Louhi", "Louhi") // Louhi on Ice
        ];
    }

    private List<EurekaFate> InitializePyrosFates()
    {
        return
        [
            new(1388, 38, 795, 484, new Vector3(289.079f, 0f, 229.340f), "Leucosia", "Leucosia"), // Medias Res
            new(1389, 39, 795, 484, new Vector3(371.929f, 0f, 398.315f), "Flauros", "Flauros"), // High Voltage
            new(1390, 40, 795, 484, new Vector3(532.646f, 0f, 501.803f), "The Sophist", "Sophist"), // On the Nonexistent
            new(1391, 41, 795, 484, new Vector3(078.030f, 0f, 788.137f), "Graffiacane", "Doll"), // Creepy Doll
            new(1392, 42, 795, 484, new Vector3(-110.167f, 0f, 388.126f), "Askalaphos", "Owl"), // Quiet, Please
            new(1393, 43, 795, 484, new Vector3(-180.768f, 0f, -369.788f), "Grand Duke Batym", "Batym"), // Up and Batym
            new(1394, 44, 795, 484, new Vector3(-567.044f, 0f, -363.884f), "Aetolus", "Aetolus"), // Rondo Aetolus
            new(1395, 45, 795, 484, new Vector3(-442.154f, 0f, -519.942f), "Lesath", "Lesath"), // Scorchpion King
            new(1396, 46, 795, 484, new Vector3(-312.680f, 0f, -751.977f), "Eldthurs", "Eldthurs"), // Burning Hunger
            new(1397, 47, 795, 484, new Vector3(-008.461f, 0f, -463.125f), "Iris", "Iris"), // Dry Iris
            new(1398, 48, 795, 484, new Vector3(017.849f, 0f, -655.973f), "Lamebrix Strikebocks", "Lamebrix"), // Thirty Whacks
            new(1399, 49, 795, 484, new Vector3(298.700f, 0f, -627.158f), "Dux", "Dux"), // Put Up Your Dux
            new(1400, 50, 795, 484, new Vector3(442.155f, 0f, -503.844f), "Lumber Jack", "Jack"), // You Do Know Jack
            new(1401, 51, 795, 484, new Vector3(528.319f, 0f, -314.761f), "Glaukopis", "Glaukopis"), // Mister Bright-eyes
            new(1402, 52, 795, 484, new Vector3(-499.806f, 0f, 641.563f), "Ying-Yang", "YY"), // Haunter of the Dark
            new(1403, 53, 795, 484, new Vector3(128.825f, 0f, 431.166f), "Skoll", "Skoll"), // Heavens' Warg
            new(1404, 54, 795, 484, new Vector3(722.758f, 0f, -776.778f), "Penthesilea", "Penny") // Lost Epic
        ];
    }

    private List<EurekaFate> InitializeHydatosFates()
    {
        return
        [
            new(1412, 55, 827, 515, new Vector3(-518.110f, 0f, -288.121f), "Khalamari", "Khalamari"), // I Ink, Therefore I Am
            new(1413, 56, 827, 515, new Vector3(-570.634f, 0f, -656.247f), "Stegodon", "Stegodon"), // From Tusk till Dawn
            new(1414, 57, 827, 515, new Vector3(-676.863f, 0f, -441.800f), "Molech", "Molech"), // Bullheaded Berserker
            new(1415, 58, 827, 515, new Vector3(-737.209f, 0f, -829.455f), "Piasa", "Piasa"), // Mad, Bad, and Fabulous to Know
            new(1416, 59, 827, 515, new Vector3(-681.433f, 0f, -243.361f), "Frostmane", "Frostmane"), // Fearful Symmetry
            new(1417, 60, 827, 515, new Vector3(207.847f, 0f, -736.817f), "Daphne", "Daphne"), // Crawling Chaos
            new(1418, 61, 827, 515, new Vector3(371.037f, 0f, -366.978f), "King Goldemar", "Golde"), // Duty-free
            new(1419, 62, 827, 515, new Vector3(792.925f, 0f, -194.505f), "Leuke", "Leuke"), // Leukewarm Reception
            new(1420, 63, 827, 515, new Vector3(554.145f, 0f, -309.968f), "Barong", "Barong"), // Robber Barong
            new(1421, 64, 827, 515, new Vector3(747.895f, 0f, -878.876f), "Ceto", "Ceto"), // Stone-cold Killer
            new(1423, 65, 827, 515, new Vector3(564.046f, 0f, -568.686f), "Provenance Watcher", "PW"), // Crystalline Provenance
            new(1424, 0, 827, 515, new Vector3(266.106f, 0f, -097.0941f), "Ovni", "Ovni"), // I Don't Want to Believe
            new(1422, 0, 827, 515, new Vector3(-125.776f, 0f, -111.181f), "Tristitia", "Support") // The Baldesion Arsenal: Expedition Support
        ];
    }
#pragma warning restore format
}