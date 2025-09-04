import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardComponent } from './dashboard.component';

@NgModule({
  exports: [DashboardComponent],
  imports: [CommonModule, DashboardComponent],
})
export class DashboardModule {}
