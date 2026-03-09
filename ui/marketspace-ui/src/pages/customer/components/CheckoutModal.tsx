import { useForm, type AnyFieldApi } from "@tanstack/react-form";
import { z } from "zod";
import { X, CreditCard, MapPin } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { faker } from "@faker-js/faker";

const checkoutSchema = z.object({
  firstName: z.string().min(1, "Required"),
  lastName: z.string().min(1, "Required"),
  emailAddress: z.string().email("Invalid email"),
  addressLine: z.string().min(1, "Required"),
  country: z.string().min(1, "Required"),
  state: z.string().min(1, "Required"),
  zipCode: z.string().min(1, "Required"),
  cardName: z.string().min(1, "Required"),
  cardNumber: z.string().min(1, "Required"),
  expiration: z.string().min(1, "Required"),
  cvv: z.string().min(1, "Required"),
  paymentMethod: z.number(),
});

export type CheckoutFormData = z.infer<typeof checkoutSchema>;

interface CheckoutModalProps {
  open: boolean;
  submitting: boolean;
  defaultEmail?: string;
  onClose: () => void;
  onSubmit: (data: CheckoutFormData) => void;
}

// Outer gate — mounts the inner form fresh each time the modal opens,
// ensuring defaultValues (e.g. defaultEmail) are always up-to-date.
export function CheckoutModal(props: CheckoutModalProps) {
  if (!props.open) return null;
  return <CheckoutModalContent {...props} />;
}

function FieldRow({
  label,
  field,
  ...inputProps
}: { label: string; field: AnyFieldApi } & React.InputHTMLAttributes<HTMLInputElement>) {
  const error = field.state.meta.errors[0];
  return (
    <div className="flex flex-col gap-1">
      <label htmlFor={field.name} className="text-sm font-medium">
        {label}
      </label>
      <Input
        id={field.name}
        value={field.state.value as string}
        onChange={(e) => field.handleChange(e.target.value)}
        onBlur={field.handleBlur}
        aria-invalid={!!error}
        {...inputProps}
      />
      {!!error && <p className="text-xs text-red-600">{String(error)}</p>}
    </div>
  );
}

function getMockCheckoutData(defaultEmail?: string): CheckoutFormData {
  return {
    firstName: faker.person.firstName(),
    lastName: faker.person.lastName(),
    emailAddress: defaultEmail ?? faker.internet.email(),
    addressLine: faker.location.streetAddress(),
    country: faker.location.countryCode(),
    state: faker.location.state(),
    zipCode: faker.location.zipCode(),
    cardName: faker.person.fullName(),
    cardNumber: faker.finance.creditCardNumber(),
    expiration: faker.date.future({ years: 2 }).toLocaleDateString("en-US", { month: "2-digit", year: "2-digit" }),
    cvv: faker.finance.creditCardCVV(),
    paymentMethod: 0,
  };
}

function CheckoutModalContent({ submitting, defaultEmail, onClose, onSubmit }: CheckoutModalProps) {
  const form = useForm({
    defaultValues: getMockCheckoutData(defaultEmail),
    validators: { onSubmit: checkoutSchema },
    onSubmit: async ({ value }) => onSubmit(value),
  });

  const handleClose = () => {
    if (submitting) return;
    form.reset();
    onClose();
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="absolute inset-0 bg-black/50" onClick={handleClose} />
      <div className="relative z-10 flex h-full w-full max-w-lg flex-col overflow-hidden rounded-none bg-background shadow-2xl sm:h-auto sm:max-h-[90vh] sm:rounded-lg">
        <div className="flex items-center justify-between border-b px-5 py-4">
          <h2 className="text-lg font-semibold">Checkout</h2>
          <button
            type="button"
            className="rounded p-1 hover:bg-muted disabled:opacity-50"
            onClick={handleClose}
            disabled={submitting}
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        <form
          onSubmit={(e) => {
            e.preventDefault();
            form.handleSubmit();
          }}
          className="flex flex-col overflow-hidden"
        >
          <div className="flex-1 space-y-5 overflow-y-auto px-5 py-4">
            <section className="space-y-3">
              <h3 className="flex items-center gap-2 text-sm font-semibold uppercase tracking-wide text-muted-foreground">
                <MapPin className="h-4 w-4" />
                Shipping Address
              </h3>

              <div className="grid grid-cols-2 gap-3">
                <form.Field name="firstName" validators={{ onChange: z.string().min(1, "Required") }}>
                  {(field) => <FieldRow label="First Name" field={field} placeholder="John" />}
                </form.Field>
                <form.Field name="lastName" validators={{ onChange: z.string().min(1, "Required") }}>
                  {(field) => <FieldRow label="Last Name" field={field} placeholder="Doe" />}
                </form.Field>
              </div>

              <form.Field name="emailAddress" validators={{ onChange: z.string().email("Invalid email") }}>
                {(field) => (
                  <FieldRow label="Email" field={field} type="email" placeholder="john@example.com" />
                )}
              </form.Field>

              <form.Field name="addressLine" validators={{ onChange: z.string().min(1, "Required") }}>
                {(field) => <FieldRow label="Address" field={field} placeholder="123 Main St" />}
              </form.Field>

              <div className="grid grid-cols-3 gap-3">
                <form.Field name="country" validators={{ onChange: z.string().min(1, "Required") }}>
                  {(field) => <FieldRow label="Country" field={field} placeholder="US" />}
                </form.Field>
                <form.Field name="state" validators={{ onChange: z.string().min(1, "Required") }}>
                  {(field) => <FieldRow label="State" field={field} placeholder="NY" />}
                </form.Field>
                <form.Field name="zipCode" validators={{ onChange: z.string().min(1, "Required") }}>
                  {(field) => <FieldRow label="ZIP Code" field={field} placeholder="10001" />}
                </form.Field>
              </div>
            </section>

            <section className="space-y-3">
              <h3 className="flex items-center gap-2 text-sm font-semibold uppercase tracking-wide text-muted-foreground">
                <CreditCard className="h-4 w-4" />
                Payment
              </h3>

              <form.Field name="cardName" validators={{ onChange: z.string().min(1, "Required") }}>
                {(field) => <FieldRow label="Name on Card" field={field} placeholder="John Doe" />}
              </form.Field>

              <form.Field name="cardNumber" validators={{ onChange: z.string().min(1, "Required") }}>
                {(field) => (
                  <FieldRow label="Card Number" field={field} placeholder="4242 4242 4242 4242" maxLength={19} />
                )}
              </form.Field>

              <div className="grid grid-cols-2 gap-3">
                <form.Field name="expiration" validators={{ onChange: z.string().min(1, "Required") }}>
                  {(field) => (
                    <FieldRow label="Expiration (MM/YY)" field={field} placeholder="12/28" maxLength={5} />
                  )}
                </form.Field>
                <form.Field name="cvv" validators={{ onChange: z.string().min(1, "Required") }}>
                  {(field) => <FieldRow label="CVV" field={field} placeholder="123" maxLength={4} />}
                </form.Field>
              </div>
            </section>
          </div>

          <div className="border-t px-5 py-4">
            <form.Subscribe selector={(s) => s.isSubmitting}>
              {(isSubmitting) => (
                <Button type="submit" className="w-full" disabled={submitting || isSubmitting}>
                  {submitting || isSubmitting ? "Placing order..." : "Place Order"}
                </Button>
              )}
            </form.Subscribe>
          </div>
        </form>
      </div>
    </div>
  );
}
