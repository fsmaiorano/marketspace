import {Router} from '@tanstack/react-router';
import {rootRoute} from '@/routes/root';
import {indexRoute} from '@/routes/indexRoute';
import {authRoute} from '@/routes/auth-route';
import {homeRoute} from "@/routes/home-route.tsx";
import {customerRoute} from "@/routes/customer-route.tsx";
import {merchantRoute} from "@/routes/merchant-route.tsx";

const routeTree = rootRoute.addChildren([
    indexRoute,
    authRoute,
    homeRoute,
    customerRoute,
    merchantRoute
]);

export const router = new Router({
    routeTree,
});
