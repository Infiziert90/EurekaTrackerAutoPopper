using System;
using Dalamud.Game.Text.SeStringHandling;
using System.Collections.Generic;
using System.Numerics;
using Lumina.Excel.Sheets;

namespace EurekaTrackerAutoPopper;

public class Library
{
    private Configuration Configuration { get; init; }

    public Library(Configuration configuration)
    {
        Configuration = configuration;
    }

    public sealed record EurekaFate(ushort FateId, ushort TrackerId, SeString MapLink, string Name, string ShortName)
    {
        public static EurekaFate Empty => new(0, 0, "", "", "");

        public bool Equals(EurekaFate? other) => other != null && FateId == other.FateId;
        public override int GetHashCode() => HashCode.Combine(FateId, TrackerId, MapLink, Name, ShortName);
    }

    // represents a elemental/fairy seen by the user
    public readonly List<Fairy> ExistingFairies = [];
    public record Fairy
    {
        public readonly uint ObjectId;
        public readonly Vector3 Pos;
        public readonly SeString MapLink;

        public Fairy(uint objectId, uint fairy, Vector3 pos)
        {
            ObjectId = objectId;
            Pos = pos;

            var map = FairyToTerritory[fairy];
            MapLink = SeString.CreateMapLink(map.TerritoryId, map.MapId, (int) pos.X * 1000, (int) pos.Z * 1000);  // directX Z = Y
        }
    }

    // represents a random coffer the user has already spotted
    public readonly HashSet<uint> ExistingTreasure = [];
    public record Treasure
    {
        public readonly uint ObjectId;
        public readonly Vector3 Pos;
        public readonly SeString MapLink;

        public readonly string Type;

        public Treasure(uint objectId, Vector3 pos, Lumina.Excel.Sheets.Treasure treasureRow)
        {
            ObjectId = objectId;
            Pos = pos;

            if (treasureRow.SGB.RowId == 1597)
                Type = "Silver";
            else
                Type = "Bronze";

            MapLink = SeString.CreateMapLink(Plugin.ClientState.TerritoryType, Plugin.ClientState.MapId, (int) pos.X * 1000, (int) pos.Z * 1000);  // directX Z = Y
        }
    }

    // represents a bunny carrot on the ground which the user has already spotted
    public HashSet<uint> ExistingBunnyCarrots = [];
    public record BunnyCarrot
    {
        public readonly uint ObjectId;
        public readonly Vector3 Pos;
        public readonly SeString MapLink;

        public BunnyCarrot(uint objectId, Vector3 pos)
        {
            ObjectId = objectId;
            Pos = pos;

            MapLink = SeString.CreateMapLink(Plugin.ClientState.TerritoryType, Plugin.ClientState.MapId, (int) pos.X * 1000, (int) pos.Z * 1000);  // directX Z = Y
        }
    }

    public static readonly List<uint> Fairies =
    [
        7184, // Anemos
        7567, // Pagos
        7764, // Pyros
        8131 // Hydatos
    ];

    public record Map(uint TerritoryId, uint MapId);
    private static readonly Dictionary<uint, Map> FairyToTerritory = new()
    {
        { 7184, new Map(732, 414) },
        { 7567, new Map(763, 467) },
        { 7764, new Map(795, 484) },
        { 8131, new Map(827, 515) }
    };

    public static readonly Dictionary<uint, Map> TerritoryToMap = new()
    {
        { 732, new Map(732, 414) },
        { 763, new Map(763, 467) },
        { 795, new Map(795, 484) },
        { 827, new Map(827, 515) }
    };

    public void CleanCaches()
    {
        ExistingFairies.Clear();
        ExistingTreasure.Clear();
        ExistingBunnyCarrots.Clear();
    }

    // randomize flag X and Y in a range of +0.5 and -0.5, fitting well into the 1 radius of the fate circle
    private static readonly Random Rand = new();
    private const double MaxValue = 50.0;
    private const double MinValue = -50.0;
    private static double Randomize(double coord) => coord + (Rand.NextDouble() * (MaxValue - MinValue) + MinValue);

    private SeString CreateMapLink(uint territoryId, uint mapId, double xRaw, double yRaw)
    {
        xRaw = Configuration.RandomizeMapCoords ? Randomize(xRaw) : xRaw;
        yRaw = Configuration.RandomizeMapCoords ? Randomize(yRaw) : yRaw;
        return SeString.CreateMapLink(territoryId, mapId, (int) xRaw * 1000, (int) yRaw * 1000);
    }

