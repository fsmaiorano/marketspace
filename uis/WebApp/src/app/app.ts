import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from './shared/components/header/header.component';
import {DashboardModule} from '@app/features/dashboard/dashboard.module';
import {CheckoutModule} from '@app/features/checkout/checkout.module';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, DashboardModule, CheckoutModule, HeaderComponent],
  templateUrl: './app.html',
  styleUrls: ['./app.scss'],
  standalone: true,
})
export class App {
  protected readonly title = signal('WebApp');
}
