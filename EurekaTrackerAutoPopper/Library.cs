using Dalamud.Game.Text.SeStringHandling;
using System.Collections.Generic;

namespace EurekaTrackerAutoPopper
{
    public static class Library
    {
        public class EurekaFate
        {
            public ushort fateId;
            public ushort? trackerId;
            public string name;
            public string shortName;
            public SeString mapLink;

            public EurekaFate(ushort fateId, ushort? trackerId, SeString mapLink, string name, string shortName)
            {
                this.fateId = fateId;
                this.trackerId = trackerId;
                this.name = name;
                this.shortName = shortName;
                this.mapLink = mapLink;
            }
        }

        private static List<EurekaFate>? anemosFates = null;
        private static List<EurekaFate>? pagosFates = null;
        private static List<EurekaFate>? pyrosFates = null;
        private static List<EurekaFate>? hydatosFates = null;
        private static Dictionary<ushort, List<EurekaFate>>? territoryToFateDictionary = null;

        public static readonly Dictionary<ushort, short> TerritoryToTrackerDictionary = new()
        {
            { 827, 4 },
            { 795, 3 },
            { 763, 2 },
            { 732, 1 },
        };

        public static Dictionary<ushort, List<EurekaFate>> TerritoryToFateDictionary => territoryToFateDictionary ??= new()
        {
            { 827, HydatosFates },
            { 795, PyrosFates },
            { 763, PagosFates },
            { 732, AnemosFates },
        };

        public static List<EurekaFate> AnemosFates => anemosFates ??= InitializeAnemosFates();
        public static List<EurekaFate> PagosFates => pagosFates ??= InitializePagosFates();
        public static List<EurekaFate> PyrosFates => pyrosFates ??= InitializePyrosFates();
        public static List<EurekaFate> HydatosFates => hydatosFates ??= InitializeHydatosFates();

