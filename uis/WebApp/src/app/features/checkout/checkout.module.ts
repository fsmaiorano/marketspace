import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {CheckoutComponent} from '@app/features/checkout/checkout.component';


@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    CheckoutComponent,
    CheckoutComponent
  ],
  exports: [CheckoutComponent],
})
export class CheckoutModule {
}
