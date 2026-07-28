export const OCCULT_RESPAWN = 1800; // 30 minutes
export const TOWER_SPAWN_TIMER = 3600; // 1 hour

// CE Cooldown times (in seconds)
export const CE_COOLDOWN_MONSTER_KILL = 3600; // 60 minutes
export const CE_COOLDOWN_RANDOM_SPAWN = 7200; // 120 minutes

// API Configuration
export const BASE_URL = "https://infi.ovh/api/OccultTrackerV3";
export const BASE_ANON_KEY = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJyb2xlIjoiYW5vbiJ9.Ur6wgi_rD4dr3uLLvbLoaEvfLCu4QFWdrF-uHRtbl_s";

// API Headers
export const API_HEADERS = {
    apikey: BASE_ANON_KEY,
    Authorization: `Bearer ${BASE_ANON_KEY}`,
    Prefer: "return=representation",
};

// Equivalent to the WorldDCGroupType Excel Sheet
export const DATACENTER_NAMES = {
    0: {
        name: "Unknown",
        selectable: false,
    },
    1: {
        name: "Elemental",
        selectable: true,
        region: "Japan",
    },
    2: {
        name: "Gaia",
        selectable: true,
        region: "Japan",
    },
    3: {
        name: "Mana",
        selectable: true,
        region: "Japan"
    },
    4: {
        name: "Aether",
        selectable: true,
        region: "North America"
    },
    5: {
        name: "Primal",
        selectable: true,
        region: "North America",
    },
    6: {
        name: "Chaos",
        selectable: true,
        region: "Europe",
    },
    7: {
        name: "Light",
        selectable: true,
        region: "Europe",
    },
    8: {
        name: "Crystal",
        selectable: true,
        region: "North America",
    },
    9: {
        name: "Materia",
        selectable: true,
        region: "Oceania"
    },
    10: {
        name: "Meteor",
        selectable: true,
        region: "Japan"
    },
    11: {
        name: "Dynamis",
        selectable: true,
        region: "North America"
    },

    12: {
        name: "Shadow",
        selectable: false,
        region: "Europe"
    },
    13: {
        name: "NA Cloud DC (Beta)",
        selectable: false,
        region: "North America"
    },
    99: {
        name: "Beta",
        selectable: false
    },
    101: {
        name: "陆行鸟",
        selectable: true,
        region: "China"
    },
    102: {
        name: "莫古力",
        selectable: true,
        region: "China"
    },
    103: {
        name: "猫小胖",
        selectable: true,
        region: "China"
    },
    104: {
        name: "豆豆柴",
        selectable: true,
        region: "China"
    },
    151: {
        name: "[empty]",
        selectable: false
    },
    201: {
        name: "Eorzea",
        selectable: true,
        region: "Korea"
    }
}

