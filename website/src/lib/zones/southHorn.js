import monks from '$lib/assets/33.png';
import inks from '$lib/assets/37.png';
import byblos from '$lib/assets/39.png';
import peta from '$lib/assets/41.png';
import fan from '$lib/assets/42.png';
import garula from '$lib/assets/44.png';

const FATES = {
    // BASIC FATES
    1962: {
        name: {
            "en": "Rough Waters",
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
            "fr": "Défi : collecte mortelle",
            "ja": "命の収奪者「ライフギャザラー」",
            "de": "Seelen sammeln für den bösen Zweck"
        },
        drops: [47748],
        encounter_id: 24,
    },
};

const POT_FATES = {
    1976: {
        name: {
            "en": "Pleading Pots",
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
        drops: [47749, 47738],
        encounter_id: 40,
    },
    1977: {
        name: {
            "en": "Persistent Pots",
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
        drops: [47745, 47737],
        encounter_id: 18,
    },
};

const ENCOUNTERS = {
    33: {
        name: {
            "en": "Scourge of the Mind",
            "fr": "Défi : l'encéphalophage",
            "ja": "脳髄愛好家「マインドフレイア」",
            "de": "Die Geißel des Geistes",
        },
        drops: [49831, 49826, 47744],
        encounter_id: 33,
        spawn_type: true, // monster kill - Crescent Monk
        monster: {
            "en": "Crescent Monk",
            "fr": "Moine de Lunule",
            "ja": "クレセント・モンク",
            "de": "Kreszentia-Mönch"
        },
        monster_image: monks,
    },
    34: {
        name: {
            "en": "The Black Regiment",
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
            "fr": "Défi : Cloître à perpétuité",
            "ja": "封印大妖「クロイスターデーモン」",
            "de": "Das versiegelte Unheil",
        },
        drops: [49831, 49826, 47745, 47728, 48008],
        encounter_id: 37,
        spawn_type: true, // monster kill - Crescent Inkstain
        monster: {
            "en": "Crescent Inkstain",
            "fr": "Tache d'encre de Lunule",
            "ja": "クレセント・インクステイン",
            "de": "Kreszentia-Tintenfleck"
        },
        monster_image: inks,
    },
    38: {
        name: {
            "en": "Trial by Claw",
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
            "fr": "Défi : idole surprise",
            "ja": "神秘の偶像「ミシカルアイドル」",
            "de": "Von uralten Magien",
        },
        drops: [49833, 49828, 47746, 47729],
        encounter_id: 39,
        spawn_type: true, // monster kill - Crescent Byblos
        monster: {
            "en": "Crescent Byblos",
            "fr": "Byblos de Lunule",
            "ja": "クレセント・ビブロス",
            "de": "Kreszentia-Byblos"
        },
        monster_image: byblos,
    },
    40: {
        name: {
            "en": "Company of Stone",
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
            "fr": "Défi : homo selachus",
            "ja": "伝説の鮫「ニーム・ペタロドゥス」",
            "de": "Jäger aus alten Legenden",
        },
        drops: [49833, 49828, 47747, 47731],
        encounter_id: 41,
        spawn_type: true, // monster kill - Crescent Petalodite
        monster: {
            "en": "Crescent Petalodite",
            "fr": "Petalodus inférieur de Lunule",
            "ja": "クレセント・レッサーペタロドゥス",
            "de": "Niederer Kreszentia-Petalodus"
        },
        monster_image: peta,
    },
    42: {
        name: {
            "en": "On the Hunt",
            "fr": "Défi : à rugir debout",
            "ja": "二足の獅子「ランパントライオン」",
            "de": "Gefürchtetes Gebrüll",
        },
        drops: [49827, 49832, 47748, 47757],
        encounter_id: 42,
        spawn_type: true, // monster kill - Crescent Fan
        monster: {
            "en": "Crescent Fan",
            "fr": "Ventilateur de Lunule",
            "ja": "クレセント・ファン",
            "de": "Kreszentia-Ventilator"
        },
        monster_image: fan,
    },
    43: {
        name: {
            "en": "With Extreme Prejudice",
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
            "fr": "Défi : mammouth fâché",
            "ja": "鳥嫌いの巨獣「ネオガルラ」",
            "de": "Keine Gnade dem Gefieder",
        },
        drops: [49827, 49832, 47749],
        encounter_id: 44,
        spawn_type: true, // monster kill - Crescent Garula
        monster: {
            "en": "Crescent Garula",
            "fr": "Garula de Lunule",
            "ja": "クレセント・ガルラ",
            "de": "Kreszentia-Garula"
        },
        monster_image: garula,
    },
    45: {
        name: {
            "en": "Cursed Concern",
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
            "fr": "Tour fourchue de la Force",
            "ja": "フォークタワー：力の塔",
            "de": "Der Turm des Blutes"
        },
        drops: [47868, 47734, 47735, 47736, 47737],
        encounter_id: 48,
        type: "tower"
    },
};

export const SOUTH_HORN = {
    id: "south_horn",
    label: {
        en: "South Horn",
        fr: "L'île de Lunule méridionale",
        de: "Südliche Kreszentia",
        ja: "南征編",
    },
    // Condensed form for tight spaces (e.g. the tracker list's Zone column).
    shortLabel: {
        en: "South",
        fr: "Mérid.",
        de: "Südl.",
        ja: "南",
    },

    // The raw FFXIV territory id the plugin uploads (EnumHelper.Territory.SouthHorn),
    // used by $lib/zones/index.js to resolve the zone independently of tracker_type.
    territory: 1252,

    // Pot fates live in the same lookup as the regular fates, they are only
    // tracked separately (pot_history) because they share a respawn cycle.
    fates: { ...FATES, ...POT_FATES },
    encounters: ENCOUNTERS,

    fateIds: Object.keys(FATES).map(Number),
    potFateIds: Object.keys(POT_FATES).map(Number),
    encounterIds: Object.keys(ENCOUNTERS).map(Number),

    towerId: 48,
    towerIcon: "ui/icon/063000/063978_hr1.tex",
    towerSpawnTimer: 3600, // 1 hour
    potRespawn: 1800, // 30 minutes
};
