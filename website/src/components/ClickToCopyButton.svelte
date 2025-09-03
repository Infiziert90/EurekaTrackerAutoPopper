<script>
    import { Tooltip } from "flowbite-svelte";

    let { text, title, children, class: className = '', ...rest } = $props();
    let tooltipString = $state(text || 'Click to copy');

    let tooltipTextUpdateTimeout;

    function handleClick() {
        navigator.clipboard.writeText(text);
        tooltipString = 'Copied!';

        clearTimeout(tooltipTextUpdateTimeout);

        tooltipTextUpdateTimeout = setTimeout(() => {
            tooltipString = title || text || 'Click to copy';
        }, 1000);
    }
</script>

<button 
    {...rest}
    onclick={handleClick} 
    class="{className}" 

>
    {@render children()}
</button>
<Tooltip
    arrow={false}
    class="bg-black/80 rounded-md text-white text-xs p-1.5 border border-white/20"
>
    {tooltipString}
</Tooltip>