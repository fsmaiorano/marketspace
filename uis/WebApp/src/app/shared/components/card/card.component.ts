import { MatCardModule } from '@angular/material/card';
import { Component, Input, Output, EventEmitter } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import {Catalog} from '@app/shared/models/catalog';

@Component({
  selector: 'app-card',
  imports: [MatCardModule, MatButtonModule],
  templateUrl: './card.component.html',
  styleUrl: './card.component.scss',
  standalone: true,
})
export class CardComponent {
  @Input() catalog: Catalog | null = null;
  @Output() addToCart = new EventEmitter<Catalog>();

  constructor() {}

  onAddToCart(): void {
    if (this.catalog) {
      this.addToCart.emit(this.catalog);
    }
  }
}
