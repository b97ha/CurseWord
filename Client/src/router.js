import Vue from "vue";
import Router from "vue-router";

import auth from "./auth";

import Home from "./views/home";
import defaultLayout from "./layouts/side-nav-outer-toolbar";
import simpleLayout from "./layouts/single-card";
import MovieSave from "./views/Movie/MovieSave";
import Movies from "./views/Movie/Index";
//import PlayMovie from "./views/Movie/PlayMovie";

Vue.use(Router);

const router = new Router({
  routes: [
    {
      path: "/home",
      name: "home",
      meta: { requiresAuth: true },
      components: {
        layout: defaultLayout,
        content: Home
      }
    },
    
    {
      path: "/addMovie",
      name: "addMovie",
      meta: { requiresAuth: true },
      components: {
        layout: defaultLayout,
        content: MovieSave
      }
    },
     
    {
      path: "/movies",
      name: "movies",
      meta: { requiresAuth: true },
      components: {
        layout: defaultLayout,
        content: Movies
      }
    },
     
    {
      path: "/playMovie/:fileName",
      name: "playMovie",
      meta: { requiresAuth: true },
      components: {
        layout: defaultLayout,
        content:()=> import("./views/Movie/PlayMovie")
      }
    },
    
    {
      path: "/login-form",
      name: "login-form",
      meta: { requiresAuth: false },
      components: {
        layout: simpleLayout,
        content: () =>
          import(/* webpackChunkName: "login" */ "./views/login-form")
      },
      props: {
        layout: {
          title: "Sign In"
        }
      }
    },
    {
      path: "/reset-password",
      name: "reset-password",
      meta: { requiresAuth: false },
      components: {
        layout: simpleLayout,
        content: () =>
          import(/* webpackChunkName: "login" */ "./views/reset-password-form")
      },
      props: {
        layout: {
          title: "Reset Password",
          description: "Please enter the email address that you used to register, and we will send you a link to reset your password via Email."
        }
      }
    },
    {
      path: "/create-account",
      name: "create-account",
      meta: { requiresAuth: false },
      components: {
        layout: simpleLayout,
        content: () =>
          import(/* webpackChunkName: "login" */ "./views/create-account-form")
      },
      props: {
        layout: {
          title: "Sign Up"
        }
      }
    },
    {
      path: "/change-password/:recoveryCode",
      name: "change-password",
      meta: { requiresAuth: false },
      components: {
        layout: simpleLayout,
        content: () =>
          import(/* webpackChunkName: "login" */ "./views/change-password-form")
      },
      props: {
        layout: {
          title: "Change Password"
        }
      }
    },
    {
      path: "/",
      redirect: "/home"
    },
    {
      path: "/recovery",
      redirect: "/home"
    },
    {
      path: "*",
      redirect: "/home"
    }
    
  ]
});

router.beforeEach((to, from, next) => {

  if (to.name === "login-form" && auth.loggedIn()) {
    next({ name: "home" });
  }

  if (to.matched.some(record => record.meta.requiresAuth)) {
    if (!auth.loggedIn()) {
      next({
        name: "login-form",
        query: { redirect: to.fullPath }
      });
    } else {
      next();
    }
  } else {
    next();
  }
});

export default router;
