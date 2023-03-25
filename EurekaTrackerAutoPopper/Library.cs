using System;
using Dalamud.Game.Text.SeStringHandling;
using System.Collections.Generic;

namespace EurekaTrackerAutoPopper
{
    public class Library
    {
        private Configuration Configuration { get; init; }

        public Library(Configuration configuration)
        {
            Configuration = configuration;
        }

        public record EurekaFate(ushort FateId, ushort TrackerId, SeString MapLink, string Name, string ShortName)
        {
            public static EurekaFate Empty => new(0, 0, "", "", "");
        }

        // represents a elemental/fairy seen by the user
        public Dictionary<uint, Fairy> ExistingFairies = new();
        public record Fairy
        {
            public readonly SeString MapLink;

            public Fairy(uint fairy, float x, float y)
            {
                Map map = FairyToTerritory[fairy];
                MapLink = SeString.CreateMapLink(map.TerritoryId, map.MapId, (int) x * 1000, (int) y * 1000);
            }
        }

        public static readonly List<uint> Fairies = new()
        {
            7184, // Anemos
            7567, // Pagos
            7764, // Pyros
            8131, // Hydatos
        };

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

        // Bunnies
        public class Bunny
        {
            public uint FateId;
            public string Name;
            public uint TerritoryId;

            public bool Alive;
            public long LastSeenAlive = -1;

            public Bunny(uint id, string name, uint territoryId)
            {
                FateId = id;
                Name = name;
                TerritoryId = territoryId;
            }
        }

        public readonly List<Bunny> Bunnies = new()
        {
            new Bunny(1367, "Down the Rabbit Hole", 763), // Pagos
            new Bunny(1368, "Curiouser and Curiouser", 763), // Pagos
            new Bunny(1407, "We're All Mad Here", 795), // Pyros
            new Bunny(1408, "Uncommon Nonsense", 795), // Pyros
            new Bunny(1425, "Drink Me", 827) // Hydatos
        };

        public static readonly List<uint> BunnyMaps = new()
        {
            763,
            795,
            827
        };

        public void ResetBunnies()
        {
            foreach (var bunny in Bunnies)
            {
                bunny.Alive = false;
                bunny.LastSeenAlive = -1;
            }
        }

        // randomize flag X and Y in a range of +0.5 and -0.5, fitting well into the 1 radius of the fate circle
        private static readonly Random Rand = new();
        private const double MaxValue = 0.5;
        private const double MinValue = -0.5;
        private static float Randomize(float coord)
        {
            return (float) (coord + (Rand.NextDouble() * (MaxValue - MinValue) + MinValue));
        }

        private SeString CreateMapLink(uint territoryId, uint mapId, float xCoord, float yCoord)
        {
            xCoord = Configuration.RandomizeMapCoords ? Randomize(xCoord) : xCoord;
            yCoord = Configuration.RandomizeMapCoords ? Randomize(yCoord) : yCoord;
            return SeString.CreateMapLink(territoryId, mapId, xCoord, yCoord);
        }

        private List<EurekaFate> anemosFates = null!;
        private List<EurekaFate> pagosFates = null!;
        private List<EurekaFate> pyrosFates = null!;
        private List<EurekaFate> hydatosFates = null!;
        private Dictionary<ushort, List<EurekaFate>>? territoryToFateDictionary;

        public static readonly Dictionary<ushort, short> TerritoryToTrackerDictionary = new()
        {
            { 827, 4 },
            { 795, 3 },
            { 763, 2 },
            { 732, 1 },
        };

        public Dictionary<ushort, List<EurekaFate>> TerritoryToFateDictionary => territoryToFateDictionary ??= new()
        {
            { 827, HydatosFates },
            { 795, PyrosFates },
            { 763, PagosFates },
            { 732, AnemosFates },
        };

        public List<EurekaFate> AnemosFates => anemosFates;
        public List<EurekaFate> PagosFates => pagosFates;
        public List<EurekaFate> PyrosFates => pyrosFates;
        public List<EurekaFate> HydatosFates => hydatosFates;

