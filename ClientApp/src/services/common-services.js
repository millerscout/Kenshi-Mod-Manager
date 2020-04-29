import { $http } from "@/services/http-common";
import storeConfig from "@/store/store-config.js";

const services = {
	getConfig
};
export default services;

function getConfig() {

	if (storeConfig.state.isSandBox) {
		return new Promise((resolve) => {
			resolve({
				"gamePath": "",
				"SteamModsPath": ""
			})
		});
	} else {
		return $http.get(`Internal/getConfig`);
	}
}
