<script>
    import { currentLanguage, AVAILABLE_LANGUAGES } from '$lib/stores';
    
    function handleLanguageChange(event) {
        const selectedLanguage = event.target.value;
        currentLanguage.set(selectedLanguage);
    }
</script>

<div class="language-switcher">
    <fieldset class="flex flex-wrap items-center border-1 border-slate-700 hover:border-white focus-within:border-white rounded-md px-1 transition-colors duration-200">
        <legend class="sr-only">Select Language</legend>
        {#each AVAILABLE_LANGUAGES as language}
            <label class="flex items-center cursor-pointer">
                <input
                    type="radio"
                    name="language"
                    value={language.code}
                    checked={$currentLanguage === language.code}
                    onchange={handleLanguageChange}
                    class="sr-only"
                />
                <span class="px-2 py-1 transition-colors {
                    $currentLanguage === language.code 
                        ? 'underline text-white'
                        : 'text-gray-300 hover:underline focus:underline hover:text-white focus:text-white'
                }">
                    {language.letter}
                </span>
            </label>
        {/each}
    </fieldset>
</div>