        public void Initialize()
        {
            anemosFates = InitializeAnemosFates();
            pagosFates = InitializePagosFates();
            pyrosFates = InitializePyrosFates();
            hydatosFates = InitializeHydatosFates();
        }

#pragma warning disable format
        private List<EurekaFate> InitializeAnemosFates()
        {
            return new()
            {
                new EurekaFate(1332, 1,  CreateMapLink(732, 414, 14.0f, 22.3f), "Sabotender Corrido", "Sabo"),          // Unsafety Dance
                new EurekaFate(1348, 2,  CreateMapLink(732, 414, 29.8f, 27.3f), "The Lord of Anemos", "Lord"),          // The Shadow over Anemos
                new EurekaFate(1333, 3,  CreateMapLink(732, 414, 26.0f, 28.0f), "Teles", "Teles"),                      // Teles House
                new EurekaFate(1328, 4,  CreateMapLink(732, 414, 17.1f, 22.2f), "The Emperor of Anemos", "Emperor"),    // The Swarm Never Sets
                new EurekaFate(1344, 5,  CreateMapLink(732, 414, 26.0f, 22.0f), "Callisto", "Callisto"),                // One Missed Callisto
                new EurekaFate(1347, 6,  CreateMapLink(732, 414, 23.2f, 22.2f), "Number", "Number"),                    // By Numbers
                new EurekaFate(1345, 7,  CreateMapLink(732, 414, 19.1f, 19.6f), "Jahannam", "Jaha"),                    // Disinherit the Wind
                new EurekaFate(1334, 8,  CreateMapLink(732, 414, 14.7f, 15.3f), "Amemet", "Amemet"),                    // Prove Your Amemettle
                new EurekaFate(1335, 9,  CreateMapLink(732, 414, 12.9f, 13.2f), "Caym", "Caym"),                        // Caym What May
                new EurekaFate(1336, 10, CreateMapLink(732, 414, 28.2f, 20.3f), "Bombadeel", "Bomba"),                  // The Killing of a Sacred Bombardier
                new EurekaFate(1339, 11, CreateMapLink(732, 414, 24.9f, 18.2f), "Serket", "Serket"),                    // Short Serket 2
                new EurekaFate(1346, 12, CreateMapLink(732, 414, 21.9f, 14.5f), "Judgmental Julika", "Julika"),         // Don't Judge Me, Morbol
                new EurekaFate(1343, 13, CreateMapLink(732, 414, 20.0f, 13.0f), "The White Rider", "Rider"),            // When You Ride Alone
                new EurekaFate(1337, 14, CreateMapLink(732, 414, 26.5f, 14.7f), "Polyphemus", "Poly"),                  // Sing, Muse
                new EurekaFate(1342, 15, CreateMapLink(732, 414, 29.1f, 13.0f), "Simurgh's Strider", "Strider"),        // Simurghasbord
                new EurekaFate(1341, 16, CreateMapLink(732, 414, 34.9f, 19.1f), "King Hazmat", "Hazmat"),               // To the Mat
                new EurekaFate(1331, 17, CreateMapLink(732, 414, 35.2f, 22.0f), "Fafnir", "Fafnir"),                    // Wine and Honey
                new EurekaFate(1340, 18, CreateMapLink(732, 414, 8.0f,  18.0f), "Amarok", "Amarok"),                    // I Amarok
                new EurekaFate(1338, 19, CreateMapLink(732, 414, 8.0f,  23.0f), "Lamashtu", "Lamashtu"),                // Drama Lamashtu
                new EurekaFate(1329, 20, CreateMapLink(732, 414, 7.0f,  22.0f), "Pazuzu", "Paz"),                       // Wail in the Willows
            };
        }