export const OCCULT_FATES = {
    // BASIC FATES
    1962: {
        name: {
            "en": "Rough Waters",
            "zh": "涌潮海魔——纳木",
            "fr": "Défi : pas de vagues",
            "ja": "波起こしの海魔「ナンム」",
            "de": "Die Wogen glätten"
        },
        drops: [47744],
        encounter_id: 28,
    },
    1963: {
        name: {
            "en": "The Golden Guardian",
            "zh": "古代怪石——金色石面",
            "fr": "Défi : tombe vingt-quatre carats",
            "ja": "金色の怪石「ゴールデンブロックス」",
            "de": "Brocken statt Barren"
        },
        drops: [47744],
        encounter_id: 14,
    },
    1964: {
        name: {
            "en": "King of the Crescent",
            "zh": "悲鸣收集者——罗普罗斯",
            "fr": "Défi : la bête sadique",
            "ja": "悲鳴の蒐集者「ロプロス」",
            "de": "Geißel Kreszentias"
        },
        drops: [47749],
        encounter_id: 10,
    },
    1965: {
        name: {
            "en": "The Winged Terror",
            "zh": "甲板清扫者——巨大鸟",
            "fr": "Défi : je fus zu",
            "ja": "甲板の掃除人「巨大鳥」",
            "de": "Dämonenvogel am Spieß"
        },
        drops: [47747],
        encounter_id: 27,
    },
    1966: {
        name: {
            "en": "An Unending Duty",
            "zh": "神罚石兽——西西弗斯",
            "fr": "Défi : pierre de tonnerre",
            "ja": "神罰の石獣「シジフォス」",
            "de": "Los, Sisyphos, Donnerblitz!"
        },
        drops: [47746],
        encounter_id: 26,
    },
    1967: {
        name: {
            "en": "Brain Drain",
            "zh": "进化的毒鸟——高等魔鸟",
            "fr": "Défi : le poison de l'évolution",
            "ja": "進化せし怪蛇「アドバンスドエイビス」",
            "de": "Aspho ... asphy ... scheintot!"
        },
        drops: [47747],
        encounter_id: 24,
    },
    1968: {
        name: {
            "en": "A Delicate Balance",
            "zh": "湿度猎手——除湿之火",
            "fr": "Défi : humidité zéro",
            "ja": "湿度の狩手「ディヒューミディファイア」",
            "de": "Kampf um Leben und Schweiß"
        },
        drops: [47745],
        encounter_id: 25,
    },
    1969: {
        name: {
            "en": "Sworn to Soil",
            "zh": "土壤守护者——癫泥怪",
            "fr": "Défi : rester de boue",
            "ja": "土壌の守り手「マッドマッド」",
            "de": "Kampf um Leben und Boden unter den Füßen"
        },
        drops: [47745],
        encounter_id: 18,
    },
    1970: {
        name: {
            "en": "A Prying Eye",
            "zh": "监视之瞳——岛屿监视者",
            "fr": "Défi : un œil sur l'île",
            "ja": "監視の瞳「アイルオブザーバー」",
            "de": "Wächter des Lebens"
        },
        drops: [47744],
        encounter_id: 29,
    },
    1971: {
        name: {
            "en": "Fatal Allure",
            "zh": "美丽的咒杀者——执行者",
            "fr": "Défi : la belle mort",
            "ja": "美しき呪殺者「イグゼクレーター」",
            "de": "Tückische Schönheit"
        },
        drops: [47749],
        encounter_id: 17,
    },
    1972: {
        name: {
            "en": "Serving Darkness",
            "zh": "凶恶使魔——生命收割者",
            "fr": "Défi : collecte mortelle",
            "ja": "命の収奪者「ライフギャザラー」",
            "de": "Seelen sammeln für den bösen Zweck"
        },
        drops: [47748],
        encounter_id: 24,
    },

    // BUNNY FATES
    1976: {
        name: {
            "en": "Pleading Pots",
            "zh": "瑟瑟发抖的魔法罐",
            "fr": "Pas de pot pour les pots",
            "ja": "しあわせのマジックポット",
            "de": "Freude im Pott"
        },
        suffix: {
            "en": "(North)",
            "fr": "(Nord)",
            "ja": "(北)",
            "de": "(Nord)"
        },
        drops: [47749,47738],
        encounter_id: 40,
    },
    1977: {
        name: {
            "en": "Persistent Pots",
            "zh": "幸福的魔法罐",
            "fr": "Mauvais œil pour les pots",
            "ja": "カチカチのマジックポット",
            "de": "Wunder im Pott"
        },
        suffix: {
            "en": "(South)",
            "fr": "(Sud)",
            "ja": "(南)",
            "de": "(Süden)"
        },
        drops: [47745,47737],
        encounter_id: 18,
    }
}

