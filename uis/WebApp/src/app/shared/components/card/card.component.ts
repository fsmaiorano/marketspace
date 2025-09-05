import { MatCardModule } from '@angular/material/card';
import { Component, Input } from '@angular/core';
import { CatalogDto } from '@app/shared/models/catalogdto';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-card',
  imports: [MatCardModule, MatButtonModule],
  templateUrl: './card.component.html',
  styleUrl: './card.component.scss',
  standalone: true,
})
export class CardComponent {
  @Input() catalog: CatalogDto | null = null;

  constructor() {}
}
