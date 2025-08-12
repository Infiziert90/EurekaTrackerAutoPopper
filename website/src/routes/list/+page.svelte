<script>
    import { base } from "$app/paths";
    import { onMount, onDestroy } from "svelte";
    import { OCCULT_FATES, OCCULT_ENCOUNTERS, BASE_URL, API_HEADERS } from "$lib/const";
    import { currentLanguage } from "$lib/stores";
    import AutoTimeFormatted from "../../components/AutoTimeFormatted.svelte";
    import LanguageSwitcher from "../../components/LanguageSwitcher.svelte";

    let trackers = $state([]);
    let loading = $state(true);
    let error = $state(null);
    let refreshInterval = $state(null);

    async function fetchRecentTrackers(hours = 1) {
        try {
            const oneHourAgo = Math.floor(
                (Date.now() - hours * 60 * 60 * 1000) / 1000,
            );
            const response = await fetch(
                `${BASE_URL}?last_update=gte.${oneHourAgo}`,
                {
                    headers: API_HEADERS,
                },
            );
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            const data = await response.json();
            return data;
        } catch (error) {
            console.error("Error fetching recent trackers:", error);
            throw error;
        }
    }

    async function loadRecentTrackers() {
        try {
            loading = true;
            error = null;

            const data = await fetchRecentTrackers(1);
            // sort by last_update
            data.sort((a, b) => b.last_update - a.last_update);

            // Process the data to add last_ce information and cap timestamps
            const currentTime = Math.floor(Date.now() / 1000);
            trackers = data.map((tracker) => {
                let isCeActive = false;
                let activeCeFateId = null;
                let recentCeFateId = null;

                // Cap the last_update timestamp to current time if it's in the future
                const cappedLastUpdate = Math.min(tracker.last_update, currentTime);

                // Try to parse encounter_history to get the last active CE
                if (tracker.encounter_history) {
                    try {
                        const encounterHistory = JSON.parse(tracker.encounter_history);
                        // Find the first CE that is currently active (death_time < spawn_time)
                        const activeCe = encounterHistory.find(
                            (ce) => ce.death_time < ce.spawn_time
                        );
                        if (activeCe) {
                            activeCeFateId = activeCe.fate_id;
                            isCeActive = true;
                        } else {
                            // If no active CE, find the most recently seen CE
                            const recentCe = encounterHistory
                                .filter(ce => ce.last_seen > 0)
                                .sort((a, b) => b.last_seen - a.last_seen)[0];
                            if (recentCe) {
                                recentCeFateId = recentCe.fate_id;
                                isCeActive = false;
                            }
                        }
                    } catch (e) {
                        console.warn(
                            "Failed to parse encounter_history for tracker:",
                            tracker.tracker_id,
                        );
                    }
                }

                return {
                    ...tracker,
                    last_update: cappedLastUpdate,
                    active_ce_fate_id: activeCeFateId,
                    recent_ce_fate_id: recentCeFateId,
                    is_ce_active: isCeActive,
                };
            });
        } catch (err) {
            console.error("Error loading recent trackers:", err);
            error = err.message;
        } finally {
            loading = false;
        }
    }

    onMount(() => {
        loadRecentTrackers();
        
        // Set up auto-refresh every minute
        refreshInterval = setInterval(() => {
            loadRecentTrackers();
        }, 60000); // 60 seconds
    });

    onDestroy(() => {
        if (refreshInterval) {
            clearInterval(refreshInterval);
        }
    });
</script>

<svelte:head>
	<title>Occult Tracker - Recent Trackers</title>
</svelte:head>

<div class="bg-slate-950 p-2 mb-2">
    <div
        class="max-w-6xl px-8 mx-auto flex flex-col lg:flex-row items-center justify-between"
    >
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
        <LanguageSwitcher />
    </div>
</div>

<div class="px-4">
    {#if loading}
        <div class="text-white mb-8">
            <p>Loading recent trackers...</p>
        </div>
    {:else if error}
        <div class="text-red-400 mb-8">
            <p>Error: {error}</p>
        </div>
    {:else if trackers.length === 0}
        <div class="text-white mb-8">
            <p>No trackers updated in the last hour.</p>
        </div>
    {:else}
        <div class="max-w-6xl mx-auto mb-8">
            <table class="w-full">
                <thead>
                    <tr>
                        <th>Tracker ID</th>
                        <th>Last Updated</th>
                        <th>Last/Current CE</th>
                    </tr>
                </thead>
                <tbody>
                    {#each trackers as tracker}
                        <tr class="relative group cursor-pointer hover:bg-slate-800 transition-colors duration-200">
                            <td class="relative">
                                {tracker.tracker_id}
                                <a
                                    href={`${base}/${tracker.tracker_id}`}
                                    class="absolute inset-0 z-10"
                                    aria-label={`View tracker ${tracker.tracker_id}`}
                                ></a>
                            </td>
                            <td class="relative">
                                <AutoTimeFormatted timestamp={tracker.last_update} format="relative" />
                                <a
                                    href={`${base}/${tracker.tracker_id}`}
                                    class="absolute inset-0 z-10"
                                    aria-label={`View tracker ${tracker.tracker_id}`}
                                ></a>
                            </td>
                            <td class="relative">
                                {#if tracker.active_ce_fate_id || tracker.recent_ce_fate_id}
                                    {@const fateId = tracker.active_ce_fate_id || tracker.recent_ce_fate_id}
                                    {@const ceName = OCCULT_ENCOUNTERS[fateId]?.name?.[$currentLanguage] || OCCULT_ENCOUNTERS[fateId]?.name?.en || "Unknown CE"}
                                    <span class="flex items-center gap-2">
                                        <span class={`w-2 h-2 rounded-full ${tracker.is_ce_active ? 'bg-green-500' : 'bg-gray-500'}`} title={tracker.is_ce_active ? 'Currently Active' : 'Not Active'}></span>
                                        {ceName}
                                    </span>
                                {:else}
                                    None
                                {/if}
                                <a
                                    href={`${base}/${tracker.tracker_id}`}
                                    class="absolute inset-0 z-10"
                                    aria-label={`View tracker ${tracker.tracker_id}`}
                                ></a>
                            </td>
                        </tr>
                    {/each}
                </tbody>
            </table>
        </div>
    {/if}
</div>
