
<script>
    import { base } from '$app/paths';
    import { BASE_URL, API_HEADERS, SAMPLE_SOUTH_HORN_TRACKER, DATACENTER_NAMES } from '$lib/const.js';
    import { goto } from '$app/navigation';
    
    let formData = $state({
        password: '',
        datacenter: 0
    });
    
    let isLoading = $state(false);
    let result = $state(null);
    
    // Group datacenters by region for the select dropdown
    // Build a mapping of datacenter regions to their selectable datacenters for the dropdown.
    // Example output: { "Japan": [ {name: ..., id: ...}, ... ], "Europe": [ ... ], ... }
    const datacentersByRegion = Object.entries(DATACENTER_NAMES)
        // Only include datacenters that are selectable
        .filter(([id, datacenter]) => datacenter.selectable)
        // Group datacenters by their region
        .reduce((regions, [id, datacenter]) => {
            // Initialize the region array if it doesn't exist
            if (!regions[datacenter.region]) {
                regions[datacenter.region] = [];
            }
            // Add the datacenter to the appropriate region, including its numeric id
            regions[datacenter.region].push({ ...datacenter, id: Number(id) });
            return regions;
        }, {});
    
    async function createTracker(event) {
        event.preventDefault();
        
        // Validate password is not empty
        if (!formData.password.trim()) {
            alert('Password is required');
            return;
        }
        
        isLoading = true;
        
        try {
            // Create the tracker data based on the sample structure
            const trackerData = {
                ...SAMPLE_SOUTH_HORN_TRACKER,
                password: formData.password,
                datacenter: formData.datacenter
            };
            
            const response = await fetch(`${BASE_URL}`, {
                method: 'POST',
                headers: {
                    ...API_HEADERS,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(trackerData)
            });
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            const data = await response.json();
            result = data[0]; // API returns an array, we want the first item
            
            // Automatically navigate to the tracker page if we have a tracker_id
            if (result && result.tracker_id) {
                // Save password in localStorage before navigation
                localStorage.setItem(`tracker_password_${result.tracker_id}`, formData.password);
                
                // Navigate to tracker page without password in URL
                await goto(`${base}/${result.tracker_id}`);
            }
            
        } catch (error) {
            console.error('Error creating tracker:', error);
            alert('Failed to create tracker. Please try again.');
        } finally {
            isLoading = false;
        }
    }
    
    function resetForm() {
        result = null;
        formData.password = '';
        formData.datacenter = 0;
    }
</script>

<svelte:head>
	<title>Create New Tracker - Occult Tracker</title>
</svelte:head>

<div class="flex flex-col justify-center h-full w-full">
    <div class="bg-slate-950 text-center p-20">
        <h1 class="w-fit mx-auto mb-4">
            <a href={`${base}/`} aria-label="Occult Tracker">
                <img src={`${base}/logo.svg`} alt="Occult Tracker" height="80" class="h-20"/>
            </a>
        </h1>
        
        <h2 class="text-2xl text-white mb-8">Create New Tracker</h2>
        
        <form onsubmit={createTracker} class="max-w-md mx-auto text-left">
            <div class="mb-4">
                <label for="password" class="block text-white text-sm font-medium mb-2">Password <span class="text-red-500">*</span></label>
                <input 
                    type="text" 
                    id="password" 
                    bind:value={formData.password}
                    class="w-full border-b-2 border-white p-2 focus:outline-none text-white placeholder-slate-400"
                    placeholder="Enter password"
                    required
                />
            </div>

            <div class="mb-4">
                <label for="datacenter" class="block text-white text-sm font-medium mb-2">Datacenter <span class="text-red-500">*</span></label>
                <select 
                    id="datacenter"
                    bind:value={formData.datacenter}
                    class="w-full border-b-2 border-white p-2 focus:outline-none text-white placeholder-slate-400 bg-slate-950"
                    required
                >
                    {#each Object.entries(datacentersByRegion) as [region, datacenters]}
                        {#if region == 'undefined'}
                            {#each datacenters as datacenter}
                                <option value={datacenter.id}>{datacenter.name}</option>
                            {/each}
                        {:else}
                            <optgroup label={region}>
                                {#each datacenters as datacenter}
                                    <option value={datacenter.id}>{datacenter.name}</option>
                                {/each}
                            </optgroup>
                        {/if}
                    {/each}
                </select>
            </div>
        
            <button 
                type="submit"
                disabled={isLoading}
                class="w-full bg-white hover:bg-white/80 disabled:bg-slate-600 text-black font-medium py-2 px-4 transition-colors duration-200 disabled:cursor-not-allowed cursor-pointer"
            >
                {isLoading ? 'Creating...' : 'Create Tracker'}
            </button>
        </form>
        
        <a href={`${base}/`} class="block w-fit mx-auto bg-slate-800/90 px-4 py-2 mt-8 text-white hover:bg-slate-800/80 transition-colors duration-200 cursor-pointer">
            Back to Home
        </a>
    </div>
</div>
