import { createRouter, createWebHistory } from 'vue-router';
import type { RouteRecordRaw } from 'vue-router';
import LoginUser from '../views/LoginUser.vue';
import HomePage from '../views/HomePage.vue';
import AccountCreation from '../views/AccountCreation.vue';
import UpdateProfile from '../views/UpdateProfile.vue';
import ProjectCreation from '../views/ProjectCreation.vue';
import ProjectManagement from '../views/ProjectManagement.vue';
import Dashboard from '../views/DashBoard.vue';
import Store from '../views/StorePage.vue';
import UserList from '../views/UserList.vue';

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
    {
        path: '/updateprofile',
        name: 'UpdateProfile',
        component: UpdateProfile,
    },
    {
        path: '/create-project',
        name: 'ProjectCreation',
        component: ProjectCreation
    },
    {
        path: '/manage-project/:id',
        name: 'ProjectManagement',
        component: ProjectManagement,
        props: true
    },
    {
        path: '/dashboard',
        name: 'Dashboard',
        component: Dashboard
    },
    {
        path: '/storepage',
        name: 'StorePage',
        component: Store
    },
    {
        path: '/userlist',
        name: 'UserList',
        component: UserList
    }
];

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes,
});

export default router;
