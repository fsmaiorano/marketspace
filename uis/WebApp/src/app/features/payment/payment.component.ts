import { Component, inject, signal, computed, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatStepperModule } from '@angular/material/stepper';
import { MatRadioModule } from '@angular/material/radio';
import { MatDividerModule } from '@angular/material/divider';
import { CommonModule } from '@angular/common';
import { CartCheckoutRequest } from '../../shared/models/cart-checkout-request';

@Component({
  selector: 'app-payment',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatStepperModule,
    MatRadioModule,
    MatDividerModule,
  ],
  templateUrl: './payment.component.html',
  styleUrl: './payment.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaymentComponent {
  private readonly fb = inject(FormBuilder);

  // Signals for state management
  readonly isLoading = signal(false);
  readonly currentStep = signal(0);

  // Form groups
  readonly personalInfoForm: FormGroup;
  readonly shippingInfoForm: FormGroup;
  readonly paymentInfoForm: FormGroup;

  // Payment method options
  readonly paymentMethods = [
    { value: 1, label: 'Credit Card', icon: 'credit_card' },
    { value: 2, label: 'Debit Card', icon: 'payment' },
    { value: 3, label: 'PayPal', icon: 'account_balance_wallet' },
  ];

  // Countries list (simplified)
  readonly countries = [
    'United States',
    'Canada',
    'United Kingdom',
    'Germany',
    'France',
    'Brazil',
    'Australia',
    'Japan',
  ];

  // Computed properties
  readonly isFormValid = computed(() => {
    return (
      this.personalInfoForm?.valid && this.shippingInfoForm?.valid && this.paymentInfoForm?.valid
    );
  });

  readonly checkoutRequest = computed((): CartCheckoutRequest | null => {
    if (!this.isFormValid()) return null;

    return {
      username: this.personalInfoForm.get('username')?.value || '',
      firstName: this.personalInfoForm.get('firstName')?.value || '',
      lastName: this.personalInfoForm.get('lastName')?.value || '',
      emailAddress: this.personalInfoForm.get('emailAddress')?.value || '',
      addressLine: this.shippingInfoForm.get('addressLine')?.value || '',
      country: this.shippingInfoForm.get('country')?.value || '',
      state: this.shippingInfoForm.get('state')?.value || '',
      zipCode: this.shippingInfoForm.get('zipCode')?.value || '',
      cardName: this.paymentInfoForm.get('cardName')?.value || '',
      cardNumber: this.paymentInfoForm.get('cardNumber')?.value || '',
      expiration: this.paymentInfoForm.get('expiration')?.value || '',
      cvv: this.paymentInfoForm.get('cvv')?.value || '',
      paymentMethod: this.paymentInfoForm.get('paymentMethod')?.value || 1,
    };
  });

  constructor() {
    // Initialize forms
    this.personalInfoForm = this.fb.group({
      username: ['', [Validators.required, Validators.minLength(3)]],
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      emailAddress: ['', [Validators.required, Validators.email]],
    });

    this.shippingInfoForm = this.fb.group({
      addressLine: ['', [Validators.required, Validators.minLength(5)]],
      country: ['', Validators.required],
      state: ['', [Validators.required, Validators.minLength(2)]],
      zipCode: ['', [Validators.required, Validators.pattern(/^\d{5}(-\d{4})?$/)]],
    });

    this.paymentInfoForm = this.fb.group({
      paymentMethod: [1, Validators.required],
      cardName: ['', [Validators.required, Validators.minLength(2)]],
      cardNumber: ['', [Validators.required, Validators.pattern(/^\d{16}$/)]],
      expiration: ['', [Validators.required, Validators.pattern(/^(0[1-9]|1[0-2])\/\d{2}$/)]],
      cvv: ['', [Validators.required, Validators.pattern(/^\d{3,4}$/)]],
    });
  }

  // Methods
  nextStep(): void {
    if (this.currentStep() < 2) {
      this.currentStep.set(this.currentStep() + 1);
    }
  }

  previousStep(): void {
    if (this.currentStep() > 0) {
      this.currentStep.set(this.currentStep() - 1);
    }
  }

  async processPayment(): Promise<void> {
    if (!this.isFormValid()) {
      console.error('Form is not valid');
      return;
    }

    this.isLoading.set(true);

    try {
      const request = this.checkoutRequest();
      console.log('Processing payment with request:', request);

      // Simulate API call
      await new Promise((resolve) => setTimeout(resolve, 2000));

      console.log('Payment processed successfully');
      // Here you would typically navigate to a success page or show a success message
    } catch (error) {
      console.error('Payment processing failed:', error);
      // Handle error - show error message to user
    } finally {
      this.isLoading.set(false);
    }
  }

  // Utility methods for form validation
  getErrorMessage(formGroup: FormGroup, fieldName: string): string {
    const field = formGroup.get(fieldName);
    if (field?.hasError('required')) {
      return `${fieldName} is required`;
    }
    if (field?.hasError('email')) {
      return 'Please enter a valid email address';
    }
    if (field?.hasError('minlength')) {
      const minLength = field.errors?.['minlength']?.requiredLength;
      return `${fieldName} must be at least ${minLength} characters`;
    }
    if (field?.hasError('pattern')) {
      switch (fieldName) {
        case 'zipCode':
          return 'Please enter a valid ZIP code (12345 or 12345-6789)';
        case 'cardNumber':
          return 'Please enter a valid 16-digit card number';
        case 'expiration':
          return 'Please enter expiration in MM/YY format';
        case 'cvv':
          return 'Please enter a valid CVV (3-4 digits)';
        default:
          return 'Please enter a valid value';
      }
    }
    return '';
  }

  isFieldInvalid(formGroup: FormGroup, fieldName: string): boolean {
    const field = formGroup.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }
}
