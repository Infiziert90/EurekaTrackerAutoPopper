import { OCCULT_RESPAWN } from "$lib/const";

/*
 * Calculates the respawn time of pot fates in Occult Crescent, which is exactly 30 minutes after the last one spawned
 * 
 * @param {Object} pot - The pot fate object
 * @param {string} returnType - The type of return value (seconds, timestamp)
 * @returns {number} The timestamp of the next pot fate
 */
export function calculateOccultRespawn(pot, returnType = 'seconds') {
    const now = Math.floor(Date.now() / 1000);
    const target = pot.spawn_time + OCCULT_RESPAWN;
    const remaining = target - now;

    return returnType === 'seconds' ? remaining : target;
}

/*
 * Calculates the pot status for occult trackers
 * 
 * @param {Array} potHistory - Array of pot fate objects
 * @returns {Object} Object containing the next pot fate and its status
 */
export function calculatePotStatus(potHistory) {
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
            // Set last_seen to 30 min previously
            nextSpawn.last_seen = lastAlive.spawn_time - OCCULT_RESPAWN;
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