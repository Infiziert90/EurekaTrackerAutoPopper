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
        
        <div class="text-white mb-8">
            <p>Tracker creation functionality coming soon...</p>
        </div>
        
        <a href={`${base}/`} class="block w-fit mx-auto bg-slate-800/90 px-4 py-2 mt-4 text-white hover:bg-slate-800/80 transition-colors duration-200 cursor-pointer">
            Back to Home
        </a>
    </div>
</div>

<script>
    import { base } from '$app/paths';
</script>