export const OCCULT_ENCOUNTERS = {
    33: {
        name: {
            "en": "Scourge of the Mind",
            "zh": "脑髓爱好者——夺心魔",
            "fr": "Défi : l'encéphalophage",
            "ja": "脳髄愛好家「マインドフレイア」",
            "de": "Die Geißel des Geistes",
        },
        drops: [49831, 49826, 47744],
        encounter_id: 33,
        spawn_type: true, // monster kill - Crescent Monk
        monster: {
            "en": "Crescent Monk",
            "zh": "新月鬼鱼",
            "fr": "Moine de Lunule",
            "ja": "クレセント・モンク",
            "de": "Kreszentia-Mönch"
        },
    },
    34: {
        name: {
            "en": "The Black Regiment",
            "zh": "黑色连队",
            "fr": "Plumes d'encre",
            "ja": "黒の連隊",
            "de": "Das schwarze Regiment",
        },
        drops: [49831, 49826, 47749, 47752, 47732],
        encounter_id: 34,
        spawn_type: false, // random spawn - Automatic
    },
    35: {
        name: {
            "en": "The Unbridled",
            "zh": "愤怒的人造人——新月狂战士",
            "fr": "Défi : le rageux",
            "ja": "怒れる人造人間「クレセント・バーサーカー」",
            "de": "Zorn auf zwei Beinen",
        },
        drops: [49831, 49826, 47744, 47751, 47730],
        encounter_id: 35,
        spawn_type: false, // random spawn - Automatic
    },
    36: {
        name: {
            "en": "Crawling Death",
            "zh": "潜影撕裂者——死亡爪",
            "fr": "Défi : face de griffes",
            "ja": "忍び寄る爪「デスクロー」",
            "de": "Das messerscharfe Schicksal",
        },
        drops: [49831, 49826, 47744],
        encounter_id: 36,
        spawn_type: false, // random spawn - Automatic
    },
    37: {
        name: {
            "en": "Calamity Bound",
            "zh": "挣脱封印的大妖异——回廊恶魔",
            "fr": "Défi : Cloître à perpétuité",
            "ja": "封印大妖「クロイスターデーモン」",
            "de": "Das versiegelte Unheil",
        },
        drops: [49831, 49826, 47745, 47728, 48008],
        encounter_id: 37,
        spawn_type: true, // monster kill - Crescent Inkstain
        monster: {
            "en": "Crescent Inkstain",
            "zh": "新月墨渍",
            "fr": "Tache d'encre de Lunule",
            "ja": "クレセント・インクステイン",
            "de": "Kreszentia-Tintenfleck"
        },
    },
    38: {
        name: {
            "en": "Trial by Claw",
            "zh": "拟造使魔——水晶龙",
            "fr": "Défi : écailles de cristal",
            "ja": "模造されしもの「水晶竜」",
            "de": "Tödliche Schönheit",
        },
        drops: [49833, 49828, 47746],
        encounter_id: 38,
        spawn_type: false, // random spawn - Automatic
    },
    39: {
        name: {
            "en": "From Times Bygone",
            "zh": "双极的造物——神秘土偶",
            "fr": "Défi : idole surprise",
            "ja": "神秘の偶像「ミシカルアイドル」",
            "de": "Von uralten Magien",
        },
        drops: [49833, 49828, 47746, 47729],
        encounter_id: 39,
        spawn_type: true, // monster kill - Crescent Byblos
        monster: {
            "en": "Crescent Byblos",
            "zh": "新月比布鲁斯",
            "fr": "Byblos de Lunule",
            "ja": "クレセント・ビブロス",
            "de": "Kreszentia-Byblos"
        },
    },
    40: {
        name: {
            "en": "Company of Stone",
            "zh": "石制骑士团",
            "fr": "L'armée des argileux",
            "ja": "石造りの守護騎士たち",
            "de": "Die steinerne Staffel",
        },
        drops: [49827, 49832, 47748],
        encounter_id: 40,
        spawn_type: false, // random spawn - Automatic
    },
    41: {
        name: {
            "en": "Shark Attack",
            "zh": "传说中的鲨鱼——尼姆瓣齿鲨",
            "fr": "Défi : homo selachus",
            "ja": "伝説の鮫「ニーム・ペタロドゥス」",
            "de": "Jäger aus alten Legenden",
        },
        drops: [49833, 49828, 47747, 47731],
        encounter_id: 41,
        spawn_type: true, // monster kill - Crescent Petalodite
        monster: {
            "en": "Crescent Petalodite",
            "zh": "新月小瓣齿鲨",
            "fr": "Petalodus inférieur de Lunule",
            "ja": "クレセント・レッサーペタロドゥス",
            "de": "Niederer Kreszentia-Petalodus"
        },
    },
    42: {
        name: {
            "en": "On the Hunt",
            "zh": "双足狮人——跃立狮",
            "fr": "Défi : à rugir debout",
            "ja": "二足の獅子「ランパントライオン」",
            "de": "Gefürchtetes Gebrüll",
        },
        drops: [49827, 49832, 47748, 47757],
        encounter_id: 42,
        spawn_type: true, // monster kill - Crescent Fan
        monster: {
            "en": "Crescent Fan",
            "zh": "新月风扇",
            "fr": "Ventilateur de Lunule",
            "ja": "クレセント・ファン",
            "de": "Kreszentia-Ventilator"
        },
    },
    43: {
        name: {
            "en": "With Extreme Prejudice",
            "zh": "防卫指令",
            "fr": "Cordon de sécurité",
            "ja": "セキュリティ・コマンドー",
            "de": "Mit absoluter Sicherheit",
        },
        drops: [49833, 49828, 47747],
        encounter_id: 43,
        spawn_type: false, // random spawn - Automati
    },
    44: {
        name: {
            "en": "Noise Complaint",
            "zh": "厌鸟巨兽——进化加鲁拉",
            "fr": "Défi : mammouth fâché",
            "ja": "鳥嫌いの巨獣「ネオガルラ」",
            "de": "Keine Gnade dem Gefieder",
        },
        drops:  [49827, 49832, 47749],
        encounter_id: 44,
        spawn_type: true, // monster kill - Crescent Garula
        monster: {
            "en": "Crescent Garula",
            "zh": "新月加鲁拉",
            "fr": "Garula de Lunule",
            "ja": "クレセント・ガルラ",
            "de": "Kreszentia-Garula"
        },
    },
    45: {
        name: {
            "en": "Cursed Concern",
            "zh": "贩卖诅咒的商贩——金钱龟",
            "fr": "Défi : être aux pièces",
            "ja": "呪いの商亀「コイントートス」",
            "de": "Das Feilschen ums Verfluchte",
        },
        drops: [49827, 49832, 47747, 47733],
        encounter_id: 45,
        spawn_type: false, // random spawn - Automatic
    },
    46: {
        name: {
            "en": "Eternal Watch",
            "zh": "城塞守卫——复原狮像",
            "fr": "Défi : comme un lion nouveau",
            "ja": "復元された獅子像「リペアドライオン」",
            "de": "Ausgebessert und verbessert",
        },
        drops: [49827, 49832, 47748],
        encounter_id: 46,
        spawn_type: false, // random spawn - Automatic
    },
    47: {
        name: {
            "en": "Flame of Dusk",
            "zh": "昏暗妖魂——鬼火苗",
            "fr": "Défi : les ailes de pierre",
            "ja": "昏き篝火「ヒンキーパンク」",
            "de": "Das finsterste Feuer",
        },
        drops: [49833, 49828, 47746],
        encounter_id: 47,
        spawn_type: false, // random spawn - Automatic
    },

    // SPECIAL ENCOUNTER
    48: {
        name: {
            "en": "The Forked Tower: Blood",
            "zh": "两歧塔力之塔",
            "fr": "Tour fourchue de la Force",
            "ja": "フォークタワー：力の塔",
            "de": "Der Turm des Blutes"
        },
        drops: [47868, 47734, 47735, 47736, 47737],
        encounter_id: 48,
        type: "tower"
    }
}

