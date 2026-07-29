import { CE_COOLDOWN_MONSTER_KILL, CE_COOLDOWN_RANDOM_SPAWN } from "$lib/const";

/*
 * Resolves a language map ({ en: ..., fr: ... }) down to a single string.
 * Falls back to English, then to a caller supplied placeholder, so that data we
 * do not know about yet renders as text instead of throwing.
 *
 * @param {Object} map - A language keyed map, may be undefined
 * @param {string} language - The current language code
 * @param {string} fallback - Text to use when the map is unknown
 * @returns {string} The localized string
 */
export function localized(map, language, fallback = "Unknown") {
    return map?.[language] ?? map?.en ?? fallback;
}

/*
 * Resolves the localized name of a zone entry (fate, encounter, item...).
 *
 * @param {Object} entry - An entry with a name map, may be undefined
 * @param {string} language - The current language code
 * @param {string} fallback - Text to use when the entry is unknown
 * @returns {string} The localized name
 */
export function localizedName(entry, language, fallback = "Unknown") {
    return localized(entry?.name, language, fallback);
}

/*
 * Resolves the localized suffix of a fate (the (North)/(South) pot markers).
 *
 * @param {Object} entry - A fate entry, may be undefined
 * @param {string} language - The current language code
 * @returns {string} The suffix prefixed with a space, or an empty string
 */
export function localizedSuffix(entry, language) {
    const suffix = localized(entry?.suffix, language, "");
    return suffix ? ` ${suffix}` : "";
}

/*
 * Checks if a fate/encounter is currently alive based on spawn and death times
 * 
 * @param {Object} fate - The fate/encounter object with spawn_time and death_time properties
 * @param {number} now - Current timestamp in seconds (defaults to current time)
 * @returns {boolean} True if the fate/encounter is alive, false otherwise
 */
export function isAlive(fate, now = Math.floor(Date.now() / 1000)) {
    // No spawn recorded yet - never alive. Sentinels differ by source: manual
    // trackers (created via /new) initialize with -1, plugin-fed trackers with 0.
    if (fate.spawn_time <= 0) {
        return false;
    }

    // A death is valid only if it's after the spawn
    const hasValidDeath = fate.death_time > fate.spawn_time;

    // If no valid death time, alive once spawned
    if (!hasValidDeath) {
        return true;
    }

    // If current time is before spawn, not alive
    if (now < fate.spawn_time) {
        return false;
    }

    // Alive if now is still before death_time
    const alive = now <= fate.death_time;
    return alive;
}

/*
 * Calculates the respawn time of pot fates in Occult Crescent, which is exactly one
 * respawn cycle after the last one spawned
 *
 * @param {Object} pot - The pot fate object
 * @param {Object} zone - The zone the tracker belongs to
 * @param {string} returnType - The type of return value (seconds, timestamp)
 * @returns {number} The timestamp of the next pot fate
 */
export function calculateOccultRespawn(pot, zone, returnType = 'seconds') {
    const now = Math.floor(Date.now() / 1000);
    const target = pot.spawn_time + zone.potRespawn;
    const remaining = target - now;

    return returnType === 'seconds' ? remaining : target;
}

/*
 * Calculates the pot status for occult trackers
 *
 * @param {Array} potHistory - Array of pot fate objects
 * @param {Object} zone - The zone the tracker belongs to
 * @returns {Object} Object containing the next pot fate and its status
 */
export function calculatePotStatus(potHistory, zone) {
    if (!potHistory || potHistory.length === 0) {
        return { bunny: null, status: null };
    }

    // Sort pot_history by last_seen (ascending), and get the nextSpawn and the lastAlive
    const sortedHistory = [...potHistory].sort((a, b) => a.last_seen - b.last_seen);
    
    const nextSpawn = sortedHistory[0];
    const lastAlive = sortedHistory[sortedHistory.length - 1];

    let bunny = null;

    // If both are -1, then no pot has spawned
    if (nextSpawn.last_seen == -1 && lastAlive.last_seen == -1) {
        bunny = nextSpawn;
    // If our last alive is still active then show it
    } else if (lastAlive.alive) {
        bunny = lastAlive;
    // Else, apply the time of the latest spawn to calculate the next spawn
    } else {
        if (nextSpawn.last_seen == -1) {
            // Set last_seen to one respawn cycle previously
            nextSpawn.last_seen = lastAlive.spawn_time - zone.potRespawn;
        }

        nextSpawn.spawn_time = lastAlive.spawn_time;
        bunny = nextSpawn;
    }

    return { bunny };
}

