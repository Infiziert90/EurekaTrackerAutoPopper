<script>
    import { onDestroy, onMount } from "svelte";
    import { page } from "$app/stores";
    import { base } from "$app/paths";
    import { OCCULT_RESPAWN,OCCULT_ENCOUNTERS, OCCULT_FATES } from "$lib/const";
    import { currentLanguage } from "$lib/stores";
    import { LoaderPinwheel, Frown, CircleQuestionMark, Pyramid } from "@lucide/svelte";
    import LanguageSwitcher from "../../components/LanguageSwitcher.svelte";
    import AutoTimeFormatted from "../../components/AutoTimeFormatted.svelte";
    import { calculateOccultRespawn, formatSeconds } from "$lib/utils";
    import Time from "svelte-time";

    const uid = $page.params.uid;
    const BASE_URL = "https://infi.ovh/api/";
    const BASE_ANON_KEY =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJyb2xlIjoiYW5vbiJ9.Ur6wgi_rD4dr3uLLvbLoaEvfLCu4QFWdrF-uHRtbl_s";

    const timeFormatter = new Intl.RelativeTimeFormat('en', { numeric: 'auto' });

    let trackerResults = $state([]);
    let bunny = $state(null); // The next pot fate to spawn, named "bunny" to match the Dalamud plugin
    let fetchInterval = $state(null);
    let isLoading = $state(true);
    let error = $state(null);

    // Fetch tracker data from API
    async function fetchTrackerData() {
        try {
            isLoading = true;
            error = null;

            // FETCH TRACKER DATA
            const response = await fetch(
                `${BASE_URL}OccultTrackerV2?tracker_id=eq.${uid}`,
                {
                    headers: {
                        apikey: BASE_ANON_KEY,
                        Authorization: `Bearer ${BASE_ANON_KEY}`,
                        Prefer: "return=representation",
                    },
                },
            );
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            trackerResults = data[0];
            
            // ADD DATA TO STATE
            trackerResults.encounter_history = JSON.parse(trackerResults.encounter_history);
            trackerResults.pot_history = JSON.parse(trackerResults.pot_history);
            trackerResults.pot_history.forEach(pot => {
                pot.last_seen = pot.last_seen;
                pot.spawn_time = pot.spawn_time;
                pot.name = OCCULT_FATES[pot.fate_id].name[$currentLanguage];
            });

            // Sort pot_history by last_seen (ascending), and get the nextSpawn and the lastAlive
            trackerResults.pot_history.sort((a, b) => a.last_seen - b.last_seen);
            const nextSpawn = trackerResults.pot_history[0];
            const lastAlive = trackerResults.pot_history[trackerResults.pot_history.length - 1];

            // If both are -1, then no pot has spawned
            if (nextSpawn == -1 && lastAlive == -1) {
                bunny = nextSpawn;
            } else { 
                // Else, apply the time of the latest spawn to calculate the next spawn
                if (nextSpawn.last_seen == -1) {
                    // Set last_seen to 30 min previously
                    nextSpawn.last_seen = lastAlive.spawn_time - OCCULT_RESPAWN;
                }                

                nextSpawn.spawn_time = lastAlive.spawn_time;
                bunny = nextSpawn;
            }

        } catch (err) {
            console.error("Error fetching tracker data:", err);
            error = err.message;
        } finally {
            isLoading = false;
        }
    }

    onMount(() => {
        // Initial fetch
        fetchTrackerData();

        // Set up polling every 30 seconds
        fetchInterval = setInterval(fetchTrackerData, 30000);
    });

    // Clean up interval on component destroy
    onDestroy(() => {
        if (fetchInterval) {
            clearInterval(fetchInterval);
        }
    });
</script>

<svelte:head>
	<title>Occult Tracker - {uid}</title>
</svelte:head>

<div
    class="flex flex-col h-full w-full {isLoading && trackerResults.length === 0 || error || trackerResults.length === 0 ? 'justify-center' : ''}"
