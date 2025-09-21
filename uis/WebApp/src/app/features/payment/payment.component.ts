import {
  Component,
  inject,
  signal,
  computed,
  ChangeDetectionStrategy,
  ViewChild,
} from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatStepperModule, MatStepper } from '@angular/material/stepper';
import { MatRadioModule } from '@angular/material/radio';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { CommonModule } from '@angular/common';
import { CartCheckoutRequest } from '../../shared/models/cart-checkout-request';
import { UserStoreService } from '@app/core/store/user.store.service';

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
    MatTooltipModule,
  ],
  templateUrl: './payment.component.html',
  styleUrl: './payment.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaymentComponent {
  private userStore = inject(UserStoreService);
  private readonly fb = inject(FormBuilder);

  @ViewChild('stepper') stepper!: MatStepper;

  readonly isLoading = signal(false);
  readonly formsValidSignal = signal(false);

  readonly personalInfoForm: FormGroup;
  readonly shippingInfoForm: FormGroup;
  readonly paymentInfoForm: FormGroup;

  readonly paymentMethods = [
    { value: 1, label: 'Credit Card', icon: 'credit_card' },
    { value: 2, label: 'Debit Card', icon: 'payment' },
    { value: 3, label: 'PayPal', icon: 'account_balance_wallet' },
  ];

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

  readonly isFormValid = computed(() => {
    if (!this.personalInfoForm || !this.shippingInfoForm || !this.paymentInfoForm) {
      console.log('❌ Forms not initialized yet');
      return false;
    }

    const personalValid = this.personalInfoForm.valid;
    const shippingValid = this.shippingInfoForm.valid;
    const paymentValid = this.paymentInfoForm.valid;

    console.log('🔍 Form Validation Check:', {
      personal: {
        valid: personalValid,
        status: this.personalInfoForm.status,
        errors: this.personalInfoForm.errors,
        values: this.personalInfoForm.value
      },
      shipping: {
        valid: shippingValid,
        status: this.shippingInfoForm.status,
        errors: this.shippingInfoForm.errors,
        values: this.shippingInfoForm.value
      },
      payment: {
        valid: paymentValid,
        status: this.paymentInfoForm.status,
        errors: this.paymentInfoForm.errors,
        values: this.paymentInfoForm.value
      },
      overall: personalValid && shippingValid && paymentValid
    });

    return personalValid && shippingValid && paymentValid;
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
    this.personalInfoForm = this.fb.group({
      username: [this.userStore.username(), [Validators.required, Validators.minLength(3)]],
      firstName: [this.userStore.firstName(), [Validators.required, Validators.minLength(2)]],
      lastName: [this.userStore.lastName(), [Validators.required, Validators.minLength(2)]],
      emailAddress: [this.userStore.emailAddress(), [Validators.required, Validators.email]],
    });

    this.shippingInfoForm = this.fb.group({
      addressLine: [this.userStore.addressLine(), [Validators.required, Validators.minLength(5)]],
      country: [this.userStore.country(), Validators.required],
      state: [this.userStore.state(), [Validators.required, Validators.minLength(2)]],
      zipCode: [
        this.userStore.zipCode(),
        [Validators.required, Validators.pattern(/^\d{5}(-\d{4})?$/)],
      ],
    });

    this.paymentInfoForm = this.fb.group({
      paymentMethod: [this.userStore.paymentMethod() || 1, Validators.required],
      cardName: [this.userStore.cardName(), [Validators.required, Validators.minLength(2)]],
      cardNumber: [
        this.userStore.cardNumber(),
        [Validators.required, Validators.pattern(/^\d{16}$/)],
      ],
      expiration: [
        this.userStore.expiration(),
        [Validators.required, Validators.pattern(/^(0[1-9]|1[0-2])\/\d{2}$/)],
      ],
      cvv: [this.userStore.cvv(), [Validators.required, Validators.pattern(/^\d{3,4}$/)]],
    });

    this.setupFormSubscriptions();
  }

  private setupFormSubscriptions(): void {
    this.personalInfoForm.valueChanges.subscribe((values) => {
      this.userStore.updateUsername(values.username || '');
      this.userStore.updateFirstName(values.firstName || '');
      this.userStore.updateLastName(values.lastName || '');
      this.userStore.updateEmailAddress(values.emailAddress || '');
      this.updateFormValiditySignal();
    });

    this.shippingInfoForm.valueChanges.subscribe((values) => {
      this.userStore.updateAddressLine(values.addressLine || '');
      this.userStore.updateCountry(values.country || '');
      this.userStore.updateState(values.state || '');
      this.userStore.updateZipCode(values.zipCode || '');
      this.updateFormValiditySignal();
    });

    this.paymentInfoForm.valueChanges.subscribe((values) => {
      this.userStore.updateCardName(values.cardName || '');
      this.userStore.updateCardNumber(values.cardNumber || '');
      this.userStore.updateExpiration(values.expiration || '');
      this.userStore.updateCvv(values.cvv || '');
      this.userStore.updatePaymentMethod(values.paymentMethod || 1);
      this.updateFormValiditySignal();
    });

    this.personalInfoForm.statusChanges.subscribe(() => this.updateFormValiditySignal());
    this.shippingInfoForm.statusChanges.subscribe(() => this.updateFormValiditySignal());
    this.paymentInfoForm.statusChanges.subscribe(() => this.updateFormValiditySignal());
  }

  private updateFormValiditySignal(): void {
    const isValid = this.personalInfoForm?.valid &&
                   this.shippingInfoForm?.valid &&
                   this.paymentInfoForm?.valid;
    this.formsValidSignal.set(isValid);
  }

  loadUserDataToForms(): void {
    this.personalInfoForm.patchValue({
      username: this.userStore.username(),
      firstName: this.userStore.firstName(),
      lastName: this.userStore.lastName(),
      emailAddress: this.userStore.emailAddress(),
    });

    this.shippingInfoForm.patchValue({
      addressLine: this.userStore.addressLine(),
      country: this.userStore.country(),
      state: this.userStore.state(),
      zipCode: this.userStore.zipCode(),
    });

    this.paymentInfoForm.patchValue({
      paymentMethod: this.userStore.paymentMethod() || 1,
      cardName: this.userStore.cardName(),
      cardNumber: this.userStore.cardNumber(),
      expiration: this.userStore.expiration(),
      cvv: this.userStore.cvv(),
    });
  }

  clearPersonalInfoForm(): void {
    this.personalInfoForm.reset();
    this.userStore.updatePersonalInfo({
      firstName: '',
      lastName: '',
      emailAddress: '',
    });
    this.userStore.updateUsername('');
  }

  clearShippingInfoForm(): void {
    this.shippingInfoForm.reset();
    this.userStore.clearAddressInfo();
  }

  clearPaymentInfoForm(): void {
    this.paymentInfoForm.reset();
    this.paymentInfoForm.patchValue({ paymentMethod: 1 });
    this.userStore.clearPaymentInfo();
  }

  readonly hasPersonalData = computed(
    () =>
      this.userStore.username() ||
      this.userStore.firstName() ||
      this.userStore.lastName() ||
      this.userStore.emailAddress()
  );

  readonly hasShippingData = computed(
    () =>
      this.userStore.addressLine() ||
      this.userStore.country() ||
      this.userStore.state() ||
      this.userStore.zipCode()
  );

  readonly hasPaymentData = computed(
    () =>
      this.userStore.cardName() ||
      this.userStore.cardNumber() ||
      this.userStore.expiration() ||
      this.userStore.cvv()
  );

  populateTestData(): void {
    this.userStore.updateAllUserData({
      username: 'johndoe',
      firstName: 'John',
      lastName: 'Doe',
      emailAddress: 'john.doe@example.com',
      addressLine: '123 Main Street, Apt 4B',
      country: 'United States',
      state: 'CA',
      zipCode: '90210',
      cardName: 'John Doe',
      cardNumber: '1234567890123456',
      expiration: '12/25',
      cvv: '123',
      paymentMethod: 1,
    });

    this.personalInfoForm.patchValue({
      username: 'johndoe',
      firstName: 'John',
      lastName: 'Doe',
      emailAddress: 'john.doe@example.com',
    });
    this.personalInfoForm.markAllAsTouched();

    this.shippingInfoForm.patchValue({
      addressLine: '123 Main Street, Apt 4B',
      country: 'United States',
      state: 'CA',
      zipCode: '90210',
    });
    this.shippingInfoForm.markAllAsTouched();

    this.paymentInfoForm.patchValue({
      paymentMethod: 1,
      cardName: 'John Doe',
      cardNumber: '1234567890123456',
      expiration: '12/25',
      cvv: '123',
    });
    this.paymentInfoForm.markAllAsTouched();

    this.personalInfoForm.updateValueAndValidity();
    this.shippingInfoForm.updateValueAndValidity();
    this.paymentInfoForm.updateValueAndValidity();

    setTimeout(() => {
      this.updateFormValiditySignal();
      console.log('✅ Manual signal update after test data:', this.formsValidSignal());
    }, 0);
  }

  nextStep(): void {
    if (this.stepper) {
      this.stepper.next();
    }
  }

  previousStep(): void {
    if (this.stepper) {
      this.stepper.previous();
    }
  }

  goToStep(index: number): void {
    if (this.stepper && index >= 0 && index < 3) {
      this.stepper.selectedIndex = index;
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
      if (request) {
        this.userStore.updateAllUserData(request);
      }

      console.log('Processing payment with request:', request);

      await new Promise((resolve) => setTimeout(resolve, 2000));

      console.log('Payment processed successfully');
      console.log('User data saved to store:', this.userStore.userData());
    } catch (error) {
      console.error('Payment processing failed:', error);
    } finally {
      this.isLoading.set(false);
    }
  }

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

  debugFormStatus(): void {
    console.log('=== FORM DEBUG STATUS ===');
    console.log('Personal Form:', {
      valid: this.personalInfoForm.valid,
      values: this.personalInfoForm.value,
      errors: this.personalInfoForm.errors
    });
    console.log('Shipping Form:', {
      valid: this.shippingInfoForm.valid,
      values: this.shippingInfoForm.value,
      errors: this.shippingInfoForm.errors
    });
    console.log('Payment Form:', {
      valid: this.paymentInfoForm.valid,
      values: this.paymentInfoForm.value,
      errors: this.paymentInfoForm.errors
    });
    console.log('Overall isFormValid:', this.isFormValid());
    console.log('User Store Data:', this.userStore.userData());
    console.log('=== END DEBUG ===');
  }
}
