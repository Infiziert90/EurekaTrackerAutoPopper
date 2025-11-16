<script>
    import { base } from "$app/paths";
    import { page } from "$app/stores";
    import { goto } from "$app/navigation";
    import { onMount, onDestroy } from "svelte";
    import { Clock, Globe, ChevronUp, ChevronDown, ChevronsUpDown, X } from "@lucide/svelte";
    import { DATACENTER_NAMES, OCCULT_FATES, OCCULT_ENCOUNTERS, BASE_URL, API_HEADERS } from "$lib/const";
    import { calculatePotStatus, calculateOccultRespawn, isAlive } from "$lib/utils";
    import { currentLanguage } from "$lib/stores";
    import { DataTable } from "@careswitch/svelte-data-table";
    import AutoTimeFormatted from "../../components/AutoTimeFormatted.svelte";
    import LanguageSwitcher from "../../components/LanguageSwitcher.svelte";

    let trackers = $state([]);
    let loading = $state(true);
    let error = $state(null);
    let refreshInterval = $state(null);
    
    // Applied filter - this is what actually filters the API calls
    let appliedDatacenterIds = $state(new Set());
    
    // Temporary selections in dialog - only applied when user clicks "Apply"
    let tempDatacenterIds = $state(new Set());
    
    let table = $state(null);
    let datacenterDialog = $state(null);
    
    // Get all selectable datacenters grouped by region
    const datacentersByRegion = Object.entries(DATACENTER_NAMES)
        .filter(([id, dc]) => dc.selectable)
        .reduce((regions, [id, dc]) => {
            if (!regions[dc.region]) {
                regions[dc.region] = [];
            }
            regions[dc.region].push({ id: parseInt(id), name: dc.name });
            return regions;
        }, {});
    
    const sortedRegions = Object.keys(datacentersByRegion).sort();
    
    // Helper to get selected datacenter objects for display
    function getSelectedDatacenters() {
        return Array.from(appliedDatacenterIds)
            .map(id => {
                const dc = DATACENTER_NAMES[id];
                return dc ? { value: id, label: dc.name } : null;
            })
            .filter(Boolean);
    }
    
    // Open dialog: copy applied selections to temp selections
    function openDatacenterDialog() {
        tempDatacenterIds = new Set(appliedDatacenterIds);
        datacenterDialog?.showModal();
    }
    
    // Close dialog: discard temp selections
    function closeDatacenterDialog() {
        datacenterDialog?.close();
    }
    
    // Toggle individual datacenter in dialog (temp state)
    function toggleDatacenter(dcId) {
        tempDatacenterIds = new Set(tempDatacenterIds);
        if (tempDatacenterIds.has(dcId)) {
            tempDatacenterIds.delete(dcId);
        } else {
            tempDatacenterIds.add(dcId);
        }
    }
    
    // Toggle entire region in dialog (temp state)
    function toggleRegion(region) {
        const regionDcs = datacentersByRegion[region] || [];
        const regionIds = new Set(regionDcs.map(dc => dc.id));
        const allSelected = regionIds.size > 0 && Array.from(regionIds).every(id => tempDatacenterIds.has(id));
        
        tempDatacenterIds = new Set(tempDatacenterIds);
        if (allSelected) {
            // Deselect all in region
            regionIds.forEach(id => tempDatacenterIds.delete(id));
        } else {
            // Select all in region
            regionIds.forEach(id => tempDatacenterIds.add(id));
        }
    }
    
    // Check if datacenter is selected in dialog (temp state)
    function isDatacenterSelected(dcId) {
        return tempDatacenterIds.has(dcId);
    }
    
    // Check if region is fully selected in dialog (temp state)
    function isRegionFullySelected(region) {
        const regionDcs = datacentersByRegion[region] || [];
        return regionDcs.length > 0 && regionDcs.every(dc => tempDatacenterIds.has(dc.id));
    }
    
    // Check if region is partially selected in dialog (temp state)
    function isRegionPartiallySelected(region) {
        const regionDcs = datacentersByRegion[region] || [];
        const selectedCount = regionDcs.filter(dc => tempDatacenterIds.has(dc.id)).length;
        return selectedCount > 0 && selectedCount < regionDcs.length;
    }
    
    // Apply selections: copy temp to applied, update URL, and fetch
    async function applySelections() {
        // Capture IDs before closing dialog
        const ids = Array.from(tempDatacenterIds);
        console.log('Applying filter with datacenter IDs:', ids);
        
        // Copy temp selections to applied filter
        appliedDatacenterIds = new Set(ids);
        
        // Close dialog
        closeDatacenterDialog();
        
        // Update URL params
        const params = new URLSearchParams();
        if (ids.length > 0) {
            params.set('dc', ids.join(','));
        }
        const newUrl = $page.url.pathname + (params.toString() ? '?' + params.toString() : '');
        await goto(newUrl, { replaceState: true, noScroll: true });
        
        // Ensure state is set (in case of any reactivity issues)
        appliedDatacenterIds = new Set(ids);
        
        // Fetch with new filter - pass IDs directly to avoid state timing issues
        await loadRecentTrackers(ids);
    }
    
    // Initialize filters from URL params (only on mount)
    function initializeFilters() {
        const dcParam = $page.url.searchParams.get('dc');
        if (dcParam) {
            const ids = dcParam.split(',').map(id => parseInt(id)).filter(id => !isNaN(id) && DATACENTER_NAMES[id]?.selectable);
            appliedDatacenterIds = new Set(ids);
        } else {
            appliedDatacenterIds = new Set();
        }
    }
    
    // Update indeterminate state for region checkboxes
    $effect(() => {
        sortedRegions.forEach(region => {
            const checkbox = document.getElementById(`region-${region}`);
            if (checkbox) {
                checkbox.indeterminate = isRegionPartiallySelected(region);
            }
        });
    });

    async function fetchRecentTrackers(filterIds = null) {
        try {
            const thirtyMinutesAgo = Math.floor(
                (Date.now() - 30 * 60 * 1000) / 1000,
            );
            
            // Use provided filterIds or fall back to applied filter
            const selectedIds = filterIds !== null ? filterIds : Array.from(appliedDatacenterIds);
            console.log('Fetching with datacenter filter:', selectedIds);
            
            // Build query parameters
            const params = new URLSearchParams();
            params.set('last_update', `gte.${thirtyMinutesAgo}`);
            
            // Filter by selected datacenters if any are selected
            if (selectedIds.length > 0) {
                params.set('datacenter', `in.(${selectedIds.join(',')})`);
            }
            
            const url = `${BASE_URL}?${params.toString()}`;
            console.log('API URL:', url);
            
            const response = await fetch(url, {
                headers: API_HEADERS,
            });
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

    async function processTrackerData(data) {
        // Process the data to add last_ce information and cap timestamps
        const currentTime = Math.floor(Date.now() / 1000);
        return data.map((tracker) => {
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
                    potHistory.forEach(pot => {
                        pot.alive = isAlive(pot);
                    });
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
                    encounterHistory.forEach(ce => {
                        ce.alive = isAlive(ce);
                    });
                    const activeCe = encounterHistory.find(
                        (ce) => ce.alive
                    );
                    if (activeCe) {
                        activeCeFateId = activeCe.fate_id;
                        isCeActive = true;
                    } else {
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
                    fateHistory.forEach(fate => {
                        fate.alive = isAlive(fate);
                    });
                    const activeFate = fateHistory.find(
                        (fate) => fate.alive
                    );
                    if (activeFate) {
                        activeFateId = activeFate.fate_id;
                        isFateActive = true;
                    } else {
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
                datacenter_name: DATACENTER_NAMES[tracker.datacenter]?.name || "Unknown",
            };
        });
    }

    async function loadRecentTrackers(filterIds = null) {
        try {
            loading = true;
            error = null;

            const data = await fetchRecentTrackers(filterIds);
            const processedData = await processTrackerData(data);
            trackers = processedData;
            
            // Always recreate the table to ensure it updates properly
            table = new DataTable({
                data: trackers,
                columns: [
                    { id: 'tracker_id', key: 'tracker_id', name: 'Tracker ID', sortable: true },
                    { id: 'last_update', key: 'last_update', name: 'Last Updated', sortable: true },
                    { id: 'datacenter', key: 'datacenter_name', name: 'Datacenter', sortable: true },
                    { id: 'pot_status', key: 'pot_status_text', name: 'Pot Status', sortable: true },
                    { id: 'ce', key: 'active_ce_fate_id', name: 'Last/Current CE', sortable: false },
                    { id: 'fate', key: 'active_fate_id', name: 'Last/Current Fate', sortable: false },
                ],
            });
        } catch (err) {
            console.error("Error loading recent trackers:", err);
            error = err.message;
        } finally {
            loading = false;
        }
    }

    async function refreshTrackers() {
        try {
            error = null;
            const data = await fetchRecentTrackers();
            const processedData = await processTrackerData(data);
            trackers = processedData;
            // Recreate table to ensure it updates
            if (table) {
                table = new DataTable({
                    data: trackers,
                    columns: [
                        { id: 'tracker_id', key: 'tracker_id', name: 'Tracker ID', sortable: true },
                        { id: 'last_update', key: 'last_update', name: 'Last Updated', sortable: true },
                        { id: 'datacenter', key: 'datacenter_name', name: 'Datacenter', sortable: true },
                        { id: 'pot_status', key: 'pot_status_text', name: 'Pot Status', sortable: true },
                        { id: 'ce', key: 'active_ce_fate_id', name: 'Last/Current CE', sortable: false },
                        { id: 'fate', key: 'active_fate_id', name: 'Last/Current Fate', sortable: false },
                    ],
                });
            }
        } catch (err) {
            console.error("Error refreshing trackers:", err);
        }
    }

    // Clear all filters
    async function clearAllFilters() {
        appliedDatacenterIds = new Set();
        
        // Update URL
        const newUrl = $page.url.pathname;
        await goto(newUrl, { replaceState: true, noScroll: true });
        
        // Refresh data
        await loadRecentTrackers();
    }

    onMount(() => {
        initializeFilters();
        loadRecentTrackers();

        // Set up auto-refresh every minute
        refreshInterval = setInterval(refreshTrackers, 60000);
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
        class="max-w-6xl px-8 mx-auto flex flex-col gap-5 lg:flex-row items-center justify-between"
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
        <div class="flex grow flex-row flex-wrap items-center justify-center lg:justify-between gap-2">
            <!-- Datacenter Filter -->
            <div class="flex flex-row flex-wrap items-center gap-2">
                <span class="text-white text-sm">Filter by Datacenter:</span>
                <button
                    type="button"
                    onclick={openDatacenterDialog}
                    class="px-2 py-1 text-center text-black text-xs font-medium transition-colors cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed bg-white hover:bg-white/80"
                >
                    {#if appliedDatacenterIds.size === 0}
                        Select datacenters...
                    {:else if appliedDatacenterIds.size === 1}
                        {@const selected = getSelectedDatacenters()}
                        {selected[0]?.label || 'Selected'}
                    {:else}
                        {appliedDatacenterIds.size} selected
                    {/if}
                </button>
                {#if appliedDatacenterIds.size > 0}
                    <button
                        type="button"
                        onclick={clearAllFilters}
                        class="h-6 w-6 text-center text-white text-xs font-medium transition-colors cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed bg-red-600 hover:bg-red-700 flex items-center justify-center"
                        title="Clear All"
                    >
                        <X class="w-4 h-4" />
                    </button>
                {/if}
            </div>
            <LanguageSwitcher />
        </div>
    </div>
</div>

<div class="px-4">
    {#if loading}
        <div class="text-white mb-8 text-center px-20 py-10 bg-slate-950 w-fit mx-auto rounded">
            <p>Loading recent trackers...</p>
        </div>
    {:else if error}
        <div class="text-red-400 mb-8 text-center px-20 py-10 bg-slate-950 w-fit mx-auto rounded">
            <p>Error: {error}</p>
        </div>
    {:else if !table || table.allRows.length === 0}
        <div class="text-white mb-8 text-center px-20 py-10 bg-slate-950 w-fit mx-auto rounded">
            <p>No trackers found.</p>
        </div>
    {:else}
        <div class="w-full max-w-6xl mx-auto mb-4">
            <table class="table-auto w-full border-separate border-spacing-y-0.5 text-sm md:text-base">
                <thead>
                    <tr>
                        {#each table.columns as column (column.id)}
                            <th class="text-left px-2 {column.id === 'last_update' ? 'hidden sm:table-cell' : ''} {column.id === 'fate' ? 'hidden md:table-cell' : ''} {column.id === 'ce' ? 'hidden sm:table-cell' : ''}">
                                {#if column.sortable}
                                    <button
                                        class="flex items-center gap-1 hover:text-gray-300 transition-colors"
                                        onclick={() => table.toggleSort(column.id)}
                                    >
                                        {#if column.id === 'last_update'}
                                            <span class="inline md:hidden text-center">
                                                <Clock class="w-4 h-4 inline-block" />
                                            </span>
                                            <span class="hidden md:inline">Last Updated</span>
                                        {:else if column.id === 'datacenter'}
                                            <span class="inline md:hidden text-center">
                                                <Globe class="w-4 h-4 inline-block" />
                                            </span>
                                            <span class="hidden md:inline">{column.name}</span>
                                        {:else}
                                            {column.name}
                                        {/if}
                                        <span class="text-xs">
                                            {#if table.getSortState(column.id) === 'asc'}
                                                <ChevronUp class="w-3 h-3 inline" />
                                            {:else if table.getSortState(column.id) === 'desc'}
                                                <ChevronDown class="w-3 h-3 inline" />
                                            {:else}
                                                <ChevronsUpDown class="w-3 h-3 inline opacity-50" />
                                            {/if}
                                        </span>
                                    </button>
                                {:else}
                                    {column.name}
                                {/if}
                            </th>
                        {/each}
                    </tr>
                </thead>
                <tbody>
                    {#each table.allRows as row (row.tracker_id)}
                        <tr class="relative group cursor-pointer bg-slate-900/90 hover:bg-slate-800 transition-colors duration-200">
                            <!-- Tracker ID -->
                            <td class="relative px-2 font-mono">
                                {row.tracker_id}
                                <a
                                    href={`${base}/${row.tracker_id}`}
                                    class="absolute inset-0 z-10"
                                    aria-label={`View tracker ${row.tracker_id}`}
                                ></a>
                            </td>

                            <!-- Last Updated -->
                            <td class="relative hidden sm:table-cell px-2 truncate">
                                <AutoTimeFormatted timestamp={row.last_update} format="relative" disableUpdate={false} />
                                <a
                                    href={`${base}/${row.tracker_id}`}
                                    class="absolute inset-0 z-10"
                                    aria-label={`View tracker ${row.tracker_id}`}
                                ></a>
                            </td>

                            <!-- Datacenter -->
                            <td class="relative px-2 truncate">
                                {row.datacenter_name}
                                <a
                                    href={`${base}/${row.tracker_id}`}
                                    class="absolute inset-0 z-10"
                                    aria-label={`View tracker ${row.tracker_id}`}
                                ></a>
                            </td>

                            <!-- Pot Status -->
                            <td class="relative px-2 truncate">
                                {#if row.pot_status_text}
                                    {#if row.pot_status_text === "Alive"}
                                        Alive
                                    {:else if row.pot_status_text === "Soon"}
                                        Soon
                                    {:else}
                                        <AutoTimeFormatted timestamp={row.pot_status_text} format="relative" disableUpdate={false} />
                                    {/if}
                                {:else}
                                    None
                                {/if}
                                <a
                                    href={`${base}/${row.tracker_id}`}
                                    class="absolute inset-0 z-10"
                                    aria-label={`View tracker ${row.tracker_id}`}
                                ></a>
                            </td>

                            <!-- Last/Current CE -->
                            <td class="hidden sm:table-cell relative px-2 truncate">
                                {#if row.active_ce_fate_id || row.recent_ce_fate_id}
                                    {@const fateId = row.active_ce_fate_id || row.recent_ce_fate_id}
                                    {@const ceName = OCCULT_ENCOUNTERS[fateId]?.name?.[$currentLanguage] || OCCULT_ENCOUNTERS[fateId]?.name?.en || "Unknown CE"}
                                    <span class="flex items-center gap-2">
                                        <span class={`w-2 h-2 rounded-full ${row.is_ce_active ? 'bg-green-500' : 'bg-gray-500'}`} title={row.is_ce_active ? 'Currently Active' : 'Not Active'}></span>
                                        {ceName}
                                    </span>
                                {:else}
                                    None
                                {/if}
                                <a
                                    href={`${base}/${row.tracker_id}`}
                                    class="absolute inset-0 z-10"
                                    aria-label={`View tracker ${row.tracker_id}`}
                                ></a>
                            </td>

                            <!-- Last/Current Fate -->
                            <td class="hidden md:table-cell relative px-2 truncate">
                                {#if row.active_fate_id || row.recent_fate_id}
                                    {@const fateId = row.active_fate_id || row.recent_fate_id}
                                    {@const fateName = OCCULT_FATES[fateId]?.name?.[$currentLanguage] || OCCULT_FATES[fateId]?.name?.en || "Unknown Fate"}
                                    <span class="flex items-center gap-2">
                                        <span class={`w-2 h-2 rounded-full ${row.is_fate_active ? 'bg-green-500' : 'bg-gray-500'}`} title={row.is_fate_active ? 'Currently Active' : 'Not Active'}></span>
                                        {fateName}
                                    </span>
                                {:else}
                                    None
                                {/if}
                                <a
                                    href={`${base}/${row.tracker_id}`}
                                    class="absolute inset-0 z-10"
                                    aria-label={`View tracker ${row.tracker_id}`}
                                ></a>
                            </td>
                        </tr>
                    {/each}
                </tbody>
            </table>
        </div>
    {/if}
</div>

<!-- Datacenter Selection Dialog -->
<dialog
    bind:this={datacenterDialog}
    class="fixed top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-full max-w-lg max-h-[80vh] bg-slate-950 backdrop:backdrop-blur-sm backdrop:bg-slate-800/30 px-4 py-6 overflow-auto"
    onclose={closeDatacenterDialog}
    onclick={(e) => {
        if (e.target === datacenterDialog) {
            datacenterDialog.close();
        }
    }}
>
    <div class="flex justify-between items-center mb-6">
        <h2 class="text-2xl font-bold text-white">Select Datacenters</h2>
        <button
            type="button"
            onclick={closeDatacenterDialog}
            class="text-white hover:text-white/80 transition hover:cursor-pointer -mx-4 px-4 -my-6 py-6"
            aria-label="Close"
        >
            <X class="w-8 h-8" />
        </button>
    </div>

    <form onsubmit={(e) => { e.preventDefault(); applySelections(); }} class="flex flex-col gap-4">
        <div class="max-h-[50vh] overflow-y-auto border border-white/20 p-4">
            {#each sortedRegions as region}
                {@const regionCheckboxId = `region-${region}`}
                {@const isFull = isRegionFullySelected(region)}
                <div class="mb-4 last:mb-0">
                    <label 
                        for={regionCheckboxId}
                        class="flex items-center gap-2 cursor-pointer hover:bg-white/10 p-1 mb-1"
                    >
                        <input
                            id={regionCheckboxId}
                            type="checkbox"
                            checked={isFull}
                            onchange={() => toggleRegion(region)}
                            class="w-3 h-3 cursor-pointer"
                        />
                        <h3 class="font-semibold text-white text-sm">{region}</h3>
                    </label>
                    <div class="space-y-1 pl-5">
                        {#each datacentersByRegion[region] as dc}
                            {@const dcCheckboxId = `dc-${dc.id}`}
                            <label 
                                for={dcCheckboxId}
                                class="flex items-center gap-2 cursor-pointer hover:bg-white/10 p-0.5"
                            >
                                <input
                                    id={dcCheckboxId}
                                    type="checkbox"
                                    checked={isDatacenterSelected(dc.id)}
                                    onchange={() => toggleDatacenter(dc.id)}
                                    class="w-3 h-3 cursor-pointer"
                                />
                                <span class="text-white text-sm">{dc.name}</span>
                            </label>
                        {/each}
                    </div>
                </div>
            {/each}
        </div>

        <div class="flex gap-2">
            <button
                type="button"
                onclick={closeDatacenterDialog}
                class="flex-1 bg-slate-700 hover:bg-slate-600 disabled:bg-slate-600 text-white font-medium py-2 px-4 transition-colors duration-200 disabled:cursor-not-allowed cursor-pointer"
            >
                Cancel
            </button>
            <button
                type="submit"
                class="flex-1 bg-white hover:bg-white/80 disabled:bg-slate-600 text-black font-medium py-2 px-4 transition-colors duration-200 disabled:cursor-not-allowed cursor-pointer"
            >
                Apply
            </button>
        </div>
    </form>
</dialog>
