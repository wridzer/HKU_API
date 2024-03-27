import { createRouter, createWebHistory } from 'vue-router';
import type { RouteRecordRaw } from 'vue-router';
import LoginUser from '../views/LoginUser.vue';
import HomePage from '../views/HomePage.vue';
import AccountCreation from '../views/AccountCreation.vue';

const routes: Array<RouteRecordRaw> = [
    {
        path: '/',
        name: 'HomePage',
        component: HomePage,
    },
    {
        path: '/loginUser',
        name: 'LoginUser',
        component: LoginUser,
    },
    {
        path: '/accountcreation',
        name: 'AccountCreation',
        component: AccountCreation,
    },
];

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes,
});

export default router;