        private List<EurekaFate> InitializePagosFates()
        {
            return new()
            {
                new EurekaFate(1351, 21, CreateMapLink(763, 467, 21.0f, 26.0f), "The Snow Queen", "Queen"),             // Eternity
                new EurekaFate(1369, 22, CreateMapLink(763, 467, 25.0f, 28.0f), "Taxim", "Taxim"),                      // Cairn Blight 451
                new EurekaFate(1353, 23, CreateMapLink(763, 467, 29.0f, 30.0f), "Ash Dragon", "Dragon"),                // Ash the Magic Dragon
                new EurekaFate(1354, 24, CreateMapLink(763, 467, 32.0f, 26.0f), "Glavoid", "Glavoid"),                  // Conqueror Worm
                new EurekaFate(1355, 25, CreateMapLink(763, 467, 34.0f, 21.0f), "Anapos", "Anapos"),                    // Melting Point
                new EurekaFate(1366, 26, CreateMapLink(763, 467, 29.0f, 22.0f), "Hakutaku", "Haku"),                    // The Wobbler in Darkness
                new EurekaFate(1357, 27, CreateMapLink(763, 467, 17.0f, 16.0f), "King Igloo", "Igloo"),                 // Does It Have to Be a Snowman
                new EurekaFate(1356, 28, CreateMapLink(763, 467, 10.0f, 10.0f), "Asag", "Asag"),                        // Disorder in the Court
                new EurekaFate(1352, 29, CreateMapLink(763, 467, 9.8f, 19.0f), "Surabhi", "Surabhi"),                  // Cows for Concern
                new EurekaFate(1360, 30, CreateMapLink(763, 467, 8.7f,  15.4f), "King Arthro", "Arthro"),               // Morte Arthro
                new EurekaFate(1358, 31, CreateMapLink(763, 467, 13.0f, 18.0f), "Mindertaur/Eldertaur", "Brothers"),    // Brothers
                new EurekaFate(1361, 32, CreateMapLink(763, 467, 26.5f, 16.9f), "Holy Cow", "Holy Cow"),                // Apocalypse Cow
                new EurekaFate(1362, 33, CreateMapLink(763, 467, 31.0f, 18.6f), "Hadhayosh", "Behe"),                   // Third Impact
                new EurekaFate(1359, 34, CreateMapLink(763, 467, 25.0f, 19.0f), "Horus", "Horus"),                      // Eye of Horus
                new EurekaFate(1363, 35, CreateMapLink(763, 467, 23.6f, 25.0f), "Arch Angra Mainyu", "Mainyu"),         // Eye Scream for Ice Cream
                new EurekaFate(1365, 36, CreateMapLink(763, 467, 22.0f, 14.0f), "Copycat Cassie", "Cassie"),            // Cassie and the Copycats
                new EurekaFate(1364, 37, CreateMapLink(763, 467, 36.0f, 19.0f), "Louhi", "Louhi"),                      // Louhi on Ice
            };
        }

