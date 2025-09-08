import { Routes } from '@angular/router';
import { DashboardComponent } from '@app/features/dashboard/dashboard.component';
import {CheckoutComponent} from '@app/features/checkout/checkout.component';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full',
  },
  {
    path: 'dashboard',
    component: DashboardComponent,
  },
  {
    path: 'checkout',
    component: CheckoutComponent,
  },
];
