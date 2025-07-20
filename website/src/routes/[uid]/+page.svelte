<script>
    import { onDestroy, onMount } from "svelte";
    import { page } from "$app/stores";
    import { OCCULT_ENCOUNTERS, OCCULT_FATES } from "$lib/const";
    import { LoaderPinwheel, Frown, CircleQuestionMark, Pyramid } from "@lucide/svelte";
    import Time from "svelte-time";

    const uid = $page.params.uid;
    const SUPABASE_URL = "https://xzwnvwjxgmaqtrxewngh.supabase.co/rest/v1/";
    const SUPABASE_ANON_KEY =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inh6d252d2p4Z21hcXRyeGV3bmdoIiwicm9sZSI6ImFub24iLCJpYXQiOjE2ODk3NzcwMDIsImV4cCI6MjAwNTM1MzAwMn0.aNYTnhY_Sagi9DyH5Q9tCz9lwaRCYzMC12SZ7q7jZBc";

    const timeFormatter = new Intl.RelativeTimeFormat('en', { numeric: 'auto' });

    let trackerResults = $state([]);
    let fetchInterval = $state(null);
    let isLoading = $state(true);
    let error = $state(null);

    // Fetch tracker data from Supabase
    async function fetchTrackerData() {
        try {
            isLoading = true;
            error = null;

            const response = await fetch(
                `${SUPABASE_URL}OccultTracker?identifier=eq.${uid}`,
                {
                    headers: {
                        apikey: SUPABASE_ANON_KEY,
                        Authorization: `Bearer ${SUPABASE_ANON_KEY}`,
                        Prefer: "return=representation",
                    },
                },
            );

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            trackerResults = data[0];
            trackerResults.encounter_history = JSON.parse(trackerResults.encounter_history);
            trackerResults.pot_history = JSON.parse(trackerResults.pot_history);
            console.log(trackerResults);
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
                <a href="/" aria-label="Occult Tracker">
                    <img
                        src="/logo.png"
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
                class="max-w-6xl px-8 mx-auto flex flex-row items-center justify-between"
            >
                <h1>
                    <a href="/" aria-label="Occult Tracker">
                        <img
                            src="/logo.png"
                            alt="Occult Tracker"
                            height="80"
                            class="h-20 w-auto"
                        />
                    </a>
                </h1>
                <div>
                    <p>Tracker ID: {uid}</p>
                </div>
            </div>
        </div>

        <!-- Encounter History -->    
        <div class="max-w-6xl w-full mx-auto mb-4">
            <h2 class="text-2xl font-bold">
                <img src="/icons/ce.png" alt="Critical Encounter Icon" class="w-[1lh] h-[1lh] inline-block mr-2" />
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
                        <tr class="bg-slate-800">
                            <td class="px-2">{OCCULT_ENCOUNTERS[encounter.fate_id].name}</td>
                            <td class="px-2">{OCCULT_ENCOUNTERS[encounter.fate_id].aetheryte}</td>
                            <td class="px-2">
                                {#if encounter.last_seen != -1}
                                    <Time timestamp={encounter.last_seen * 1000} relative />
                                {:else}
                                    N/A
                                {/if}
                            </td>
                        </tr>
                    {/each}
                </tbody>
            </table>
        </div>

        <!-- Pot History -->
        <div class="max-w-6xl w-full mx-auto mb-4">
            <h2 class="text-2xl font-bold">
                <img src="/icons/bunny.png" alt="Bunny Icon" class="w-[1lh] h-[1lh] inline-block mr-2" />
                Bunny History
            </h2>
            <table class="table-auto w-full border-separate border-spacing-y-0.5 text-sm">
                <thead>
                    <tr class="text-left">
                        <th class="px-2">Fate</th>
                        <th class="px-2">Aetheryte</th>
                        <th class="px-2">Last Seen</th>
                    </tr>
                </thead>
                <tbody>
                    {#each trackerResults.pot_history as pot}
                        <tr class="bg-slate-800">
                            <td class="px-2">{OCCULT_FATES[pot.fate_id].name}</td>
                            <td class="px-2">{OCCULT_FATES[pot.fate_id].aetheryte}</td>
                            <td class="px-2">
                                {#if pot.last_seen != -1}
                                    <Time timestamp={pot.last_seen * 1000} relative />
                                {:else}
                                    N/A
                                {/if}
                            </td>
                        </tr>
                    {/each}
                </tbody>
            </table>
        </div>
    {/if}
</div>
