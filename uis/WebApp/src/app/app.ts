import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { DashboardModule } from './dashboard/dashboard.module';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, DashboardModule],
  templateUrl: './app.html',
  styleUrls: ['./app.scss'],
  standalone: true,
})
export class App {
  protected readonly title = signal('WebApp');
}
