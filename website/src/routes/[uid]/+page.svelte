<script>
    import { onDestroy, onMount } from "svelte";
    import { page } from "$app/stores";
    import { base } from "$app/paths";
    import { TOWER_SPAWN_TIMER, OCCULT_RESPAWN, OCCULT_ENCOUNTERS, OCCULT_FATES, BASE_URL, API_HEADERS } from "$lib/const";
    import { currentLanguage } from "$lib/stores";
    import { LoaderPinwheel, Frown, CircleQuestionMark, Pyramid } from "@lucide/svelte";
    import LanguageSwitcher from "../../components/LanguageSwitcher.svelte";
    import AutoTimeFormatted from "../../components/AutoTimeFormatted.svelte";
    import ItemIcon from "../../components/ItemIcon.svelte";
    import { calculateOccultRespawn, formatSeconds } from "$lib/utils";

    const uid = $page.params.uid;


    let trackerResults = $state([]);
    let bunny = $state(null); // The next pot fate to spawn, named "bunny" to match the Dalamud plugin
    var activeCE = $state(null);
    var activeBunny = $state(null);
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
                `${BASE_URL}?tracker_id=eq.${uid}`,
                {
                    headers: API_HEADERS,
                },
            );
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            trackerResults = data[0];
            
            // ADD DATA TO STATE
            activeCE = null;
            trackerResults.encounter_history = JSON.parse(trackerResults.encounter_history);
            trackerResults.encounter_history.forEach(encounter => {
                encounter.name = OCCULT_ENCOUNTERS[encounter.fate_id].name[$currentLanguage];
                encounter.alive = encounter.death_time < encounter.spawn_time;

                // If any encounter is alive, set activeCE to the first one we find
                if (encounter.alive && !activeCE) {
                    activeCE = encounter;
                }

                // When we get the Forked Tower, calculate the spawn_timer
                if (encounter.fate_id === 48) {
                    encounter.spawn_timer = TOWER_SPAWN_TIMER - (300 * encounter.killed_ces) - (60 * encounter.killed_fates);
                }
            });

            activeBunny = null;
            trackerResults.pot_history = JSON.parse(trackerResults.pot_history);
            trackerResults.pot_history.forEach(pot => {
                pot.name = OCCULT_FATES[pot.fate_id].name[$currentLanguage];
                pot.alive = pot.death_time < pot.spawn_time;
                
                // If any pot is alive, set activeBunny to the first one we find
                if (pot.alive && !activeBunny) {
                    activeBunny = pot;
                }
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
                        src={`${base}/logo.svg`}
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
            <div class="max-w-6xl px-8 mx-auto flex flex-col lg:flex-row items-center justify-between">
                <h1>
                    <a href={`${base}/`} aria-label="Occult Tracker">
                        <img
                            src={`${base}/logo.svg`}
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
                    <h2 class="text-2xl font-extrabold">
                        <img src={`${base}/icons/forked_tower.png`} alt="Forked Tower Icon" class="w-16 h-16 inline-block mr-2" />
                        {OCCULT_ENCOUNTERS[48].name[$currentLanguage]}
                    </h2>

                    <!-- Pick it from the encounter_history -->
                    {#each trackerResults.encounter_history as encounter}
                        {#if encounter.fate_id === 48}
                            {#if encounter.death_time < encounter.spawn_time}
                                <p class="text-green-400">Tower is up and recruiting!</p>
                            {:else}
                                <p>Last seen: <AutoTimeFormatted timestamp={encounter.last_seen} /></p>

                            {/if}
                            <p>Previous respawn times:
                                {#each encounter.respawn_times as time, i}
                                    <span>{formatSeconds(time) + (i < encounter.respawn_times.length - 1 ? ', ' : '')}</span>
                                {/each}
                            </p>

                            <details>
                                <summary class="bg-slate-900/90 p-2 py-0.5 rounded-md mt-2">Spawn Prediction</summary>
                                <p class="text-green-400 pt-2">
                                    Predicted spawn time: 
                                    {#if (encounter.last_seen + encounter.spawn_timer) > (new Date().getTime() / 1000)}
                                        <AutoTimeFormatted timestamp={encounter.last_seen + encounter.spawn_timer} />
                                    {:else}
                                        N/A
                                    {/if}
                                </p>
                                
                                {#if activeCE || activeBunny}
                                    <p class="text-blue-400">Upcoming reductions:</p>
                                    <ul class="list-disc list-inside text-blue-400">
                                        <!-- Display the active encounter and fate -->
                                        {#if activeBunny}
                                            <li> -1 minute ({activeBunny.name})</li>
                                        {/if}
                                        {#if activeCE}
                                            <li> -5 minutes ({activeCE.name})</li>
                                        {/if}
                                    </ul> 
                                {/if}
                            </details>   
                        {/if}
                    {/each}
                </div>

                <!-- Pot Fate-->
                <div class="bg-slate-800/90 p-4">
                    <h2 class="text-2xl font-extrabold">
                        <img src={`${base}/icons/bunny.png`} alt="Pot Fate Icon" class="w-16 h-16 inline-block mr-2" />
                        Pot Fate
                    </h2>

                    <p>FATE: {OCCULT_FATES[bunny.fate_id].name[$currentLanguage]}</p>
                    {#if bunny.death_time < bunny.spawn_time}
                        <p class="text-green-400">Alive</p>
                    {:else}
                        <p>Spawns in: <AutoTimeFormatted timestamp={calculateOccultRespawn(bunny, 'timestamp')} format="full" /></p>
                    {/if}
                </div>
            </div>

            <!-- Encounter History -->    
            <div class="max-w-6xl w-full mx-auto mb-4">
                <h2 class="text-2xl font-extrabold">
                    <img src={`${base}/icons/ce.png`} alt="Critical Encounter Icon" class="w-[1lh] h-[1lh] inline-block mr-2" />
                    Encounter History
                </h2>
                <table class="table-fixed w-full border-separate border-spacing-y-0.5 text-sm md:text-base">
                    <thead>
                        <tr class="text-left">
                            <th class="px-2 w-1/2">Encounter</th>
                            <th class="px-2 w-1/3 hidden md:table-cell">Drops</th>
                            <th class="px-2 w-1/6 text-end">Last Seen</th>
                        </tr>
                    </thead>
                    <tbody>
                        {#each trackerResults.encounter_history as encounter}
                            <tr class={encounter.death_time < encounter.spawn_time ? 'bg-amber-800/90' : 'bg-slate-900/90'}>

                                <td class="px-2 w-1/2">{OCCULT_ENCOUNTERS[encounter.fate_id].name[$currentLanguage]}</td>
                                <td class="px-2 w-1/3 hidden md:table-cell">
                                    <div class="flex flex-wrap gap-1">
                                        {#each OCCULT_ENCOUNTERS[encounter.fate_id].drops as drop}
                                            <ItemIcon itemId={drop} />
                                        {/each}
                                    </div>
                                </td>
                                <td class="px-2 w-1/6 text-end">
                                    <p class="text-nowrap">
                                        <span class="hidden md:inline">
                                            {encounter.death_time < encounter.spawn_time ? '(Alive)' : ''}
                                        </span>
                                        {#if encounter.last_seen != -1}
                                            <AutoTimeFormatted timestamp={encounter.last_seen} />
                                        {:else}
                                            N/A
                                        {/if}
                                    </p>
                                </td>
                            </tr>
                        {/each}
                    </tbody>
                </table>
            </div>
        </div>
    {/if}
</div>
