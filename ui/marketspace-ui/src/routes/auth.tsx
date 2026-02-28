import {Route} from '@tanstack/react-router';
import AuthPage from '../pages/AuthPage';
import {rootRoute} from './root';

export const authRoute = new Route({
    getParentRoute: () => rootRoute,
    path: 'auth',
    component: AuthPage,
});
