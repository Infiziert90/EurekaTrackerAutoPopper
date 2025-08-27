<script>
    import { base } from "$app/paths";
    import { onMount, onDestroy } from "svelte";
    import { Clock, Globe } from "@lucide/svelte";
    import { DATACENTER_NAMES, OCCULT_FATES, OCCULT_ENCOUNTERS, BASE_URL, API_HEADERS } from "$lib/const";
    import { calculatePotStatus, calculateOccultRespawn } from "$lib/utils";
    import { currentLanguage } from "$lib/stores";
    import AutoTimeFormatted from "../../components/AutoTimeFormatted.svelte";
    import LanguageSwitcher from "../../components/LanguageSwitcher.svelte";

    let trackers = $state([]);
    let loading = $state(true);
    let error = $state(null);
    let refreshInterval = $state(null);

    async function fetchRecentTrackers() {
        try {
            const thirtyMinutesAgo = Math.floor(
                (Date.now() - 30 * 60 * 1000) / 1000,
            );
            const response = await fetch(
                `${BASE_URL}?last_update=gte.${thirtyMinutesAgo}`,
                {
                    headers: API_HEADERS,
                },
            );
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            const data = await response.json();

            // filter out trackers with a datacenter field set to 0
            return data.filter(tracker => tracker.datacenter !== 0);
        } catch (error) {
            console.error("Error fetching recent trackers:", error);
            throw error;
        }
    }

    async function loadRecentTrackers() {
        try {
            loading = true;
            error = null;

            const data = await fetchRecentTrackers();
            // sort by last_update
            data.sort((a, b) => b.last_update - a.last_update);

            // Process the data to add last_ce information and cap timestamps
            const currentTime = Math.floor(Date.now() / 1000);
            trackers = data.map((tracker) => {
                let isCeActive = false;
                let activeCeFateId = null;
                let recentCeFateId = null;
                
                let isFateActive = false;
                let activeFateId = null;
                let recentFateId = null;

                // Cap the last_update timestamp to current time if it's in the future
                const cappedLastUpdate = Math.min(tracker.last_update, currentTime);
                
                // Process pot status
                let potStatus = null;
                let potStatusText = null;
                if (tracker.pot_history) {
                    try {
                        const potHistory = JSON.parse(tracker.pot_history);
                        const potData = calculatePotStatus(potHistory);
                        if (potData.bunny) {
                            potStatus = potData.bunny;
                            if (potData.bunny.alive === true) {
                                potStatusText = "Alive";
                            } else {
                                const respawnTime = calculateOccultRespawn(potData.bunny, 'timestamp');
                                const now = Math.floor(Date.now() / 1000);
                                if (respawnTime <= now) {
                                    potStatusText = "Soon";
                                } else {
                                    potStatusText = respawnTime;
                                }
                            }
                        }
                    } catch (e) {
                        console.warn(
                            "Failed to parse pot_history for tracker:",
                            tracker.tracker_id,
                        );
                    }
                }

                // Try to parse encounter_history to get the last active CE
                if (tracker.encounter_history) {
                    try {
                        const encounterHistory = JSON.parse(tracker.encounter_history);
                        // Find the first CE that is currently active (death_time < spawn_time AND also different than -1)
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

                // Try to parse fate_history to get the last active fate
                if (tracker.fate_history) {
                    try {
                        const fateHistory = JSON.parse(tracker.fate_history);
                        // Find the first fate that is currently active (death_time < spawn_time AND also different than -1)
                        const activeFate = fateHistory.find(
                            (fate) => fate.death_time < fate.spawn_time
                        );
                        if (activeFate) {
                            activeFateId = activeFate.fate_id;
                            isFateActive = true;
                        } else {
                            // If no active fate, find the most recently seen fate
                            const recentFate = fateHistory
                                .filter(fate => fate.last_seen > 0)
                                .sort((a, b) => b.last_seen - a.last_seen)[0];
                            if (recentFate) {
                                recentFateId = recentFate.fate_id;
                                isFateActive = false;
                            }
                        }
                    } catch (e) {
                        console.warn(
                            "Failed to parse fate_history for tracker:",
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
                    active_fate_id: activeFateId,
                    recent_fate_id: recentFateId,
                    is_fate_active: isFateActive,
                    pot_status: potStatus,
                    pot_status_text: potStatusText,
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
	<title>Occult Tracker - Trackers List</title>
</svelte:head>

<div class="bg-slate-950 p-2 mb-2 sticky top-0 z-10 overscroll-pseudo-elt">
    <div
        class="max-w-6xl px-8 mx-auto flex flex-col lg:flex-row items-center justify-between"
    >
        <h1>
            <a href={`${base}/`} aria-label="Occult Tracker">
                <img
                    src={`${base}/logo.svg`}
                    alt="Occult Tracker"
                    height="80"
                    class="h-14 md:h-20 w-auto"
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
        <div class="w-full max-w-6xl mx-auto mb-4">
            <table class="table-auto w-full border-separate border-spacing-y-0.5 text-sm md:text-base">
                <thead>
                    <tr>
                        <th class="text-left px-2">Tracker ID</th>
                        <th class="text-left truncate px-2">
                            <span class="inline md:hidden text-center">
                                <Clock class="w-4 h-4 inline-block" />
                            </span>
                            <span class="hidden md:inline">Last Updated</span>
                        </th>
                        <th class="text-left truncate px-2">
                            <span class="inline md:hidden text-center">
                                <Globe class="w-4 h-4 inline-block" />
                            </span>
                            <span class="hidden md:inline">Datacenter</span>
                        </th>
                        <th class="text-left hidden sm:table-cell truncate px-2">Last/Current CE</th>
                        <th class="text-left hidden md:table-cell truncate px-2">Pot Status</th>
                        <th class="text-left hidden md:table-cell truncate px-2">Last/Current Fate</th>
                    </tr>
                </thead>
                <tbody>
                    {#each trackers as tracker}
                        <tr class="relative group cursor-pointer bg-slate-900/90 hover:bg-slate-800 transition-colors duration-200">
                            <td class="relative px-2 font-mono">
                                {tracker.tracker_id}
                                <a
                                    href={`${base}/${tracker.tracker_id}`}
                                    class="absolute inset-0 z-10"
                                    aria-label={`View tracker ${tracker.tracker_id}`}
                                ></a>
                            </td>
                            <td class="relative px-2 truncate">
                                <AutoTimeFormatted timestamp={tracker.last_update} format="relative" disableUpdate={true} />
                                <a
                                    href={`${base}/${tracker.tracker_id}`}
                                    class="absolute inset-0 z-10"
                                    aria-label={`View tracker ${tracker.tracker_id}`}
                                ></a>
                            </td>
                            <td class="relative px-2 truncate">
                                {DATACENTER_NAMES[tracker.datacenter]?.name || "Unknown"}
                                <a
                                    href={`${base}/${tracker.tracker_id}`}
                                    class="absolute inset-0 z-10"
                                    aria-label={`View tracker ${tracker.tracker_id}`}
                                ></a>
                            </td>
                            <td class="hidden sm:table-cell relative px-2 truncate">
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
                            <td class="hidden md:table-cell relative px-2 truncate">
                                {#if tracker.pot_status_text}
                                    {#if tracker.pot_status_text === "Alive"}
                                        Alive
                                    {:else if tracker.pot_status_text === "Soon"}
                                        Soon
                                    {:else}
                                        In <AutoTimeFormatted timestamp={tracker.pot_status_text} format="relative" disableUpdate={true} />
                                    {/if}
                                {:else}
                                    None
                                {/if}
                                <a
                                    href={`${base}/${tracker.tracker_id}`}
                                    class="absolute inset-0 z-10"
                                    aria-label={`View tracker ${tracker.tracker_id}`}
                                ></a>
                            </td>
                            <td class="hidden md:table-cell relative px-2 truncate">
                                {#if tracker.active_fate_id || tracker.recent_fate_id}
                                    {@const fateId = tracker.active_fate_id || tracker.recent_fate_id}
                                    {@const fateName = OCCULT_FATES[fateId]?.name?.[$currentLanguage] || OCCULT_FATES[fateId]?.name?.en || "Unknown Fate"}
                                    <span class="flex items-center gap-2">
                                        <span class={`w-2 h-2 rounded-full ${tracker.is_fate_active ? 'bg-green-500' : 'bg-gray-500'}`} title={tracker.is_fate_active ? 'Currently Active' : 'Not Active'}></span>
                                        {fateName}
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
