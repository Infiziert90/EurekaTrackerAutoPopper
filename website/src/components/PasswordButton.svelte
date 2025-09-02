<script>
    import { Tooltip } from "flowbite-svelte";
    import { X, AlertCircle } from "@lucide/svelte";
    import { createEventDispatcher } from "svelte";

    const dispatch = createEventDispatcher();

    let { text, title, children, class: className = "", expectedPassword = "", ...rest } = $props();
    let password = $state("");
    let dialog = $state(null);
    let dialogClickListener = $state(null);
    let errorMessage = $state("");
    let isSubmitting = $state(false);

    function openPasswordDialog() {
        try {
            if (!dialog) {
                console.warn('Dialog element not found');
                return;
            }
            
            password = "";
            errorMessage = "";
            dialog.showModal();

            dialogClickListener = dialog.addEventListener('click', function(event) {
                if (!dialog) return;
                
                var rect = dialog.getBoundingClientRect();
                var isInDialog = (rect.top <= event.clientY && event.clientY <= rect.top + rect.height &&
                    rect.left <= event.clientX && event.clientX <= rect.left + rect.width);
                if (!isInDialog) {
                    dialog.close();
                }
            });
        } catch (error) {
            console.error(error);
        }
    }

    function closePasswordDialog() {
        try {
            if (!dialog) {
                console.warn('Dialog element not found');
                return;
            }
            
            dialog.close();
            if (dialogClickListener) {
                dialog.removeEventListener('click', dialogClickListener);
            }
            // remove focus on any page element to avoid tooltip
            if (document.activeElement) {
                document.activeElement.blur();
            }
        } catch (error) {
            console.error(error);
        }
    }

    function handleSubmit(event) {
        event.preventDefault();
        
        if (!password.trim()) {
            errorMessage = "Password is required";
            return;
        }

        isSubmitting = true;
        errorMessage = "";

        // Validate password
        if (password === expectedPassword) {
            // Password is correct - emit success event and close dialog
            dispatch('passwordCorrect', { password });
            closePasswordDialog();
        } else {
            // Password is incorrect - show error message
            errorMessage = "Incorrect password";
        }
        
        isSubmitting = false;
    }

    function handleKeydown(event) {
        if (event.key === 'Enter') {
            handleSubmit(event);
        }
    }
</script>

<button {...rest} class={className} onclick={openPasswordDialog}>
    {@render children()}
</button>
<Tooltip
    arrow={false}
    class="bg-black/80 rounded-md text-white text-xs p-1.5 border border-white/20"
>
    Enter tracker password
</Tooltip>

<dialog 
    bind:this={dialog}
    class="fixed top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-full h-min max-w-lg max-h-lg bg-slate-950 backdrop:backdrop-blur-sm backdrop:bg-slate-800/30 px-4 py-6 overflow-auto"
    onclose={closePasswordDialog}
>
    <div class="flex justify-between items-center mb-6">
        <h2 class="text-2xl font-bold text-white">Enter tracker password</h2> 
        <button type="button" class="text-white hover:text-white/80 transition hover:cursor-pointer -mx-4 px-4 -my-6 py-6" onclick={closePasswordDialog}>
            <X class="w-8 h-8" />
        </button>
    </div>

    <form onsubmit={handleSubmit} class="flex flex-col gap-4">
        <div class="mb-4">
            <label for="password" class="block text-white text-sm font-medium mb-2">Password <span class="text-red-500">*</span></label>
            <!-- Type text is wanted so that password is visible -->
            <input 
                type="text" 
                id="password" 
                bind:value={password}
                onkeydown={handleKeydown}
                class="w-full border-b-2 border-white p-2 focus:outline-none text-white placeholder-slate-400"
                placeholder="Enter password"
                required
                disabled={isSubmitting}
            />
        </div>

        <!-- Error message display -->
        {#if errorMessage}
            <div class="flex items-center gap-2 p-3 bg-red-600/20 border border-red-600/30 rounded text-red-400 text-sm">
                <AlertCircle class="w-4 h-4 flex-shrink-0" />
                <span>{errorMessage}</span>
            </div>
        {/if}

        <button 
            type="submit"
            disabled={isSubmitting}
            class="w-full bg-white hover:bg-white/80 disabled:bg-slate-600 text-black font-medium py-2 px-4 transition-colors duration-200 disabled:cursor-not-allowed cursor-pointer"
        >
            {#if isSubmitting}
                Unlocking...
            {:else}
                Unlock
            {/if}
        </button>
    </form>
</dialog>
