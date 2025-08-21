using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace EurekaTrackerAutoPopper;

public static class OccultChests
{
    private const double InRange = 80.0f;

    public static readonly Dictionary<uint, List<(Vector3, uint)>> TreasurePosition = new()
    {
        { 1252, [
            (new Vector3(-283.98572f, 115.983765f, 377.03516f), 1597), // Counter: 861
            (new Vector3(277.7904f, 103.77649f, 241.90125f), 1596), // Counter: 806
            (new Vector3(-401.66327f, 85.03845f, 332.5398f), 1596), // Counter: 697
            (new Vector3(-372.67108f, 74.99805f, 527.4281f), 1596), // Counter: 682
            (new Vector3(609.61304f, 107.98804f, 117.2655f), 1596), // Counter: 601
            (new Vector3(256.1532f, 73.16687f, 492.3628f), 1596), // Counter: 592
            (new Vector3(870.6644f, 95.68933f, -388.35742f), 1596), // Counter: 583
            (new Vector3(-825.1621f, 2.9754639f, -832.2728f), 1597), // Counter: 578
            (new Vector3(697.322f, 69.99304f, 597.9247f), 1597), // Counter: 575
            (new Vector3(666.5292f, 79.11792f, -480.36932f), 1596), // Counter: 559
            (new Vector3(-444.11383f, 90.684326f, 26.230225f), 1596), // Counter: 548
            (new Vector3(642.96936f, 69.99304f, 407.79736f), 1596), // Counter: 542
            (new Vector3(-645.68555f, 202.99072f, 710.17017f), 1597), // Counter: 540
            (new Vector3(779.0187f, 96.08594f, -256.2448f), 1596), // Counter: 538
            (new Vector3(-118.97461f, 4.989685f, -708.4612f), 1596), // Counter: 520
            (new Vector3(726.28357f, 108.140625f, -67.91791f), 1596), // Counter: 492
            (new Vector3(596.45984f, 70.29822f, 622.76636f), 1596), // Counter: 485
            (new Vector3(294.8805f, 56.076904f, 640.2228f), 1596), // Counter: 476
            (new Vector3(-491.02008f, 2.9754639f, -529.59485f), 1596), // Counter: 468
            (new Vector3(770.7484f, 107.98804f, -143.5722f), 1597), // Counter: 466
            (new Vector3(471.18323f, 70.29822f, 530.022f), 1596), // Counter: 463
            (new Vector3(788.8761f, 120.378296f, 109.391846f), 1596), // Counter: 456
            (new Vector3(-648.0049f, 74.99805f, 403.95203f), 1596), // Counter: 420
            (new Vector3(55.283447f, 111.31445f, -289.0822f), 1596), // Counter: 417
            (new Vector3(-487.11377f, 98.527466f, -205.46277f), 1596), // Counter: 415
            (new Vector3(354.1161f, 95.65869f, -288.92963f), 1596), // Counter: 412
            (new Vector3(35.721313f, 65.11023f, 648.9509f), 1596), // Counter: 411
            (new Vector3(-197.19238f, 74.906494f, 618.3412f), 1596), // Counter: 410
            (new Vector3(-729.427f, 4.989685f, -724.81885f), 1596), // Counter: 409
            (new Vector3(433.70715f, 70.29822f, 683.52783f), 1596), // Counter: 404
            (new Vector3(517.7539f, 67.88733f, 236.1333f), 1597), // Counter: 402
            (new Vector3(-756.8322f, 76.55444f, 97.3678f), 1596), // Counter: 393
            (new Vector3(475.73047f, 95.994385f, -87.08331f), 1596), // Counter: 387
            (new Vector3(-661.7075f, 2.9754639f, -579.4919f), 1596), // Counter: 367
            (new Vector3(-884.123f, 3.7994385f, -682.0325f), 1596), // Counter: 362
            (new Vector3(-343.16016f, 52.32312f, -382.1317f), 1596), // Counter: 357
            (new Vector3(-550.13354f, 106.98096f, 627.74084f), 1596), // Counter: 352
            (new Vector3(-158.64807f, 98.61902f, -132.73828f), 1596), // Counter: 351
            (new Vector3(-729.9153f, 116.53308f, -79.05707f), 1596), // Counter: 343
            (new Vector3(142.1073f, 16.403442f, -574.0597f), 1596), // Counter: 338
            (new Vector3(-451.6823f, 2.9754639f, -775.5703f), 1596), // Counter: 336
            (new Vector3(-225.02484f, 74.99805f, 804.9896f), 1596), // Counter: 335
            (new Vector3(-856.9619f, 68.833374f, -93.15637f), 1596), // Counter: 332
            (new Vector3(-682.7955f, 135.60681f, -195.26971f), 1597), // Counter: 330
            (new Vector3(835.08044f, 69.99304f, 699.09204f), 1596), // Counter: 321
            (new Vector3(-140.45929f, 22.354431f, -414.2672f), 1596), // Counter: 310
            (new Vector3(140.97803f, 55.98523f, 770.99243f), 1596), // Counter: 310
            (new Vector3(8.987488f, 103.196655f, 426.96265f), 1596), // Counter: 303
            (new Vector3(386.92297f, 96.787964f, -451.37714f), 1596), // Counter: 300
            (new Vector3(-676.41724f, 170.9773f, 640.37524f), 1596), // Counter: 296
            (new Vector3(245.59387f, 109.11719f, -18.173523f), 1596), // Counter: 285
            (new Vector3(826.688f, 121.99585f, 434.9889f), 1596), // Counter: 273
            (new Vector3(-713.80176f, 62.05847f, 192.61462f), 1596), // Counter: 271
            (new Vector3(-25.68097f, 102.22009f, 150.16394f), 1596), // Counter: 263
            (new Vector3(-798.24524f, 105.57703f, -310.5669f), 1597), // Counter: 255
            (new Vector3(490.40967f, 62.45508f, -590.56995f), 1596), // Counter: 252
            (new Vector3(-256.88562f, 120.98877f, 125.078125f), 1596), // Counter: 252
            (new Vector3(-585.2903f, 4.989685f, -864.8356f), 1596), // Counter: 251
            (new Vector3(-716.1517f, 170.9773f, 794.4304f), 1596), // Counter: 243
            (new Vector3(-767.4525f, 115.61755f, -235.00421f), 1596), // Counter: 225
            (new Vector3(-600.27466f, 138.99438f, 802.6398f), 1596), // Counter: 220
            (new Vector3(617.08997f, 66.300415f, -703.8834f), 1596), // Counter: 212
            (new Vector3(-729.5491f, 106.98096f, 561.1504f), 1596), // Counter: 208
            (new Vector3(869.29126f, 109.97168f, 581.2008f), 1596), // Counter: 207
            (new Vector3(-394.88824f, 106.73682f, 175.43298f), 1596), // Counter: 195
            (new Vector3(-784.7562f, 138.99438f, 699.7634f), 1596), // Counter: 195
            (new Vector3(381.73486f, 22.171326f, -743.64844f), 1596), // Counter: 150
            (new Vector3(-680.5371f, 104.844604f, -354.78754f), 1596), // Counter: 120
            ] // 69
        },
    };

    public static readonly Dictionary<uint, List<Vector3>> PotNorthPosition = new()
    {
        { 1252, [
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
    };

    public static readonly Dictionary<uint, List<Vector3>> PotSouthPosition = new()
    {
        { 1252, [
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
    };

    public static readonly Dictionary<uint, List<Vector3>> RerollPosition = new()
    {
        { 1252, [
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
    };

    public static readonly Dictionary<uint, List<Vector3>> BunnyPosition = new()
    {
        { 1252, [
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
    };

    private static readonly Vector3[] CombinedPositions = PotNorthPosition[1252].Concat(PotSouthPosition[1252]).Concat(RerollPosition[1252]).ToArray();
    public static Vector3 CalculateDistance(uint territoryId, Vector3 player)
    {
        var bestPos = (Dif: InRange, Pos: Vector3.Zero);
        if (!PotNorthPosition.ContainsKey(territoryId) || !PotSouthPosition.ContainsKey(territoryId) || !RerollPosition.ContainsKey(territoryId))
            return bestPos.Pos;

        foreach (var pos in CombinedPositions)
        {
            var dif = Utils.GetDistance(player, pos);
            if (dif < bestPos.Dif)
                bestPos = (dif, pos);
        }

        return bestPos.Pos;
    }
}