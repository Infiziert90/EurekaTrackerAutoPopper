<script>
    import { onMount, onDestroy } from 'svelte';
    import { formatSeconds } from '$lib/utils';

    let { timestamp, seconds, format = 'simple' } = $props();

    let finalString = $state('');
    let interval;

    function updateTime() {
        // Calculate delta every time to get the current time difference
        let delta;
        if (timestamp) {
            delta = Math.floor((new Date().getTime() / 1000) - timestamp);
        } else {
            delta = seconds;
        }
        
        finalString = formatSeconds(delta, format);
    }

    onMount(() => {
        updateTime();
        interval = setInterval(updateTime, 1000);
    });

    onDestroy(() => {
        clearInterval(interval);
    });
</script>

<span title={timestamp ? new Date(Number(timestamp) * 1000).toISOString() : null} class="tabular-nums">
    {finalString}
</span>
