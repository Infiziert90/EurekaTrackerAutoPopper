<script>
    import { onDestroy, onMount } from "svelte";
    import { page } from "$app/stores";
    import { base } from "$app/paths";
    import { TOWER_SPAWN_TIMER, OCCULT_RESPAWN, OCCULT_ENCOUNTERS, OCCULT_FATES, BASE_URL, API_HEADERS, DATACENTER_NAMES } from "$lib/const";
    import { currentLanguage } from "$lib/stores";
    import { LoaderPinwheel, Frown, CircleQuestionMark, Pyramid, Lock, Unlock, Skull, Link, Clipboard } from "@lucide/svelte";
    import AutoTimeFormatted from "../../components/AutoTimeFormatted.svelte";
    import ClickToCopyButton from "../../components/ClickToCopyButton.svelte";
    import ItemIcon from "../../components/ItemIcon.svelte";
    import LanguageSwitcher from "../../components/LanguageSwitcher.svelte";
    import PasswordButton from "../../components/PasswordButton.svelte";
    import { calculateOccultRespawn, formatSeconds, calculatePotStatus, isAlive } from "$lib/utils";

    const uid = $page.params.uid;

    let trackerResults = $state([]);
    let bunny = $state(null); // The next pot fate to spawn, named "bunny" to match the Dalamud plugin
    var activeCE = $state(null);
    var activeFate = $state(null);
    var activeBunny = $state(null);
    let fetchInterval = $state(null);
    let isLoading = $state(true);
    let error = $state(null);
    
    // New state variables for tracker type 2 functionality
    let isPasswordUnlocked = $state(false);
    let trackerType = $state(1);
    let originalData = $state(null);
    let isUpdating = $state(false);
    let updateMessage = $state("");
    let updateMessageType = $state(""); // "success" or "error"
    
    // URL password will be checked in onMount
    let urlPassword = null;

    // Function to handle password authentication using alert()
    function unlockWithPassword() {
        const password = prompt("Enter tracker password:");
        if (password === trackerResults.password) {
            isPasswordUnlocked = true;
            // Store password in localStorage for persistence
            localStorage.setItem(`tracker_password_${uid}`, password);
            
            // Clean up URL password if it was used
            localStorage.removeItem(`url_password_${uid}`);
            
            // Remove password from URL if present
            if (urlPassword) {
                const newUrl = new URL(window.location);
                newUrl.searchParams.delete('password');
                window.history.replaceState({}, '', newUrl);
            }
        } else if (password !== null) {
            alert("Incorrect password");
        }
    }

    // Function to handle status updates for both mobs and fates
    async function handleStatusUpdate({ encounter, type, status }) {
        if (!isPasswordUnlocked || !originalData) return;
        
        isUpdating = true;
        updateMessage = "";
        
        try {
            // Create a copy of the original data
            const updatedData = { ...originalData };
            
            // Determine which history to update based on type
            const historyKey = type === 'ce' ? 'encounter_history' : type === 'fate' ? 'fate_history' : 'pot_history';
            const history = JSON.parse(updatedData[historyKey]);
            const targetItem = history.find(item => item.fate_id === encounter.fate_id);
            
            if (targetItem) {
                if (status === 'spawned') {
                    targetItem.spawn_time = Math.floor(Date.now() / 1000);
                    targetItem.death_time = -1; // Ensure death_time is -1 when spawning
                    targetItem.last_seen = targetItem.spawn_time;
                } else if (status === 'dead') {
                    targetItem.death_time = Math.floor(Date.now() / 1000);
                    targetItem.last_seen = targetItem.death_time;
                }
                
                updatedData[historyKey] = JSON.stringify(history);
                updatedData.last_update = Math.floor(Date.now() / 1000);
            }
            
            // Send only the updated history data
            const response = await fetch(BASE_URL, {
                method: 'PATCH',
                headers: {
                    ...API_HEADERS,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    [historyKey]: updatedData[historyKey],
                    last_update: updatedData.last_update
                })
            });
            
            if (response.ok) {
                const successKey = type === 'ce' 
                    ? (status === 'spawned' ? 'mobSpawnedSuccess' : 'mobDeadSuccess')
                    : type === 'fate' 
                        ? (status === 'spawned' ? 'fateSpawnedSuccess' : 'fateDeadSuccess')
                        : (status === 'spawned' ? 'potSpawnedSuccess' : 'potDeadSuccess');
                updateMessage = 'Update successful';
                updateMessageType = "success";
                
                // Update the current display data immediately for better UX
                const currentHistory = trackerResults[historyKey];
                const currentItem = currentHistory.find(item => item.fate_id === encounter.fate_id);
                if (currentItem) {
                    if (status === 'spawned') {
                        currentItem.spawn_time = Math.floor(Date.now() / 1000);
                        currentItem.death_time = -1;
                        currentItem.last_seen = currentItem.spawn_time;
                        currentItem.alive = true;
                    } else if (status === 'dead') {
                        currentItem.death_time = Math.floor(Date.now() / 1000);
                        currentItem.last_seen = currentItem.death_time;
                        currentItem.alive = false;
                    }
                }
                
                // Refresh the data after successful update
                await fetchTrackerData();
            } else {
                updateMessage = 'Update failed';
                updateMessageType = "error";
                console.error('Failed to update tracker data');
            }
        } catch (err) {
            updateMessage = 'Update failed';
            updateMessageType = "error";
            console.error('Error updating tracker data:', err);
        } finally {
            isUpdating = false;
            // Clear message after 3 seconds
            setTimeout(() => {
                updateMessage = "";
                updateMessageType = "";
            }, 3000);
        }
    }

    // Wrapper functions for backward compatibility and cleaner calls
    async function handleMobSpawned(encounter) {
        await handleStatusUpdate({ encounter, type: 'ce', status: 'spawned' });
    }

    async function handleMobDead(encounter) {
        await handleStatusUpdate({ encounter, type: 'ce', status: 'dead' });
    }

    async function handleFateSpawned(fate) {
        await handleStatusUpdate({ encounter: fate, type: 'fate', status: 'spawned' });
    }

    async function handleFateDead(fate) {
        await handleStatusUpdate({ encounter: fate, type: 'fate', status: 'dead' });
    }

    async function handlePotSpawned(pot) {
        await handleStatusUpdate({ encounter: pot, type: 'pot', status: 'spawned' });
    }

    async function handlePotDead(pot) {
        await handleStatusUpdate({ encounter: pot, type: 'pot', status: 'dead' });
    }


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
            
            // Store original data for POST requests
            originalData = { ...trackerResults };
            
            // Check tracker type
            trackerType = trackerResults.tracker_type || 1;
            
            // Check if stored password is still valid instead of resetting
            if (isPasswordUnlocked && originalData) {
                const storedPassword = localStorage.getItem(`tracker_password_${uid}`);
                if (storedPassword !== trackerResults.password) {
                    // Password changed, need to re-authenticate
                    isPasswordUnlocked = false;
                    localStorage.removeItem(`tracker_password_${uid}`);
                }
            }
            
            // ADD DATA TO STATE
            activeCE = null;
            
            // Safely parse encounter_history with null check
            if (trackerResults.encounter_history) {
                try {
                    trackerResults.encounter_history = JSON.parse(trackerResults.encounter_history);
                    if (Array.isArray(trackerResults.encounter_history)) {
                        trackerResults.encounter_history.forEach(encounter => {
                            encounter.name = OCCULT_ENCOUNTERS[encounter.fate_id].name[$currentLanguage];
                            encounter.alive = isAlive(encounter);

                            // If any encounter is alive, set activeCE to the first one we find
                            if (encounter.alive && !activeCE) {
                                activeCE = encounter;
                            }

                            // When we get the Forked Tower, calculate the spawn_timer
                            if (encounter.fate_id === 48) {
                                encounter.spawn_timer = TOWER_SPAWN_TIMER - (300 * encounter.killed_ces) - (60 * encounter.killed_fates);
                            }
                        });
                    }
                } catch (err) {
                    console.error('Error parsing encounter_history:', err);
                    trackerResults.encounter_history = [];
                }
            } else {
                trackerResults.encounter_history = [];
            }

            activeFate = null;

            // Safely parse fate_history with null check
            if (trackerResults.fate_history) {
                try {
                    trackerResults.fate_history = JSON.parse(trackerResults.fate_history);
                    if (Array.isArray(trackerResults.fate_history)) {
                        trackerResults.fate_history.forEach(fate => {
                            fate.name = OCCULT_FATES[fate.fate_id].name[$currentLanguage];
                            fate.alive = isAlive(fate);
                        });

                        // If any fate is alive, set activeFate to the first one we find
                        if (trackerResults.fate_history.some(fate => fate.alive)) {
                            activeFate = trackerResults.fate_history.find(fate => fate.alive);
                        }
                    }
                } catch (err) {
                    console.error('Error parsing fate_history:', err);
                    trackerResults.fate_history = [];
                }
            } else {
                trackerResults.fate_history = [];
            }

            activeBunny = null;
            
            // Safely parse pot_history with null check
            if (trackerResults.pot_history) {
                try {
                    trackerResults.pot_history = JSON.parse(trackerResults.pot_history);
                    if (Array.isArray(trackerResults.pot_history)) {
                        trackerResults.pot_history.forEach(pot => {
                            pot.name = OCCULT_FATES[pot.fate_id].name[$currentLanguage];
                            pot.alive = isAlive(pot);
                            
                            // If any pot is alive, set activeBunny to the first one we find
                            if (pot.alive && !activeBunny) {
                                activeBunny = pot;
                            }
                        });
                    }
                } catch (err) {
                    console.error('Error parsing pot_history:', err);
                    trackerResults.pot_history = [];
                }
            } else {
                trackerResults.pot_history = [];
            }

            // Calculate pot status using utility function
            const potStatus = calculatePotStatus(trackerResults.pot_history);
            bunny = potStatus.bunny;

        } catch (err) {
            console.error("Error fetching tracker data:", err);
            error = err.message;
        } finally {
            isLoading = false;
        }
    }

    onMount(() => {
        // Check for password in URL parameters and localStorage (client-side only)
        const urlParams = new URLSearchParams(window.location.search);
        urlPassword = urlParams.get('password');
        
        // Initialize password from URL or localStorage
        if (urlPassword) {
            // Store URL password for later use when tracker data loads
            localStorage.setItem(`url_password_${uid}`, urlPassword);
        }
        
        // Initial fetch
        fetchTrackerData();

        // Set up polling every 30 seconds
        fetchInterval = setInterval(fetchTrackerData, 30000);
        
        // Auto-unlock if password is stored and valid
        const checkStoredPassword = () => {
            if (trackerResults && trackerResults.password && !isPasswordUnlocked) {
                // Check stored password first
                const storedPassword = localStorage.getItem(`tracker_password_${uid}`);
                if (storedPassword === trackerResults.password) {
                    isPasswordUnlocked = true;
                    return;
                }
                
                // Check URL password if no stored password
                const urlPassword = localStorage.getItem(`url_password_${uid}`);
                if (urlPassword === trackerResults.password) {
                    isPasswordUnlocked = true;
                    // Convert URL password to stored password
                    localStorage.setItem(`tracker_password_${uid}`, urlPassword);
                    localStorage.removeItem(`url_password_${uid}`);
                    return;
                }
            }
        };
        
        // Check after initial data load
        setTimeout(checkStoredPassword, 500);
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
        <div class="bg-slate-950 p-2 mb-2 relative z-10 overscroll-pseudo-elt">
            <div class="max-w-6xl px-8 mx-auto flex flex-col gap-5 lg:flex-row items-center justify-between">
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
                    <div>
                        <table class="text-sm border-separate border-spacing-x-4 border-spacing-y-0.5 align-middle"><tbody>

                            <!-- Tracker ID row -->
                            <tr>
                                <td>
                                    ID: <span class="font-mono bg-white text-black px-1">{uid}</span>
                                </td>
                                <td>
                                    <div class="flex items-center gap-2">
                                        <ClickToCopyButton text={uid} class="cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed">
                                            <Clipboard class="w-4 h-4" />
                                        </ClickToCopyButton>
                                        <ClickToCopyButton text={`${base}/${uid}`} class="cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed">
                                            <Link class="w-4 h-4" />
                                        </ClickToCopyButton>
                                        {#if trackerType === 2 && !isPasswordUnlocked}
                                            <button onclick={unlockWithPassword} class="cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed">
                                                <Lock class="w-4 h-4" />
                                            </button>
                                            <PasswordButton class="cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed">
                                                <Lock class="w-4 h-4" />
                                            </PasswordButton>
                                        {/if}
                                    </div>
                                </td>
                            </tr>
                            <!-- Password row -->
                            {#if isPasswordUnlocked && trackerResults.password}
                                <tr>
                                    <td>
                                        Pwd: <span class="bg-white text-black px-1">{trackerResults.password}</span>
                                    </td>
                                    <td>
                                        <ClickToCopyButton text={trackerResults.password} class="cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed">
                                            <Clipboard class="w-4 h-4" />
                                        </ClickToCopyButton>
                                    </td>
                                </tr>
                            {/if}
                            <!-- Datacenter row -->
                            <tr>
                                <td>
                                    DC: <span class="bg-white text-black px-1">{DATACENTER_NAMES[trackerResults.datacenter]?.name || "Unknown"}</span>
                                </td>
                            </tr>
                        </tbody></table>
                    </div>
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
                        <img src="https://v2.xivapi.com/api/asset?path=ui/icon/063000/063978_hr1.tex&format=webp" alt="Forked Tower Icon" class="w-16 h-16 inline-block mr-2" />
                        {OCCULT_ENCOUNTERS[48].name[$currentLanguage]}
                    </h2>

                    <!-- Pick it from the encounter_history -->
                    {#if trackerResults.encounter_history && trackerResults.encounter_history.length > 0}
                        {#each trackerResults.encounter_history as encounter}
                            {#if encounter.fate_id === 48}
                                {#if encounter.alive}
                                    <p class="text-green-400">Tower is up and recruiting!</p>
                                {:else}
                                    <p>Last seen: <AutoTimeFormatted timestamp={encounter.last_seen} /></p>

                                {/if}
                                <p>Previous respawn times:
                                    {#if encounter.respawn_times && encounter.respawn_times.length > 0}
                                        {#each encounter.respawn_times as time, i}
                                            <span>{formatSeconds(time) + (i < encounter.respawn_times.length - 1 ? ', ' : '')}</span>
                                        {/each}
                                    {:else}
                                        <span class="text-slate-400">None recorded</span>
                                    {/if}
                                </p>

                                <details>
                                    <summary class="bg-slate-900/90 p-2 py-0.5 rounded-md mt-2">Spawn Prediction</summary>
                                    <p class="text-green-400 pt-2">
                                        Predicted spawn time: 
                                        {#if encounter.spawn_timer && encounter.last_seen !== -1 && (encounter.last_seen + encounter.spawn_timer) > (new Date().getTime() / 1000)}
                                            <AutoTimeFormatted timestamp={encounter.last_seen + encounter.spawn_timer} />
                                        {:else}
                                            N/A
                                        {/if}
                                    </p>
                                    
                                    {#if activeFate || activeBunny || activeCE}
                                        <p class="text-blue-400">Upcoming reductions:</p>
                                        <ul class="list-disc list-inside text-blue-400">
                                            <!-- Display the active fate, pot and encounter -->
                                            {#if activeFate && activeFate.name}
                                                <li> -1 minute ({activeFate.name})</li>
                                            {/if}
                                            {#if activeBunny && activeBunny.name}
                                                <li> -1 minute ({activeBunny.name})</li>
                                            {/if}
                                            {#if activeCE && activeCE.name}
                                                <li> -5 minutes ({activeCE.name})</li>
                                            {/if}
                                        </ul> 
                                    {/if}
                                </details>   
                            {/if}
                        {/each}
                    {:else}
                        <p class="text-slate-400">No encounter data available</p>
                    {/if}
                </div>

                <!-- Pot Fate-->
                <div class="bg-slate-800/90 p-4">
                    <h2 class="text-2xl font-extrabold">
                        <img src="https://v2.xivapi.com/api/asset?path=ui/icon/060000/060958_hr1.tex&format=webp" alt="Pot Fate Icon" class="w-16 h-16 inline-block mr-2" />
                        Pot Fate
                    </h2>

                    {#if bunny && bunny.fate_id}
                        <p>FATE: {OCCULT_FATES[bunny.fate_id].name[$currentLanguage]} {OCCULT_FATES[bunny.fate_id].suffix[$currentLanguage]}</p>
                        {#if bunny.alive === true}
                            <p class="text-green-400">Alive</p>
                        {:else}
                            {@const respawnTime = calculateOccultRespawn(bunny, 'timestamp')}
                            {@const now = Math.floor(Date.now() / 1000)}
                            {#if respawnTime <= now}
                                <p class="text-yellow-400">Soon</p>
                            {:else}
                                <p>Spawns in: <AutoTimeFormatted timestamp={respawnTime} format="full" /></p>
                            {/if}
                        {/if}
                    {:else}
                        <p class="text-slate-400">No pot fate data available</p>
                    {/if}

                    <!-- Add buttons to mark pot as dead or spawned -->
                    {#if isPasswordUnlocked}
                        <!-- Pot History -->
                        <div class="flex flex-row gap-2 mt-2">
                            {#if trackerResults.pot_history && trackerResults.pot_history.length > 0}
                                {#each trackerResults.pot_history as pot}
                                    {#if pot.alive}
                                        <button
                                            onclick={() => handlePotDead(pot)}
                                            disabled={isUpdating}
                                            class="px-2 py-1 text-center text-white text-xs font-medium transition-colors cursor-pointer w-full disabled:opacity-50 disabled:cursor-not-allowed {
                                                isUpdating ? 'bg-slate-600' : 'bg-red-600 hover:bg-red-700'
                                            }"
                                            title="Mark pot as dead"
                                        >
                                            {#if isUpdating}
                                                <LoaderPinwheel class="w-3 h-3 animate-spin inline" />
                                            {:else}
                                                KILL {OCCULT_FATES[pot.fate_id].name[$currentLanguage]}
                                            {/if}
                                        </button>
                                    {:else}
                                        <button
                                            onclick={() => handlePotSpawned(pot)}
                                            disabled={isUpdating}
                                            class="px-2 py-1 text-center text-white text-xs font-medium transition-colors cursor-pointer w-full disabled:opacity-50 disabled:cursor-not-allowed {
                                                isUpdating ? 'bg-slate-600' : 'bg-green-600 hover:bg-green-700'
                                            }"
                                            title="Mark pot as spawned"
                                        >
                                            {#if isUpdating}
                                                <LoaderPinwheel class="w-3 h-3 animate-spin inline" />
                                            {:else}
                                                POP {OCCULT_FATES[pot.fate_id].name[$currentLanguage]}
                                            {/if}
                                        </button>
                                    {/if}
                                {/each}
                            {/if}
                        </div>
                    {/if}
                </div>
            </div>

            <!-- Status Message for Tracker Type 2 -->
            {#if trackerType === 2 && updateMessage}
                <div class="max-w-6xl w-full mx-auto mb-4">
                    <div class="bg-slate-800/90 p-4">
                        <div class="text-sm font-medium {
                            updateMessageType === 'success' ? 'bg-green-600/20 text-green-400 border border-green-600/30' :
                            updateMessageType === 'error' ? 'bg-red-600/20 text-red-400 border border-red-600/30' :
                            'bg-blue-600/20 text-blue-400 border border-blue-600/30'
                        } p-2 rounded">
                            {updateMessage}
                        </div>
                    </div>
                </div>
            {/if}

            <!-- Encounter History -->    
            <div class="max-w-6xl w-full mx-auto mb-4">
                <h2 class="text-2xl font-extrabold">
                    <img src="https://v2.xivapi.com/api/asset?path=ui/icon/063000/063909.tex&format=webp" alt="Critical Encounter Icon" class="w-[1lh] h-[1lh] inline-block mr-2" />
                    Encounter History
                </h2>
                <table class="table-fixed w-full border-separate border-spacing-y-0.5 text-sm md:text-base">
                    <thead>
                        <tr class="text-left">
                            <th class="px-2 w-2/5">Encounter</th>
                            <th class="px-2 hidden md:table-cell">Drops</th>
                            <th class="px-2 w-1/5 text-end">Last Seen</th>
                            {#if trackerType === 2}
                                <th class="px-2 w-[14%] md:w-14 text-center"></th>
                            {/if}
                        </tr>
                    </thead>
                    <tbody>
                        {#if trackerResults.encounter_history && trackerResults.encounter_history.length > 0}
                            {#each trackerResults.encounter_history as encounter}
                                <tr class={encounter.alive ? 'bg-green-800/90' : 'bg-slate-900/90'}>
                                    <td class="px-2 w-2/5 truncate">{OCCULT_ENCOUNTERS[encounter.fate_id].name[$currentLanguage]}</td>
                                    <td class="px-2 hidden md:table-cell">
                                        <div class="flex flex-wrap gap-1">
                                            {#each OCCULT_ENCOUNTERS[encounter.fate_id].drops as drop}
                                                <ItemIcon itemId={drop} />
                                            {/each}
                                        </div>
                                    </td>
                                    <td class="px-2 w-1/5 text-end">
                                        <p class="text-nowrap">
                                            <span class="hidden md:inline">
                                                 {encounter.alive ? '(Alive)' : ''}
                                             </span>
                                            {#if encounter.last_seen != -1}
                                                <AutoTimeFormatted timestamp={encounter.last_seen} />
                                            {:else}
                                                N/A
                                            {/if}
                                        </p>
                                    </td>
                                    {#if trackerType === 2}
                                        <td class="px-2 w-[14%] md:w-14 text-center">
                                            {#if isPasswordUnlocked}
                                                {#if encounter.alive}
                                                    <button
                                                        onclick={() => handleMobDead(encounter)}
                                                        disabled={isUpdating}
                                                        class="px-2 py-1 text-center text-white text-xs font-medium transition-colors cursor-pointer w-full disabled:opacity-50 disabled:cursor-not-allowed {
                                                            isUpdating ? 'bg-slate-600' : 'bg-red-600 hover:bg-red-700'
                                                        }"
                                                        title="Mark mob as dead"
                                                    >
                                                        {#if isUpdating}
                                                            <LoaderPinwheel class="w-3 h-3 animate-spin inline" />
                                                        {:else}
                                                            <Skull class="w-4 h-4 inline-block" />
                                                        {/if}
                                                    </button>
                                                {:else}
                                                    <button
                                                        onclick={() => handleMobSpawned(encounter)}
                                                        disabled={isUpdating}
                                                        class="px-2 py-1 text-center text-white text-xs font-medium transition-colors cursor-pointer w-full disabled:opacity-50 disabled:cursor-not-allowed {
                                                            isUpdating ? 'bg-slate-600' : 'bg-green-600 hover:bg-green-700'
                                                        }"
                                                        title="Mark mob as spawned"
                                                    >
                                                        {#if isUpdating}
                                                            <LoaderPinwheel class="w-3 h-3 animate-spin inline" />
                                                        {:else}
                                                            POP
                                                        {/if}
                                                    </button>
                                                {/if}
                                            {:else}
                                                <div class="text-slate-500 text-xs flex items-center justify-center">
                                                    <Lock class="w-4 h-4 me-2" />
                                                </div>
                                            {/if}
                                        </td>
                                    {/if}
                                </tr>
                            {/each}
                        {:else}
                            <tr class="bg-slate-900/90">
                                <td colspan={trackerType === 2 ? 4 : 3} class="px-2 py-4 text-center text-slate-400">
                                    No encounter history available
                                </td>
                            </tr>
                        {/if}
                    </tbody>
                </table>
            </div>

            <!-- Fate History -->
            <div class="max-w-6xl w-full mx-auto mb-4">
                <h2 class="text-2xl font-extrabold">
                    <img src="https://v2.xivapi.com/api/asset?path=ui/icon/060000/060502_hr1.tex&format=webp" alt="Fate Icon" class="w-[1lh] h-[1lh] inline-block mr-2" />
                    Fate History
                </h2>

                <table class="table-fixed w-full border-separate border-spacing-y-0.5 text-sm md:text-base">
                    <thead>
                        <tr class="text-left">
                            <th class="px-2 w-2/5">Fate</th>
                            <th class="px-2 hidden md:table-cell">Drops</th>
                            <th class="px-2 w-1/5 text-end">Last Seen</th>
                            {#if trackerType === 2}
                                <th class="px-2 w-[14%] md:w-14 text-center"></th>
                            {/if}
                        </tr>
                    </thead>
                    <tbody>
                        {#if trackerResults.fate_history && trackerResults.fate_history.length > 0}
                            {#each trackerResults.fate_history as fate}
                                <tr class={fate.alive ? 'bg-green-800/90' : 'bg-slate-900/90'}>
                                    <td class="px-2 w-2/5 truncate">{OCCULT_FATES[fate.fate_id].name[$currentLanguage]}</td>
                                    <td class="px-2 hidden md:table-cell">
                                        <div class="flex flex-wrap gap-1">
                                            {#each OCCULT_FATES[fate.fate_id].drops as drop}
                                                <ItemIcon itemId={drop} />
                                            {/each}
                                        </div>
                                    </td>
                                    <td class="px-2 w-1/5 text-end">
                                        <p class="text-nowrap">
                                            <span class="hidden md:inline">
                                                {fate.alive ? '(Alive)' : ''}
                                             </span>
                                            {#if fate.last_seen != -1}
                                                <AutoTimeFormatted timestamp={fate.last_seen} />
                                            {:else}
                                                N/A
                                            {/if}
                                        </p>
                                    </td>
                                    {#if trackerType === 2}
                                        <td class="px-2 w-[14%] md:w-14 text-center">
                                            {#if isPasswordUnlocked}
                                                {#if fate.alive}
                                                    <button
                                                        onclick={() => handleFateDead(fate)}
                                                        disabled={isUpdating}
                                                        class="px-2 py-1 text-center text-white text-xs font-medium transition-colors cursor-pointer w-full disabled:opacity-50 disabled:cursor-not-allowed {
                                                            isUpdating ? 'bg-slate-600' : 'bg-red-600 hover:bg-red-700'
                                                        }"
                                                        title="Mark fate as dead"
                                                    >
                                                        {#if isUpdating}
                                                            <LoaderPinwheel class="w-3 h-3 animate-spin inline" />
                                                        {:else}
                                                            <Skull class="w-4 h-4 inline-block" />
                                                        {/if}
                                                    </button>
                                                {:else}
                                                <button
                                                    onclick={() => handleFateSpawned(fate)}
                                                    disabled={isUpdating}
                                                    class="px-2 py-1 text-center text-white text-xs font-medium transition-colors cursor-pointer w-full disabled:opacity-50 disabled:cursor-not-allowed {
                                                        isUpdating ? 'bg-slate-600' : 'bg-green-600 hover:bg-green-700'
                                                    }"
                                                    title="Mark fate as spawned"
                                                >
                                                    {#if isUpdating}
                                                        <LoaderPinwheel class="w-3 h-3 animate-spin inline" />
                                                    {:else}
                                                        POP
                                                    {/if}
                                                </button>
                                                {/if}
                                            {:else}
                                                <div class="text-slate-500 text-xs flex items-center justify-center">
                                                    <Lock class="w-4 h-4" />
                                                </div>
                                            {/if}
                                        </td>
                                    {/if}
                                </tr>
                            {/each}
                        {:else}
                            <tr class="bg-slate-900/90">
                                <td colspan={trackerType === 2 ? 4 : 3} class="px-2 py-4 text-center text-slate-400">
                                    No fate history available
                                </td>
                            </tr>
                        {/if}
                    </tbody>
                </table>
            </div>
        </div>
    {/if}
</div>
