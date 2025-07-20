<svelte:head>
	<title>Occult Tracker</title>
</svelte:head>

<div class="flex flex-col justify-center h-full w-full">
    <div class="bg-slate-950 text-center p-20">
        <h1 class="w-fit mx-auto mb-4">
            <a href="/" aria-label="Occult Tracker">
                <img src="/logo.png" alt="Occult Tracker" height="80" class="h-20"/>
            </a>
        </h1>
        
        <form class="flex flex-row items-center gap-4 max-w-sm w-full mx-auto" onsubmit={handleSubmit}>
            <input type="text" name="trackerId" placeholder="Tracker ID" class="border-b-2 border-white p-2 focus:outline-none flex-1"/>
            <button type="submit" class="bg-white text-black px-4 py-2">Enter</button>
        </form>
    </div>
</div>

<script>
    import { goto } from '$app/navigation';
    // on form submit, go to /[trackerId]
    function handleSubmit(event) {
        event.preventDefault();
        const trackerId = event.target.trackerId.value;
        goto(`/${trackerId}`);
    }
</script>