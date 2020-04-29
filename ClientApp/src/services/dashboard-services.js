import { $http } from "@/services/http-common";
import storeConfig from "@/store/store-config.js";

const services = {
    Get
};
export default services;

function Get() {
    if (storeConfig.state.isSandBox) {
        return $http.get(`/_samples/modlist.json`)
    } else {
        // return $http.get(`Home/Get`);
        return $http.get(`http://localhost:5000/Home/index`);
    }
}