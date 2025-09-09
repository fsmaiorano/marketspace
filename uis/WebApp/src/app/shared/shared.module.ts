import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { CardComponent } from './components/card/card.component';

@NgModule({
  declarations: [],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, CardComponent],
  exports: [CommonModule, FormsModule, ReactiveFormsModule, CardComponent],
  providers: [provideHttpClient(withInterceptorsFromDi())],
})
export class SharedModule {}
