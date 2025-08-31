<script>
    import { Tooltip } from "melt/builders";
    let { text, children, ...rest } = $props();
    let tooltipString = $state(text || 'Click to copy');

    let tooltipTextUpdateTimeout;

    const tooltip = new Tooltip({
        openDelay: 0,
        closeOnPointerDown: false
    });

    function handleClick() {
        navigator.clipboard.writeText(text);
        tooltipString = 'Copied!';

        clearTimeout(tooltipTextUpdateTimeout);

        tooltipTextUpdateTimeout = setTimeout(() => {
            tooltipString = text || 'Click to copy';
        }, 1000);
    }
</script>

<button {...tooltip.trigger} onclick={handleClick} {...rest}>
    {@render children()}
</button>
<div {...tooltip.content} class="bg-black/80 rounded-md px-2 py-1 text-white text-xs ">
    <div {...tooltip.arrow}></div>
    <p>{tooltipString}</p>
</div>