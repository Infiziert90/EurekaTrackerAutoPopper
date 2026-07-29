// Zone specific data (fates, encounters, tower and pot timers) lives in $lib/zones.

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

export const ITEM = {
    // DEMIATMAS - STEP 1 (7.25)
    47744: {
        name: {
            "en": "Azurite Demiatma",
            "fr": "Demi-âtma saphir",
            "ja": "青晶のデミアートマ",
            "de": "Demi-Atma Azurit"
        },
        img: "ui/icon/026000/026025.tex"
    },
    47745: {
        name: {
            "en": "Verdigris Demiatma",
            "fr": "Demi-âtma turquoise",
            "ja": "碧晶のデミアートマ",
            "de": "Demi-Atma Verdigris",
        },
        img: "ui/icon/026000/026035.tex"
    },
    47746: {
        name: {
            "en": "Malachite Demiatma",
            "fr": "Demi-âtma émeraude",
            "ja": "緑晶のデミアートマ",
            "de": "Demi-Atma Malachit",
        },
        img: "ui/icon/026000/026034.tex"
    },
    47747: {
        name: {
            "en": "Realgar Demiatma",
            "fr": "Demi-âtma corail",
            "ja": "橙晶のデミアートマ",
            "de": "Demi-Atma Realgar"
        },
        img: "ui/icon/026000/026026.tex"
    },
    47748: {
        name: {
            "en": "Caput Mortuum Demiatma",
            "fr": "Demi-âtma améthyste",
            "ja": "紫晶のデミアートマ",
            "de": "Demi-Atma Caput Mortuum"
        },
        img: "ui/icon/026000/026027.tex"
    },
    47749: {
        name: {
            "en": "Orpiment Demiatma",
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
            "fr": "Article sur le démon du Cloître",
            "ja": "探査記録:クロイスターデーモン",
            "de": "Chronikeintrag „Klosterdämon“"
        },
        img: "ui/icon/026000/026603.tex"
    },
    47729: {
        name: {
            "en": "Notes on the Mythic Idol",
            "fr": "Article sur l'idole mythique",
            "ja": "探査記録:ミシカルアイドル",
            "de": "Chronikeintrag „Mystisches Idol“"
        },
        img: "ui/icon/026000/026603.tex"
    },
    47730: {
        name: {
            "en": "Notes on the Crescent Berserker",
            "fr": "Article sur le berserker de Lunule",
            "ja": "探査記録:クレセント・バーサーカー",
            "de": "Chronikeintrag „Kreszenter Berserker“"
        },
        img: "ui/icon/026000/026603.tex"
    },
    47731: {
        name: {
            "en": "Notes on the Nymian Petalodus",
            "fr": "Article sur le petalodus de Nym",
            "ja": "探査記録:ニーム・ペタロドゥス",
            "de": "Chronikeintrag „Nymeischer Petalodus“"
        },
        img: "ui/icon/026000/026603.tex"
    },
    47732: {
        name: {
            "en": "Notes on Black Chocobos",
            "fr": "Article sur les chocobos noirs",
            "ja": "探査記録:黒チョコボ",
            "de": "Chronikeintrag „Schwarze Chocobos“"
        },
        img: "ui/icon/026000/026603.tex"
    },
    47733: {
        name: {
            "en": "Notes on the Trade Tortoise",
            "fr": "Article sur la tortue à pièces",
            "ja": "探査記録:コイントートス",
            "de": "Chronikeintrag „Münzkröte“"
        },
        img: "ui/icon/026000/026603.tex"
    },
    47734: {
        name: {
            "en": "Notes on the Demon Tablet",
            "fr": "Article sur la muraille démonique",
            "ja": "探査記録:デモンズ・タブレット",
            "de": "Chronikeintrag „Dämonentafel“"
        },
        img: "ui/icon/026000/026603.tex"
    },
    47735: {
        name: {
            "en": "Notes on the Dead Stars",
            "fr": "Article sur le trio de la Fosse",
            "ja": "探査記録:星頭の三人組",
            "de": "Chronikeintrag „Astronomisches Trio“"
        },
        img: "ui/icon/026000/026603.tex"
    },
    47736: {
        name: {
            "en": "Notes on the Marble Dragon",
            "fr": "Article sur le dragon marmoréen",
            "ja": "探査記録:マーブルドラゴン",
            "de": "Chronikeintrag „Marmordrache“"
        },
        img: "ui/icon/026000/026603.tex"
    },
    47737: {
        name: {
            "en": "Notes on Magitaur",
            "fr": "Article sur le magitaure",
            "ja": "探査記録:マギタウロス",
            "de": "Chronikeintrag „Magitaurus“"
        },
        img: "ui/icon/026000/026603.tex"
    },
    47738: {
        name: {
            "en": "Notes on Persistent Pots",
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
            "fr": "Sceau arcanique de la Force",
            "ja": "魔紋起動証:力の塔",
            "de": "Arkansiegel des Blutes"
        },
        img: "ui/icon/065000/065121.tex"
    },
    47740: {
        name: {
            "en": "Occult Coffer",
            "fr": "Boîte de monnaie abîmée",
            "ja": "サポートアイテム:古びた銭箱",
            "de": "Alte Geldkiste (Phantomgegenstand)"
        },
        img: "ui/icon/026000/026527.tex"
    },
    47741: {
        name: {
            "en": "Occult Potion",
            "fr": "Magi potion",
            "ja": "マギ・ポーション",
            "de": "Kreszenter Trank"
        },
        img: "ui/icon/020000/020603.tex"
    },
    47868: {
        name: {
            "en": "Sanguinite",
            "fr": "Gemme mystique de la Force",
            "ja": "力の魔石",
            "de": "Blutstein"
        },
        img: "ui/icon/021000/021467.tex"
    },
    48008: {
        name: {
            "en": "Voidsent Contract",
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
            "fr": "Éclat d'âme de berserker",
            "ja": "ソウルシャード:バーサーカー",
            "de": "Berserker-Seelensplitter"
        },
        img: "ui/icon/026000/026681.tex"
    },
    47752: {
        name: {
            "en": "Ranger's Soul Shard",
            "fr": "Éclat d'âme de rôdeur",
            "ja": "ソウルシャード:狩人",
            "de": "Jäger-Seelensplitter"
        },
        img: "ui/icon/026000/026681.tex"
    },
    47757: {
        name: {
            "en": "Oracle's Soul Shard",
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
            "fr": "Boucles d'oreilles de combattant magi de Lunule",
            "ja": "クレセントマギ・ファイターイヤリング",
            "de": "Kreszentia-Ohrringe des Kriegers"
        },
        img: "ui/icon/055000/055562_hr1.tex"
    },
    49827: {
        name: {
            "en": "Occult Necklace of Blood",
            "fr": "Collier de combattant magi de Lunule",
            "ja": "クレセントマギ・ファイターネックレス",
            "de": "Kreszentia-Halsband des Kriegers"
        },
        img: "ui/icon/055000/055107_hr1.tex"
    },
    49828: {
        name: {
            "en": "Occult Bracelet of Blood",
            "fr": "Bracelet de combattant magi de Lunule",
            "ja": "クレセントマギ・ファイターブレスレット",
            "de": "Kreszentia-Armband des Kriegers"
        },
        img: "ui/icon/055000/055905_hr1.tex"
    },
    49831: {
        name: {
            "en": "Occult Earrings of Magic",
            "fr": "Boucles d'oreilles de mage magi de Lunule",
            "ja": "クレセントマギ・ソーサラーイヤリング",
            "de": "Kreszentia-Ohrringe des Magiers"
        },
        img: "ui/icon/055000/055562_hr1.tex"
    },
    49832: {
        name: {
            "en": "Occult Necklace of Magic",
            "fr": "Collier de mage magi de Lunule",
            "ja": "クレセントマギ・ソーサラーネックレス",
            "de": "Kreszentia-Halsband des Magiers"
        },
        img: "ui/icon/055000/055107_hr1.tex"
    },
    49833: {
        name: {
            "en": "Occult Bracelet of Magic",
            "fr": "Bracelet de mage magi de Lunule",
            "ja": "クレセントマギ・ソーサラーブレスレット",
            "de": "Kreszentia-Armband des Magiers"
        },
        img: "ui/icon/055000/055905_hr1.tex"
    },

    // SOULS - NORTH HORN
    51972: {
        name: {
            "en": "Blue Mage's Soul Shard",
            "fr": "Éclat d'âme de mage bleu",
            "ja": "ソウルシャード:青魔道士",
            "de": "Blaumagier-Seelensplitter"
        },
        img: "ui/icon/026000/026681.tex"
    },
    51974: {
        name: {
            "en": "Necromancer's Soul Shard",
            "fr": "Éclat d'âme de nécromancien",
            "ja": "ソウルシャード:ネクロマンサー",
            "de": "Nekromanten-Seelensplitter"
        },
        img: "ui/icon/026000/026681.tex"
    },

    // FIELD NOTES - NORTH HORN
    51979: {
        name: {
            "en": "Notes on Arbatel",
            "fr": "Article sur Arbatel",
            "ja": "探査記録:アルバテル",
            "de": "Chronikeintrag „Arbatel“"
        },
        img: "ui/icon/026000/026603.tex"
    },
    51980: {
        name: {
            "en": "Notes on the Tiny Mage",
            "fr": "Article sur le petit mage",
            "ja": "探査記録:タイニーメイジ",
            "de": "Chronikeintrag „Winzige Magier“"
        },
        img: "ui/icon/026000/026603.tex"
    },
    51981: {
        name: {
            "en": "Notes on Algol",
            "fr": "Article sur Algol",
            "ja": "探査記録:アルゴル",
            "de": "Chronikeintrag „Algol“"
        },
        img: "ui/icon/026000/026603.tex"
    },
    51982: {
        name: {
            "en": "Notes on the Metamorph",
            "fr": "Article sur le métamorphe",
            "ja": "探査記録:メタモルファ",
            "de": "Chronikeintrag „Metamorpha“"
        },
        img: "ui/icon/026000/026603.tex"
    },
    51983: {
        name: {
            "en": "Notes on the Pallmagia",
            "fr": "Article sur Palimagia",
            "ja": "探査記録:ペイルマギア",
            "de": "Chronikeintrag „Bleicher Magia“"
        },
        img: "ui/icon/026000/026603.tex"
    },
    51984: {
        name: {
            "en": "Notes on the Phantom Necromancer",
            "fr": "Article sur le nécromancien magi",
            "ja": "探査記録:マギ・ネクロマンサー",
            "de": "Chronikeintrag „Kreszenter Nekromant“"
        },
        img: "ui/icon/026000/026603.tex"
    },
    51985: {
        name: {
            "en": "Notes on the Abductor",
            "fr": "Article sur le Ravisseur",
            "ja": "探査記録:アブダクター",
            "de": "Chronikeintrag „Abduktor“"
        },
        img: "ui/icon/026000/026603.tex"
    },
    51986: {
        name: {
            "en": "Notes on the Claret Dragon",
            "fr": "Article sur le dragon rubrum",
            "ja": "探査記録:ルブルムドラゴン",
            "de": "Chronikeintrag „Rubrum-Drache“"
        },
        img: "ui/icon/026000/026603.tex"
    },
    51987: {
        name: {
            "en": "Notes on the Alabaster Blade",
            "fr": "Article sur la Lame d'albâtre",
            "ja": "探査記録:アラバスターブレード",
            "de": "Chronikeintrag „Alabaster-Klinge“"
        },
        img: "ui/icon/026000/026603.tex"
    },
    51988: {
        name: {
            "en": "Notes on Conjured Calofisteri",
            "fr": "Article sur le clone de Calofisteri",
            "ja": "探査記録:カロフィステリ・ダブル",
            "de": "Chronikeintrag „Calofisteri-Doppelgängerin“"
        },
        img: "ui/icon/026000/026603.tex"
    },

    // DISPELLERS - NORTH HORN (relic quest)
    50974: {
        name: {
            "en": "Phantom Dispeller α",
            "fr": "Dissipateur spirituel alpha",
            "ja": "ファントムディスペラーα",
            "de": "Entphantomisierer α"
        },
        img: "ui/icon/026000/026229.tex"
    },
    50975: {
        name: {
            "en": "Phantom Dispeller β",
            "fr": "Dissipateur spirituel bêta",
            "ja": "ファントムディスペラーβ",
            "de": "Entphantomisierer β"
        },
        img: "ui/icon/026000/026231.tex"
    },
    50976: {
        name: {
            "en": "Phantom Dispeller γ",
            "fr": "Dissipateur spirituel gamma",
            "ja": "ファントムディスペラーγ",
            "de": "Entphantomisierer γ"
        },
        img: "ui/icon/026000/026230.tex"
    },
}
