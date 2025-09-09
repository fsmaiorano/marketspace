import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full',
  },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('@app/features/dashboard/dashboard.component').then((m) => m.DashboardComponent),
  },
  {
    path: 'checkout',
    loadComponent: () =>
      import('@app/features/checkout/checkout.component').then((m) => m.CheckoutComponent),
  },
  {
    path: 'payment',
    loadComponent: () =>
      import('@app/features/payment/payment.component').then((m) => m.PaymentComponent),
  },
];
