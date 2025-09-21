import { Injectable, computed, signal } from '@angular/core';

export interface UserData {
  username: string;
  firstName: string;
  lastName: string;
  emailAddress: string;
  addressLine: string;
  country: string;
  state: string;
  zipCode: string;
  cardName: string;
  cardNumber: string;
  expiration: string;
  cvv: string;
  paymentMethod: number;
}

@Injectable({
  providedIn: 'root',
})
export class UserStoreService {
  // Private signals for user data
  private _username = signal<string>('');
  private _firstName = signal<string>('');
  private _lastName = signal<string>('');
  private _emailAddress = signal<string>('');
  private _addressLine = signal<string>('');
  private _country = signal<string>('');
  private _state = signal<string>('');
  private _zipCode = signal<string>('');
  private _cardName = signal<string>('');
  private _cardNumber = signal<string>('');
  private _expiration = signal<string>('');
  private _cvv = signal<string>('');
  private _paymentMethod = signal<number>(0);

  // Public readonly computed signals
  readonly username = computed(() => this._username());
  readonly firstName = computed(() => this._firstName());
  readonly lastName = computed(() => this._lastName());
  readonly emailAddress = computed(() => this._emailAddress());
  readonly addressLine = computed(() => this._addressLine());
  readonly country = computed(() => this._country());
  readonly state = computed(() => this._state());
  readonly zipCode = computed(() => this._zipCode());
  readonly cardName = computed(() => this._cardName());
  readonly cardNumber = computed(() => this._cardNumber());
  readonly expiration = computed(() => this._expiration());
  readonly cvv = computed(() => this._cvv());
  readonly paymentMethod = computed(() => this._paymentMethod());

  // Computed signals for derived state
  readonly fullName = computed(() => `${this._firstName()} ${this._lastName()}`);
  readonly hasPersonalInfo = computed(
    () => this._firstName() && this._lastName() && this._emailAddress()
  );
  readonly hasAddressInfo = computed(
    () => this._addressLine() && this._country() && this._state() && this._zipCode()
  );
  readonly hasPaymentInfo = computed(
    () => this._cardName() && this._cardNumber() && this._expiration() && this._cvv()
  );
  readonly isProfileComplete = computed(
    () => this.hasPersonalInfo() && this.hasAddressInfo() && this.hasPaymentInfo()
  );

  // Get all user data as an object
  readonly userData = computed(
    (): UserData => ({
      username: this._username(),
      firstName: this._firstName(),
      lastName: this._lastName(),
      emailAddress: this._emailAddress(),
      addressLine: this._addressLine(),
      country: this._country(),
      state: this._state(),
      zipCode: this._zipCode(),
      cardName: this._cardName(),
      cardNumber: this._cardNumber(),
      expiration: this._expiration(),
      cvv: this._cvv(),
      paymentMethod: this._paymentMethod(),
    })
  );

  // Update methods
  updateUsername(username: string): void {
    this._username.set(username);
  }

  updateFirstName(firstName: string): void {
    this._firstName.set(firstName);
  }

  updateLastName(lastName: string): void {
    this._lastName.set(lastName);
  }

  updateEmailAddress(emailAddress: string): void {
    this._emailAddress.set(emailAddress);
  }

  updateAddressLine(addressLine: string): void {
    this._addressLine.set(addressLine);
  }

  updateCountry(country: string): void {
    this._country.set(country);
  }

  updateState(state: string): void {
    this._state.set(state);
  }

  updateZipCode(zipCode: string): void {
    this._zipCode.set(zipCode);
  }

  updateCardName(cardName: string): void {
    this._cardName.set(cardName);
  }

  updateCardNumber(cardNumber: string): void {
    this._cardNumber.set(cardNumber);
  }

  updateExpiration(expiration: string): void {
    this._expiration.set(expiration);
  }

  updateCvv(cvv: string): void {
    this._cvv.set(cvv);
  }

  updatePaymentMethod(paymentMethod: number): void {
    this._paymentMethod.set(paymentMethod);
  }

  // Bulk update methods
  updatePersonalInfo(
    data: Partial<Pick<UserData, 'firstName' | 'lastName' | 'emailAddress'>>
  ): void {
    if (data.firstName !== undefined) this._firstName.set(data.firstName);
    if (data.lastName !== undefined) this._lastName.set(data.lastName);
    if (data.emailAddress !== undefined) this._emailAddress.set(data.emailAddress);
  }

  updateAddressInfo(
    data: Partial<Pick<UserData, 'addressLine' | 'country' | 'state' | 'zipCode'>>
  ): void {
    if (data.addressLine !== undefined) this._addressLine.set(data.addressLine);
    if (data.country !== undefined) this._country.set(data.country);
    if (data.state !== undefined) this._state.set(data.state);
    if (data.zipCode !== undefined) this._zipCode.set(data.zipCode);
  }

  updatePaymentInfo(
    data: Partial<
      Pick<UserData, 'cardName' | 'cardNumber' | 'expiration' | 'cvv' | 'paymentMethod'>
    >
  ): void {
    if (data.cardName !== undefined) this._cardName.set(data.cardName);
    if (data.cardNumber !== undefined) this._cardNumber.set(data.cardNumber);
    if (data.expiration !== undefined) this._expiration.set(data.expiration);
    if (data.cvv !== undefined) this._cvv.set(data.cvv);
    if (data.paymentMethod !== undefined) this._paymentMethod.set(data.paymentMethod);
  }

  updateAllUserData(data: Partial<UserData>): void {
    if (data.username !== undefined) this._username.set(data.username);
    if (data.firstName !== undefined) this._firstName.set(data.firstName);
    if (data.lastName !== undefined) this._lastName.set(data.lastName);
    if (data.emailAddress !== undefined) this._emailAddress.set(data.emailAddress);
    if (data.addressLine !== undefined) this._addressLine.set(data.addressLine);
    if (data.country !== undefined) this._country.set(data.country);
    if (data.state !== undefined) this._state.set(data.state);
    if (data.zipCode !== undefined) this._zipCode.set(data.zipCode);
    if (data.cardName !== undefined) this._cardName.set(data.cardName);
    if (data.cardNumber !== undefined) this._cardNumber.set(data.cardNumber);
    if (data.expiration !== undefined) this._expiration.set(data.expiration);
    if (data.cvv !== undefined) this._cvv.set(data.cvv);
    if (data.paymentMethod !== undefined) this._paymentMethod.set(data.paymentMethod);
  }

  // Clear methods
  clearPersonalInfo(): void {
    this._firstName.set('');
    this._lastName.set('');
    this._emailAddress.set('');
  }

  clearAddressInfo(): void {
    this._addressLine.set('');
    this._country.set('');
    this._state.set('');
    this._zipCode.set('');
  }

  clearPaymentInfo(): void {
    this._cardName.set('');
    this._cardNumber.set('');
    this._expiration.set('');
    this._cvv.set('');
    this._paymentMethod.set(0);
  }

  clearAllUserData(): void {
    this._username.set('');
    this._firstName.set('');
    this._lastName.set('');
    this._emailAddress.set('');
    this._addressLine.set('');
    this._country.set('');
    this._state.set('');
    this._zipCode.set('');
    this._cardName.set('');
    this._cardNumber.set('');
    this._expiration.set('');
    this._cvv.set('');
    this._paymentMethod.set(0);
  }
}
