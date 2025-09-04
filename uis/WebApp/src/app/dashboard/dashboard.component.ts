import { Component } from '@angular/core';
import { SharedModule } from '../shared/shared.module';
import { MaterialModule } from '../shared/material.module';
import { DashboardService } from './dashboard.service';
import { GetCatalogResult } from '@app/shared/models/get-catalog-result';
import { Result } from '@app/shared/models/result';
import { CatalogDto } from '@app/shared/models/catalogdto';

@Component({
  selector: 'app-dashboard',
  imports: [SharedModule, MaterialModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent {
  catalog: CatalogDto[] | null = null;

  constructor(private dashboardService: DashboardService) {}

  ngOnInit() {
    this.dashboardService.getCatalog().subscribe((result: Result<GetCatalogResult>) => {
      console.log(result);
      this.catalog = result.data?.products || null;
    });
  }
}
