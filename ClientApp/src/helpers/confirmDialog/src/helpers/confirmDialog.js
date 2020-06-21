export default {
	name: 'src-helpers-confirm-dialog',
	components: {},
	props: [],
	data: () => ({
		showDialog: false
	}),
	computed: {
		dialog() {
			return this.$store.state.storeDialog;
		}
	},
	methods: {
		confirm() {
			this.dialog.resolve(true);
			this.$store.commit("dialog/close");
		},
		cancel() {
			this.dialog.resolve(false);
			this.$store.commit("dialog/close");
		}
	}
}


