<script>
    import { onDestroy, onMount } from "svelte";
    import { page } from "$app/stores";
    import { base } from "$app/paths";
    import { TOWER_SPAWN_TIMER, OCCULT_RESPAWN, OCCULT_ENCOUNTERS, OCCULT_FATES, BASE_URL, API_HEADERS, TRACKER_CONTROLS } from "$lib/const";
    import { currentLanguage } from "$lib/stores";
    import { LoaderPinwheel, Frown, CircleQuestionMark, Pyramid, Lock, Unlock } from "@lucide/svelte";
    import LanguageSwitcher from "../../components/LanguageSwitcher.svelte";
    import AutoTimeFormatted from "../../components/AutoTimeFormatted.svelte";
    import ItemIcon from "../../components/ItemIcon.svelte";
    import { calculateOccultRespawn, formatSeconds } from "$lib/utils";

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
    let passwordInput = $state("");
    let trackerType = $state(1);
    let originalData = $state(null);
    let isUpdating = $state(false);
    let updateMessage = $state("");
    let updateMessageType = $state(""); // "success" or "error"
    
    // URL password will be checked in onMount
    let urlPassword = null;

    // Function to handle password authentication
    function unlockWithPassword() {
        if (passwordInput === trackerResults.password) {
            isPasswordUnlocked = true;
            // Store password in localStorage for persistence
            localStorage.setItem(`tracker_password_${uid}`, passwordInput);
            // Clear the input field for security
            passwordInput = "";
            
            // Clean up URL password if it was used
            localStorage.removeItem(`url_password_${uid}`);
            
            // Remove password from URL if present
            if (urlPassword) {
                const newUrl = new URL(window.location);
                newUrl.searchParams.delete('password');
                window.history.replaceState({}, '', newUrl);
            }
        } else {
            alert("Incorrect password");
        }
    }
    
    // Function to handle logout and clear stored password
    function logout() {
        isPasswordUnlocked = false;
        localStorage.removeItem(`tracker_password_${uid}`);
        passwordInput = "";
    }

    // Function to handle mob spawned button click
    async function handleMobSpawned(encounter) {
        if (!isPasswordUnlocked || !originalData) return;
        
        isUpdating = true;
        updateMessage = "";
        
        try {
            // Create a copy of the original data
            const updatedData = { ...originalData };
            
            // Find the encounter in the encounter_history and update its spawn_time
            const encounterHistory = JSON.parse(updatedData.encounter_history);
            const targetEncounter = encounterHistory.find(e => e.fate_id === encounter.fate_id);
            
            if (targetEncounter) {
                targetEncounter.spawn_time = Math.floor(Date.now() / 1000);
                targetEncounter.death_time = -1; // Ensure death_time is -1 when spawning
                targetEncounter.last_seen = targetEncounter.spawn_time;
                updatedData.encounter_history = JSON.stringify(encounterHistory);
                updatedData.last_update = Math.floor(Date.now() / 1000);
            }
            
            // Send only the encounter_history data
            const response = await fetch(BASE_URL, {
                method: 'PATCH',
                headers: {
                    ...API_HEADERS,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    encounter_history: updatedData.encounter_history,
                    last_update: updatedData.last_update
                })
            });
            
            if (response.ok) {
                updateMessage = TRACKER_CONTROLS.mobSpawnedSuccess[$currentLanguage];
                updateMessageType = "success";
                 
                // Update the current display data immediately for better UX
                const currentEncounter = trackerResults.encounter_history.find(e => e.fate_id === encounter.fate_id);
                if (currentEncounter) {
                    currentEncounter.spawn_time = Math.floor(Date.now() / 1000);
                    currentEncounter.death_time = -1; // Ensure death_time is -1 when spawning
                    currentEncounter.last_seen = currentEncounter.spawn_time;
                    currentEncounter.alive = true;
                }
                
                // Refresh the data after successful update
                await fetchTrackerData();
            } else {
                updateMessage = TRACKER_CONTROLS.updateFailed[$currentLanguage];
                updateMessageType = "error";
                console.error('Failed to update tracker data');
            }
        } catch (err) {
            updateMessage = TRACKER_CONTROLS.updateError[$currentLanguage];
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

    // Function to handle mob dead button click
    async function handleMobDead(encounter) {
        if (!isPasswordUnlocked || !originalData) return;
        
        isUpdating = true;
        updateMessage = "";
        
        try {
            // Create a copy of the original data
            const updatedData = { ...originalData };
            
            // Find the encounter in the encounter_history and update its death_time
            const encounterHistory = JSON.parse(updatedData.encounter_history);
            const targetEncounter = encounterHistory.find(e => e.fate_id === encounter.fate_id);
            
            if (targetEncounter) {
                targetEncounter.death_time = Math.floor(Date.now() / 1000);
                targetEncounter.last_seen = targetEncounter.death_time;
                updatedData.encounter_history = JSON.stringify(encounterHistory);
                updatedData.last_update = Math.floor(Date.now() / 1000);
            }
            
            // Send only the encounter_history data
            const response = await fetch(BASE_URL, {
                method: 'PATCH',
                headers: {
                    ...API_HEADERS,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    encounter_history: updatedData.encounter_history,
                    last_update: updatedData.last_update
                })
            });
            
            if (response.ok) {
                updateMessage = TRACKER_CONTROLS.mobDeadSuccess[$currentLanguage];
                updateMessageType = "success";
                
                // Update the current display data immediately for better UX
                const currentEncounter = trackerResults.encounter_history.find(e => e.fate_id === encounter.fate_id);
                if (currentEncounter) {
                    currentEncounter.death_time = Math.floor(Date.now() / 1000);
                    currentEncounter.last_seen = currentEncounter.death_time;
                    currentEncounter.alive = false;
                }
                
                // Refresh the data after successful update
                await fetchTrackerData();
            } else {
                updateMessage = TRACKER_CONTROLS.updateFailed[$currentLanguage];
                updateMessageType = "error";
                console.error('Failed to update tracker data');
            }
        } catch (err) {
            updateMessage = TRACKER_CONTROLS.updateError[$currentLanguage];
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
                            fate.alive = fate.death_time < fate.spawn_time;
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
                            pot.alive = pot.death_time < pot.spawn_time;
                            
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

            // Sort pot_history by last_seen (ascending), and get the nextSpawn and the lastAlive
            if (trackerResults.pot_history && trackerResults.pot_history.length > 0) {
                trackerResults.pot_history.sort((a, b) => a.last_seen - b.last_seen);
                const nextSpawn = trackerResults.pot_history[0];
                const lastAlive = trackerResults.pot_history[trackerResults.pot_history.length - 1];

                // If both are -1, then no pot has spawned
                if (nextSpawn.last_seen == -1 && lastAlive.last_seen == -1) {
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
            } else {
                bunny = null;
            }

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
            passwordInput = urlPassword;
            // Store URL password for later use when tracker data loads
            localStorage.setItem(`url_password_${uid}`, urlPassword);
        } else {
            // Try to restore from localStorage
            const storedPassword = localStorage.getItem(`tracker_password_${uid}`);
            if (storedPassword) {
                passwordInput = storedPassword;
            }
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
                    
                    <!-- Password Input for Tracker Type 2 -->
                    {#if trackerType === 2}
                        <div class="flex gap-2 items-center">
                            {#if isPasswordUnlocked}
                                <div class="flex items-center gap-2 text-green-400 text-sm">
                                    <Unlock class="w-4 h-4" />
                                    <span>Unlocked</span>
                                    <button
                                        onclick={logout}
                                        class="text-red-400 hover:text-red-300 text-xs underline"
                                        title={TRACKER_CONTROLS.logout[$currentLanguage]}
                                    >
                                        {TRACKER_CONTROLS.logout[$currentLanguage]}
                                    </button>
                                </div>
                            {:else}
                                <input
                                    type="password"
                                    bind:value={passwordInput}
                                    placeholder={TRACKER_CONTROLS.passwordPlaceholder[$currentLanguage]}
                                    class="bg-slate-700 px-2 py-1 rounded border border-slate-600 focus:border-blue-400 focus:outline-none text-sm w-32"
                                    onkeydown={(e) => e.key === 'Enter' && unlockWithPassword()}
                                />
                                <button
                                    onclick={unlockWithPassword}
                                    class="bg-blue-600 hover:bg-blue-700 px-2 py-1 rounded text-white text-xs font-medium transition-colors"
                                >
                                    {TRACKER_CONTROLS.unlock[$currentLanguage]}
                                </button>
                            {/if}
                        </div>
                    {/if}
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
                                {#if encounter.death_time < encounter.spawn_time}
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
                                    
                                    {#if activeCE || activeBunny}
                                        <p class="text-blue-400">Upcoming reductions:</p>
                                        <ul class="list-disc list-inside text-blue-400">
                                            <!-- Display the active encounter and fate -->
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
                        <p>FATE: {OCCULT_FATES[bunny.fate_id].name[$currentLanguage]}</p>
                        {#if bunny.death_time < bunny.spawn_time}
                            <p class="text-green-400">Alive</p>
                        {:else}
                            <p>Spawns in: <AutoTimeFormatted timestamp={calculateOccultRespawn(bunny, 'timestamp')} format="full" /></p>
                        {/if}
                    {:else}
                        <p class="text-slate-400">No pot fate data available</p>
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
                            <th class="px-2 w-1/2">Encounter</th>
                            <th class="px-2 w-1/3 hidden md:table-cell">Drops</th>
                            <th class="px-2 w-1/6 text-end">Last Seen</th>
                            {#if trackerType === 2}
                                <th class="px-2 w-1/6 text-center">{TRACKER_CONTROLS.controls[$currentLanguage]}</th>
                            {/if}
                        </tr>
                    </thead>
                    <tbody>
                        {#if trackerResults.encounter_history && trackerResults.encounter_history.length > 0}
                            {#each trackerResults.encounter_history as encounter}
                                <tr class={encounter.spawn_time !== -1 && encounter.death_time < encounter.spawn_time ? 'bg-green-800/90' : 'bg-slate-900/90'}>
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
                                                 {encounter.spawn_time !== -1 && encounter.death_time < encounter.spawn_time ? '(Alive)' : ''}
                                             </span>
                                            {#if encounter.last_seen != -1}
                                                <AutoTimeFormatted timestamp={encounter.last_seen} />
                                            {:else}
                                                N/A
                                            {/if}
                                        </p>
                                    </td>
                                    {#if trackerType === 2}
                                        <td class="px-2 w-1/6 text-center">
                                            {#if isPasswordUnlocked}
                                                {#if encounter.spawn_time !== -1 && encounter.death_time < encounter.spawn_time}
                                                    <button
                                                        onclick={() => handleMobDead(encounter)}
                                                        disabled={isUpdating}
                                                        class="px-2 py-1 rounded text-white text-xs font-medium transition-colors cursor-pointer w-full disabled:opacity-50 disabled:cursor-not-allowed {
                                                            isUpdating ? 'bg-slate-600' : 'bg-red-600 hover:bg-red-700'
                                                        }"
                                                        title="Mark mob as dead"
                                                    >
                                                        {#if isUpdating}
                                                            <LoaderPinwheel class="w-3 h-3 animate-spin inline mr-1" />
                                                        {/if}
                                                        {TRACKER_CONTROLS.dead[$currentLanguage]}
                                                    </button>
                                                {:else}
                                                <button
                                                onclick={() => handleMobSpawned(encounter)}
                                                disabled={isUpdating}
                                                class="px-2 py-1 rounded text-white text-xs font-medium transition-colors cursor-pointer w-full disabled:opacity-50 disabled:cursor-not-allowed {
                                                    isUpdating ? 'bg-slate-600' : 'bg-green-600 hover:bg-green-700'
                                                }"
                                                title="Mark mob as spawned"
                                            >
                                                {#if isUpdating}
                                                    <LoaderPinwheel class="w-3 h-3 animate-spin inline mr-1" />
                                                {/if}
                                                {TRACKER_CONTROLS.spawned[$currentLanguage]}
                                            </button>
                                                {/if}
                                            {:else}
                                                <div class="text-slate-500 text-xs flex items-center justify-center">
                                                    <Lock class="w-4 h-4 me-2" />
                                                    {TRACKER_CONTROLS.locked[$currentLanguage]}
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
                            <th class="px-2 w-1/2">Fate</th>
                            <th class="px-2 w-1/3 hidden md:table-cell">Drops</th>
                            <th class="px-2 w-1/6 text-end">Last Seen</th>
                            {#if trackerType === 2}
                                <th class="px-2 w-1/6 text-center">{TRACKER_CONTROLS.controls[$currentLanguage]}</th>
                            {/if}
                        </tr>
                    </thead>
                    <tbody>
                        {#if trackerResults.fate_history && trackerResults.fate_history.length > 0}
                            {#each trackerResults.fate_history as fate}
                                <tr class={fate.spawn_time !== -1 && fate.death_time < fate.spawn_time ? 'bg-green-800/90' : 'bg-slate-900/90'}>
                                    <td class="px-2 w-1/2">{OCCULT_FATES[fate.fate_id].name[$currentLanguage] || fate.fate_id}</td>
                                    <td class="px-2 w-1/3 hidden md:table-cell">
                                        <div class="flex flex-wrap gap-1">
                                            {#each OCCULT_FATES[fate.fate_id].drops as drop}
                                                <ItemIcon itemId={drop} />
                                            {/each}
                                        </div>
                                    </td>
                                    <td class="px-2 w-1/6 text-end">
                                        <p class="text-nowrap">
                                            <span class="hidden md:inline">
                                                {fate.spawn_time !== -1 && fate.death_time < fate.spawn_time ? '(Alive)' : ''}
                                             </span>
                                            {#if fate.last_seen != -1}
                                                <AutoTimeFormatted timestamp={fate.last_seen} />
                                            {:else}
                                                N/A
                                            {/if}
                                        </p>
                                    </td>
                                    {#if trackerType === 2}
                                        <td class="px-2 w-1/6 text-center">
                                            {#if isPasswordUnlocked}
                                                {#if fate.spawn_time !== -1 && fate.death_time < fate.spawn_time}
                                                    <button
                                                        onclick={() => handleFateDead(fate)}
                                                        disabled={isUpdating}
                                                        class="px-2 py-1 rounded text-white text-xs font-medium transition-colors cursor-pointer w-full disabled:opacity-50 disabled:cursor-not-allowed {
                                                            isUpdating ? 'bg-slate-600' : 'bg-red-600 hover:bg-red-700'
                                                        }"
                                                        title="Mark fate as dead"
                                                    >
                                                        {#if isUpdating}
                                                            <LoaderPinwheel class="w-3 h-3 animate-spin inline mr-1" />
                                                        {/if}
                                                        {TRACKER_CONTROLS.dead[$currentLanguage]}
                                                    </button>
                                                {:else}
                                                <button
                                                    onclick={() => handleFateSpawned(fate)}
                                                    disabled={isUpdating}
                                                    class="px-2 py-1 rounded text-white text-xs font-medium transition-colors cursor-pointer w-full disabled:opacity-50 disabled:cursor-not-allowed {
                                                        isUpdating ? 'bg-slate-600' : 'bg-green-600 hover:bg-green-700'
                                                    }"
                                                    title="Mark fate as spawned"
                                                >
                                                    {#if isUpdating}
                                                        <LoaderPinwheel class="w-3 h-3 animate-spin inline mr-1" />
                                                    {/if}
                                                    {TRACKER_CONTROLS.spawned[$currentLanguage]}
                                                </button>
                                                {/if}
                                            {:else}
                                                <div class="text-slate-500 text-xs flex items-center justify-center">
                                                    <Lock class="w-4 h-4 me-2" />
                                                    {TRACKER_CONTROLS.locked[$currentLanguage]}
                                                </div>
                                            {/if}
                                        </td>
                                    {/if}
                                </tr>
                            {/each}
                        {:else}
                            <tr class="bg-slate-900/90">
                                <td colspan={2} class="px-2 py-4 text-center text-slate-400">
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