        private static List<EurekaFate> InitializeAnemosFates()
        {
#pragma warning disable format
            return new()
            {
                new EurekaFate(1332, 1,  SeString.CreateMapLink(732, 414, 14.0f, 22.3f), "Sabotender Corrido", "Sabo"),          // Unsafety Dance
                new EurekaFate(1348, 2,  SeString.CreateMapLink(732, 414, 29.8f, 27.3f), "The Lord of Anemos", "Lord"),          // The Shadow over Anemos
                new EurekaFate(1333, 3,  SeString.CreateMapLink(732, 414, 26.0f, 28.0f), "Teles", "Teles"),                      // Teles House
                new EurekaFate(1328, 4,  SeString.CreateMapLink(732, 414, 17.0f, 22.0f), "The Emperor of Anemos", "Emperor"),    // The Swarm Never Sets
                new EurekaFate(1344, 5,  SeString.CreateMapLink(732, 414, 26.0f, 22.0f), "Callisto", "Callisto"),                // One Missed Callisto
                new EurekaFate(1347, 6,  SeString.CreateMapLink(732, 414, 23.2f, 22.2f), "Number", "Number"),                    // By Numbers
                new EurekaFate(1345, 7,  SeString.CreateMapLink(732, 414, 19.1f, 19.6f), "Jahannam", "Jaha"),                    // Disinherit the Wind
                new EurekaFate(1334, 8,  SeString.CreateMapLink(732, 414, 14.7f, 15.3f), "Amemet", "Amemet"),                    // Prove Your Amemettle
                new EurekaFate(1335, 9,  SeString.CreateMapLink(732, 414, 14.0f, 13.0f), "Caym", "Caym"),                        // Caym What May
                new EurekaFate(1336, 10, SeString.CreateMapLink(732, 414, 28.2f, 20.3f), "Bombadeel", "Bomba"),                  // The Killing of a Sacred Bombardier
                new EurekaFate(1339, 11, SeString.CreateMapLink(732, 414, 25.0f, 18.0f), "Serket", "Serket"),                    // Short Serket 2
                new EurekaFate(1346, 12, SeString.CreateMapLink(732, 414, 21.9f, 14.5f), "Judgmental Julika", "Julika"),         // Don't Judge Me, Morbol
                new EurekaFate(1343, 13, SeString.CreateMapLink(732, 414, 20.0f, 13.0f), "The White Rider", "Rider"),            // When You Ride Alone
                new EurekaFate(1337, 14, SeString.CreateMapLink(732, 414, 26.5f, 14.7f), "Polyphemus", "Poly"),                  // Sing, Muse
                new EurekaFate(1342, 15, SeString.CreateMapLink(732, 414, 29.1f, 13.0f), "Simurgh's Strider", "Strider"),        // Simurghasbord
                new EurekaFate(1341, 16, SeString.CreateMapLink(732, 414, 34.9f, 19.1f), "King Hazmat", "Hazmat"),               // To the Mat
                new EurekaFate(1331, 17, SeString.CreateMapLink(732, 414, 36.0f, 22.0f), "Fafnir", "Fafnir"),                    // Wine and Honey
                new EurekaFate(1340, 18, SeString.CreateMapLink(732, 414, 8.0f,  18.0f), "Amarok", "Amarok"),                    // I Amarok
                new EurekaFate(1338, 19, SeString.CreateMapLink(732, 414, 8.0f,  23.0f), "Lamashtu", "Lamashtu"),                // Drama Lamashtu
                new EurekaFate(1329, 20, SeString.CreateMapLink(732, 414, 7.0f,  22.0f), "Pazuzu", "Paz"),                       // Wail in the Willows
            };
#pragma warning restore format
        }
        private static List<EurekaFate> InitializePagosFates()
        {
#pragma warning disable format
            return new()
            {
                new EurekaFate(1351, 21, SeString.CreateMapLink(763, 467, 21.0f, 26.0f), "The Snow Queen", "Queen"),             // Eternity
                new EurekaFate(1369, 22, SeString.CreateMapLink(763, 467, 25.0f, 28.0f), "Taxim", "Taxim"),                      // Cairn Blight 451
                new EurekaFate(1353, 23, SeString.CreateMapLink(763, 467, 29.0f, 30.0f), "Ash Dragon", "Dragon"),                // Ash the Magic Dragon
                new EurekaFate(1354, 24, SeString.CreateMapLink(763, 467, 32.0f, 26.0f), "Glavoid", "Glavoid"),                  // Conqueror Worm
                new EurekaFate(1355, 25, SeString.CreateMapLink(763, 467, 34.0f, 21.0f), "Anapos", "Anapos"),                    // Melting Point
                new EurekaFate(1366, 26, SeString.CreateMapLink(763, 467, 29.0f, 22.0f), "Hakutaku", "Haku"),                    // The Wobbler in Darkness
                new EurekaFate(1357, 27, SeString.CreateMapLink(763, 467, 17.0f, 16.0f), "King Igloo", "Igloo"),                 // Does It Have to Be a Snowman
                new EurekaFate(1356, 28, SeString.CreateMapLink(763, 467, 10.0f, 10.0f), "Asag", "Asag"),                        // Disorder in the Court
                new EurekaFate(1352, 29, SeString.CreateMapLink(763, 467, 10.0f, 20.0f), "Surabhi", "Surabhi"),                  // Cows for Concern
                new EurekaFate(1360, 30, SeString.CreateMapLink(763, 467, 8.7f,  15.4f), "King Arthro", "Arthro"),               // Morte Arthro
                new EurekaFate(1358, 31, SeString.CreateMapLink(763, 467, 13.0f, 18.0f), "Mindertaur/Eldertaur", "Brothers"),    // Brothers
                new EurekaFate(1361, 32, SeString.CreateMapLink(763, 467, 26.0f, 16.0f), "Holy Cow", "Holy Cow"),                // Apocalypse Cow
                new EurekaFate(1362, 33, SeString.CreateMapLink(763, 467, 31.0f, 18.6f), "Hadhayosh", "Behe"),                   // Third Impact
                new EurekaFate(1359, 34, SeString.CreateMapLink(763, 467, 25.0f, 19.0f), "Horus", "Horus"),                      // Eye of Horus
                new EurekaFate(1363, 35, SeString.CreateMapLink(763, 467, 23.6f, 25.0f), "Arch Angra Mainyu", "Mainyu"),         // Eye Scream for Ice Cream
                new EurekaFate(1365, 36, SeString.CreateMapLink(763, 467, 22.0f, 14.0f), "Copycat Cassie", "Cassie"),            // Cassie and the Copycats
                new EurekaFate(1364, 37, SeString.CreateMapLink(763, 467, 36.0f, 19.0f), "Louhi", "Louhi"),                      // Louhi on Ice
            };
#pragma warning restore format
        }
        private static List<EurekaFate> InitializePyrosFates()
        {
#pragma warning disable format
            return new()
            {
                new EurekaFate(1388, 38, SeString.CreateMapLink(795, 484, 27.0f, 26.0f), "Leucosia", "Leucosia"),               // Medias Res
                new EurekaFate(1389, 39, SeString.CreateMapLink(795, 484, 29.0f, 29.0f), "Flauros", "Flauros"),                // High Voltage
                new EurekaFate(1390, 40, SeString.CreateMapLink(795, 484, 31.0f, 31.0f), "The Sophist", "Sophist"),            // On the Nonexistent
                new EurekaFate(1391, 41, SeString.CreateMapLink(795, 484, 23.0f, 37.0f), "Graffiacane", "Doll"),            // Creepy Doll
                new EurekaFate(1392, 42, SeString.CreateMapLink(795, 484, 19.2f, 29.2f), "Askalaphos", "Owl"),             // Quiet, Please
                new EurekaFate(1393, 43, SeString.CreateMapLink(795, 484, 18.0f, 14.0f), "Grand Duke Batym", "Batym"),       // Up and Batym
                new EurekaFate(1394, 44, SeString.CreateMapLink(795, 484, 10.0f, 14.0f), "Aetolus", "Aetolus"),                // Rondo Aetolus
                new EurekaFate(1395, 45, SeString.CreateMapLink(795, 484, 13.0f, 11.0f), "Lesath", "Lesath"),                 // Scorchpion King
                new EurekaFate(1396, 46, SeString.CreateMapLink(795, 484, 15.0f, 6.5f),  "Eldthurs", "Eldthurs"),               // Burning Hunger
                new EurekaFate(1397, 47, SeString.CreateMapLink(795, 484, 21.0f, 11.0f), "Iris", "Iris"),                   // Dry Iris
                new EurekaFate(1398, 48, SeString.CreateMapLink(795, 484, 21.6f, 8.3f),  "Lamebrix Strikebocks", "Lamebrix"),   // Thirty Whacks
                new EurekaFate(1399, 49, SeString.CreateMapLink(795, 484, 27.0f, 9.0f),  "Dux", "Dux"),                    // Put Up Your Dux
                new EurekaFate(1400, 50, SeString.CreateMapLink(795, 484, 29.0f, 11.0f), "Lumber Jack", "Jack"),            // You Do Know Jack
                new EurekaFate(1401, 51, SeString.CreateMapLink(795, 484, 31.0f, 15.0f), "Glaukopis", "Glaukopis"),              // Mister Bright-eyes
                new EurekaFate(1402, 52, SeString.CreateMapLink(795, 484, 11.5f, 34.2f), "Ying-Yang", "YY"),              // Haunter of the Dark
                new EurekaFate(1403, 53, SeString.CreateMapLink(795, 484, 24.0f, 30.0f), "Skoll", "Skoll"),                  // Heavens' Warg
                new EurekaFate(1404, 54, SeString.CreateMapLink(795, 484, 35.0f, 6.0f),  "Penthesilea", "Penny"),            // Lost Epic
            };
#pragma warning restore format
        }
        private static List<EurekaFate> InitializeHydatosFates()
        {
#pragma warning disable format
            return new()
            {
                new EurekaFate(1412, 55,    SeString.CreateMapLink(827, 515, 11.1f, 25.2f), "Khalamari", "Khalamari"),            // I Ink, Therefore I Am
                new EurekaFate(1413, 56,    SeString.CreateMapLink(827, 515, 10.0f, 17.8f), "Stegodon", "Stegodon"),             // From Tusk till Dawn
                new EurekaFate(1414, 57,    SeString.CreateMapLink(827, 515, 7.9f,  22.1f), "Molech", "Molech"),               // Bullheaded Berserker
                new EurekaFate(1415, 58,    SeString.CreateMapLink(827, 515, 6.7f,  14.4f), "Piasa", "Piasa"),                // Mad, Bad, and Fabulous to Know
                new EurekaFate(1416, 59,    SeString.CreateMapLink(827, 515, 7.8f,  26.1f), "Frostmane", "Frostmane"),            // Fearful Symmetry
                new EurekaFate(1417, 60,    SeString.CreateMapLink(827, 515, 25.4f, 16.3f), "Daphne", "Daphne"),               // Crawling Chaos
                new EurekaFate(1418, 61,    SeString.CreateMapLink(827, 515, 28.9f, 23.6f), "King Goldemar", "Golde"),        // Duty-free
                new EurekaFate(1419, 62,    SeString.CreateMapLink(827, 515, 37.0f, 26.0f), "Leuke", "Leuke"),                // Leukewarm Reception
                new EurekaFate(1420, 63,    SeString.CreateMapLink(827, 515, 32.5f, 24.7f), "Barong", "Barong"),               // Robber Barong
                new EurekaFate(1421, 64,    SeString.CreateMapLink(827, 515, 36.5f, 13.5f), "Ceto", "Ceto"),                 // Stone-cold Killer
                new EurekaFate(1423, 65,    SeString.CreateMapLink(827, 515, 32.7f, 19.6f), "Provenance Watcher", "PW"),   // Crystalline Provenance
                new EurekaFate(1424, null,  SeString.CreateMapLink(827, 515, 26.8f, 29.0f), "Ovni", "Ovni"),                 // I Don't Want to Believe
            };
#pragma warning restore format
        }
    }
}
