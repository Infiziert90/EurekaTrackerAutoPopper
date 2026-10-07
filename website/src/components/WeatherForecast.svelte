<script>
    import { onDestroy, onMount } from "svelte";
    import { ChevronRight } from "@lucide/svelte";
    import { Tooltip } from "flowbite-svelte";
    import { XIVAPI_BASE_URL } from "$lib/const";
    import { currentLanguage } from "$lib/stores";
    import { localized } from "$lib/utils";
    import { weatherForecast, WEATHER_WINDOW_SECONDS } from "$lib/weather";
    import AutoTimeFormatted from "./AutoTimeFormatted.svelte";

    // compact: current weather only, forecast in a dropdown
    let { zone, upcoming = 3, compact = false } = $props();

    let now = $state(null);
    let interval;

    onMount(() => {
        now = Math.floor(Date.now() / 1000);
        interval = setInterval(() => {
            now = Math.floor(Date.now() / 1000);
        }, 1000);
    });

    onDestroy(() => {
        if (interval) {
            clearInterval(interval);
        }
    });

    let forecast = $derived(now === null ? [] : weatherForecast(zone, upcoming + 1, now));
    let current = $derived(forecast[0] ?? null);
    let next = $derived(forecast.slice(1));

    let elapsed = $derived(
        current ? Math.min(WEATHER_WINDOW_SECONDS, now - current.start) : 0,
    );

    // Not using flowbite's Popover: mousedown + focusin toggle it back shut on click
    let open = $state(false);
    let root = $state(null);

    $effect(() => {
        if (!open) {
            return;
        }

        const onDocumentClick = (event) => {
            if (root && !root.contains(event.target)) {
                open = false;
            }
        };
        const onKeydown = (event) => {
            if (event.key === "Escape") {
                open = false;
            }
        };

        document.addEventListener("click", onDocumentClick);
        document.addEventListener("keydown", onKeydown);

        return () => {
            document.removeEventListener("click", onDocumentClick);
            document.removeEventListener("keydown", onKeydown);
        };
    });

    function iconUrl(weather) {
        return `${XIVAPI_BASE_URL}asset?path=${weather.icon}&format=png`;
    }

    function weatherName(weather) {
        return localized(weather?.name, $currentLanguage, "Unknown");
    }
</script>

{#if current?.weather}
    {#if compact}
        <div class="relative" bind:this={root}>
            <button
                type="button"
                class="flex w-full min-w-0 flex-col gap-1 text-left cursor-pointer"
                aria-expanded={open}
                aria-label="Weather forecast"
                onclick={() => (open = !open)}
            >
                <span class="flex min-w-0 items-center gap-2 text-sm">
                    <img
                        src={iconUrl(current.weather)}
                        alt={weatherName(current.weather)}
                        class="w-7 h-7 shrink-0"
                    />
                    <span class="truncate">{weatherName(current.weather)}</span>
                    <ChevronRight
                        class="w-4 h-4 shrink-0 text-slate-500 transition-transform {open ? 'rotate-90' : ''}"
                    />
                </span>
                <progress
                    class="block h-[3px] w-full appearance-none bg-slate-700 [&::-webkit-progress-bar]:bg-slate-700 [&::-webkit-progress-value]:bg-slate-300 [&::-moz-progress-bar]:bg-slate-300"
                    value={elapsed}
                    max={WEATHER_WINDOW_SECONDS}
                    aria-label="Elapsed time in the current weather"
                ></progress>
            </button>

            {#if open}
                <div class="absolute left-0 top-full z-50 mt-2 min-w-max rounded-md border border-white/20 bg-slate-900 p-2 text-xs shadow-lg">
                    <div class="flex flex-col gap-2">
                        {#each next as entry (entry.start)}
                            {#if entry.weather}
                                <div class="flex items-center gap-3">
                                    <img src={iconUrl(entry.weather)} alt="" class="w-5 h-5 shrink-0" />
                                    <span class="grow whitespace-nowrap">{weatherName(entry.weather)}</span>
                                    <span class="text-slate-400">
                                        <AutoTimeFormatted timestamp={entry.start} countdown format="compact" expiredText="now" />
                                    </span>
                                </div>
                            {/if}
                        {/each}
                    </div>
                </div>
            {/if}
        </div>
    {:else}
        <div class="flex items-center gap-2 text-sm">
            <img
                src={iconUrl(current.weather)}
                alt={weatherName(current.weather)}
                class="w-9 h-9 shrink-0"
            />
            <span class="whitespace-nowrap">{weatherName(current.weather)}</span>

            {#if next.length > 0}
                <ChevronRight class="w-4 h-4 text-slate-500 shrink-0" />
                <div class="flex items-center gap-2">
                    {#each next as entry (entry.start)}
                        {#if entry.weather}
                            <div class="flex flex-col items-center gap-0.5">
                                <img
                                    src={iconUrl(entry.weather)}
                                    alt={weatherName(entry.weather)}
                                    class="w-6 h-6"
                                />
                                <Tooltip
                                    arrow={false}
                                    class="bg-black/80 rounded-md text-white text-xs px-2 py-1 border border-white/20"
                                >
                                    {weatherName(entry.weather)}
                                </Tooltip>
                                <span class="text-[0.65rem] text-slate-400">
                                    <AutoTimeFormatted timestamp={entry.start} countdown format="compact" expiredText="now" />
                                </span>
                            </div>
                        {/if}
                    {/each}
                </div>
            {/if}
        </div>
    {/if}
{/if}
