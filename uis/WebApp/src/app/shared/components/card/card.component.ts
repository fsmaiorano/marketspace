import { MatCardModule } from '@angular/material/card';
import { Component, input, output, ChangeDetectionStrategy } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { Catalog } from '@app/shared/models/catalog';

@Component({
  selector: 'app-card',
  imports: [MatCardModule, MatButtonModule],
  templateUrl: './card.component.html',
  styleUrl: './card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CardComponent {
  catalog = input<Catalog | null>(null);
  addToCart = output<Catalog>();

  onAddToCart(): void {
    const catalogValue = this.catalog();
    if (catalogValue) {
      this.addToCart.emit(catalogValue);
    }
  }
}
