import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardComponent } from './dashboard.component';
import { SharedModule } from '@app/shared/shared.module';

@NgModule({
  exports: [DashboardComponent],
  imports: [CommonModule, SharedModule, DashboardComponent],
})
export class DashboardModule {}
