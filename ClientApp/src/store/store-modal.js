const initialState = {
	active: false,
	title: "",
	body: "",
	resolve: null,
	reject: null
};

const state = Object.assign({}, initialState);

const mutations = {
	open: (state, payload) => {
		Object.assign(state, payload);
	},
	close: state => {
		this.state = Object.assign(state, initialState);
	}
};

const actions = {
	confirm: ({ commit }, { title, body }) => {
		return new Promise((resolve, reject) => {
			commit("open", {
				active: true,
				title,
				body,
				resolve,
				reject
			});
		});
	}
};

export default {
	namespaced: true,
	state,
	mutations,
	actions
};