    private SeString CreateAnemosLink(double x, double y) => CreateMapLink(732, 414, x, y);
    private SeString CreatePagosLink(double x, double y) => CreateMapLink(763, 467, x, y);
    private SeString CreatePyrosLink(double x, double y) => CreateMapLink(795, 484, x, y);
    private SeString CreateHydatosLink(double x, double y) => CreateMapLink(827, 515, x, y);

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
            new(1332, 1, CreateAnemosLink(-374.272, 046.065), "Sabotender Corrido", "Sabo"), // Unsafety Dance
            new(1348, 2, CreateAnemosLink(422.198, 295.295), "The Lord of Anemos", "Lord"), // The Shadow over Anemos
            new(1333, 3, CreateAnemosLink(219.879, 278.616), "Teles", "Teles"), // Teles House
            new(1328, 4, CreateAnemosLink(-216.584, 036.581), "The Emperor of Anemos", "Emperor"), // The Swarm Never Sets
            new(1344, 5, CreateAnemosLink(217.419, 069.252), "Callisto", "Callisto"), // One Missed Callisto
            new(1347, 6, CreateAnemosLink(092.410, 042.252), "Number", "Number"), // By Numbers
            new(1345, 7, CreateAnemosLink(-116.856, -092.852), "Jahannam", "Jaha"), // Disinherit the Wind
            new(1334, 8, CreateAnemosLink(-339.417, -304.344), "Amemet", "Amemet"), // Prove Your Amemettle
            new(1335, 9, CreateAnemosLink(-425.393, -409.680), "Caym", "Caym"), // Caym What May
            new(1336, 10, CreateAnemosLink(339.000, -059.000), "Bombadeel", "Bomba"), // The Killing of a Sacred Bombardier
            new(1339, 11, CreateAnemosLink(174.274, -161.008), "Serket", "Serket"), // Short Serket 2
            new(1346, 12, CreateAnemosLink(022.311, -346.764), "Judgmental Julika", "Julika"), // Don't Judge Me, Morbol
            new(1343, 13, CreateAnemosLink(-082.823, -410.886), "The White Rider", "Rider"), // When You Ride Alone
            new(1337, 14, CreateAnemosLink(254.101, -336.717), "Polyphemus", "Poly"), // Sing, Muse
            new(1342, 15, CreateAnemosLink(381.183, -420.454), "Simurgh's Strider", "Strider"), // Simurghasbord
            new(1341, 16, CreateAnemosLink(672.856, -118.538), "King Hazmat", "Hazmat"), // To the Mat
            new(1331, 17, CreateAnemosLink(695.816, -002.495), "Fafnir", "Fafnir"), // Wine and Honey
            new(1340, 18, CreateAnemosLink(-685.638, -179.588), "Amarok", "Amarok"), // I Amarok
            new(1338, 19, CreateAnemosLink(-693.540, 256.086), "Lamashtu", "Lamashtu"), // Drama Lamashtu
            new(1329, 20, CreateAnemosLink(-703.714, 008.446), "Pazuzu", "Paz") // Wail in the Willows
        ];
    }

    private List<EurekaFate> InitializePagosFates()
    {
        return
        [
            new(1351, 21, CreatePagosLink(011.382, 241.963), "The Snow Queen", "Queen"), // Eternity
            new(1369, 22, CreatePagosLink(225.633, 296.361), "Taxim", "Taxim"), // Cairn Blight 451
            new(1353, 23, CreatePagosLink(430.406, 417.229), "Ash Dragon", "Dragon"), // Ash the Magic Dragon
            new(1354, 24, CreatePagosLink(569.550, 290.306), "Glavoid", "Glavoid"), // Conqueror Worm
            new(1355, 25, CreatePagosLink(574.972, 003.143), "Anapos", "Anapos"), // Melting Point
            new(1366, 26, CreatePagosLink(374.767, 043.833), "Hakutaku", "Haku"), // The Wobbler in Darkness
            new(1357, 27, CreatePagosLink(-213.825, -266.344), "King Igloo", "Igloo"), // Does It Have to Be a Snowman
            new(1356, 28, CreatePagosLink(-548.321, -521.281), "Asag", "Asag"), // Disorder in the Court
            new(1352, 29, CreatePagosLink(-580.272, -122.945), "Surabhi", "Surabhi"), // Cows for Concern
            new(1360, 30, CreatePagosLink(-634.173, -301.533), "King Arthro", "Arthro"), // Morte Arthro
            new(1358, 31, CreatePagosLink(-379.075, -136.462), "Mindertaur/Eldertaur", "Brothers"), // Brothers
            new(1361, 32, CreatePagosLink(251.451, -225.843), "Holy Cow", "Holy Cow"), // Apocalypse Cow
            new(1362, 33, CreatePagosLink(478.674, -149.159), "Hadhayosh", "Behe"), // Third Impact
            new(1359, 34, CreatePagosLink(226.311, -063.235), "Horus", "Horus"), // Eye of Horus
            new(1363, 35, CreatePagosLink(112.608, 178.729), "Arch Angra Mainyu", "Mainyu"), // Eye Scream for Ice Cream
            new(1365, 36, CreatePagosLink(039.758, -359.487), "Copycat Cassie", "Cassie"), // Cassie and the Copycats
            new(1364, 37, CreatePagosLink(722.135, -136.944), "Louhi", "Louhi") // Louhi on Ice
        ];
    }

    private List<EurekaFate> InitializePyrosFates()
    {
        return
        [
            new(1388, 38, CreatePyrosLink(289.079, 229.340), "Leucosia", "Leucosia"), // Medias Res
            new(1389, 39, CreatePyrosLink(371.929, 398.315), "Flauros", "Flauros"), // High Voltage
            new(1390, 40, CreatePyrosLink(532.646, 501.803), "The Sophist", "Sophist"), // On the Nonexistent
            new(1391, 41, CreatePyrosLink(078.030, 788.137), "Graffiacane", "Doll"), // Creepy Doll
            new(1392, 42, CreatePyrosLink(-110.167, 388.126), "Askalaphos", "Owl"), // Quiet, Please
            new(1393, 43, CreatePyrosLink(-180.768, -369.788), "Grand Duke Batym", "Batym"), // Up and Batym
            new(1394, 44, CreatePyrosLink(-567.044, -363.884), "Aetolus", "Aetolus"), // Rondo Aetolus
            new(1395, 45, CreatePyrosLink(-442.154, -519.942), "Lesath", "Lesath"), // Scorchpion King
            new(1396, 46, CreatePyrosLink(-312.680, -751.977), "Eldthurs", "Eldthurs"), // Burning Hunger
            new(1397, 47, CreatePyrosLink(-008.461, -463.125), "Iris", "Iris"), // Dry Iris
            new(1398, 48, CreatePyrosLink(017.849, -655.973), "Lamebrix Strikebocks", "Lamebrix"), // Thirty Whacks
            new(1399, 49, CreatePyrosLink(298.700, -627.158), "Dux", "Dux"), // Put Up Your Dux
            new(1400, 50, CreatePyrosLink(442.155, -503.844), "Lumber Jack", "Jack"), // You Do Know Jack
            new(1401, 51, CreatePyrosLink(528.319, -314.761), "Glaukopis", "Glaukopis"), // Mister Bright-eyes
            new(1402, 52, CreatePyrosLink(-499.806, 641.563), "Ying-Yang", "YY"), // Haunter of the Dark
            new(1403, 53, CreatePyrosLink(128.825, 431.166), "Skoll", "Skoll"), // Heavens' Warg
            new(1404, 54, CreatePyrosLink(722.758, -776.778), "Penthesilea", "Penny") // Lost Epic
        ];
    }

    private List<EurekaFate> InitializeHydatosFates()
    {
        return
        [
            new(1412, 55, CreateHydatosLink(-518.110, -288.121), "Khalamari", "Khalamari"), // I Ink, Therefore I Am
            new(1413, 56, CreateHydatosLink(-570.634, -656.247), "Stegodon", "Stegodon"), // From Tusk till Dawn
            new(1414, 57, CreateHydatosLink(-676.863, -441.800), "Molech", "Molech"), // Bullheaded Berserker
            new(1415, 58, CreateHydatosLink(-737.209, -829.455), "Piasa", "Piasa"), // Mad, Bad, and Fabulous to Know
            new(1416, 59, CreateHydatosLink(-681.433, -243.361), "Frostmane", "Frostmane"), // Fearful Symmetry
            new(1417, 60, CreateHydatosLink(207.847, -736.817), "Daphne", "Daphne"), // Crawling Chaos
            new(1418, 61, CreateHydatosLink(371.037, -366.978), "King Goldemar", "Golde"), // Duty-free
            new(1419, 62, CreateHydatosLink(792.925, -194.505), "Leuke", "Leuke"), // Leukewarm Reception
            new(1420, 63, CreateHydatosLink(554.145, -309.968), "Barong", "Barong"), // Robber Barong
            new(1421, 64, CreateHydatosLink(747.895, -878.876), "Ceto", "Ceto"), // Stone-cold Killer
            new(1423, 65, CreateHydatosLink(564.046, -568.686), "Provenance Watcher", "PW"), // Crystalline Provenance
            new(1424, 0, CreateHydatosLink(266.106, -097.0941), "Ovni", "Ovni"), // I Don't Want to Believe
            new(1422, 0, CreateHydatosLink(-125.776, -111.181), "Tristitia", "Support") // The Baldesion Arsenal: Expedition Support
        ];
    }
#pragma warning restore format
}