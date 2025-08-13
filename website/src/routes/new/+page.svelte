
<script>
    import { base } from '$app/paths';
    import { BASE_URL, API_HEADERS, SAMPLE_SOUTH_HORN_TRACKER } from '$lib/const.js';
    import { goto } from '$app/navigation';
    
    let formData = $state({
        password: '',
    });
    
    let isLoading = $state(false);
    let result = $state(null);
    
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
                password: formData.password
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
                await goto(`${base}/${result.tracker_id}?password=${formData.password}`);
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
