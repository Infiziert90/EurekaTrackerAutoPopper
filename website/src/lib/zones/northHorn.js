import wamoura from '$lib/assets/49.png';
import blackguard from '$lib/assets/50.png';
import big_horn from '$lib/assets/53.png';
import hellhound from '$lib/assets/55.png';

const FATES = {
    2074: {
        name: {
            "en": "Raging Thrall",
            "fr": "Défi : quand le dopage fait des ravages",
            "ja": "暴力の牛魔「ミノタウロス・マキア」",
            "de": "Mit Kraft und Keule"
        },
        drops: [50974],
        weakness: ["fire"],
    },
    2075: {
        name: {
            "en": "Eye to Eye",
            "fr": "Défi : voyant malveillant",
            "ja": "呪いの宝珠「イビルシーア」",
            "de": "Auge um Auge"
        },
        drops: [50975],
        weakness: ["fire"],
    },
    2076: {
        name: {
            "en": "Shoreline Showdown",
            "fr": "Défi : chasse gardée",
            "ja": "水辺の暴君「レグナントキマイラ」",
            "de": "Wassertyrannei"
        },
        drops: [50976],
        weakness: ["wind"],
    },
    2077: {
        name: {
            "en": "Waved Away",
            "fr": "Défi : une vie de combats",
            "ja": "歴戦水馬「アーチケルピー」",
            "de": "Mit allen Wassern gewaschen"
        },
        drops: [50974],
        weakness: ["lightning"],
    },
    2078: {
        name: {
            "en": "Allure of the Occult",
            "fr": "Défi : je t'aime... moi non plus",
            "ja": "ため息モルボル「センシュアル・サンディ」",
            "de": "Auf den Atem kommt es an"
        },
        drops: [50975],
        weakness: ["fire"],
    },
    2079: {
        name: {
            "en": "Inconstant Gardener",
            "fr": "Défi : la diva destructrice",
            "ja": "自滅の歌い手「イアムベー」",
            "de": "Sing mir das Lied vom Tod"
        },
        drops: [50976],
        weakness: ["fire"],
    },
    2080: {
        name: {
            "en": "Territorial Dispute",
            "fr": "Défi : un froid de loup",
            "ja": "遺跡荒らしの氷狼「ルーインハウンド」",
            "de": "Der Hund von Kreszentia"
        },
        drops: [50975],
        weakness: ["fire"],
    },
    2081: {
        name: {
            "en": "A Rotten Affair",
            "fr": "Défi : pas de patience pour la pourriture",
            "ja": "腐都の守護者「ペイシェント・クリブ」",
            "de": "Die ewige Wächterin"
        },
        drops: [50974],
        weakness: ["wind", "lightning"],
    },
    2082: {
        name: {
            "en": "Gale-force Encounter",
            "fr": "Défi : autant en emporte la tempête",
            "ja": "暴風の操者「ストームコーラー」",
            "de": "Stürmischer Empfang"
        },
        drops: [50974],
        weakness: ["fire"],
    },
    2083: {
        name: {
            "en": "Scale Model",
            "fr": "Défi : rideau sur la méduse",
            "ja": "模造の蛇人形「デミメデューサ」",
            "de": "Stein zu Stein"
        },
        drops: [50976],
        weakness: ["ice"],
    },
    2084: {
        name: {
            "en": "Thunderregnum",
            "fr": "Défi : la reine des coeurls brisés",
            "ja": "気高き雷獣「クレセントレギナ」",
            "de": "Königliche Arroganz"
        },
        drops: [50975],
        weakness: ["fire"],
    },
};

const POT_FATES = {
    2072: {
        name: {
            "en": "Daylight Pottery",
            "fr": "Cache-cache avec le pot",
            "ja": "隠されのマジックポット",
            "de": "Versteckt im Pott"
        },
        suffix: {
            "en": "(North)",
            "fr": "(Nord)",
            "ja": "(北)",
            "de": "(Nord)"
        },
        drops: [50976],
        weakness: ["fire"],
    },
    2073: {
        name: {
            "en": "In a Pot of Bother",
            "fr": "Se prendre un vent, c'est pas de pot",
            "ja": "飛ばされのマジックポット",
            "de": "Wind im Pott"
        },
        suffix: {
            "en": "(South)",
            "fr": "(Sud)",
            "ja": "(南)",
            "de": "(Süden)"
        },
        drops: [50975],
        weakness: ["lightning"],
    },
};

