<script>
    import { Tooltip } from "flowbite-svelte";
    let { itemId } = $props();
    import { ITEM } from "$lib/const";
    import { currentLanguage } from "$lib/stores";
    const XIVAPI_BASE_URL = "https://v2.xivapi.com/api/";
</script>

<a
    href={`https://ffxiv.consolegameswiki.com/wiki/${ITEM[itemId].name["en"].replace(/ /g, "_")}`}
    target="_blank"
    class="w-4 h-4"
>
    <img
        src={`${XIVAPI_BASE_URL}asset?path=${ITEM[itemId].img}&format=jpg`}
        alt={ITEM[itemId].name[$currentLanguage]}
        class="w-full h-full"
    />
</a>
<Tooltip
    arrow={false}
    class="bg-black/80 rounded-md text-white text-xs px-2 py-1 border border-white/20"
>
    {ITEM[itemId].name[$currentLanguage]}
</Tooltip>
