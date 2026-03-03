import {Route} from "@tanstack/react-router";
import {rootRoute} from "@/routes/root.tsx";
import MerchantPage from "@/pages/merchant/MerchantPage.tsx";

export const merchantRoute = new Route({
    getParentRoute: () => rootRoute,
    path: 'merchant',
    component: MerchantPage,
});
