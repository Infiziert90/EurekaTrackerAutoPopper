import { OCCULT_RESPAWN } from "$lib/const";

/*
 * Calculates the respawn time of pot fates in Occult Crescent, which is exactly 30 minutes after the last one spawned
 * 
 * @param {Object} pot - The pot fate object
 * @returns {number} The timestamp of the next pot fate
 */
export function calculateOccultRespawn(pot) {
   const currentTime = new Date().getTime() / 1000;
   const seconds = pot.spawn_time + OCCULT_RESPAWN - currentTime;
   return seconds;
}

/*
 * Formats a number of seconds into a readable string
 * 
 * @param {number} seconds - The number of seconds to format
 * @returns {string} The formatted string
 */
export function formatSeconds(seconds) {
    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = seconds % 60;
    if (minutes > 0) {
        return `${minutes} min`;
    } else if (remainingSeconds > 0) {
        return `${remainingSeconds} sec`;
    } else {
        return 'soon/now';
    }
}