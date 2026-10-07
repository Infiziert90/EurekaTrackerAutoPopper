// Eorzea time and weather forecasting.

export const EORZEA_HOUR_SECONDS = 175;
export const EORZEA_DAY_SECONDS = EORZEA_HOUR_SECONDS * 24; // 4200
export const WEATHER_WINDOW_SECONDS = EORZEA_HOUR_SECONDS * 8; // 1400

const EORZEA_TIME_FACTOR = 3600 / EORZEA_HOUR_SECONDS; // 20.571428...

// Weather sheet rows
export const WEATHERS = {
    1: {
        name: { en: "Clear Skies", fr: "Dégagé", de: "Klar", ja: "快晴" },
        icon: "ui/icon/060000/060201_hr1.tex",
    },
    2: {
        name: { en: "Fair Skies", fr: "Clair", de: "Heiter", ja: "晴れ" },
        icon: "ui/icon/060000/060202_hr1.tex",
    },
    3: {
        name: { en: "Clouds", fr: "Couvert", de: "Wolkig", ja: "曇り" },
        icon: "ui/icon/060000/060203_hr1.tex",
    },
    7: {
        name: { en: "Rain", fr: "Pluie", de: "Regnerisch", ja: "雨" },
        icon: "ui/icon/060000/060207_hr1.tex",
    },
    190: {
        name: { en: "Atmospheric Phantasms", fr: "Fantasmes", de: "Phantasmagorien", ja: "幻怪" },
        icon: "ui/icon/060000/060238_hr1.tex",
    },
    191: {
        name: { en: "Illusory Disturbances", fr: "Illusions", de: "Trugbilder", ja: "幻妖" },
        icon: "ui/icon/060000/060239_hr1.tex",
    },
};

// From the Addon sheet (rows 1127/1129, 1130/1132)
export const CLOCK_LABELS = {
    eorzea: {
        short: { en: "ET", fr: "HE", de: "EZ", ja: "ET" },
        full: { en: "Eorzea Time", fr: "Heure éorzéenne", de: "Eorzea-Zeit", ja: "エオルゼア時間" },
    },
    local: {
        short: { en: "LT", fr: "HL", de: "OZ", ja: "LT" },
        full: { en: "Local Time", fr: "Heure locale", de: "Ortszeit", ja: "ローカル時間" },
    },
};

// WeatherRate sheet rows, referenced by a zone's `weatherRate`
export const WEATHER_RATES = {
    // Shared by South Horn (1252) and North Horn (1346).
    168: [
        { weather: 1, rate: 10 },
        { weather: 2, rate: 45 },
        { weather: 3, rate: 15 },
        { weather: 7, rate: 10 },
        { weather: 190, rate: 15 },
        { weather: 191, rate: 5 },
    ],
};

/*
 * Eorzea time, as a Date whose UTC fields read the Eorzea clock.
 *
 * @param {Date|number} at - A Date or unix timestamp in milliseconds
 * @returns {Date} A Date carrying the Eorzea clock in UTC
 */
export function eorzeaDate(at = Date.now()) {
    return new Date((at instanceof Date ? at.getTime() : at) * EORZEA_TIME_FACTOR);
}

/*
 * Current Eorzea time, broken down.
 *
 * @param {Date|number} at - A Date or unix timestamp in milliseconds
 * @returns {{hours: number, minutes: number, seconds: number}} The Eorzea clock
 */
export function eorzeaTime(at = Date.now()) {
    const date = eorzeaDate(at);
    return {
        hours: date.getUTCHours(),
        minutes: date.getUTCMinutes(),
        seconds: date.getUTCSeconds(),
    };
}

/*
 * Formats an Eorzea clock as HH:MM, matching how the game shows it.
 *
 * @param {{hours: number, minutes: number}} time - An eorzeaTime() result
 * @returns {string} The formatted clock
 */
export function formatEorzeaTime({ hours, minutes }) {
    return `${hours.toString().padStart(2, "0")}:${minutes.toString().padStart(2, "0")}`;
}

/*
 * Start of the 8-Eorzea-hour weather window containing a given moment.
 *
 * @param {number} unixSeconds - A unix timestamp in seconds
 * @returns {number} The window's start timestamp in seconds
 */
export function weatherWindowStart(unixSeconds = Math.floor(Date.now() / 1000)) {
    return Math.floor(unixSeconds / WEATHER_WINDOW_SECONDS) * WEATHER_WINDOW_SECONDS;
}

/*
 * The 0-99 forecast target the game hashes out of the Eorzea calendar.
 *
 * @param {number} unixSeconds - Any timestamp inside the window, in seconds
 * @returns {number} A value in [0, 100)
 */
export function forecastTarget(unixSeconds = Math.floor(Date.now() / 1000)) {
    const bell = unixSeconds / EORZEA_HOUR_SECONDS;
    const increment = (bell + 8 - (bell % 8)) % 24;

    const totalDays = Math.floor(unixSeconds / EORZEA_DAY_SECONDS);

    const calcBase = totalDays * 100 + increment;
    const step1 = ((calcBase << 11) ^ calcBase) >>> 0;
    const step2 = ((step1 >>> 8) ^ step1) >>> 0;

    return step2 % 100;
}

/*
 * Resolves a forecast target against a zone's rate table.
 *
 * @param {Object} zone - A zone from $lib/zones, with a `weatherRate` row id
 * @param {number} target - A forecastTarget() value
 * @returns {number|null} The Weather row id, or null if the zone has no rates
 */
function weatherForTarget(zone, target) {
    const rates = WEATHER_RATES[zone?.weatherRate];
    if (!rates) {
        return null;
    }

    let cumulative = 0;
    for (const { weather, rate } of rates) {
        cumulative += rate;
        if (target < cumulative) {
            return weather;
        }
    }

    return rates[rates.length - 1].weather;
}

/*
 * The weather a zone has at a given moment.
 *
 * @param {Object} zone - A zone from $lib/zones
 * @param {number} unixSeconds - A unix timestamp in seconds
 * @returns {number|null} The Weather row id, or null if the zone has no rates
 */
export function weatherAt(zone, unixSeconds = Math.floor(Date.now() / 1000)) {
    return weatherForTarget(zone, forecastTarget(unixSeconds));
}

/*
 * Upcoming weather windows for a zone, starting with the current one.
 *
 * @param {Object} zone - A zone from $lib/zones
 * @param {number} count - How many windows to return, including the current one
 * @param {number} from - A unix timestamp in seconds to start from
 * @returns {Array<{weather: (Object|null), weatherId: (number|null), start: number, end: number}>}
 *          One entry per window, empty if the zone has no rate table
 */
export function weatherForecast(zone, count = 5, from = Math.floor(Date.now() / 1000)) {
    if (!WEATHER_RATES[zone?.weatherRate]) {
        return [];
    }

    const firstStart = weatherWindowStart(from);

    return Array.from({ length: count }, (_, index) => {
        const start = firstStart + index * WEATHER_WINDOW_SECONDS;
        const weatherId = weatherAt(zone, start);
        return {
            weatherId,
            weather: WEATHERS[weatherId] ?? null,
            start,
            end: start + WEATHER_WINDOW_SECONDS,
        };
    });
}
