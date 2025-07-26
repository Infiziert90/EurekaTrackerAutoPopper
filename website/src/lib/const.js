export const OCCULT_RESPAWN = 1800; // 30 minutes

export const OCCULT_FATES = {
    // BUNNY FATES
    1976: { 
        name: {
            "en": "Pleading Pots",
            "fr": "Pas de pot pour les pots",
            "ja": "しあわせのマジックポット",
            "de": "Freude im Pott"
        },
        drops: [47749,47738],
        encounter_id: 40,
    },
    1977: { 
        name: {
            "en": "Persistent Pots",
            "fr": "Mauvais œil pour les pots",
            "ja": "カチカチのマジックポット",
            "de": "Wunder im Pott"
        },
        drops: [47745,47737],
        encounter_id: 18,
    }
}

export const OCCULT_ENCOUNTERS = {
    33: {
        name: {
            "en": "Scourge of the Mind",
            "fr": "Défi : l'encéphalophage",
            "ja": "脳髄愛好家「マインドフレイア」",
            "de": "Die Geißel des Geistes",
        },
        drops: [47744],
        encounter_id: 33,
    },
    34: {
        name: {
            "en": "The Black Regiment",
            "fr": "Plumes d'encre",
            "ja": "黒の連隊",
            "de": "Das schwarze Regiment",
        },
        drops: [47749, 47752, 47732],
        encounter_id: 34,
    },
    35: {
        name: {
            "en": "The Unbridled",
            "fr": "Défi : le rageux",
            "ja": "怒れる人造人間「クレセント・バーサーカー」",
            "de": "Zorn auf zwei Beinen",
        },
        drops: [47744, 47751, 47730],
        encounter_id: 35,
    },
    36: {
        name: {
            "en": "Crawling Death",
            "fr": "Défi : face de griffes",
            "ja": "忍び寄る爪「デスクロー」",
            "de": "Das messerscharfe Schicksal",
        },
        drops: [47744],
        encounter_id: 36,
    },
    37: {
        name: {
            "en": "Calamity Bound",
            "fr": "Défi : Cloître à perpétuité",
            "ja": "封印大妖「クロイスターデーモン」",
            "de": "Das versiegelte Unheil",
        },
        drops: [47745, 47728, 48008],
        encounter_id: 37,
    },
    38: {
        name: {
            "en": "Trial by Claw",
            "fr": "Défi : écailles de cristal",
            "ja": "模造されしもの「水晶竜」",
            "de": "Tödliche Schönheit",
        },
        drops: [47746],
        encounter_id: 38,
    },
    39: {
        name: {
            "en": "From Times Bygone",
            "fr": "Défi : idole surprise",
            "ja": "神秘の偶像「ミシカルアイドル」",
            "de": "Von uralten Magien",
        },
        drops: [47746, 47729],
        encounter_id: 39,
    },
    40: {
        name: {
            "en": "Company of Stone",
            "fr": "L'armée des argileux",
            "ja": "石造りの守護騎士たち",
            "de": "Die steinerne Staffel",
        },
        drops: [47748],
        encounter_id: 40,
    },
    41: {
        name: {
            "en": "Shark Attack",
            "fr": "Défi : homo selachus",
            "ja": "伝説の鮫「ニーム・ペタロドゥス」",
            "de": "Jäger aus alten Legenden",
        },
        drops: [47747, 47731],
        encounter_id: 41,
    },
    42: {
        name: {
            "en": "On the Hunt",
            "fr": "Défi : à rugir debout",
            "ja": "二足の獅子「ランパントライオン」",
            "de": "Gefürchtetes Gebrüll",
        },
        drops: [47748, 47757],
        encounter_id: 42,
    },
    43: {
        name: {
            "en": "With Extreme Prejudice",
            "fr": "Cordon de sécurité",
            "ja": "セキュリティ・コマンドー",
            "de": "Mit absoluter Sicherheit",
        },
        drops: [47747],
        encounter_id: 43,
    },
    44: {
        name: {
            "en": "Noise Complaint",
            "fr": "Défi : mammouth fâché",
            "ja": "鳥嫌いの巨獣「ネオガルラ」",
            "de": "Keine Gnade dem Gefieder",
        },
        drops:  [47749],
        encounter_id: 44,
    },
    45: {
        name: {
            "en": "Cursed Concern",
            "fr": "Défi : être aux pièces",
            "ja": "呪いの商亀「コイントートス」",
            "de": "Das Feilschen ums Verfluchte",
        },
        drops: [47747, 47733],
        encounter_id: 45,
    },
    46: {
        name: {
            "en": "Eternal Watch",
            "fr": "Défi : comme un lion nouveau",
            "ja": "復元された獅子像「リペアドライオン」",
            "de": "Ausgebessert und verbessert",
        },
        drops: [47748],
        encounter_id: 46,
    },
    47: {
        name: {
            "en": "Flame of Dusk",
            "fr": "Défi : les ailes de pierre",
            "ja": "昏き篝火「ヒンキーパンク」",
            "de": "Das finsterste Feuer",
        },
        drops: [47746],
        encounter_id: 47,
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
            "jp": "碧晶のデミアートマ",
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
}