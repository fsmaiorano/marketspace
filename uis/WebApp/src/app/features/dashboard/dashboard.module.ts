import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {SharedModule} from '@app/shared/shared.module';
import {DashboardComponent} from '@app/features/dashboard/dashboard.component';

@NgModule({
  exports: [DashboardComponent],
  imports: [CommonModule, SharedModule, DashboardComponent],
})
export class DashboardModule {
}
