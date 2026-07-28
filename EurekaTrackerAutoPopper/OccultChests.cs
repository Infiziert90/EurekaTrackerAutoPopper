using System.Collections.Generic;
using System.Numerics;

namespace EurekaTrackerAutoPopper;

public static class OccultChests
{
    private const double InRange = 80.0f;

    public static readonly Dictionary<Territory, List<(Vector3 Position, TreasureRarity Rarity, Map Map)>> TreasurePosition = new()
    {
        { Territory.SouthHorn,
            [
                (new Vector3(-283.98572f, 115.983765f, 377.03516f), TreasureRarity.Silver, Map.SouthHorn), // Counter: 861
                (new Vector3(277.7904f, 103.77649f, 241.90125f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 806
                (new Vector3(-401.66327f, 85.03845f, 332.5398f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 697
                (new Vector3(-372.67108f, 74.99805f, 527.4281f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 682
                (new Vector3(609.61304f, 107.98804f, 117.2655f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 601
                (new Vector3(256.1532f, 73.16687f, 492.3628f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 592
                (new Vector3(870.6644f, 95.68933f, -388.35742f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 583
                (new Vector3(-825.1621f, 2.9754639f, -832.2728f), TreasureRarity.Silver, Map.SouthHorn), // Counter: 578
                (new Vector3(697.322f, 69.99304f, 597.9247f), TreasureRarity.Silver, Map.SouthHorn), // Counter: 575
                (new Vector3(666.5292f, 79.11792f, -480.36932f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 559
                (new Vector3(-444.11383f, 90.684326f, 26.230225f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 548
                (new Vector3(642.96936f, 69.99304f, 407.79736f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 542
                (new Vector3(-645.68555f, 202.99072f, 710.17017f), TreasureRarity.Silver, Map.SouthHorn), // Counter: 540
                (new Vector3(779.0187f, 96.08594f, -256.2448f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 538
                (new Vector3(-118.97461f, 4.989685f, -708.4612f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 520
                (new Vector3(726.28357f, 108.140625f, -67.91791f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 492
                (new Vector3(596.45984f, 70.29822f, 622.76636f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 485
                (new Vector3(294.8805f, 56.076904f, 640.2228f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 476
                (new Vector3(-491.02008f, 2.9754639f, -529.59485f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 468
                (new Vector3(770.7484f, 107.98804f, -143.5722f), TreasureRarity.Silver, Map.SouthHorn), // Counter: 466
                (new Vector3(471.18323f, 70.29822f, 530.022f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 463
                (new Vector3(788.8761f, 120.378296f, 109.391846f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 456
                (new Vector3(-648.0049f, 74.99805f, 403.95203f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 420
                (new Vector3(55.283447f, 111.31445f, -289.0822f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 417
                (new Vector3(-487.11377f, 98.527466f, -205.46277f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 415
                (new Vector3(354.1161f, 95.65869f, -288.92963f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 412
                (new Vector3(35.721313f, 65.11023f, 648.9509f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 411
                (new Vector3(-197.19238f, 74.906494f, 618.3412f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 410
                (new Vector3(-729.427f, 4.989685f, -724.81885f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 409
                (new Vector3(433.70715f, 70.29822f, 683.52783f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 404
                (new Vector3(517.7539f, 67.88733f, 236.1333f), TreasureRarity.Silver, Map.SouthHorn), // Counter: 402
                (new Vector3(-756.8322f, 76.55444f, 97.3678f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 393
                (new Vector3(475.73047f, 95.994385f, -87.08331f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 387
                (new Vector3(-661.7075f, 2.9754639f, -579.4919f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 367
                (new Vector3(-884.123f, 3.7994385f, -682.0325f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 362
                (new Vector3(-343.16016f, 52.32312f, -382.1317f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 357
                (new Vector3(-550.13354f, 106.98096f, 627.74084f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 352
                (new Vector3(-158.64807f, 98.61902f, -132.73828f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 351
                (new Vector3(-729.9153f, 116.53308f, -79.05707f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 343
                (new Vector3(142.1073f, 16.403442f, -574.0597f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 338
                (new Vector3(-451.6823f, 2.9754639f, -775.5703f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 336
                (new Vector3(-225.02484f, 74.99805f, 804.9896f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 335
                (new Vector3(-856.9619f, 68.833374f, -93.15637f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 332
                (new Vector3(-682.7955f, 135.60681f, -195.26971f), TreasureRarity.Silver, Map.SouthHorn), // Counter: 330
                (new Vector3(835.08044f, 69.99304f, 699.09204f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 321
                (new Vector3(-140.45929f, 22.354431f, -414.2672f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 310
                (new Vector3(140.97803f, 55.98523f, 770.99243f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 310
                (new Vector3(8.987488f, 103.196655f, 426.96265f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 303
                (new Vector3(386.92297f, 96.787964f, -451.37714f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 300
                (new Vector3(-676.41724f, 170.9773f, 640.37524f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 296
                (new Vector3(245.59387f, 109.11719f, -18.173523f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 285
                (new Vector3(826.688f, 121.99585f, 434.9889f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 273
                (new Vector3(-713.80176f, 62.05847f, 192.61462f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 271
                (new Vector3(-25.68097f, 102.22009f, 150.16394f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 263
                (new Vector3(-798.24524f, 105.57703f, -310.5669f), TreasureRarity.Silver, Map.SouthHorn), // Counter: 255
                (new Vector3(490.40967f, 62.45508f, -590.56995f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 252
                (new Vector3(-256.88562f, 120.98877f, 125.078125f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 252
                (new Vector3(-585.2903f, 4.989685f, -864.8356f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 251
                (new Vector3(-716.1517f, 170.9773f, 794.4304f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 243
                (new Vector3(-767.4525f, 115.61755f, -235.00421f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 225
                (new Vector3(-600.27466f, 138.99438f, 802.6398f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 220
                (new Vector3(617.08997f, 66.300415f, -703.8834f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 212
                (new Vector3(-729.5491f, 106.98096f, 561.1504f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 208
                (new Vector3(869.29126f, 109.97168f, 581.2008f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 207
                (new Vector3(-394.88824f, 106.73682f, 175.43298f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 195
                (new Vector3(-784.7562f, 138.99438f, 699.7634f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 195
                (new Vector3(381.73486f, 22.171326f, -743.64844f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 150
                (new Vector3(-680.5371f, 104.844604f, -354.78754f), TreasureRarity.Bronze, Map.SouthHorn), // Counter: 120
            ] // 69
        },
        {
            Territory.NorthHorn,
            [
                (new Vector3(383.3138f, 33f, -175.6476f), TreasureRarity.Silver, Map.NorthHorn),
                (new Vector3(-2.305847f, 66.69136f, -814.9053f), TreasureRarity.Silver, Map.NorthHorn),
                (new Vector3(-22.66858f, 42.08691f, 628.9946f), TreasureRarity.Silver, Map.NorthHorn),
                (new Vector3(-633.6964f, 82.71846f, -146.0046f), TreasureRarity.Silver, Map.NorthHorn),
                (new Vector3(634.7919f, 60.51484f, -831.787f), TreasureRarity.Silver, Map.NorthHorn),
                (new Vector3(-645.4403f, 160.0992f, 967.9435f), TreasureRarity.Silver, Map.NorthHorn),
                (new Vector3(-815.8082f, -21.83485f, -699.3701f), TreasureRarity.Silver, Map.NorthHorn),
                (new Vector3(223.6532f, -161.8637f, -30.64362f), TreasureRarity.Silver, Map.NorthSubterrane),
                (new Vector3(676.9965f, 190.9779f, 957.4468f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(812.0001f, 192f, 669f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(673.7398f, 161.1653f, 729.666f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(758.147f, 130f, 506.8132f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(246.2266f, 66.54174f, 676.6658f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(719.3481f, 69.65454f, 268.3043f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(449.408f, 0.1465552f, 105.2345f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(649.5436f, 46.24511f, -157.7742f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(478.4506f, 12.4224f, -202.9711f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(254.7441f, 36.93214f, -605f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-26f, 0.2318999f, -437.6877f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-265.7608f, 30.17087f, -439.5194f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-232.4192f, 53.23654f, -719.9717f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(147.8688f, 61f, -868.7524f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(658.8088f, 66.1263f, -364.6757f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(950.2007f, 74.00013f, -358.9755f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(658.7231f, 60.52044f, -552.306f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(389.5362f, 60.68167f, -733.0182f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(77.06985f, 21.19984f, 536.2695f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-12.09888f, 66.65052f, 773.8625f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-278.0559f, 47.78407f, 567.9728f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-436.4424f, 0.2028036f, 166.2191f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-256.9473f, 100.6667f, 812.1967f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-504.0914f, 85.75282f, 758.3212f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-612.2136f, 66.98989f, 578.548f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-775.8944f, 70.7192f, 377.1531f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-631.7785f, 78.25452f, 240f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-923.1418f, 113.2651f, 197.9475f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-590.2075f, 87.97915f, -7f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-878.9666f, 13.13452f, -314.2021f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-581.4894f, 40.91439f, -257.4107f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-254.1409f, 1.820912f, -266.3119f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-707.3763f, 41.58638f, -396.9889f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-697.2709f, 34.89849f, -565.0217f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-439.5511f, 43.04438f, -558.4492f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-525.7809f, 46.85732f, -783.4683f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(85.59845f, 3.302996f, -281.1396f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(43.7818f, 2.454146f, -108.1916f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-168.2038f, 3.379924f, -153.4577f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-162.0424f, 3.589863f, 98.44962f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(633.1317f, 60.64236f, -910.2271f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(639.049f, 60.62531f, -698.7261f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(815.4435f, 60.5542f, -657.3135f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(865.4569f, 70.21528f, -874.0874f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-592f, 160.1012f, 767.6685f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-699.8373f, 160f, 926.3793f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-857.7925f, 159.85f, 772.2366f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-800.3965f, 157.8f, 633.3867f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-857.5991f, -12.23519f, -609.8169f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-928.626f, -11.22762f, -744.9562f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-736.0236f, 21.03466f, -881.4858f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-416.7736f, 45.93657f, -945.4311f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-144.7256f, -129.7955f, 304.9379f), TreasureRarity.Bronze, Map.NorthSubterrane),
                (new Vector3(41.2326f, -140.7708f, 168.5024f), TreasureRarity.Bronze, Map.NorthSubterrane),
                (new Vector3(161f, -151.7595f, 16.00002f), TreasureRarity.Bronze, Map.NorthSubterrane),
                (new Vector3(313.9192f, -139.5295f, 180.0712f), TreasureRarity.Bronze, Map.NorthSubterrane),
                (new Vector3(447.8859f, 62.90584f, 463.3448f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(279.0932f, 143f, -356.1478f), TreasureRarity.Bronze, Map.NorthHorn),
                (new Vector3(-287.7408f, -92f, 125.6662f), TreasureRarity.Bronze, Map.NorthSubterrane),
                (new Vector3(222.9122f, 90.40005f, 913.6289f), TreasureRarity.Bronze, Map.NorthHorn),
            ]
        },
    };

    public static readonly Dictionary<Territory, List<Vector3>> PotNorthPosition = new()
    {
        { Territory.SouthHorn,
            [
                new Vector3(571.5841f, 51.451305f, -813.1642f), // Counter: 544
                new Vector3(662.4388f, 120f, 161.1339f), // Counter: 499
                new Vector3(606.4641f, 108.07402f, 184.8517f), // Counter: 498
                new Vector3(-312.2778f, 103.19944f, -35.25348f), // Counter: 498
                new Vector3(587.7039f, 78.8956f, -545.8168f), // Counter: 497
                new Vector3(891.2597f, 120f, -20.672f), // Counter: 495
                new Vector3(878.1131f, 108.28959f, -91.1057f), // Counter: 489
                new Vector3(803.6609f, 95.99998f, -354.1809f), // Counter: 486
                new Vector3(341.4413f, 95.99999f, 194.7507f), // Counter: 484
                new Vector3(570.2421f, 64.66201f, 272.1734f), // Counter: 482
                new Vector3(-216.372f, 5.4469404f, -510.1361f), // Counter: 481
                new Vector3(684.4223f, 96.10129f, -165.4811f), // Counter: 481
                new Vector3(-188.1745f, 2.999999f, -717.2005f), // Counter: 474
                new Vector3(-476.3011f, 101.44228f, -86.69939f), // Counter: 473
                new Vector3(80.19762f, 101.27949f, 391.2263f), // Counter: 469
                new Vector3(-534.6993f, 2.999998f, -651.6244f), // Counter: 469
                new Vector3(-165.2374f, 95.33837f, 437.4505f), // Counter: 467
                new Vector3(330.8659f, 6.7168036f, -654.5339f), // Counter: 467
                new Vector3(-333.3444f, 2.9999998f, -861.1722f), // Counter: 463
                new Vector3(-313.2906f, 108.10962f, 70.76207f), // Counter: 463
                new Vector3(-459.1735f, 93.57443f, 5.054043f), // Counter: 463
                new Vector3(-54.69518f, 99.40573f, 405.0261f), // Counter: 462
                new Vector3(-382.4396f, 109.30187f, -378.3482f), // Counter: 458
                new Vector3(263.2559f, 100.38499f, 326.6834f), // Counter: 457
                new Vector3(224.7233f, 68.7328f, 518.668f), // Counter: 456
                new Vector3(19.73968f, 26.045855f, -420.977f), // Counter: 452
                new Vector3(705.2716f, 68.143616f, 358.6714f), // Counter: 451
                new Vector3(-660.5336f, 98f, -216.7666f), // Counter: 446
                new Vector3(-324.2736f, 121f, 203.2017f), // Counter: 445
                new Vector3(-386.5904f, -0.13994062f, -461.0976f), // Counter: 429
            ] // 30
        },
        {
            Territory.NorthHorn,
            [
                new Vector3(47.6f, 3.8843424f, -218.3f), // Counter: 14 // Treasures: Pot Bronze: 9,Pot Silver: 5 // FateId: 0:14
                new Vector3(-455.989f, 39.688915f, -365.5418f), // Counter: 13 // Treasures: Pot Bronze: 7,Pot Silver: 4,Pot Gold: 2 // FateId: 0:13
                new Vector3(889.2178f, 53.999996f, 155.9825f), // Counter: 13 // Treasures: Pot Bronze: 8,Pot Silver: 5 // FateId: 0:12, 1976:1
                new Vector3(190.3622f, 3.880325f, -204.7095f), // Counter: 13 // Treasures: Pot Bronze: 8,Pot Silver: 2,Pot Gold: 3 // FateId: 0:13
                new Vector3(-184.5137f, 71.1816f, 667.8036f), // Counter: 13 // Treasures: Pot Bronze: 3,Pot Silver: 7,Pot Gold: 3 // FateId: 0:13
                new Vector3(-223.8233f, 10.891144f, -353.9438f), // Counter: 12 // Treasures: Pot Bronze: 9,Pot Silver: 3 // FateId: 0:12
                new Vector3(-330f, 42f, -628f), // Counter: 12 // Treasures: Pot Bronze: 6,Pot Silver: 5,Pot Gold: 1 // FateId: 0:12
                new Vector3(452.6f, 57.10005f, -310.3f), // Counter: 11 // Treasures: Pot Bronze: 6,Pot Silver: 5 // FateId: 0:11
                new Vector3(593f, 39.622505f, 34f), // Counter: 11 // Treasures: Pot Bronze: 7,Pot Silver: 2,Pot Gold: 2 // FateId: 0:11
                new Vector3(-127f, 71.47446f, 808.4f), // Counter: 11 // Treasures: Pot Bronze: 3,Pot Silver: 7,Pot Gold: 1 // FateId: 0:11
                new Vector3(714.698f, 69.24771f, 262.6901f), // Counter: 10 // Treasures: Pot Bronze: 5,Pot Silver: 3,Pot Gold: 2 // FateId: 0:10
                new Vector3(-172.6f, 6.0019975f, 103.2f), // Counter: 10 // Treasures: Pot Bronze: 6,Pot Silver: 3,Pot Gold: 1 // FateId: 0:10
                new Vector3(1.768392f, 71.555756f, -872.2798f), // Counter: 10 // Treasures: Pot Bronze: 3,Pot Silver: 7 // FateId: 0:10
                new Vector3(929.4178f, 54f, -1.817501f), // Counter: 9 // Treasures: Pot Bronze: 7,Pot Silver: 2 // FateId: 0:9
                new Vector3(-252.1626f, 66.55432f, -879.5855f), // Counter: 8 // Treasures: Pot Bronze: 5,Pot Gold: 3 // FateId: 0:8
                new Vector3(-586.3f, 47.81013f, -715.2f), // Counter: 8 // Treasures: Pot Bronze: 7,Pot Silver: 1 // FateId: 0:8
                new Vector3(52f, 25.316154f, 552f), // Counter: 8 // Treasures: Pot Bronze: 4,Pot Silver: 4 // FateId: 0:8
                new Vector3(-747.4032f, 28.970308f, -492.1095f), // Counter: 8 // Treasures: Pot Bronze: 3,Pot Silver: 5 // FateId: 0:8
                new Vector3(927.0178f, 54f, -155.2175f), // Counter: 8 // Treasures: Pot Bronze: 2,Pot Silver: 5,Pot Gold: 1 // FateId: 0:8
                new Vector3(912.2978f, 61.18964f, -461.5099f), // Counter: 7 // Treasures: Pot Bronze: 2,Pot Silver: 4,Pot Gold: 1 // FateId: 0:7
                new Vector3(-512f, 41.999996f, -389f), // Counter: 7 // Treasures: Pot Bronze: 3,Pot Silver: 3,Pot Gold: 1 // FateId: 0:7
                new Vector3(-269.6122f, 107.93719f, 875.6997f), // Counter: 7 // Treasures: Pot Bronze: 4,Pot Silver: 2,Pot Gold: 1 // FateId: 0:7
                new Vector3(210f, 98.400055f, 916f), // Counter: 7 // Treasures: Pot Bronze: 6,Pot Silver: 1 // FateId: 0:7
                new Vector3(782.4979f, 70.34123f, -56.4099f), // Counter: 7 // Treasures: Pot Bronze: 1,Pot Silver: 4,Pot Gold: 2 // FateId: 0:7
                new Vector3(28.10088f, 3.9999995f, -16.69861f), // Counter: 7 // Treasures: Pot Bronze: 5,Pot Silver: 1,Pot Gold: 1 // FateId: 0:7
                new Vector3(71.10001f, 81.074875f, 942.3f), // Counter: 7 // Treasures: Pot Bronze: 5,Pot Gold: 2 // FateId: 0:7
                new Vector3(-628.4385f, 49.07533f, -449.5009f), // Counter: 7 // Treasures: Pot Bronze: 4,Pot Silver: 3 // FateId: 0:7
                new Vector3(939.2178f, 80.269966f, -273.1175f), // Counter: 7 // Treasures: Pot Bronze: 2,Pot Silver: 5 // FateId: 0:7
                new Vector3(-975.4507f, 17.57744f, -526.2878f), // Counter: 7 // Treasures: Pot Bronze: 3,Pot Silver: 4 // FateId: 0:7
                new Vector3(-88.43135f, 2.400001f, 4.891054f), // Counter: 7 // Treasures: Pot Bronze: 2,Pot Silver: 4,Pot Gold: 1 // FateId: 0:7
                new Vector3(948.5978f, 63.594563f, -567.0099f), // Counter: 7 // Treasures: Pot Bronze: 4,Pot Silver: 2,Pot Gold: 1 // FateId: 0:7
                new Vector3(-834f, 18.913685f, -587.4f), // Counter: 6 // Treasures: Pot Bronze: 2,Pot Silver: 3,Pot Gold: 1 // FateId: 0:6
                new Vector3(93.4f, 3.7155468f, -114.3f), // Counter: 6 // Treasures: Pot Bronze: 3,Pot Silver: 3 // FateId: 0:6
                new Vector3(237.9156f, -0.29999995f, 309.4334f), // Counter: 6 // Treasures: Pot Bronze: 3,Pot Silver: 1,Pot Gold: 2 // FateId: 0:6
                new Vector3(-190f, 61.75258f, -763f), // Counter: 6 // Treasures: Pot Bronze: 4,Pot Silver: 2 // FateId: 0:6
                new Vector3(-109.5452f, 8.047999f, -210.1855f), // Counter: 6 // Treasures: Pot Bronze: 3,Pot Silver: 2,Pot Gold: 1 // FateId: 0:6
                new Vector3(194.2296f, -0.3000001f, 352.9844f), // Counter: 6 // Treasures: Pot Bronze: 3,Pot Silver: 1,Pot Gold: 2 // FateId: 0:6
                new Vector3(546.56f, 36.120197f, 143.3104f), // Counter: 6 // Treasures: Pot Bronze: 1,Pot Silver: 4,Pot Gold: 1 // FateId: 0:6
                new Vector3(440.298f, 60.615795f, -926.5872f), // Counter: 6 // Treasures: Pot Bronze: 4,Pot Silver: 2 // FateId: 0:6
                new Vector3(-960f, 48f, -425.8f), // Counter: 5 // Treasures: Pot Bronze: 1,Pot Silver: 2,Pot Gold: 2 // FateId: 0:5
                new Vector3(928.8978f, 74.0003f, -332.8099f), // Counter: 5 // Treasures: Pot Bronze: 2,Pot Silver: 3 // FateId: 0:5
                new Vector3(-86f, 60.596237f, -737f), // Counter: 5 // Treasures: Pot Bronze: 4,Pot Silver: 1 // FateId: 0:5
                new Vector3(32.4f, 56.835186f, -777.3f), // Counter: 5 // Treasures: Pot Bronze: 3,Pot Silver: 2 // FateId: 0:5
                new Vector3(0.9425046f, 41.80327f, 623.2599f), // Counter: 5 // Treasures: Pot Bronze: 3,Pot Silver: 2 // FateId: 0:5
                new Vector3(-15.89468f, 4.0000005f, -20.29277f), // Counter: 5 // Treasures: Pot Bronze: 2,Pot Silver: 2,Pot Gold: 1 // FateId: 0:5
                new Vector3(151.9998f, 61.106945f, -842.0175f), // Counter: 4 // Treasures: Pot Bronze: 2,Pot Gold: 2 // FateId: 0:4
                new Vector3(782.8808f, 60.390976f, -611.7695f), // Counter: 4 // Treasures: Pot Gold: 4 // FateId: 0:4
                new Vector3(830.0979f, 77.75924f, -148.9099f), // Counter: 4 // Treasures: Pot Bronze: 3,Pot Silver: 1 // FateId: 0:4
                new Vector3(385f, 33f, -177f), // Counter: 4 // Treasures: Pot Bronze: 2,Pot Silver: 2 // FateId: 0:4
                new Vector3(-596f, 41.869873f, -285f), // Counter: 4 // Treasures: Pot Bronze: 2,Pot Silver: 1,Pot Gold: 1 // FateId: 0:4
                new Vector3(810.8979f, 78.39757f, -278.8099f), // Counter: 4 // Treasures: Pot Bronze: 2,Pot Silver: 2 // FateId: 0:4
                new Vector3(-259.6f, 3.6823246f, 56.9f), // Counter: 4 // Treasures: Pot Bronze: 4 // FateId: 0:4
                new Vector3(-498.7f, 11.051006f, 128.9f), // Counter: 4 // Treasures: Pot Bronze: 1,Pot Silver: 2,Pot Gold: 1 // FateId: 0:4
                new Vector3(-113.4943f, 5.0879984f, -74.15943f), // Counter: 4 // Treasures: Pot Bronze: 2,Pot Silver: 1,Pot Gold: 1 // FateId: 0:4
                new Vector3(-530f, 67.77658f, -58f), // Counter: 4 // Treasures: Pot Silver: 3,Pot Gold: 1 // FateId: 0:4
                new Vector3(321.198f, 59.85f, -889.8872f), // Counter: 4 // Treasures: Pot Bronze: 2,Pot Silver: 1,Pot Gold: 1 // FateId: 0:4
                new Vector3(-853.493f, 58f, -323.8983f), // Counter: 4 // Treasures: Pot Bronze: 1,Pot Silver: 3 // FateId: 0:4
                new Vector3(11.98766f, 68.15505f, 795.707f), // Counter: 4 // Treasures: Pot Bronze: 4 // FateId: 0:4
                new Vector3(-251.781f, 65.949005f, -864.3828f), // Counter: 4 // Treasures: Pot Bronze: 2,Pot Silver: 2 // FateId: 0:4
                new Vector3(-631.9453f, 160f, 808.8979f), // Counter: 4 // Treasures: Pot Gold: 4 // FateId: 0:4
                new Vector3(-661f, 160f, 937f), // Counter: 3 // Treasures: Pot Gold: 3 // FateId: 0:3
                new Vector3(-839.9977f, 160f, 740f), // Counter: 3 // Treasures: Pot Gold: 3 // FateId: 0:3
                new Vector3(-487.8f, 48.000015f, -953.2f), // Counter: 3 // Treasures: Pot Gold: 3 // FateId: 0:3
                new Vector3(-339.8588f, 85.47024f, 861.5197f), // Counter: 3 // Treasures: Pot Bronze: 2,Pot Silver: 1 // FateId: 0:3
                new Vector3(-536.1014f, 87.01824f, 149.8447f), // Counter: 3 // Treasures: Pot Bronze: 2,Pot Silver: 1 // FateId: 0:3
                new Vector3(-809f, 6.3495464f, -879f), // Counter: 2 // Treasures: Pot Gold: 2 // FateId: 0:2
                new Vector3(671.2f, 60.99496f, -550.1f), // Counter: 2 // Treasures: Pot Gold: 2 // FateId: 0:2
                new Vector3(701f, 59.999992f, -945f), // Counter: 2 // Treasures: Pot Gold: 2 // FateId: 0:2
                new Vector3(-637.2283f, 32f, -950.4841f), // Counter: 1 // Treasures: Pot Gold: 1 // FateId: 0:1
                new Vector3(-656.9f, 23.036425f, -799.3f), // Counter: 1 // Treasures: Pot Gold: 1 // FateId: 0:1
                new Vector3(-527f, 160.1012f, 834f), // Counter: 1 // Treasures: Pot Gold: 1 // FateId: 0:1
                new Vector3(626.3f, 61.119125f, -844.9f), // Counter: 1 // Treasures: Pot Gold: 1 // FateId: 0:1
                new Vector3(925.6533f, 70.21527f, -906.2195f), // Counter: 1 // Treasures: Pot Gold: 1 // FateId: 0:1
                new Vector3(-603f, 32f, -869f), // Counter: 1 // Treasures: Pot Gold: 1 // FateId: 0:1
                new Vector3(909f, 97.05797f, -961.8f), // Counter: 1 // Treasures: Pot Gold: 1 // FateId: 0:1
                new Vector3(-623f, 160f, 883f), // Counter: 1 // Treasures: Pot Gold: 1 // FateId: 0:1
            ]
        },
    };

    public static readonly Dictionary<Territory, List<Vector3>> PotSouthPosition = new()
    {
        { Territory.SouthHorn,
            [
                new Vector3(-195.4419f, 110.15342f, -287.8911f), // Counter: 530
                new Vector3(74.73397f, 110.494316f, -394.1289f), // Counter: 511
                new Vector3(-386.437f, 98.60658f, -221.7847f), // Counter: 509
                new Vector3(-554.6146f, 99.01769f, -309.1231f), // Counter: 501
                new Vector3(107.0611f, 105.699875f, 146.7059f), // Counter: 485
                new Vector3(825.9521f, 70f, 772.4054f), // Counter: 483
                new Vector3(-836.7586f, 106.999985f, 597.2944f), // Counter: 481
                new Vector3(67.45271f, 69.477974f, 745.8658f), // Counter: 479
                new Vector3(69.70596f, 111.56108f, -239.064f), // Counter: 478
                new Vector3(301.8741f, 103.784424f, 70.59854f), // Counter: 475
                new Vector3(-38.97946f, 102.073296f, -175.4589f), // Counter: 469
                new Vector3(-60.72729f, 69.687035f, 828.4997f), // Counter: 468
                new Vector3(17.60418f, 65.93209f, 674.6207f), // Counter: 463
                new Vector3(393.2685f, 57.545956f, 844.6924f), // Counter: 459
                new Vector3(393.0191f, 104f, -124.1651f), // Counter: 458
                new Vector3(-798.7886f, 84.22545f, -4.822005f), // Counter: 457
                new Vector3(440.8355f, 70.3f, 876.4097f), // Counter: 457
                new Vector3(-734.1434f, 170.99998f, 683.7238f), // Counter: 450
                new Vector3(423.3505f, 70.3f, 578.9013f), // Counter: 450
                new Vector3(200.1241f, 56f, 624.2285f), // Counter: 449
                new Vector3(-603.3457f, 139f, 858.6771f), // Counter: 445
                new Vector3(-829.598f, 62.66814f, 66.82948f), // Counter: 438 //
                new Vector3(-645.3027f, 135.69208f, -73.54771f), // Counter: 437
                new Vector3(-836.1612f, 107f, 770.2822f), // Counter: 436
                new Vector3(-676.6202f, 128.57442f, 1.531581f), // Counter: 435
                new Vector3(-713.6796f, 203f, 710.08f), // Counter: 426
                new Vector3(781.2514f, 70f, 560.0701f), // Counter: 420
                new Vector3(-746.1318f, 172.00023f, 828.8809f), // Counter: 419
                new Vector3(-730.5441f, 107.694275f, -371.4776f), // Counter: 332
                new Vector3(-810.8279f, 114.053925f, -226.8324f), // Counter: 297
            ] // 30
        },
        {
            Territory.NorthHorn,
            [
            ]
        },
    };

    public static readonly Dictionary<Territory, List<Vector3>> RerollPosition = new()
    {
        { Territory.SouthHorn,
            [
                new Vector3(-676.4631f, 5f, -769.7955f), // Counter: 123 // Treasures: Gold: 123
                new Vector3(-823.9183f, 140.00032f, 677.6934f), // Counter: 118 // Treasures: Gold: 118
                new Vector3(-886.4718f, 107f, 712.4964f), // Counter: 118 // Treasures: Gold: 118
                new Vector3(-625.7809f, 171f, 810.8691f), // Counter: 114 // Treasures: Gold: 114
                new Vector3(-813.9943f, 5f, -663.3634f), // Counter: 108 // Treasures: Gold: 108
                new Vector3(-842.8967f, 75.76903f, -125.0559f), // Counter: 107 // Treasures: Gold: 107
                new Vector3(-680.0345f, 201f, 739.9117f), // Counter: 107 // Treasures: Gold: 107
                new Vector3(-793.0552f, 5f, -777.3126f), // Counter: 106 // Treasures: Gold: 106
                new Vector3(-708.6777f, 171f, 669.5714f), // Counter: 105 // Treasures: Gold: 105
                new Vector3(-718.0424f, 5f, -633.8791f), // Counter: 105 // Treasures: Gold: 105
                new Vector3(-868.8489f, 67.5054f, -59.44909f), // Counter: 100 // Treasures: Gold: 100
                new Vector3(-803.5182f, 3f, -602.7497f), // Counter: 96 // Treasures: Gold: 96
                new Vector3(-732.2048f, 139f, 828.8491f), // Counter: 95 // Treasures: Gold: 95
                new Vector3(-659.1158f, 12.198493f, -508.7968f), // Counter: 95 // Treasures: Gold: 95
                new Vector3(-785.997f, 162.39513f, 790.5948f), // Counter: 95 // Treasures: Gold: 95
                new Vector3(-840.8771f, 107.26465f, -250.273f), // Counter: 90 // Treasures: Gold: 90
                new Vector3(-708.687f, 141.16982f, -139.3283f), // Counter: 85 // Treasures: Gold: 85
                new Vector3(-796.66f, 114.15647f, -228.9318f), // Counter: 83 // Treasures: Gold: 83
                new Vector3(-776.6315f, 5f, -486.978f), // Counter: 80 // Treasures: Gold: 80
                new Vector3(-758.8058f, 127.66496f, -183.164f), // Counter: 77 // Treasures: Gold: 77
            ] // 20
        },
        {
            Territory.NorthHorn,
            [
            ]
        },
    };

    public static readonly Dictionary<Territory, List<Vector3>> BunnyPosition = new()
    {
        { Territory.SouthHorn,
            [
                new Vector3(283.6546f, 55.999996f, 587.3107f), // Counter: 230
                new Vector3(-439.0463f, 115.82392f, 184.4665f), // Counter: 217
                new Vector3(477.4074f, 96.10128f, 138.6543f), // Counter: 213
                new Vector3(-743.601f, 96.39003f, 84.43998f), // Counter: 211
                new Vector3(-575.6361f, 162.39511f, 668.7043f), // Counter: 208
                new Vector3(865.0009f, 95.99958f, -214.6744f), // Counter: 204
                new Vector3(248.9159f, 55.999996f, 791.1138f), // Counter: 197
                new Vector3(-490.3187f, 3f, -741.0153f), // Counter: 197
                new Vector3(720.4133f, 120f, 271.05f), // Counter: 196
                new Vector3(466.2025f, 70.3f, 563.2519f), // Counter: 196
                new Vector3(-701.8768f, 201f, 718.7181f), // Counter: 192
                new Vector3(-273.0878f, 75f, 850.0336f), // Counter: 188
                new Vector3(650.2321f, 108f, 141.1927f), // Counter: 186
                new Vector3(827.2007f, 108f, -156.4444f), // Counter: 182
                new Vector3(845.5334f, 98f, 777.4331f), // Counter: 182
                new Vector3(772.3591f, 70.3f, 531.1259f), // Counter: 177
                new Vector3(-84.73673f, 2.999999f, -796.0166f), // Counter: 176
                new Vector3(-843.8602f, 83.657074f, -36.78173f), // Counter: 176
                new Vector3(-727.8528f, 81.47683f, 328.9311f), // Counter: 175
                new Vector3(-400.528f, 2.999999f, -518.3032f), // Counter: 174
                new Vector3(-806.5123f, 107f, 887.6146f), // Counter: 170
                new Vector3(-174.0473f, 121.00001f, 107.6488f), // Counter: 166
                new Vector3(-771.6308f, 5f, -694.0016f), // Counter: 160
                new Vector3(-710.266f, 3f, -451.5128f), // Counter: 148
                new Vector3(-554.0244f, 110.698654f, -365.897f), // Counter: 144
            ] // 25
        },
        {
            Territory.NorthHorn,
            [
                new Vector3(-857.4f, 71.45287f, 379.6f), // Counter: 11
                new Vector3(7.60699f, 4.3169565f, -35.67316f), // Counter: 10
                new Vector3(287.2872f, 142.99992f, -366.9024f), // Counter: 10
                new Vector3(-608.8f, 59.286507f, 373.9f), // Counter: 9
                new Vector3(-254f, 54.388798f, -739f), // Counter: 9
                new Vector3(-560.9f, 50.74249f, -447f), // Counter: 9
                new Vector3(-500f, 48.000004f, -867.6f), // Counter: 9
                new Vector3(226f, 90.400055f, 904f), // Counter: 9
                new Vector3(-258.7481f, 3.588304f, 53.59217f), // Counter: 7
                new Vector3(-604f, 160.05638f, 939.1f), // Counter: 7
                new Vector3(756.858f, 68.92707f, -79.33746f), // Counter: 6
                new Vector3(-814.6948f, 5.6813054f, -561.0853f), // Counter: 6
                new Vector3(-129.7795f, 8.029996f, -171.18f), // Counter: 5
                new Vector3(-847.9f, 114f, 196.6f), // Counter: 5
                new Vector3(-808f, 6.3495464f, -879f), // Counter: 4
                new Vector3(960f, 97.05797f, -879f), // Counter: 4
                new Vector3(625.8f, 61.06923f, -846.3f), // Counter: 3
                new Vector3(-956.1f, 157.8f, 720.2f), // Counter: 3
                new Vector3(-581f, 160f, 791f), // Counter: 3
                new Vector3(108f, 22.332209f, -556f), // Counter: 3
                new Vector3(-35f, 72.89336f, -860f), // Counter: 2
                new Vector3(882.1526f, 53.999996f, 115.9092f), // Counter: 2
                new Vector3(923f, 80.26997f, -277f), // Counter: 2
                new Vector3(853.9f, 70.20017f, -343.3f), // Counter: 2
                new Vector3(-124f, 76.75548f, 777f), // Counter: 1
            ] // 25
        },
    };

    public static Vector3 CalculateDistance(Territory territory, Vector3 player)
    {
        var bestPos = (Dif: InRange, Pos: Vector3.Zero);
        if (!TerritoryHelper.PlayerInOccult())
            return bestPos.Pos;

        foreach (var pos in PotNorthPosition[territory])
        {
            var dif = Utils.GetDistance(player, pos);
            if (dif < bestPos.Dif)
                bestPos = (dif, pos);
        }

        foreach (var pos in PotSouthPosition[territory])
        {
            var dif = Utils.GetDistance(player, pos);
            if (dif < bestPos.Dif)
                bestPos = (dif, pos);
        }

        foreach (var pos in RerollPosition[territory])
        {
            var dif = Utils.GetDistance(player, pos);
            if (dif < bestPos.Dif)
                bestPos = (dif, pos);
        }

        return bestPos.Pos;
    }
}
