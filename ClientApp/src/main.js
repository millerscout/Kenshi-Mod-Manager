/* Requisites */

import Vue from 'vue'
import App from './App.vue'
import 'roboto-fontface/css/roboto/roboto-fontface.css'
import '@mdi/font/css/materialdesignicons.css'

import Vuex from "vuex";


import { BootstrapVue, IconsPlugin } from 'bootstrap-vue'
import VueContextMenu from 'vue-context-menu'

import 'bootstrap/dist/css/bootstrap.css'

import 'bootstrap-vue/dist/bootstrap-vue.css'


Vue.use(BootstrapVue)
Vue.use(IconsPlugin)
Vue.use(VueContextMenu)
    /* Internal. */
import storeConfig from "./store/store-config";
import storeDialog from "./store/store-confirmDialog";

/* configuration */
Vue.config.productionTip = false
Vue.use(Vuex);

const store = new Vuex.Store({
    state: {},
    modules: {
        storeConfig,
        storeDialog,
    }
});

store.dispatch("storeConfig/setSandbox", false);
store.dispatch("storeConfig/loadConfigs");

new Vue({
    render: h => h(App),
    store
}).$mount('#app')