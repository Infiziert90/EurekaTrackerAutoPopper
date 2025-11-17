<script>
    import { onMount, onDestroy } from 'svelte';
    import { formatSeconds } from '$lib/utils';

    let { timestamp, seconds, format = 'simple', disableUpdate = false, noNegative = false, countdown = false } = $props();

    let finalString = $state('');
    let interval;

    function updateTime() {
        // If seconds or timestamp is set to -1, then we want to return 'Never'
        if (seconds === -1 || timestamp === -1) {
            finalString = 'Never';
            return;
        }

        // If we set noNegative to true, then we want to return 'soon' if the timestamp is in the future
        if (noNegative && timestamp > new Date().getTime() / 1000) {
            finalString = 'Soon';
            return;
        }

        // Calculate delta every time to get the current time difference
        let delta;
        const now = Math.floor(new Date().getTime() / 1000);

        if (timestamp) {
            if (countdown) {
                // For countdown mode, calculate remaining time until timestamp
                delta = timestamp - now;
                // If countdown is complete (delta <= 0), show "Can pop" or similar
                if (delta <= 0) {
                    finalString = 'Can pop';
                    return;
                }
            } else {
                // Calculate the absolute difference between current time and timestamp
                // This handles both past and future timestamps correctly
                delta = Math.abs(now - timestamp);
            }
        } else {
            if (countdown) {
                // For countdown with seconds, just use the seconds value directly
                delta = seconds;
                if (delta <= 0) {
                    finalString = 'Can pop';
                    return;
                }
            } else {
                delta = Math.abs(seconds);
            }
        }
        
        finalString = formatSeconds(delta, format);
    }

    onMount(() => {
        updateTime();
        if (!disableUpdate) {
            interval = setInterval(updateTime, 1000);
        }
    });

    onDestroy(() => {
        if (interval) {
            clearInterval(interval);
        }
    });
</script>

<span title={timestamp ? new Date(Number(timestamp) * 1000).toLocaleString() : null} class="tabular-nums">
    {finalString}
</span>
