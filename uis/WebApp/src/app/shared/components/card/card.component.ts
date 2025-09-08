import { MatCardModule } from '@angular/material/card';
import { Component, Input } from '@angular/core';
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

  constructor() {}
}
