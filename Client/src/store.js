import Vue from "vue";
import Vuex from "vuex";

Vue.use(Vuex);

export const store = new Vuex.Store({
  state: {
    baseUrl: "",
  },
  mutations: {
    baseUrl(state, baseUrl) {
      state.baseUrl = baseUrl;
    },
  },
  getters: {
    getBaseUrl(state) {
      return state.baseUrl;
    },
  },
  actions: {},
  modules: {},
});
