using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Plugin.Services;
using EurekaTrackerAutoPopper.Data;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine.Layer;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace EurekaTrackerAutoPopper;

public class Fates
{
    private readonly Plugin Plugin;

    public HashSet<uint> TriggerMonsters;

    public readonly List<Fate> BunnyFates =
    [
        // Eureka
        new(1367, Territory.Pagos, new Vector3(-168.20723f, -737.0106f, 304.78036f), " (South)"),
        new(1368, Territory.Pagos, new Vector3(-45.060673f, -542.2534f, -10.444342f), " (North)"),
        new(1407, Territory.Pyros, new Vector3(123.93088f, 706.2543f, 235.71927f), " (South)"),
        new(1408, Territory.Pyros, new Vector3(172.66713f, 679.68823f, -514.0787f), " (North)"),
        new(1425, Territory.Hydatos, new Vector3(-369.96432f, 499.13068f, -477.4539f), ""),

        // Occult
        new(1976, Territory.SouthHorn, new Vector3(204.66835f, 111.81729f, -204.96242f), [47749, 47738], OccultAetheryte.CrystallizedCaverns, 40, " (North)"),
        new(1977, Territory.SouthHorn, new Vector3(-479.8395f, 75f, 524.78894f), [47745, 47738], OccultAetheryte.Stonemarsh, 18, " (South)"),

        new(2072, Territory.NorthHorn, new Vector3(233f, 7.729229f, -470f), [50976], OccultAetheryte.SinkingSanctuary, 12, " (North)"),
        new(2073, Territory.NorthHorn, new Vector3(-505.2822f, 53.14409f, 244.041f), [50975], OccultAetheryte.SuspendedMasonry, 24, " (South)"),
    ];