export const ITEM = {
    // DEMIATMAS - STEP 1 (7.25)
    47744: {
        name: {
            "en": "Azurite Demiatma",
            "zh": "青色半魂晶",
            "fr": "Demi-âtma saphir",
            "ja": "青晶のデミアートマ",
            "de": "Demi-Atma Azurit"
        },
        img: "ui/icon/026000/026025.tex"
    },
    47745: {
        name: {
            "en": "Verdigris Demiatma",
            "zh": "碧色半魂晶",
            "fr": "Demi-âtma turquoise",
            "ja": "碧晶のデミアートマ",
            "de": "Demi-Atma Verdigris",
        },
        img: "ui/icon/026000/026035.tex"
    },
    47746: {
        name: {
            "en": "Malachite Demiatma",
            "zh": "绿色半魂晶",
            "fr": "Demi-âtma émeraude",
            "ja": "緑晶のデミアートマ",
            "de": "Demi-Atma Malachit",
        },
        img: "ui/icon/026000/026034.tex"
    },
    47747: {
        name: {
            "en": "Realgar Demiatma",
            "zh": "橙色半魂晶",
            "fr": "Demi-âtma corail",
            "ja": "橙晶のデミアートマ",
            "de": "Demi-Atma Realgar"
        },
        img: "ui/icon/026000/026026.tex"
    },
    47748: {
        name: {
            "en": "Caput Mortuum Demiatma",
            "zh": "紫色半魂晶",
            "fr": "Demi-âtma améthyste",
            "ja": "紫晶のデミアートマ",
            "de": "Demi-Atma Caput Mortuum"
        },
        img: "ui/icon/026000/026027.tex"
    },
    47749: {
        name: {
            "en": "Orpiment Demiatma",
            "zh": "黄色半魂晶",
            "fr": "Demi-âtma ambre",
            "ja": "黄晶のデミアートマ",
            "de": "Demi-Atma Orpiment"
        },
        img: "ui/icon/026000/026029.tex"
    },

    // FIELD NOTES
    47728: {
        name: {
            "en": "Notes on the Cloister Demon",
            "zh": "调查记录：回廊恶魔",
            "fr": "Article sur le démon du Cloître",
            "ja": "探査記録:クロイスターデーモン",
            "de": "Chronikeintrag „Klosterdämon“"
        },
        img: "ui/icon/026000/026603.tex"
    },
    47729: {
        name: {
            "en": "Notes on the Mythic Idol",
            "zh": "调查记录：神秘土偶",
            "fr": "Article sur l'idole mythique",
            "ja": "探査記録:ミシカルアイドル",
            "de": "Chronikeintrag „Mystisches Idol“"
        },
        img: "ui/icon/026000/026603.tex"
    },
    47730: {
        name: {
            "en": "Notes on the Crescent Berserker",
            "zh": "调查记录：新月狂战士",
            "fr": "Article sur le berserker de Lunule",
            "ja": "探査記録:クレセント・バーサーカー",
            "de": "Chronikeintrag „Kreszenter Berserker“"
        },
        img: "ui/icon/026000/026603.tex"
    },
    47731: {
        name: {
            "en": "Notes on the Nymian Petalodus",
            "zh": "调查记录：尼姆瓣齿鲨",
            "fr": "Article sur le petalodus de Nym",
            "ja": "探査記録:ニーム・ペタロドゥス",
            "de": "Chronikeintrag „Nymeischer Petalodus“"
        },
        img: "ui/icon/026000/026603.tex"
    },
    47732: {
        name: {
            "en": "Notes on Black Chocobos",
            "zh": "调查记录：黑陆行鸟",
            "fr": "Article sur les chocobos noirs",
            "ja": "探査記録:黒チョコボ",
            "de": "Chronikeintrag „Schwarze Chocobos“"
        },
        img: "ui/icon/026000/026603.tex"
    },
    47733: {
        name: {
            "en": "Notes on the Trade Tortoise",
            "zh": "调查记录：金钱龟",
            "fr": "Article sur la tortue à pièces",
            "ja": "探査記録:コイントートス",
            "de": "Chronikeintrag „Münzkröte“"
        },
        img: "ui/icon/026000/026603.tex"
    },
    47734: {
        name: {
            "en": "Notes on the Demon Tablet",
            "zh": "调查记录：恶魔板",
            "fr": "Article sur la muraille démonique",
            "ja": "探査記録:デモンズ・タブレット",
            "de": "Chronikeintrag „Dämonentafel“"
        },
        img: "ui/icon/026000/026603.tex"
    },
    47735: {
        name: {
            "en": "Notes on the Dead Stars",
            "zh": "调查记录：星头三兄弟",
            "fr": "Article sur le trio de la Fosse",
            "ja": "探査記録:星頭の三人組",
            "de": "Chronikeintrag „Astronomisches Trio“"
        },
        img: "ui/icon/026000/026603.tex"
    },
    47736: {
        name: {
            "en": "Notes on the Marble Dragon",
            "zh": "调查记录：大理石龙",
            "fr": "Article sur le dragon marmoréen",
            "ja": "探査記録:マーブルドラゴン",
            "de": "Chronikeintrag „Marmordrache“"
        },
        img: "ui/icon/026000/026603.tex"
    },
    47737: {
        name: {
            "en": "Notes on Magitaur",
            "zh": "调查记录：魔陶洛斯",
            "fr": "Article sur le magitaure",
            "ja": "探査記録:マギタウロス",
            "de": "Chronikeintrag „Magitaurus“"
        },
        img: "ui/icon/026000/026603.tex"
    },
    47738: {
        name: {
            "en": "Notes on Persistent Pots",
            "zh": "调查记录：撒娇罐",
            "fr": "Article sur les pots persistants",
            "ja": "探査記録:おねだりポット",
            "de": "Chronikeintrag „Wunderpott“"
        },
        img: "ui/icon/026000/026603.tex"
    },

    // OTHER DROPS
    47739: {
        name: {
            "en": "Sanguine Cipher",
            "zh": "魔纹启动凭证：力之塔",
            "fr": "Sceau arcanique de la Force",
            "ja": "魔紋起動証:力の塔",
            "de": "Arkansiegel des Blutes"
        },
        img: "ui/icon/065000/065121.tex"
    },
    47740: {
        name: {
            "en": "Occult Coffer",
            "zh": "辅助道具：古旧的钱箱",
            "fr": "Boîte de monnaie abîmée",
            "ja": "サポートアイテム:古びた銭箱",
            "de": "Alte Geldkiste (Phantomgegenstand)"
        },
        img: "ui/icon/026000/026527.tex"
    },
    47741: {
        name: {
            "en": "Occult Potion",
            "zh": "魔恢复药",
            "fr": "Magi potion",
            "ja": "マギ・ポーション",
            "de": "Kreszenter Trank"
        },
        img: "ui/icon/020000/020603.tex"
    },
    47868: {
        name: {
            "en": "Sanguinite",
            "zh": "力之魔石",
            "fr": "Gemme mystique de la Force",
            "ja": "力の魔石",
            "de": "Blutstein"
        },
        img: "ui/icon/021000/021467.tex"
    },
    48008: {
        name: {
            "en": "Voidsent Contract",
            "zh": "大妖异的契约书",
            "fr": "Parchemin abîmé",
            "ja": "大妖異の契約書",
            "de": "Nichts­gesand­ten-Paktschrift"
        },
        img: "ui/icon/026000/026187.tex"
    },

    // SOULS
    47751: {
        name: {
            "en": "Berserker's Soul Shard",
            "zh": "灵魂碎晶：狂战士",
            "fr": "Éclat d'âme de berserker",
            "ja": "ソウルシャード:バーサーカー",
            "de": "Berserker-Seelensplitter"
        },
        img: "ui/icon/026000/026681.tex"
    },
    47752: {
        name: {
            "en": "Ranger's Soul Shard",
            "zh": "灵魂碎晶：猎人",
            "fr": "Éclat d'âme de rôdeur",
            "ja": "ソウルシャード:狩人",
            "de": "Jäger-Seelensplitter"
        },
        img: "ui/icon/026000/026681.tex"
    },
    47757: {
        name: {
            "en": "Oracle's Soul Shard",
            "zh": "灵魂碎晶：预言师",
            "fr": "Éclat d'âme de devin",
            "ja": "ソウルシャード:予言士",
            "de": "Seher-Seelensplitter"
        },
        img: "ui/icon/026000/026681.tex"
    },

    // Accessories
    49826: {
        name: {
            "en": "Occult Earrings of Blood",
            "zh": "力之新月魔耳饰",
            "fr": "Boucles d'oreilles de combattant magi de Lunule",
            "ja": "クレセントマギ・ファイターイヤリング",
            "de": "Kreszentia-Ohrringe des Kriegers"
        },
        img: "ui/icon/055000/055562_hr1.tex"
    },
    49827: {
        name: {
            "en": "Occult Necklace of Blood",
            "zh": "力之新月魔项链",
            "fr": "Collier de combattant magi de Lunule",
            "ja": "クレセントマギ・ファイターネックレス",
            "de": "Kreszentia-Halsband des Kriegers"
        },
        img: "ui/icon/055000/055107_hr1.tex"
    },
    49828: {
        name: {
            "en": "Occult Bracelet of Blood",
            "zh": "力之新月魔手镯",
            "fr": "Bracelet de combattant magi de Lunule",
            "ja": "クレセントマギ・ファイターブレスレット",
            "de": "Kreszentia-Armband des Kriegers"
        },
        img: "ui/icon/055000/055905_hr1.tex"
    },
    49831: {
        name: {
            "en": "Occult Bracelet of Magic",
            "zh": "魔之新月魔手镯",
            "fr": "Boucles d'oreilles de mage magi de Lunule",
            "ja": "クレセントマギ・ソーサラーイヤリング",
            "de": "Kreszentia-Ohrringe des Magiers"
        },
        img: "ui/icon/055000/055562_hr1.tex"
    },
    49832: {
        name: {
            "en": "Occult Bracelet of Magic",
            "zh": "魔之新月魔手镯",
            "fr": "Collier de mage magi de Lunule",
            "ja": "クレセントマギ・ソーサラーネックレス",
            "de": "Kreszentia-Halsband des Magiers"
        },
        img: "ui/icon/055000/055107_hr1.tex"
    },
    49833: {
        name: {
            "en": "Occult Bracelet of Magic",
            "zh": "魔之新月魔手镯",
            "fr": "Bracelet de mage magi de Lunule",
            "ja": "クレセントマギ・ソーサラーブレスレット",
            "de": "Kreszentia-Armband des Magiers"
        },
        img: "ui/icon/055000/055905_hr1.tex"
    },
}

