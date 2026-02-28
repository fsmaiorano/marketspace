import {Route} from "@tanstack/react-router";
import {rootRoute} from "./root.tsx";
import HomePage from "../pages/HomePage.tsx";

export const homeRoute = new Route({
    getParentRoute: () => rootRoute,
    path: 'home',
    component: HomePage,
});
