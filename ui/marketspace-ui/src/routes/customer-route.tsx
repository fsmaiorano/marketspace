import {Route} from "@tanstack/react-router";
import {rootRoute} from "@/routes/root.tsx";
import CustomerPage from "@/pages/customer/CustomerPage.tsx";

export const customerRoute = new Route({
    getParentRoute: () => rootRoute,
    path: 'customer',
    component: CustomerPage,
});