>
    {#if (isLoading && trackerResults.length === 0) || error || trackerResults.length === 0}
        <div class="bg-slate-950 text-center p-20">
            <h1 class="w-fit mx-auto mb-4">
                <a href={`${base}/`} aria-label="Occult Tracker">
                    <img
                        src={`${base}/logo.png`}
                        alt="Occult Tracker"
                        height="80"
                        class="h-20"
                    />
                </a>
            </h1>

            {#if isLoading && trackerResults.length === 0}
                <div class="flex flex-col items-center justify-center">
                    <LoaderPinwheel class="animate-spin" />
                    <div class="mt-2">Loading tracker data...</div>
                </div>
            {:else if error}
                <div class="flex flex-col items-center justify-center text-red-400">
                    <Frown />
                    <div class="mt-2">An error occurred while fetching tracker data.</div>
                </div>
            {:else if trackerResults.length === 0}
                <div class="flex flex-col items-center justify-center">
                    <CircleQuestionMark />
                    <div class="mt-2">No tracker data found for {uid}.</div>
                </div>
            {/if}
        </div>
    {:else}
        <div class="bg-slate-950 p-2 mb-2">
            <div
                class="max-w-6xl px-8 mx-auto flex flex-col lg:flex-row items-center justify-between"
            >
                <h1>
                    <a href={`${base}/`} aria-label="Occult Tracker">
                        <img
                            src={`${base}/logo.png`}
                            alt="Occult Tracker"
                            height="80"
                            class="h-20 w-auto"
                        />
                    </a>
                </h1>
                <div class="flex flex-col items-end gap-2">
                    <p>Tracker ID: {uid}</p>
                    <LanguageSwitcher />
                </div>
            </div>
        </div>

        <div class="px-4">
            <!-- 2col, Forked Tower & Pot Fate -->
            <div class="max-w-6xl w-full mx-auto grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">

                <!-- Forked Tower: Blood -->
                <div class="bg-slate-800/90 p-4">
                    <h2 class="text-2xl font-bold">
                        <img src={`${base}/icons/forked_tower.png`} alt="Forked Tower Icon" class="w-16 h-16 inline-block mr-2" />
                        {OCCULT_ENCOUNTERS[48].name[$currentLanguage]}
                    </h2>

                    <!-- Pick it from the encounter_history -->
                    {#each trackerResults.encounter_history as encounter}
                        {#if encounter.fate_id === 48}
                            <p>Last seen: <AutoTimeFormatted timestamp={encounter.last_seen} /></p>
                            <p>Previous respawn times:

                                {#each encounter.respawn_times as time, i}
                                    <span>{formatSeconds(time) + (i < encounter.respawn_times.length - 1 ? ', ' : '')}</span>
                                {/each}
                            </p>
                        {/if}
                    {/each}
                </div>

                <!-- Pot Fate-->
                <div class="bg-slate-800/90 p-4">
                    <h2 class="text-2xl font-bold">
                        <img src={`${base}/icons/bunny.png`} alt="Pot Fate Icon" class="w-16 h-16 inline-block mr-2" />
                        Pot Fate
                    </h2>

                    <p>Incoming FATE: {OCCULT_FATES[bunny.fate_id].name[$currentLanguage]}</p>
                    <p>Respawns in: <AutoTimeFormatted timestamp={calculateOccultRespawn(bunny, 'timestamp')} format="full" /></p>
                </div>
            </div>

            <!-- Encounter History -->    
            <div class="max-w-6xl w-full mx-auto mb-4">
                <h2 class="text-2xl font-bold">
                    <img src={`${base}/icons/ce.png`} alt="Critical Encounter Icon" class="w-[1lh] h-[1lh] inline-block mr-2" />
                    Encounter History
                </h2>
                <table class="table-auto w-full border-separate border-spacing-y-0.5 text-sm">
                    <thead>
                        <tr class="text-left">
                            <th class="px-2">Encounter</th>
                            <th class="px-2">Aetheryte</th>
                            <th class="px-2">Last Seen</th>
                        </tr>
                    </thead>
                    <tbody>
                        {#each trackerResults.encounter_history as encounter}
                            <tr class="bg-slate-800/90">
                                <td class="px-2">{OCCULT_ENCOUNTERS[encounter.fate_id].name[$currentLanguage]}</td>
                                <td class="px-2">{OCCULT_ENCOUNTERS[encounter.fate_id].aetheryte}</td>
                                <td class="px-2 text-end">
                                    {#if encounter.last_seen != -1}
                                        <AutoTimeFormatted timestamp={encounter.last_seen} />
                                    {:else}
                                        N/A
                                    {/if}
                                </td>
                            </tr>
                        {/each}
                    </tbody>
                </table>
            </div>
        </div>
    {/if}
</div>
