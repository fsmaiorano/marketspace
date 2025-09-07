import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { DashboardModule } from './dashboard/dashboard.module';
import { HeaderComponent } from './shared/components/header/header.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, DashboardModule, HeaderComponent],
  templateUrl: './app.html',
  styleUrls: ['./app.scss'],
  standalone: true,
})
export class App {
  protected readonly title = signal('WebApp');
}
