<script>
    import { base } from "$app/paths";
	import { page } from '$app/state';
    import { Frown } from "@lucide/svelte";
</script>

<div class="flex flex-col h-full w-full justify-center">
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

        <div class="flex flex-col items-center justify-center text-red-400">
            <Frown />
            <div class="mt-2">
                <div class="text-2xl font-bold">{page.status}</div>
                <div>{page.error.message}</div>
            </div>
        </div>
    </div>
</div>