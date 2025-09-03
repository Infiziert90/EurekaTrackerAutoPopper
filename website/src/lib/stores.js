import { writable } from 'svelte/store';

// Available languages
export const AVAILABLE_LANGUAGES = [
    { code: 'en', name: 'English', letter: 'E' },
    { code: 'fr', name: 'Français', letter: 'F' },
    { code: 'ja', name: '日本語', letter: 'J' },
    { code: 'de', name: 'Deutsch', letter: 'D' }
];

// Language store with localStorage persistence
function createLanguageStore() {
    // Get initial value from localStorage or default to 'en'
    const storedLanguage = typeof window !== 'undefined' ? localStorage.getItem('language') : null;
    const initialLanguage = storedLanguage || 'en';
    
    const { subscribe, set, update } = writable(initialLanguage);
    
    return {
        subscribe,
        set: (language) => {
            if (typeof window !== 'undefined') {
                localStorage.setItem('language', language);
            }
            set(language);
        },
        update
    };
}

export const currentLanguage = createLanguageStore(); 