import {RootRoute} from '@tanstack/react-router';
import App from '../App';
import NotFoundPage from '../pages/NotFoundPage';

export const rootRoute = new RootRoute({
    component: App,
    errorComponent: NotFoundPage,
    notFoundComponent: NotFoundPage,
});
