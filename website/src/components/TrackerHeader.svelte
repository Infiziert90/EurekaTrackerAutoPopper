<script>
    import { page } from "$app/stores";
    import { base } from "$app/paths";
    import { Clipboard, Link, Lock, Unlock } from "@lucide/svelte";
    import { DATACENTER_NAMES } from "$lib/const";
    import { currentLanguage } from "$lib/stores";
    import { localized } from "$lib/utils";
    import ClickToCopyButton from "./ClickToCopyButton.svelte";
    import EorzeaClock from "./EorzeaClock.svelte";
    import LanguageSwitcher from "./LanguageSwitcher.svelte";
    import PasswordButton from "./PasswordButton.svelte";
    import WeatherForecast from "./WeatherForecast.svelte";

    let { uid, zone, tracker, canEdit, isPasswordUnlocked, onPasswordCorrect } = $props();

    let datacenter = $derived(DATACENTER_NAMES[tracker.datacenter]?.name || "Unknown");
    let zoneLabel = $derived(localized(zone.label, $currentLanguage, "Unknown"));
    let zoneShortLabel = $derived(localized(zone.shortLabel, $currentLanguage, zoneLabel));
    let trackerUrl = $derived(`${$page.url.origin}${base}/${uid}`);

    const buttonClass = "cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed";
</script>

{#snippet actions()}
    <ClickToCopyButton text={uid} class={buttonClass}>
        <Clipboard class="w-4 h-4" />
    </ClickToCopyButton>
    <ClickToCopyButton text={trackerUrl} class={buttonClass}>
        <Link class="w-4 h-4" />
    </ClickToCopyButton>
    {#if canEdit}
        {#if !isPasswordUnlocked}
            <PasswordButton
                expectedPassword={tracker.password}
                on:passwordCorrect={onPasswordCorrect}
                class={buttonClass}
            >
                <Lock class="w-4 h-4" />
            </PasswordButton>
        {:else}
            <div class="text-green-400" title="Tracker unlocked">
                <Unlock class="w-4 h-4" />
            </div>
        {/if}
    {/if}
{/snippet}

{#snippet logo(className)}
    <a href={`${base}/`} aria-label="Occult Tracker">
        <img src={`${base}/logo.svg`} alt="Occult Tracker" height="80" class={className} />
    </a>
{/snippet}

<div class="bg-slate-950 mb-2 relative z-10 overscroll-pseudo-elt">
    <!-- Phone / tablet layout -->
    <div class="lg:hidden">
        <div class="flex items-center justify-between gap-3 px-3 py-2 md:px-6 md:py-3">
            <h1>{@render logo("h-9 md:h-12 w-auto")}</h1>
            <LanguageSwitcher />
        </div>

        <div class="flex items-center justify-between gap-3 border-t border-white/10 px-3 py-2 md:px-6 md:py-3">
            <div class="md:hidden">
                <WeatherForecast {zone} compact />
            </div>
            <div class="hidden md:block">
                <WeatherForecast {zone} />
            </div>
            <EorzeaClock stacked />
        </div>

        <div class="flex items-center gap-2 border-t border-white/10 px-3 py-1.5 text-xs text-slate-400 md:gap-3 md:px-6 md:py-2 md:text-sm">
            <span class="font-mono text-white">{uid}</span>
            <span aria-hidden="true">·</span>
            <span>{datacenter}</span>
            <span aria-hidden="true">·</span>
            <span class="md:hidden">{zoneShortLabel}</span>
            <span class="hidden truncate md:inline">{zoneLabel}</span>
            <span class="ml-auto flex shrink-0 items-center gap-3 text-slate-300">
                {@render actions()}
            </span>
        </div>

        {#if isPasswordUnlocked && tracker.password}
            <div class="flex items-center gap-2 border-t border-white/10 px-3 py-1.5 text-xs text-slate-400 md:gap-3 md:px-6 md:py-2 md:text-sm">
                Pwd: <span class="font-mono text-white">{tracker.password}</span>
                <ClickToCopyButton text={tracker.password} class={buttonClass}>
                    <Clipboard class="w-4 h-4" />
                </ClickToCopyButton>
            </div>
        {/if}
    </div>

    <!-- Desktop layout -->
    <div class="hidden lg:flex max-w-6xl px-8 py-2 mx-auto gap-5 items-center justify-between">
        <h1>{@render logo("h-20 w-auto")}</h1>

        <div class="flex grow flex-row flex-wrap items-center justify-between gap-4">
            <div class="flex flex-col items-start gap-y-1 text-sm">
                <div class="flex items-center gap-2">
                    <span>ID: <span class="font-mono bg-white text-black px-1">{uid}</span></span>
                    {@render actions()}
                </div>

                {#if isPasswordUnlocked && tracker.password}
                    <div class="flex items-center gap-2">
                        <span>Pwd: <span class="bg-white text-black px-1">{tracker.password}</span></span>
                        <ClickToCopyButton text={tracker.password} class={buttonClass}>
                            <Clipboard class="w-4 h-4" />
                        </ClickToCopyButton>
                    </div>
                {/if}

                <div>DC: <span class="bg-white text-black px-1">{datacenter}</span></div>

                <div><span class="bg-slate-700 text-white px-1">{zoneLabel}</span></div>
            </div>

            <div class="flex flex-col items-center gap-2">
                <WeatherForecast {zone} />
                <EorzeaClock />
            </div>

            <LanguageSwitcher />
        </div>
    </div>
</div>