    public readonly List<Fate> AllFates =
    [
        new(1597, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // Sneak & Spell
        new(1598, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // None of Them Knew They Were Robots
        new(1599, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // The Beasts Must Die
        new(1600, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // Unrest for the Wicked
        new(1601, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // More Machine Now than Man
        new(1602, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // Can Carnivorous Plants Bloom Even on a Battlefield?
        new(1603, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // Seeq and Destroy
        new(1604, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // All Pets Are Of
        new(1605, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // Conflicting with the First Law
        new(1606, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // Brought to Heal
        new(1607, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // The Monster Mash
        new(1608, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // Red (Chocobo) Alert
        new(1609, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // Unicorn Flakes
        new(1610, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // Parts and Recreation
        new(1611, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // The Element of Supplies
        new(1612, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // Heavy Boots of Lead
        new(1613, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // No Camping Allowed
        new(1614, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // Scavengers of Man's Sorrow
        new(1615, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // Help Wanted
        new(1616, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // Pyromancer Supreme
        new(1617, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // Waste the Rainbow
        new(1618, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // The Wild Bunch
        new(1619, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // My Family and Other Animals
        new(1620, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // I'm a Mechanical Man
        new(1621, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // Murder Death Kill
        new(1622, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // Desperately Seeking Something
        new(1623, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // Supplies Party
        new(1624, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // Demonstrably Demonic
        new(1625, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // For Absent Friends
        new(1626, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // Of Steel and Flame
        new(1627, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // Let Slip the Dogs of War
        new(1628, Territory.Bozja, new Vector3(0f, 0f, 0f), []), // The War Against the Machines

        new(1717, Territory.Zadnor, new Vector3(0f, 0f, 0f), []), // Of Beasts and Braggadocio
        new(1718, Territory.Zadnor, new Vector3(0f, 0f, 0f), []), // Parts and Parcel
        new(1719, Territory.Zadnor, new Vector3(0f, 0f, 0f), []), // An Immoral Dilemma
        new(1720, Territory.Zadnor, new Vector3(0f, 0f, 0f), []), // Deadly Divination
        new(1721, Territory.Zadnor, new Vector3(0f, 0f, 0f), []), // A Wrench in the Reconnaissance Effort
        new(1722, Territory.Zadnor, new Vector3(0f, 0f, 0f), []), // Another Pilot Episode
        new(1723, Territory.Zadnor, new Vector3(0f, 0f, 0f), []), // Breaking the Ice
        new(1724, Territory.Zadnor, new Vector3(0f, 0f, 0f), []), // Meet the Puppetmaster
        new(1725, Territory.Zadnor, new Vector3(0f, 0f, 0f), []), // Challenge Accepted
        new(1726, Territory.Zadnor, new Vector3(0f, 0f, 0f), []), // Th'uban the Terrible
        new(1727, Territory.Zadnor, new Vector3(0f, 0f, 0f), []), // An End to Atrocities
        new(1728, Territory.Zadnor, new Vector3(0f, 0f, 0f), []), // A Just Pursuit
        new(1729, Territory.Zadnor, new Vector3(0f, 0f, 0f), []), // Tanking Up
        new(1730, Territory.Zadnor, new Vector3(0f, 0f, 0f), []), // Supersoldier Rising
        new(1731, Territory.Zadnor, new Vector3(0f, 0f, 0f), []), // Demented Mentor
        new(1732, Territory.Zadnor, new Vector3(0f, 0f, 0f), []), // Sever the Strings
        new(1733, Territory.Zadnor, new Vector3(0f, 0f, 0f), []), // The Beasts Are Back
        new(1734, Territory.Zadnor, new Vector3(0f, 0f, 0f), []), // Still Only Counts as One
        new(1735, Territory.Zadnor, new Vector3(0f, 0f, 0f), []), // Seeq and You Will Find
        new(1736, Territory.Zadnor, new Vector3(0f, 0f, 0f), []), // Mean-spirited
        new(1737, Territory.Zadnor, new Vector3(0f, 0f, 0f), []), // A Relic Unleashed
        new(1738, Territory.Zadnor, new Vector3(0f, 0f, 0f), []), // When Mages Rage
        new(1739, Territory.Zadnor, new Vector3(0f, 0f, 0f), []), // Hypertuned Havoc
        new(1740, Territory.Zadnor, new Vector3(0f, 0f, 0f), []), // Attack of the Supersoldiers
        new(1741, Territory.Zadnor, new Vector3(0f, 0f, 0f), []), // The Student Becalms the Master
        new(1742, Territory.Zadnor, new Vector3(0f, 0f, 0f), []), // Attack of the Machines

        new(1962, Territory.SouthHorn, new Vector3(151.38765f, 56f, 670.072f), [47744], OccultAetheryte.Eldergrowth, 28), // Rough Waters
        new(1963, Territory.SouthHorn, new Vector3(364.61816f, 70f, 489.55896f), [47744], OccultAetheryte.Eldergrowth, 14), // The Golden Guardian
        new(1964, Territory.SouthHorn, new Vector3(-217.46391f, 116.70241f, 265.08792f), [47749], OccultAetheryte.Stonemarsh, 10), // King of the Crescent
        new(1965, Territory.SouthHorn, new Vector3(-221.13199f, 107f, 40.158627f), [47747], OccultAetheryte.TheWanderersHaven, 27), // The Winged Terror
        new(1966, Territory.SouthHorn, new Vector3(-221.1495f, 106.99999f, 40.21738f), [47746], OccultAetheryte.CrystallizedCaverns, 26), // An Unending Duty
        new(1967, Territory.SouthHorn, new Vector3(-40.98325f, 111.68926f, -316.82162f), [47747], OccultAetheryte.CrystallizedCaverns, 24), // Brain Drain
        new(1968, Territory.SouthHorn, new Vector3(-369.61337f, 75f, 649.92035f), [47745], OccultAetheryte.Stonemarsh, 25), // A Delicate Balance
        new(1969, Territory.SouthHorn, new Vector3(-589.41364f, 96.2f, 330.6984f), [47745], OccultAetheryte.Stonemarsh, 18), // Sworn to Soil
        new(1970, Territory.SouthHorn, new Vector3(-57.992046f, 69.50635f, 561.93933f), [47744], OccultAetheryte.Stonemarsh, 29), // A Prying Eye
        new(1971, Territory.SouthHorn, new Vector3(76.327644f, 96.94907f, 275.7444f), [47749], OccultAetheryte.Eldergrowth, 17), // Fatal Allure
        new(1972, Territory.SouthHorn, new Vector3(413.7364f, 95.999985f, -14.67076f), [47748], OccultAetheryte.Eldergrowth, 24), // Serving Darkness

        new(2074, Territory.NorthHorn, new Vector3(724f, 70f, 220f), [50974], OccultAetheryte.CrownOfKarnak, 26, weakness: Weakness.Fire), // Raging Thrall
        new(2075, Territory.NorthHorn, new Vector3(510f, 16.76658f, -29.99999f), [50975], OccultAetheryte.CrownOfKarnak, 37, weakness: Weakness.Fire), // Eye to Eye
        new(2076, Territory.NorthHorn, new Vector3(95f, 10f, 470f), [50976], OccultAetheryte.CrownOfKarnak, 24, weakness: Weakness.Wind), // Shoreline Showdown
        new(2077, Territory.NorthHorn, new Vector3(330f, 0f, -250f), [50974], OccultAetheryte.SinkingSanctuary, 21, weakness: Weakness.Lightning), // Waved Away
        new(2078, Territory.NorthHorn, new Vector3(-402.0002f, 29.76808f, -252.9997f), [50975], OccultAetheryte.MolderingOutskirts, 10, weakness: Weakness.Fire), // Allure of the Occult
        new(2079, Territory.NorthHorn, new Vector3(-170f, 30f, -500f), [50976], OccultAetheryte.MolderingOutskirts, 14, weakness: Weakness.Fire), // Inconstant Gardener
        new(2080, Territory.NorthHorn, new Vector3(-90f, 67.47852f, 865.9999f), [50975], OccultAetheryte.SuspendedMasonry, 35, weakness: Weakness.Fire), // Territorial Dispute
        new(2081, Territory.NorthHorn, new Vector3(-440f, 47.02659f, -790f), [50974], OccultAetheryte.MolderingOutskirts, 28, weakness: Weakness.Wind), // A Rotten Affair
        new(2082, Territory.NorthHorn, new Vector3(-855.7433f, 70.67716f, 482.1518f), [50974], OccultAetheryte.SuspendedMasonry, 22, weakness: Weakness.Fire), // Gale-force Encounter
        new(2083, Territory.NorthHorn, new Vector3(-661.0049f, 87f, -54.00021f), [50976], OccultAetheryte.MolderingOutskirts, 39, weakness: Weakness.Ice), // Scale Model
        new(2084, Territory.NorthHorn, new Vector3(140f, 37f, -708f), [50975], OccultAetheryte.SinkingSanctuary, 17, weakness: Weakness.Fire), // Thunderregnum
    ];

    public readonly List<Fate> AllCriticalEncounters =
    [
        new(1, Territory.Bozja, new Vector3(0f, 0f, 0f), [], trigger: 13879), // Kill It with Fire
        new(2, Territory.Bozja, new Vector3(0f, 0f, 0f), [], trigger: 13879), // The Baying of the Hound(s)
        new(3, Territory.Bozja, new Vector3(0f, 0f, 0f), [], trigger: 13879), // Vigil for the Lost
        new(4, Territory.Bozja, new Vector3(0f, 0f, 0f), [], trigger: 13879), // Aces High
        new(5, Territory.Bozja, new Vector3(0f, 0f, 0f), [], trigger: 13879), // The Shadow of Death's Hand
        new(6, Territory.Bozja, new Vector3(0f, 0f, 0f), [], trigger: 13879), // The Final Furlong
        new(7, Territory.Bozja, new Vector3(0f, 0f, 0f), [], trigger: 13879), // The Hunt for Red Choctober
        new(8, Territory.Bozja, new Vector3(0f, 0f, 0f), [], trigger: 13879), // Beast of Man
        new(9, Territory.Bozja, new Vector3(0f, 0f, 0f), [], trigger: 13879), // The Fires of War
        new(10, Territory.Bozja, new Vector3(0f, 0f, 0f), [], trigger: 13879), // Patriot Games
        new(11, Territory.Bozja, new Vector3(0f, 0f, 0f), [], trigger: 13879), // Trampled under Hoof
        new(12, Territory.Bozja, new Vector3(0f, 0f, 0f), [], trigger: 13879), // And the Flames Went Higher
        new(13, Territory.Bozja, new Vector3(0f, 0f, 0f), [], trigger: 13879), // Metal Fox Chaos
        new(14, Territory.Bozja, new Vector3(0f, 0f, 0f), [], trigger: 13879), // Rise of the Robots
        new(15, Territory.Bozja, new Vector3(0f, 0f, 0f), [], trigger: 13879), // Where Strode the Behemoth

        new(16, Territory.Bozja, new Vector3(0f, 0f, 0f), [], special: true), // Castrum

        new(17, Territory.Zadnor, new Vector3(0f, 0f, 0f), [], trigger: 13879), // On Serpents' Wings
        new(18, Territory.Zadnor, new Vector3(0f, 0f, 0f), [], trigger: 13879), // Feeling the Burn
        new(19, Territory.Zadnor, new Vector3(0f, 0f, 0f), [], trigger: 13879), // The Broken Blade
        new(20, Territory.Zadnor, new Vector3(0f, 0f, 0f), [], trigger: 13879), // From Beyond the Grave
        new(21, Territory.Zadnor, new Vector3(0f, 0f, 0f), [], trigger: 13879), // With Diremite and Main
        new(22, Territory.Zadnor, new Vector3(0f, 0f, 0f), [], trigger: 13879), // Here Comes the Cavalry
        new(23, Territory.Zadnor, new Vector3(0f, 0f, 0f), [], trigger: 13879), // Head of the Snake
        new(24, Territory.Zadnor, new Vector3(0f, 0f, 0f), [], trigger: 13879), // There Would Be Blood
        new(25, Territory.Zadnor, new Vector3(0f, 0f, 0f), [], trigger: 13879), // Never Cry Wolf
        new(26, Territory.Zadnor, new Vector3(0f, 0f, 0f), [], trigger: 13879), // Time to Burn
        new(27, Territory.Zadnor, new Vector3(0f, 0f, 0f), [], trigger: 13879), // Lean, Mean, Magitek Machines
        new(28, Territory.Zadnor, new Vector3(0f, 0f, 0f), [], trigger: 13879), // Worn to a Shadow
        new(29, Territory.Zadnor, new Vector3(0f, 0f, 0f), [], trigger: 13879), // A Familiar Face
        new(30, Territory.Zadnor, new Vector3(0f, 0f, 0f), [], trigger: 13879), // Looks to Die For
        new(31, Territory.Zadnor, new Vector3(0f, 0f, 0f), [], trigger: 13879), // Taking the Lyon's Share

        new(32, Territory.Zadnor, new Vector3(0f, 0f, 0f), [], special: true), // The Dalriada

        new(33, Territory.SouthHorn, new Vector3(299.92032f, 70f, 729.9832f), [49831, 49826, 47744], OccultAetheryte.Eldergrowth, 30, trigger: 13879), // Scourge of the Mind
        new(34, Territory.SouthHorn, new Vector3(450.28986f, 65f, 356.46573f), [49831, 49826, 47749, 47752, 47732], OccultAetheryte.Eldergrowth, 10), // The Black Regiment
        new(35, Territory.SouthHorn, new Vector3(620.17365f, 79f, 800.0485f), [49831, 49826, 47744, 47751, 47730], OccultAetheryte.Eldergrowth, 48), // The Unbridled
        new(36, Territory.SouthHorn, new Vector3(680.90576f, 74f, 534.0728f), [49831, 49826, 47744], OccultAetheryte.Eldergrowth, 33), // Crawling Death
        new(37, Territory.SouthHorn, new Vector3(-340.11813f, 75f, 800.0618f), [49831, 49826, 47745, 47728, 48008], OccultAetheryte.Stonemarsh, 33, trigger: 13875), // Calamity Bound
        new(38, Territory.SouthHorn, new Vector3(-413.43665f, 92f, 74.68839f), [49833, 49828, 47746], OccultAetheryte.CrystallizedCaverns, 17), // Trial by Claw
        new(39, Territory.SouthHorn, new Vector3(-799.84845f, 43.99998f, 245.20094f), [49833, 49828, 47746, 47729], OccultAetheryte.Stonemarsh, 37, trigger: 13895), // From Times Bygone
        new(40, Territory.SouthHorn, new Vector3(676.5143f, 96.03f, -254.43198f), [49827, 49832, 47748], OccultAetheryte.ExpeditionBaseCamp, 36), // Company of Stone
        new(41, Territory.SouthHorn, new Vector3(-117.018456f, 1f, -850.34644f), [49833, 49828, 47747, 47731], OccultAetheryte.TheWanderersHaven, 17, trigger: 13913), // Shark Attack
        new(42, Territory.SouthHorn, new Vector3(629.3389f, 108f, -52.77268f), [49827, 49832, 47748, 47757], OccultAetheryte.Eldergrowth, 42, trigger: 13876), // On the Hunt
        new(43, Territory.SouthHorn, new Vector3(-353.2408f, 5f, -606.3008f), [49833, 49828, 47747], OccultAetheryte.TheWanderersHaven, 12), // With Extreme Prejudice
        new(44, Territory.SouthHorn, new Vector3(457.3497f, 97f, -357.9041f), [49827, 49832, 47749], OccultAetheryte.ExpeditionBaseCamp, 36, trigger: 13884), // Noise Complaint
        new(45, Territory.SouthHorn, new Vector3(72.06891f, 20f, -549.957f), [49827, 49832, 47747, 47733], OccultAetheryte.TheWanderersHaven, 17), // Cursed Concern
        new(46, Territory.SouthHorn, new Vector3(870.55774f, 122f, 180.04774f), [49827, 49832, 47748], OccultAetheryte.Eldergrowth, 57), // Eternal Watch
        new(47, Territory.SouthHorn, new Vector3(-569.202f, 97f, -158.79793f), [49833, 49828, 47746], OccultAetheryte.CrystallizedCaverns, 14), // Flame of Dusk

        new(48, Territory.SouthHorn, new Vector3(63.066174f, 126.499985f, 3.8296576f), [47868, 47734, 47735, 47736, 47737], OccultAetheryte.Eldergrowth, 25, special: true), // The Forked Tower: Blood

        new(49, Territory.NorthHorn, new Vector3(-870f, 20f, -560f), [49826, 49831, 50974], OccultAetheryte.MolderingOutskirts, 36, trigger: 14908, weakness: Weakness.Fire), // Many Mouths to Feed
        new(50, Territory.NorthHorn, new Vector3(-215f, 18f, -65f), [49832, 49827, 51988, 50976], OccultAetheryte.UnhallowedHamlet, 17, trigger: 14896, weakness: Weakness.Wind), // Doubled Trouble
        new(51, Territory.NorthHorn, new Vector3(-519f, 48f, -641f), [49831, 49826, 51987, 50975], OccultAetheryte.MolderingOutskirts, 17, weakness: Weakness.Lightning), // Quarried Away
        new(52, Territory.NorthHorn, new Vector3(659f, 132f, 659f), [49833, 49828, 51979, 50974], OccultAetheryte.NorthHornBaseCamp, 27, weakness: Weakness.Fire), // Forbidden Folios
        new(53, Territory.NorthHorn, new Vector3(-688f, 90f, 150f), [49827, 49832, 51986, 50975], OccultAetheryte.SuspendedMasonry, 33, trigger: 14887, weakness: Weakness.Fire), // Cursed Resurgence
        new(54, Territory.NorthHorn, new Vector3(765f, 70f, 0f), [49831, 49826, 51981, 50975], OccultAetheryte.CrownOfKarnak, 42, weakness: Weakness.Fire), // Imbalanced Diet
        new(55, Territory.NorthHorn, new Vector3(169.9999f, 4f, -136f), [49832, 49827, 50974], OccultAetheryte.UnhallowedHamlet, 15, trigger: 14897, weakness: Weakness.Ice), // Web of Terror
        new(56, Territory.NorthHorn, new Vector3(238.0022f, 15f, 367f), [49833, 49828, 50976], OccultAetheryte.CrownOfKarnak, 18, weakness: Weakness.Ice), // A Beast Unleashed
        new(57, Territory.NorthHorn, new Vector3(224f, 52f, -860f), [49832, 49827, 51974, 51984, 50975], OccultAetheryte.SinkingSanctuary, 28, weakness: Weakness.Wind), // Dark Artistry
        new(58, Territory.NorthHorn, new Vector3(-390f, 67.99994f, 700f), [49833, 49828, 50976], OccultAetheryte.SuspendedMasonry, 12, weakness: Weakness.Lightning), // Familiar Tactics
        new(59, Territory.NorthHorn, new Vector3(807f, 61f, -562f), [49831, 49826, 51972, 51983, 50974], OccultAetheryte.SinkingSanctuary, 29, weakness: Weakness.Fire), // Appalling Behavior
        new(60, Territory.NorthHorn, new Vector3(152f, 70f, 716f), [49833, 49828, 51980, 50975], OccultAetheryte.CrownOfKarnak, 22, weakness: Weakness.Lightning), // Tiny Terror
        new(61, Territory.NorthHorn, new Vector3(-150f, 70f, -860f), [49832, 49827, 51985, 50976], OccultAetheryte.MolderingOutskirts, 33, weakness: Weakness.Lightning), // Lost on the Wind
        new(62, Territory.NorthHorn, new Vector3(-82f, 12f, 485f), [49833, 49828, 50974], OccultAetheryte.SuspendedMasonry, 31, weakness: Weakness.Ice), // Ahead of the Competition
        new(63, Territory.NorthHorn, new Vector3(500f, 56.00003f, -310f), [49831, 49826, 51982, 50976], OccultAetheryte.SinkingSanctuary, 27, weakness: Weakness.Wind), // Accept No Imitators

        new(64, Territory.NorthHorn, new Vector3(-320.06552f, 11.4999996f, 422.0136f), [], OccultAetheryte.SuspendedMasonry, 20, special: true), // The Forked Tower: Magic
        new(65, Territory.NorthHorn, new Vector3(-320.06552f, 11.4999996f, 422.0136f), [], OccultAetheryte.SuspendedMasonry, 20, special: true), // The Forked Tower: Magic Extreme
    ];

    public Fates(Plugin plugin)
    {
        Plugin = plugin;

        TriggerMonsters = AllCriticalEncounters.Where(f => f.TriggeredBy > 0).Select(f => f.TriggeredBy).ToHashSet();
    }

    public void Dispose()
    {
        RemoveEvents();

        BunnyFates.Clear();
        AllFates.Clear();
        AllCriticalEncounters.Clear();
    }

    public void RegisterEvents()
    {
        if (TerritoryHelper.PlayerInOccult())
            Plugin.Framework.Update += ScanOccultCEs;
        else if (TerritoryHelper.PlayerInBozja())
            Plugin.Framework.Update += ScanBozjaCEs;

        Plugin.Framework.Update += ScanFates;
    }

    public void RemoveEvents()
    {
        Plugin.Framework.Update -= ScanOccultCEs;
        Plugin.Framework.Update -= ScanBozjaCEs;
        Plugin.Framework.Update -= ScanFates;
    }

    public IEnumerable<Fate> GetBunnyForTerritory()
        => BunnyFates.Where(f => f.Territory == (Territory)Plugin.ClientState.TerritoryType);

    public IEnumerable<Fate> GetFatesForTerritory()
        => AllFates.Where(f => f.Territory == (Territory)Plugin.ClientState.TerritoryType);

    public IEnumerable<Fate> GetCEForTerritory()
        => AllCriticalEncounters.Where(f => f.Territory == (Territory)Plugin.ClientState.TerritoryType);

    public IEnumerable<Fate> GetCEWithoutSpecial()
        => AllCriticalEncounters.Where(f => f.Territory == (Territory)Plugin.ClientState.TerritoryType).Where(f => !f.SpecialEngagement);

    public IEnumerable<Fate> GetCEsSkipExtremeForTerritory()
        => AllCriticalEncounters.Where(f => f.Territory == (Territory)Plugin.ClientState.TerritoryType).Where(f => f.FateId != 65);

    public Fate GetNormalTowerForTerritory()
        => GetCEsSkipExtremeForTerritory().FirstOrDefault(f => f.SpecialEngagement) ?? AllCriticalEncounters[^2];

    public IEnumerable<Fate> GetSpawnableCEsForTerritory()
        => GetCEForTerritory().Where(f => f.TriggeredBy > 0);

    public unsafe void ReadLayout()
    {
        var locations = new Dictionary<uint, Fate>();
        foreach (var fate in AllCriticalEncounters.Where(f => f.Territory == Territory.NorthHorn))
        {
            var o = Sheets.DynamicEventSheet.GetRow(fate.FateId).LGBEventObject;
            if (o == 0)
                continue;
            locations.Add(o, fate);
        }

        foreach (var fate in AllFates.Where(f => f.Territory == Territory.NorthHorn))
        {
            locations.Add(Sheets.FateSheet.GetRow(fate.FateId).Location, fate);
        }

        foreach (var fate in BunnyFates.Where(f => f.Territory == Territory.NorthHorn))
        {
            locations.Add(Sheets.FateSheet.GetRow(fate.FateId).Location, fate);
        }

        var l = LayoutWorld.Instance();
        foreach (var layer in l->ActiveLayout->Layers.Values)
        {
            if (!layer.IsNull)
            {
                foreach (var pair in layer.Value->Instances)
                {
                    // pair.Item2.Value->Id.Type == InstanceType.EventObject || pair.Item2.Value->Id.Type == InstanceType.Treasure
                    // if (pair.Item2.Value->Id.Type == InstanceType.Treasure)
                    // {
                    //     var gameEventObject = (TreasureLayoutInstance*)pair.Item2.Value;
                    //     var pos = pair.Item2.Value->GetTransformImpl()->Translation;
                    //     Plugin.Log.Information($"{gameEventObject->BaseId}: (new Vector3({pos.X}f, {pos.Y}f, {pos.Z}f), {Sheets.TreasureSheet.GetRow(gameEventObject->BaseId).SGB.RowId}),");
                    // }
                    //
                    if (pair.Item2.Value->Id.Type == InstanceType.EventObject)
                    {
                        var gameEventObject = (GameObjectLayoutInstance*)pair.Item2.Value;
                        if (gameEventObject->BaseId != 2014695)
                            continue;

                        var pos = pair.Item2.Value->GetTransformImpl()->Translation;
                        Plugin.Log.Information($"{gameEventObject->BaseId}: (new Vector3({pos.X}f, {pos.Y}f, {pos.Z}f),");
                    }

                    // if (locations.ContainsKey(pair.Item1))
                    // {
                    //     var pos = pair.Item2.Value->GetTransformImpl()->Translation;
                    //     var fate = locations[pair.Item1];
                    //     var mapPos = MapUtil.WorldToMap(new Vector2(pos.X, pos.Z));
                    //     Plugin.Log.Information($"{fate.Name}: {mapPos.X:F2}, {mapPos.Y:F2}");
                    // }
                }
            }
        }
    }

    private unsafe void ScanFates(IFramework _)
    {
        var local = Plugin.ObjectTable.LocalPlayer;
        if (local == null)
            return;

        var towerEngagement = Plugin.Fates.GetNormalTowerForTerritory();
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        foreach (var bnuuy in GetBunnyForTerritory())
        {
            foreach (var fate in Plugin.FateTable)
            {
                if (fate.FateId != bnuuy.FateId)
                    continue;

                var isAlive = bnuuy.Alive;
                bnuuy.Update(fate, currentTime);

                // Freshly spawned fate
                if (!isAlive)
                    Plugin.TrackerHandler.UpdateRunningTracker();

                if (bnuuy.PlayedSound || !Plugin.Configuration.PlayBunnyEffect)
                    continue;

                bnuuy.PlayedSound = true;
                UIGlobals.PlaySoundEffect((uint)Plugin.Configuration.BunnySoundEffect);
            }

            if (!bnuuy.Alive || bnuuy.LastSeenAlive == currentTime)
                continue;

            bnuuy.Alive = false;
            bnuuy.PlayedSound = false;
            bnuuy.DeathTime = bnuuy.LastSeenAlive;

            // Only increase if tower is not active
            if (!towerEngagement.Alive)
                towerEngagement.KilledFates += 1;

            // Bunny has died, update our running tracker
            Plugin.TrackerHandler.UpdateRunningTracker();
        }

        foreach (var occultFate in GetFatesForTerritory())
        {
            foreach (var fate in Plugin.FateTable)
            {
                if (fate.StartTimeEpoch == 0)
                    continue;

                if (fate.FateId != occultFate.FateId)
                    continue;

                var isAlive = occultFate.Alive;
                occultFate.Update(fate, currentTime);

                // Freshly spawned fate
                if (!isAlive)
                    Plugin.TrackerHandler.InstanceCheckAsync(fate, local);

                if (occultFate.PlayedSound || !Plugin.Configuration.PlayFateEffect)
                    continue;

                occultFate.PlayedSound = true;
                UIGlobals.PlaySoundEffect((uint)Plugin.Configuration.FateSoundEffect);
            }

            if (!occultFate.Alive || occultFate.LastSeenAlive == currentTime)
                continue;

            occultFate.Alive = false;
            occultFate.PlayedSound = false;
            occultFate.DeathTime = occultFate.LastSeenAlive;

            // Only increase if tower is not active
            if (!towerEngagement.Alive)
                towerEngagement.KilledFates += 1;

            // Fate has died, update our running tracker
            Plugin.TrackerHandler.UpdateRunningTracker();
        }
    }

    private unsafe void ScanOccultCEs(IFramework _)
    {
        var local = Plugin.ObjectTable.LocalPlayer;
        if (local == null)
            return;

        var publicContent = PublicContentOccultCrescent.GetInstance();
        if (publicContent == null)
        {
            // Reset all states while publicContent is not initialized yet
            // This can happen if people get timeout from an active Encounter
            foreach (var occultCE in AllCriticalEncounters)
                occultCE.State = DynamicEventState.Inactive;

            return;
        }

        var towerEngagement = Plugin.Fates.GetNormalTowerForTerritory();

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        foreach (var occultCE in AllCriticalEncounters)
        {
            foreach (ref var criticalEncounter in publicContent->DynamicEventContainer.Events)
            {
                if (criticalEncounter.State == DynamicEventState.Inactive)
                    continue;

                if (criticalEncounter.DynamicEventId != occultCE.FateId)
                    continue;

                var isAlive = occultCE.Alive;
                occultCE.Update(ref criticalEncounter, currentTime);

                // Freshly spawned CE
                if (!isAlive)
                    Plugin.TrackerHandler.UpdateRunningTracker();

                if (occultCE.PlayedSound)
                    continue;
                occultCE.PlayedSound = true;

                // Forked Tower
                if (towerEngagement.FateId == criticalEncounter.DynamicEventId)
                {
                    if (!Plugin.Configuration.PlayTowerEffect)
                        continue;

                    UIGlobals.PlaySoundEffect((uint)Plugin.Configuration.TowerSoundEffect);
                }
                else
                {
                    if (!Plugin.Configuration.PlayEncounterEffect)
                        continue;

                    UIGlobals.PlaySoundEffect((uint)Plugin.Configuration.EncounterSoundEffect);
                }
            }

            if (!occultCE.Alive || occultCE.LastSeenAlive == currentTime)
                continue;

            occultCE.Alive = false;
            occultCE.PlayedSound = false;
            occultCE.State = DynamicEventState.Inactive;
            occultCE.DeathTime = occultCE.LastSeenAlive;
            occultCE.TriggerKills = 0;

            // Only increase if tower is not active
            if (!towerEngagement.Alive)
                towerEngagement.KilledCEs += 1;

            if (towerEngagement.FateId == occultCE.FateId)
            {
                towerEngagement.KilledFates = 0;
                towerEngagement.KilledCEs = 0;
            }

            // CE has died, update our running tracker
            Plugin.TrackerHandler.UpdateRunningTracker();
        }
    }

    private unsafe void ScanBozjaCEs(IFramework _)
    {
        var local = Plugin.ObjectTable.LocalPlayer;
        if (local == null)
            return;

        var publicContent = PublicContentBozja.GetInstance();
        if (publicContent == null)
        {
            // Reset all states while publicContent is not initialized yet
            // This can happen if people get timeout from an active Encounter
            foreach (var occultCE in AllCriticalEncounters)
                occultCE.State = DynamicEventState.Inactive;

            return;
        }

        var towerEngagement = Plugin.Fates.GetNormalTowerForTerritory();

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        foreach (var occultCE in AllCriticalEncounters)
        {
            foreach (ref var criticalEncounter in publicContent->DynamicEventContainer.Events)
            {
                if (criticalEncounter.State == DynamicEventState.Inactive)
                    continue;

                if (criticalEncounter.DynamicEventId != occultCE.FateId)
                    continue;

                var isAlive = occultCE.Alive;
                occultCE.Update(ref criticalEncounter, currentTime);

                // Freshly spawned CE
                if (!isAlive)
                    Plugin.TrackerHandler.UpdateRunningTracker();

                if (occultCE.PlayedSound)
                    continue;
                occultCE.PlayedSound = true;

                // Forked Tower
                if (towerEngagement.FateId == criticalEncounter.DynamicEventId)
                {
                    if (!Plugin.Configuration.PlayTowerEffect)
                        continue;

                    UIGlobals.PlaySoundEffect((uint)Plugin.Configuration.TowerSoundEffect);
                }
                else
                {
                    if (!Plugin.Configuration.PlayEncounterEffect)
                        continue;

                    UIGlobals.PlaySoundEffect((uint)Plugin.Configuration.EncounterSoundEffect);
                }
            }

            if (!occultCE.Alive || occultCE.LastSeenAlive == currentTime)
                continue;

            occultCE.Alive = false;
            occultCE.PlayedSound = false;
            occultCE.State = DynamicEventState.Inactive;
            occultCE.DeathTime = occultCE.LastSeenAlive;
            occultCE.TriggerKills = 0;

            // Only increase if tower is not active
            if (!towerEngagement.Alive)
                towerEngagement.KilledCEs += 1;

            if (towerEngagement.FateId == occultCE.FateId)
            {
                towerEngagement.KilledFates = 0;
                towerEngagement.KilledCEs = 0;
            }

            // CE has died, update our running tracker
            Plugin.TrackerHandler.UpdateRunningTracker();
        }
    }

    public void Reset()
    {
        foreach (var fate in BunnyFates)
            fate.Reset();

        foreach (var fate in AllFates)
            fate.Reset();

        foreach (var fate in AllCriticalEncounters)
            fate.Reset();
    }
}