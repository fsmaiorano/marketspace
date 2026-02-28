import { Route } from '@tanstack/react-router';
import { rootRoute } from './root';
import AuthPage from "../pages/AuthPage.tsx";

export const indexRoute = new Route({
  getParentRoute: () => rootRoute,
  path: '/',
  component: AuthPage,
});

