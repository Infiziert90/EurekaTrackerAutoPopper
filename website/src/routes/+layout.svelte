<script>
	import '../app.css';
	
	let { children } = $props();
</script>

<div class="flex flex-col min-h-screen h-full bg-slate-950/80 text-white">
	<main class="flex-1 overflow-y-auto">
		{@render children()}
	</main>
	
	<footer class="bg-slate-950 text-gray-300 text-xs px-4 py-2 text-center">
		<p>
			FINAL FANTASY XIV ©2010 - {new Date().getFullYear()} SQUARE ENIX CO., LTD. FINAL FANTASY is a registered trademark of Square Enix Holdings Co., Ltd. All material used under license. 
		</p>
	</footer>
</div>

