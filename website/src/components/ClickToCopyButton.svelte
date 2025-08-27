<script>
    import { Tooltip } from "melt/builders";
    let { text, children } = $props();
    let tooltipString = $state(text || 'Click to copy');

    let tooltipTextUpdateTimeout;

    const tooltip = new Tooltip({
        openDelay: 30,
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

<button {...tooltip.trigger} onclick={handleClick}>
    {@render children()}
</button>
<div {...tooltip.content}>
    <div {...tooltip.arrow}></div>
    <p>{tooltipString}</p>
</div>
  