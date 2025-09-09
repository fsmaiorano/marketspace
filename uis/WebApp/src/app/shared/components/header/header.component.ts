import {Component, inject} from '@angular/core';
import {MatButtonModule} from '@angular/material/button';
import {MatIconModule} from '@angular/material/icon';
import {MatToolbarModule} from '@angular/material/toolbar';
import {MatBadgeModule} from '@angular/material/badge';
import {MarketspaceStoreService} from '@app/core/store/marketspace.store.service';
import {Router} from '@angular/router';

@Component({
  selector: 'app-header',
  imports: [MatToolbarModule, MatButtonModule, MatIconModule, MatBadgeModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss',
  standalone: true,
})
export class HeaderComponent {
  private router = inject(Router);
  private cartStore = inject(MarketspaceStoreService);

  async goToCheckout() {
    await this.router.navigate(['/checkout']);
  }

  async goToDashboard() {
    await this.router.navigate(['/dashboard']);
  }
}