/*
 * Formats a number of seconds into a readable string
 * 
 * @param {number} secondsToFormat - The number of seconds to format
 * @param {string} format - The format to use (simple, relative, full)
 * @returns {string} The formatted string
 */
export function formatSeconds(secondsToFormat, format = 'simple') {
    const hours = Math.abs(Math.floor(secondsToFormat / 3600));
    const minutes = Math.abs(Math.floor((secondsToFormat % 3600) / 60));
    const seconds = Math.abs(secondsToFormat % 60);
    let finalString = '';

    if (format === 'relative') {
        const parts = [];
        if (hours) parts.push(`${hours}h`);
        if (minutes) parts.push(`${minutes}m`);
        parts.push(`${seconds}s`);
        finalString = parts.join(' ');
    } else if (format === 'full') {
        const parts = [];
        if (hours) parts.push(`${hours} hour${hours !== 1 ? 's' : ''}`);
        if (minutes) parts.push(`${minutes} minute${minutes !== 1 ? 's' : ''}`);
        parts.push(`${seconds} second${seconds !== 1 ? 's' : ''}`);
        finalString = parts.join(', ');
    } else {
        const h = hours.toString().padStart(2, '0');
        const m = minutes.toString().padStart(2, '0');
        const s = seconds.toString().padStart(2, '0');
        finalString = hours ? `${h}:${m}:${s}` : `${m}:${s}`;
    }

    return finalString;
}

/*
 * Calculates the CE cooldown status
 *
 * @param {Object} encounter - The encounter object with death_time property
 * @param {Object} zone - The zone the tracker belongs to
 * @param {number} now - Current timestamp in seconds (defaults to current time)
 * @returns {Object} Object containing cooldownEndTime, remainingSeconds, and canPop status
 */
export function calculateCECooldown(encounter, zone, now = Math.floor(Date.now() / 1000)) {
    // If encounter is alive or has no death time, no cooldown
    if (encounter.alive || !encounter.death_time || encounter.death_time === -1) {
        return {
            cooldownEndTime: null,
            remainingSeconds: 0,
            canPop: true
        };
    }

    // The Forked Tower doesn't use the cooldown system
    if (encounter.fate_id === zone.towerId) {
        return {
            cooldownEndTime: null,
            remainingSeconds: 0,
            canPop: true
        };
    }

    // Get the cooldown time based on spawn type from the zone's encounters
    const encounterData = zone.encounters[encounter.fate_id];
    const isMonsterKill = encounterData?.spawn_type === true;
    const cooldownTime = isMonsterKill ? CE_COOLDOWN_MONSTER_KILL : CE_COOLDOWN_RANDOM_SPAWN;
    
    // Calculate when cooldown ends
    const cooldownEndTime = encounter.death_time + cooldownTime;
    
    // Calculate remaining seconds
    const remainingSeconds = Math.max(0, cooldownEndTime - now);
    
    return {
        cooldownEndTime,
        remainingSeconds,
        canPop: remainingSeconds <= 0
    };
}

/*
 * Builds the initial payload for a new tracker. The histories are derived from the
 * zone's own id lists, so a zone only has to be described once in $lib/zones.
 *
 * @param {Object} zone - The zone the tracker is for
 * @param {Object} options - password, datacenter and tracker_type of the new tracker
 * @returns {Object} The payload to POST to the API
 */
export function buildTrackerTemplate(zone, { password, datacenter, trackerType }) {
    const blankEntry = (fateId) => ({
        fate_id: fateId,
        spawn_time: -1,
        death_time: -1,
        last_seen: -1,
        respawn_times: [],
        killed_fates: 0,
        killed_ces: 0,
    });

    return {
        password,
        datacenter,
        tracker_type: trackerType,
        territory: zone.territory,
        last_fate: "",
        encounter_history: JSON.stringify(zone.encounterIds.map(blankEntry)),
        fate_history: JSON.stringify(zone.fateIds.map(blankEntry)),
        pot_history: JSON.stringify(zone.potFateIds.map(blankEntry)),
    };
}
