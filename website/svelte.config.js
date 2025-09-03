import adapter from '@sveltejs/adapter-static';

/** @type {import('@sveltejs/kit').Config} */
const config = {
    kit: {
        adapter: adapter({
            fallback: '404.html'
        }),
        paths: {
            base: process.env.NODE_ENV === 'production' ? '' : '',
        }
    },
    compilerOptions: {
        runes: true
    }
};

export default config;
