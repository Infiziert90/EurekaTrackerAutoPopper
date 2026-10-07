<script>
    import { onDestroy, onMount } from "svelte";
    import { currentLanguage } from "$lib/stores";
    import { localized } from "$lib/utils";
    import { CLOCK_LABELS, eorzeaDate } from "$lib/weather";

    let { stacked = false } = $props();

    // Created on mount to avoid a server/client locale mismatch
    let formatter = null;
    let eorzeaFormatter = null;

    let localTime = $state("");
    let eorzeaTimeString = $state("");
    let interval;

    function tick() {
        const now = new Date();
        localTime = formatter.format(now);
        eorzeaTimeString = eorzeaFormatter.format(eorzeaDate(now));
    }

    onMount(() => {
        const options = { hour: "numeric", minute: "2-digit" };
        formatter = new Intl.DateTimeFormat(undefined, options);
        eorzeaFormatter = new Intl.DateTimeFormat(undefined, { ...options, timeZone: "UTC" });

        tick();
        interval = setInterval(tick, 1000);
    });

    onDestroy(() => {
        if (interval) {
            clearInterval(interval);
        }
    });
</script>

{#if eorzeaTimeString}
    <div class="flex {stacked ? 'flex-col items-end gap-0.5' : 'items-center gap-3'} text-sm">
        <span class="flex items-baseline gap-1 whitespace-nowrap" title={localized(CLOCK_LABELS.eorzea.full, $currentLanguage)}>
            <span class="bg-slate-700 text-white px-1 py-1 text-xs leading-none [text-box:trim-both_cap_alphabetic]">{localized(CLOCK_LABELS.eorzea.short, $currentLanguage)}</span>
            <span class="tabular-nums">{eorzeaTimeString}</span>
        </span>
        <span class="flex items-baseline gap-1 whitespace-nowrap" title={localized(CLOCK_LABELS.local.full, $currentLanguage)}>
            <span class="bg-slate-700 text-white px-1 py-1 text-xs leading-none [text-box:trim-both_cap_alphabetic]">{localized(CLOCK_LABELS.local.short, $currentLanguage)}</span>
            <span class="tabular-nums">{localTime}</span>
        </span>
    </div>
{/if}