        private List<EurekaFate> InitializePyrosFates()
        {
            return new()
            {
                new EurekaFate(1388, 38, CreateMapLink(795, 484, 27.0f, 26.0f), "Leucosia", "Leucosia"),                // Medias Res
                new EurekaFate(1389, 39, CreateMapLink(795, 484, 29.0f, 29.0f), "Flauros", "Flauros"),                  // High Voltage
                new EurekaFate(1390, 40, CreateMapLink(795, 484, 31.0f, 31.0f), "The Sophist", "Sophist"),              // On the Nonexistent
                new EurekaFate(1391, 41, CreateMapLink(795, 484, 23.0f, 37.0f), "Graffiacane", "Doll"),                 // Creepy Doll
                new EurekaFate(1392, 42, CreateMapLink(795, 484, 19.2f, 29.2f), "Askalaphos", "Owl"),                   // Quiet, Please
                new EurekaFate(1393, 43, CreateMapLink(795, 484, 18.0f, 14.0f), "Grand Duke Batym", "Batym"),           // Up and Batym
                new EurekaFate(1394, 44, CreateMapLink(795, 484, 10.0f, 14.0f), "Aetolus", "Aetolus"),                  // Rondo Aetolus
                new EurekaFate(1395, 45, CreateMapLink(795, 484, 12.6f, 11.0f), "Lesath", "Lesath"),                    // Scorchpion King
                new EurekaFate(1396, 46, CreateMapLink(795, 484, 15.0f, 6.5f), "Eldthurs", "Eldthurs"),                 // Burning Hunger
                new EurekaFate(1397, 47, CreateMapLink(795, 484, 21.0f, 11.0f), "Iris", "Iris"),                        // Dry Iris
                new EurekaFate(1398, 48, CreateMapLink(795, 484, 21.6f, 8.3f), "Lamebrix Strikebocks", "Lamebrix"),     // Thirty Whacks
                new EurekaFate(1399, 49, CreateMapLink(795, 484, 27.0f, 9.0f), "Dux", "Dux"),                           // Put Up Your Dux
                new EurekaFate(1400, 50, CreateMapLink(795, 484, 29.0f, 11.0f), "Lumber Jack", "Jack"),                 // You Do Know Jack
                new EurekaFate(1401, 51, CreateMapLink(795, 484, 31.0f, 15.0f), "Glaukopis", "Glaukopis"),              // Mister Bright-eyes
                new EurekaFate(1402, 52, CreateMapLink(795, 484, 11.5f, 34.2f), "Ying-Yang", "YY"),                     // Haunter of the Dark
                new EurekaFate(1403, 53, CreateMapLink(795, 484, 24.0f, 30.0f), "Skoll", "Skoll"),                      // Heavens' Warg
                new EurekaFate(1404, 54, CreateMapLink(795, 484, 35.0f, 6.0f), "Penthesilea", "Penny"),                 // Lost Epic
            };
        }

        private List<EurekaFate> InitializeHydatosFates()
        {
            return new()
            {
                new EurekaFate(1412, 55,    CreateMapLink(827, 515, 11.1f, 25.2f), "Khalamari", "Khalamari"),           // I Ink, Therefore I Am
                new EurekaFate(1413, 56,    CreateMapLink(827, 515, 10.0f, 17.8f), "Stegodon", "Stegodon"),             // From Tusk till Dawn
                new EurekaFate(1414, 57,    CreateMapLink(827, 515, 7.9f,  22.1f), "Molech", "Molech"),                 // Bullheaded Berserker
                new EurekaFate(1415, 58,    CreateMapLink(827, 515, 6.7f,  14.4f), "Piasa", "Piasa"),                   // Mad, Bad, and Fabulous to Know
                new EurekaFate(1416, 59,    CreateMapLink(827, 515, 7.8f,  26.1f), "Frostmane", "Frostmane"),           // Fearful Symmetry
                new EurekaFate(1417, 60,    CreateMapLink(827, 515, 25.4f, 16.3f), "Daphne", "Daphne"),                 // Crawling Chaos
                new EurekaFate(1418, 61,    CreateMapLink(827, 515, 28.9f, 23.6f), "King Goldemar", "Golde"),           // Duty-free
                new EurekaFate(1419, 62,    CreateMapLink(827, 515, 37.0f, 26.0f), "Leuke", "Leuke"),                   // Leukewarm Reception
                new EurekaFate(1420, 63,    CreateMapLink(827, 515, 32.5f, 24.7f), "Barong", "Barong"),                 // Robber Barong
                new EurekaFate(1421, 64,    CreateMapLink(827, 515, 36.5f, 13.5f), "Ceto", "Ceto"),                     // Stone-cold Killer
                new EurekaFate(1423, 65,    CreateMapLink(827, 515, 32.7f, 19.6f), "Provenance Watcher", "PW"),         // Crystalline Provenance
                new EurekaFate(1424, 1337,  CreateMapLink(827, 515, 26.8f, 29.0f), "Ovni", "Ovni"),                     // I Don't Want to Believe
            };
        }
#pragma warning restore format
    }
}
