import {Router} from '@tanstack/react-router';
import {rootRoute} from './root';
import {indexRoute} from './indexRoute';
import {authRoute} from './auth';
import {homeRoute} from "./home.tsx";

const routeTree = rootRoute.addChildren([
    indexRoute,
    authRoute,
    homeRoute
]);

export const router = new Router({
    routeTree,
});
