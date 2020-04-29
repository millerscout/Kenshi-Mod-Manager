import services from "@/services/common-services";

const initialState = {};

const state = Object.assign({}, initialState);

const mutations = {
	config: function (state, payload) {
		state.config = payload
	},
	isSandbox: function (state, payload) {
		state.isSandBox = payload
	},
};

const actions = {
	loadConfigs: function ({ commit }) {
		commit("config", services.getConfig());
	},
	setSandbox: function ({ commit }, value) {
		commit("isSandbox", value);
	}
};

export default {
	namespaced: true,
	mutations,
	state,
	actions
};
