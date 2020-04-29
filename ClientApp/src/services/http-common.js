import axios from "axios";

export const $http = axios.create({
	baseURL: `` 
});

$http.interceptors.response.use(
	function(response) {
		return response;
	},
	function(error) {
		if (error.response.status === 500) {
			alert("(WIP custom message) Error")
		} else {
			return Promise.reject(error);
		}
	}
);
