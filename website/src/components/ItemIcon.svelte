<script>
    let { itemId } = $props();
    import { ITEM } from "$lib/const";
    import { currentLanguage } from "$lib/stores";
    const XIVAPI_BASE_URL = "https://v2.xivapi.com/api/";
</script>

<a
    href={`https://ffxiv.consolegameswiki.com/wiki/${ITEM[itemId].name["en"].replace(/ /g, "_")}`}
    target="_blank"
    data-tooltip={ITEM[itemId].name[$currentLanguage]}
    class="relative w-4 h-4 before:content-[attr(data-tooltip)] before:absolute before:bottom-full before:left-1/2 before:-translate-x-1/2 before:mb-1 before:bg-black/80 before:rounded-md before:text-white before:text-xs before:px-2 before:py-1 before:pointer-events-none before:opacity-0 before:transition-opacity before:duration-300 before:ease-in-out before:z-10 before:whitespace-nowrap hover:before:opacity-100 focus:before:opacity-100"
>
    <img
        src={`${XIVAPI_BASE_URL}asset?path=${ITEM[itemId].img}&format=jpg`}
        alt={ITEM[itemId].name[$currentLanguage]}
        class="w-full h-full"
    />
</a>