const ENCOUNTERS = {
    49: {
        name: {
            "en": "Many Mouths to Feed",
            "fr": "Défi : appétit vorace",
            "ja": "四つ顎の魔樹「ペレキュス」",
            "de": "Verfressen bis in die Wurzeln"
        },
        drops: [50974],
        spawn_type: true, // monster kill - Wamoura
        monster: {
            "en": "Crescent Wamoura",
            "fr": "Wamoura de Lunule",
            "ja": "クレセント・ワモーラ",
            "de": "Kreszentia-Wamoura"
        },
        monster_image: wamoura,
        weakness: ["ice"],
    },
    50: {
        name: {
            "en": "Doubled Trouble",
            "fr": "Défi : les dérives du clonage",
            "ja": "魔女の複製体「カロフィステリ・ダブル」",
            "de": "Doppelt gehext hält besser"
        },
        drops: [49832, 49827, 51988, 50976],
        spawn_type: true, // monster kill - Crescent Blackguard
        monster: {
            "en": "Crescent Blackguard",
            "fr": "Garde noir de Lunule",
            "ja": "クレセント・ブラックガード",
            "de": "Kreszentia-Düsterwächter"
        },
        monster_image: blackguard,
        weakness: ["wind"],
    },
    51: {
        name: {
            "en": "Quarried Away",
            "fr": "Défi : l'homme à albâtre",
            "ja": "白の守護者「アラバスターブレード」",
            "de": "Schwert und Stein"
        },
        drops: [49831, 49826, 51987, 50975],
        spawn_type: false, // random spawn - Automatic
        weakness: ["lightning"],
    },
    52: {
        name: {
            "en": "Forbidden Folios",
            "fr": "Défi : magie taboue",
            "ja": "禁忌の魔道書「アルバテル」",
            "de": "Verbotenes Wissen"
        },
        drops: [49833, 49828, 51979, 50974],
        spawn_type: false, // random spawn - Automatic
        weakness: ["fire"],
    },
    53: {
        name: {
            "en": "Cursed Resurgence",
            "fr": "Défi : dragon en décomposition",
            "ja": "暗紅の屍竜「ルブルムドラゴン」",
            "de": "Gift und Kralle"
        },
        drops: [51986, 50975],
        spawn_type: true, // monster kill - Crescent Big Horn
        monster: {
            "en": "Crescent Big Horn",
            "fr": "Encorné de Lunule",
            "ja": "クレセント・ビッグホーン",
            "de": "Kreszentia-Großhorn"
        },
        monster_image: big_horn,
        weakness: ["fire"],
    },
    54: {
        name: {
            "en": "Imbalanced Diet",
            "fr": "Défi : Algol l'insatiable",
            "ja": "大食の呪鬼「アルゴル」",
            "de": "Unersättliches Unheil"
        },
        drops: [49831, 49826, 51981, 50975],
        spawn_type: false,
        weakness: ["fire"],
    },
    55: {
        name: {
            "en": "Web of Terror",
            "fr": "Défi : cruelles chasseresses",
            "ja": "猟奇の母蜘蛛「クレセント・アルケニー」",
            "de": "Wie eine Fliege im Netz"
        },
        drops: [49832, 49827, 50974],
        spawn_type: true, // monster kill - Crescent Hellhound
        monster: {
            "en": "Crescent Hellhound",
            "fr": "Limier de Lunule",
            "ja": "クレセント・ヘルハウンド",
            "de": "Kreszentia-Höllenhund"
        },
        monster_image: hellhound,
        weakness: ["ice"],
    },
    56: {
        name: {
            "en": "A Beast Unleashed",
            "fr": "Défi : le familier se rebelle",
            "ja": "反逆の使い魔「アトラス・カーバンクル」",
            "de": "Rubinrote Rebellion"
        },
        drops: [49833, 49828, 50976],
        weakness: ["ice"],
    },
    57: {
        name: {
            "en": "Dark Artistry",
            "fr": "Défi : croisade pour un cadavre",
            "ja": "死霊使いの亡霊「マギ・ネクロマンサー」",
            "de": "Auf Tod komm raus"
        },
        drops: [49832, 49827, 51974, 51984, 50975],
        spawn_type: false,
        weakness: ["wind"],
    },
    58: {
        name: {
            "en": "Familiar Tactics",
            "fr": "Défi : la voie de l'orme",
            "ja": "求道の人造人間「エルムギガース」",
            "de": "Auf dem Weg der Erleuchtung"
        },
        drops: [49833, 49828, 50976],
        spawn_type: false,
        weakness: ["lightning"],
    },
    59: {
        name: {
            "en": "Appalling Behavior",
            "fr": "Défi : vert de rancœur",
            "ja": "呪いを継ぐ者「ペイルマギア」",
            "de": "Verflucht noch eins"
        },
        drops: [49831, 49826, 51972, 51983, 50974],
        spawn_type: false,
        weakness: ["fire"],
    },
    60: {
        name: {
            "en": "Tiny Terror",
            "fr": "Défi : petits mais costauds",
            "ja": "魔道兵団「タイニーメイジ」",
            "de": "Klein aber oho"
        },
        drops: [49833, 49828, 51980, 50975],
        spawn_type: false,
        weakness: ["lightning"],
    },
    61: {
        name: {
            "en": "Lost on the Wind",
            "fr": "Défi : alerte enlèvement",
            "ja": "絶島の誘拐者「アブダクター」",
            "de": "Vom Winde verweht"
        },
        drops: [49832, 49827, 51985, 50976],
        spawn_type: false, // random spawn - Automatic
        weakness: ["lightning"],
    },
    62: {
        name: {
            "en": "Ahead of the Competition",
            "fr": "Défi : têtes à claques",
            "ja": "覚醒の多頭竜「マギ・ヒュドラ」",
            "de": "Mehr Köpfe als Verstand"
        },
        drops: [49833, 49828, 50974],
        spawn_type: false,
        weakness: ["ice"],
    },
    63: {
        name: {
            "en": "Accept No Imitators",
            "fr": "Défi : attention aux contrefaçons",
            "ja": "変化の使い魔「メタモルファ」",
            "de": "Mieses Mimikry"
        },
        drops: [49831, 49826, 51982, 50976],
        spawn_type: false,
        weakness: ["wind"],
    },

    // SPECIAL ENCOUNTER - the Forked Tower, handled through the tower* fields
    // below rather than the generic encounter list.
    64: {
        name: {
            "en": "The Forked Tower: Magic",
            "fr": "Tour fourchue de la Magie",
            "ja": "フォークタワー：魔の塔",
            "de": "Der Turm der Magie"
        },
        drops: [],
        type: "tower",
    },
};

export const NORTH_HORN = {
    id: "north_horn",
    label: {
        en: "North Horn",
        fr: "L'île de Lunule septentrionale",
        de: "Nördliche Kreszentia",
        ja: "北征編",
    },
    // Condensed form for tight spaces (e.g. the tracker list's Zone column).
    shortLabel: {
        en: "North",
        fr: "Sept.",
        de: "Nördl.",
        ja: "北",
    },

    // The raw FFXIV territory id the plugin uploads, used by $lib/zones/index.js
    // to resolve the zone independently of tracker_type.
    territory: 1346,

    // Pot fates live in the same lookup as the regular fates, they are only
    // tracked separately (pot_history) because they share a respawn cycle.
    fates: { ...FATES, ...POT_FATES },
    encounters: ENCOUNTERS,

    fateIds: Object.keys(FATES).map(Number),
    potFateIds: Object.keys(POT_FATES).map(Number),
    encounterIds: Object.keys(ENCOUNTERS).map(Number),

    towerId: 64,
    towerIcon: "ui/icon/063000/063978_hr1.tex",
    towerSpawnTimer: 3600, // unconfirmed
    potRespawn: 1800, // unconfirmed
};
