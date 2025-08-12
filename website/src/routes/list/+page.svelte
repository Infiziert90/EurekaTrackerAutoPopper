<script>
    import { base } from "$app/paths";
    import { onMount } from "svelte";
    import { OCCULT_FATES, BASE_URL, API_HEADERS } from "$lib/const";

    let trackers = $state([]);
    let loading = $state(true);
    let error = $state(null);

    async function fetchRecentTrackers(hours = 6) {
        try {
            const sixHoursAgo = Math.floor(
                (Date.now() - hours * 60 * 60 * 1000) / 1000,
            );
            const response = await fetch(
                `${BASE_URL}?last_update=gte.${sixHoursAgo}`,
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

            const data = await fetchRecentTrackers(6);
            // sort by last_update
            data.sort((a, b) => b.last_update - a.last_update);

            // Process the data to add last_fate information
            trackers = data.map((tracker) => {
                let lastFate = "";

                // Try to parse pot_history to get the last active fate
                if (tracker.pot_history) {
                    try {
                        const potHistory = JSON.parse(tracker.pot_history);
                        const lastActivePot = potHistory.find(
                            (pot) => pot.death_time < pot.spawn_time,
                        );
                        if (lastActivePot) {
                            lastFate =
                                OCCULT_FATES[lastActivePot.fate_id]?.name?.en ||
                                "Unknown Fate";
                        }
                    } catch (e) {
                        console.warn(
                            "Failed to parse pot_history for tracker:",
                            tracker.tracker_id,
                        );
                    }
                }

                return {
                    ...tracker,
                    last_fate: lastFate,
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
    });

    function formatTime(timestamp) {
        const date = new Date(timestamp * 1000); // Convert from Unix timestamp
        const now = new Date();
        const diffMs = now - date;
        const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
        const diffMinutes = Math.floor(
            (diffMs % (1000 * 60 * 60)) / (1000 * 60),
        );

        if (diffHours > 0) {
            return `${diffHours}h ${diffMinutes}m ago`;
        } else {
            return `${diffMinutes}m ago`;
        }
    }
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
            <p>No trackers updated in the last 6 hours.</p>
        </div>
    {:else}
        <div class="max-w-4xl mx-auto mb-8">
            <table class="w-full">
                <thead>
                    <tr>
                        <th>Tracker ID</th>
                        <th>Last Updated</th>
                        <th>View Tracker</th>
                    </tr>
                </thead>
                <tbody>
                    {#each trackers as tracker}
                        <tr>
                            <td>{tracker.tracker_id}</td>
                            <td>{formatTime(tracker.last_update)}</td>
                            <td>
                                <a
                                    href={`${base}/${tracker.tracker_id}`}
                                    class="inline-block bg-blue-600 hover:bg-blue-700 text-white px-3 py-1 rounded text-sm transition-colors duration-200"
                                >
                                    View Tracker
                                </a>
                            </td>
                        </tr>
                    {/each}
                </tbody>
            </table>
        </div>
    {/if}
</div>
