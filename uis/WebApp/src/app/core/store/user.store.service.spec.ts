import { TestBed } from '@angular/core/testing';
import { UserStoreService, UserData } from './user.store.service';

describe('UserStoreService', () => {
  let service: UserStoreService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(UserStoreService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('initial state', () => {
    it('should have empty initial values', () => {
      expect(service.username()).toBe('');
      expect(service.firstName()).toBe('');
      expect(service.lastName()).toBe('');
      expect(service.emailAddress()).toBe('');
      expect(service.addressLine()).toBe('');
      expect(service.country()).toBe('');
      expect(service.state()).toBe('');
      expect(service.zipCode()).toBe('');
      expect(service.cardName()).toBe('');
      expect(service.cardNumber()).toBe('');
      expect(service.expiration()).toBe('');
      expect(service.cvv()).toBe('');
      expect(service.paymentMethod()).toBe(0);
    });

    it('should have empty full name initially', () => {
      expect(service.fullName()).toBe(' ');
    });

    it('should not have complete info sections initially', () => {
      expect(service.hasPersonalInfo()).toBeFalsy();
      expect(service.hasAddressInfo()).toBeFalsy();
      expect(service.hasPaymentInfo()).toBeFalsy();
      expect(service.isProfileComplete()).toBeFalsy();
    });
  });

  describe('individual field updates', () => {
    it('should update username', () => {
      service.updateUsername('testuser');
      expect(service.username()).toBe('testuser');
    });

    it('should update firstName', () => {
      service.updateFirstName('John');
      expect(service.firstName()).toBe('John');
    });

    it('should update lastName', () => {
      service.updateLastName('Doe');
      expect(service.lastName()).toBe('Doe');
    });

    it('should update emailAddress', () => {
      service.updateEmailAddress('john.doe@example.com');
      expect(service.emailAddress()).toBe('john.doe@example.com');
    });

    it('should update addressLine', () => {
      service.updateAddressLine('123 Main St');
      expect(service.addressLine()).toBe('123 Main St');
    });

    it('should update country', () => {
      service.updateCountry('USA');
      expect(service.country()).toBe('USA');
    });

    it('should update state', () => {
      service.updateState('CA');
      expect(service.state()).toBe('CA');
    });

    it('should update zipCode', () => {
      service.updateZipCode('12345');
      expect(service.zipCode()).toBe('12345');
    });

    it('should update cardName', () => {
      service.updateCardName('John Doe');
      expect(service.cardName()).toBe('John Doe');
    });

    it('should update cardNumber', () => {
      service.updateCardNumber('1234567890123456');
      expect(service.cardNumber()).toBe('1234567890123456');
    });

    it('should update expiration', () => {
      service.updateExpiration('12/25');
      expect(service.expiration()).toBe('12/25');
    });

    it('should update cvv', () => {
      service.updateCvv('123');
      expect(service.cvv()).toBe('123');
    });

    it('should update paymentMethod', () => {
      service.updatePaymentMethod(1);
      expect(service.paymentMethod()).toBe(1);
    });
  });

  describe('computed properties', () => {
    beforeEach(() => {
      service.updateFirstName('John');
      service.updateLastName('Doe');
    });

    it('should compute full name correctly', () => {
      expect(service.fullName()).toBe('John Doe');
    });

    it('should detect when personal info is complete', () => {
      service.updateEmailAddress('john.doe@example.com');
      expect(service.hasPersonalInfo()).toBeTruthy();
    });

    it('should detect when address info is complete', () => {
      service.updateAddressLine('123 Main St');
      service.updateCountry('USA');
      service.updateState('CA');
      service.updateZipCode('12345');
      expect(service.hasAddressInfo()).toBeTruthy();
    });

    it('should detect when payment info is complete', () => {
      service.updateCardName('John Doe');
      service.updateCardNumber('1234567890123456');
      service.updateExpiration('12/25');
      service.updateCvv('123');
      expect(service.hasPaymentInfo()).toBeTruthy();
    });

    it('should detect when profile is complete', () => {
      // Personal info
      service.updateEmailAddress('john.doe@example.com');
      // Address info
      service.updateAddressLine('123 Main St');
      service.updateCountry('USA');
      service.updateState('CA');
      service.updateZipCode('12345');
      // Payment info
      service.updateCardName('John Doe');
      service.updateCardNumber('1234567890123456');
      service.updateExpiration('12/25');
      service.updateCvv('123');

      expect(service.isProfileComplete()).toBeTruthy();
    });
  });

  describe('bulk update methods', () => {
    it('should update personal info', () => {
      const personalData = {
        firstName: 'Jane',
        lastName: 'Smith',
        emailAddress: 'jane.smith@example.com',
      };

      service.updatePersonalInfo(personalData);

      expect(service.firstName()).toBe('Jane');
      expect(service.lastName()).toBe('Smith');
      expect(service.emailAddress()).toBe('jane.smith@example.com');
    });

    it('should update address info', () => {
      const addressData = {
        addressLine: '456 Oak Ave',
        country: 'Canada',
        state: 'ON',
        zipCode: 'K1A 0A9',
      };

      service.updateAddressInfo(addressData);

      expect(service.addressLine()).toBe('456 Oak Ave');
      expect(service.country()).toBe('Canada');
      expect(service.state()).toBe('ON');
      expect(service.zipCode()).toBe('K1A 0A9');
    });

    it('should update payment info', () => {
      const paymentData = {
        cardName: 'Jane Smith',
        cardNumber: '9876543210987654',
        expiration: '06/27',
        cvv: '456',
        paymentMethod: 2,
      };

      service.updatePaymentInfo(paymentData);

      expect(service.cardName()).toBe('Jane Smith');
      expect(service.cardNumber()).toBe('9876543210987654');
      expect(service.expiration()).toBe('06/27');
      expect(service.cvv()).toBe('456');
      expect(service.paymentMethod()).toBe(2);
    });

    it('should update all user data', () => {
      const userData: UserData = {
        username: 'janesmith',
        firstName: 'Jane',
        lastName: 'Smith',
        emailAddress: 'jane.smith@example.com',
        addressLine: '456 Oak Ave',
        country: 'Canada',
        state: 'ON',
        zipCode: 'K1A 0A9',
        cardName: 'Jane Smith',
        cardNumber: '9876543210987654',
        expiration: '06/27',
        cvv: '456',
        paymentMethod: 2,
      };

      service.updateAllUserData(userData);

      expect(service.userData()).toEqual(userData);
    });
  });

  describe('clear methods', () => {
    beforeEach(() => {
      // Set up some data first
      service.updatePersonalInfo({
        firstName: 'John',
        lastName: 'Doe',
        emailAddress: 'john.doe@example.com',
      });
      service.updateAddressInfo({
        addressLine: '123 Main St',
        country: 'USA',
        state: 'CA',
        zipCode: '12345',
      });
      service.updatePaymentInfo({
        cardName: 'John Doe',
        cardNumber: '1234567890123456',
        expiration: '12/25',
        cvv: '123',
        paymentMethod: 1,
      });
    });

    it('should clear personal info', () => {
      service.clearPersonalInfo();

      expect(service.firstName()).toBe('');
      expect(service.lastName()).toBe('');
      expect(service.emailAddress()).toBe('');
      expect(service.hasPersonalInfo()).toBeFalsy();
    });

    it('should clear address info', () => {
      service.clearAddressInfo();

      expect(service.addressLine()).toBe('');
      expect(service.country()).toBe('');
      expect(service.state()).toBe('');
      expect(service.zipCode()).toBe('');
      expect(service.hasAddressInfo()).toBeFalsy();
    });

    it('should clear payment info', () => {
      service.clearPaymentInfo();

      expect(service.cardName()).toBe('');
      expect(service.cardNumber()).toBe('');
      expect(service.expiration()).toBe('');
      expect(service.cvv()).toBe('');
      expect(service.paymentMethod()).toBe(0);
      expect(service.hasPaymentInfo()).toBeFalsy();
    });

    it('should clear all user data', () => {
      service.clearAllUserData();

      expect(service.username()).toBe('');
      expect(service.firstName()).toBe('');
      expect(service.lastName()).toBe('');
      expect(service.emailAddress()).toBe('');
      expect(service.addressLine()).toBe('');
      expect(service.country()).toBe('');
      expect(service.state()).toBe('');
      expect(service.zipCode()).toBe('');
      expect(service.cardName()).toBe('');
      expect(service.cardNumber()).toBe('');
      expect(service.expiration()).toBe('');
      expect(service.cvv()).toBe('');
      expect(service.paymentMethod()).toBe(0);
      expect(service.isProfileComplete()).toBeFalsy();
    });
  });

  describe('userData computed signal', () => {
    it('should return complete user data object', () => {
      const expectedData: UserData = {
        username: 'testuser',
        firstName: 'John',
        lastName: 'Doe',
        emailAddress: 'john.doe@example.com',
        addressLine: '123 Main St',
        country: 'USA',
        state: 'CA',
        zipCode: '12345',
        cardName: 'John Doe',
        cardNumber: '1234567890123456',
        expiration: '12/25',
        cvv: '123',
        paymentMethod: 1,
      };

      service.updateAllUserData(expectedData);

      expect(service.userData()).toEqual(expectedData);
    });
  });
});
