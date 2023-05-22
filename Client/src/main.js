import 'devextreme/dist/css/dx.common.css';
import './themes/generated/theme.base.css';
import './themes/generated/theme.additional.css';
import { store } from "./store";
import Vue from "vue";
import axios from "axios";
import App from "./App";
import router from "./router";
import appInfo from "./app-info";
import notify from 'devextreme/ui/notify';
import VueCoreVideoPlayer from 'vue-core-video-player'

Vue.use(VueCoreVideoPlayer)


if (axios.defaults.baseURL === "" || axios.defaults.baseURL === undefined) {
    axios.get("/baseUrl.json").then((res) => {
    axios.defaults.baseURL = res.data.ApiBaseUrl;
    store.commit("baseUrl", res.data.ApiBaseUrl);
  }) 
  // axios.defaults.baseURL = appInfo.ApiBaseUrl;
  // store.commit("baseUrl", appInfo.ApiBaseUrl)
}

axios.defaults.headers.get.Accepts = "application/json";

axios.defaults.baseURL = "https://localhost:5001/";
Vue.prototype.$notify = notify;
Vue.prototype.$axios = axios;


Vue.config.productionTip = false;
Vue.prototype.$appInfo = appInfo;

new Vue({
  store: store,
  axios,
  router,
  notify,
  render: h => h(App)
}).$mount("#app");
