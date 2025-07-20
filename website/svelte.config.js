import adapter from '@sveltejs/adapter-static';

/** @type {import('@sveltejs/kit').Config} */
const config = { kit: { adapter: adapter() }, compilerOptions: { runes: true } };

export default config;