export const SAMPLE_SOUTH_HORN_TRACKER = {
    // id, last_update and tracker_id are to be generated when making a new tracker
    //"id": 0000,
    //"last_update": -1,
    //"tracker_id": "",
    "password": "",
    "tracker_type": 2,
    "last_fate": "",
    "encounter_history": "[{\"fate_id\":33,\"spawn_time\":-1,\"death_time\":-1,\"last_seen\":-1,\"respawn_times\":[],\"killed_fates\":0,\"killed_ces\":0},{\"fate_id\":34,\"spawn_time\":-1,\"death_time\":-1,\"last_seen\":-1,\"respawn_times\":[],\"killed_fates\":0,\"killed_ces\":0},{\"fate_id\":35,\"spawn_time\":-1,\"death_time\":-1,\"last_seen\":-1,\"respawn_times\":[],\"killed_fates\":0,\"killed_ces\":0},{\"fate_id\":36,\"spawn_time\":-1,\"death_time\":-1,\"last_seen\":-1,\"respawn_times\":[],\"killed_fates\":0,\"killed_ces\":0},{\"fate_id\":37,\"spawn_time\":-1,\"death_time\":-1,\"last_seen\":-1,\"respawn_times\":[],\"killed_fates\":0,\"killed_ces\":0},{\"fate_id\":38,\"spawn_time\":-1,\"death_time\":-1,\"last_seen\":-1,\"respawn_times\":[],\"killed_fates\":0,\"killed_ces\":0},{\"fate_id\":39,\"spawn_time\":-1,\"death_time\":-1,\"last_seen\":-1,\"respawn_times\":[],\"killed_fates\":0,\"killed_ces\":0},{\"fate_id\":40,\"spawn_time\":-1,\"death_time\":-1,\"last_seen\":-1,\"respawn_times\":[],\"killed_fates\":0,\"killed_ces\":0},{\"fate_id\":41,\"spawn_time\":-1,\"death_time\":-1,\"last_seen\":-1,\"respawn_times\":[],\"killed_fates\":0,\"killed_ces\":0},{\"fate_id\":42,\"spawn_time\":-1,\"death_time\":-1,\"last_seen\":-1,\"respawn_times\":[],\"killed_fates\":0,\"killed_ces\":0},{\"fate_id\":43,\"spawn_time\":-1,\"death_time\":-1,\"last_seen\":-1,\"respawn_times\":[],\"killed_fates\":0,\"killed_ces\":0},{\"fate_id\":44,\"spawn_time\":-1,\"death_time\":-1,\"last_seen\":-1,\"respawn_times\":[],\"killed_fates\":0,\"killed_ces\":0},{\"fate_id\":45,\"spawn_time\":-1,\"death_time\":-1,\"last_seen\":-1,\"respawn_times\":[],\"killed_fates\":0,\"killed_ces\":0},{\"fate_id\":46,\"spawn_time\":-1,\"death_time\":-1,\"last_seen\":-1,\"respawn_times\":[],\"killed_fates\":0,\"killed_ces\":0},{\"fate_id\":47,\"spawn_time\":-1,\"death_time\":-1,\"last_seen\":-1,\"respawn_times\":[],\"killed_fates\":0,\"killed_ces\":0},{\"fate_id\":48,\"spawn_time\":-1,\"death_time\":-1,\"last_seen\":-1,\"respawn_times\":[],\"killed_fates\":0,\"killed_ces\":0}]",
    "fate_history": "[{\"fate_id\":1962,\"spawn_time\":-1,\"death_time\":-1,\"last_seen\":-1,\"respawn_times\":[],\"killed_fates\":0,\"killed_ces\":0},{\"fate_id\":1963,\"spawn_time\":-1,\"death_time\":-1,\"last_seen\":-1,\"respawn_times\":[],\"killed_fates\":0,\"killed_ces\":0},{\"fate_id\":1964,\"spawn_time\":-1,\"death_time\":-1,\"last_seen\":-1,\"respawn_times\":[],\"killed_fates\":0,\"killed_ces\":0},{\"fate_id\":1965,\"spawn_time\":-1,\"death_time\":-1,\"last_seen\":-1,\"respawn_times\":[],\"killed_fates\":0,\"killed_ces\":0},{\"fate_id\":1966,\"spawn_time\":-1,\"death_time\":-1,\"last_seen\":-1,\"respawn_times\":[],\"killed_fates\":0,\"killed_ces\":0},{\"fate_id\":1967,\"spawn_time\":-1,\"death_time\":-1,\"last_seen\":-1,\"respawn_times\":[2041,2042,1528],\"killed_fates\":0,\"killed_ces\":0},{\"fate_id\":1968,\"spawn_time\":-1,\"death_time\":-1,\"last_seen\":-1,\"respawn_times\":[],\"killed_fates\":0,\"killed_ces\":0},{\"fate_id\":1969,\"spawn_time\":-1,\"death_time\":-1,\"last_seen\":-1,\"respawn_times\":[],\"killed_fates\":0,\"killed_ces\":0},{\"fate_id\":1970,\"spawn_time\":-1,\"death_time\":-1,\"last_seen\":-1,\"respawn_times\":[],\"killed_fates\":0,\"killed_ces\":0},{\"fate_id\":1971,\"spawn_time\":-1,\"death_time\":-1,\"last_seen\":-1,\"respawn_times\":[],\"killed_fates\":0,\"killed_ces\":0},{\"fate_id\":1972,\"spawn_time\":-1,\"death_time\":-1,\"last_seen\":-1,\"respawn_times\":[],\"killed_fates\":0,\"killed_ces\":0}]",
    "pot_history": "[{\"fate_id\":1976,\"spawn_time\":-1,\"death_time\":-1,\"last_seen\":-1,\"respawn_times\":[],\"killed_fates\":0,\"killed_ces\":0},{\"fate_id\":1977,\"spawn_time\":-1,\"death_time\":-1,\"last_seen\":-1,\"respawn_times\":[],\"killed_fates\":0,\"killed_ces\":0}]",
}
