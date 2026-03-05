export interface UseFormConfig<T> {
  initialValues: T;
  onSubmit: (values: T) => Promise<void> | void;
  validationSchema?: any;
}

export function createFieldError(message: string) {
  return message;
}

export function isFormDirty(fieldStates: any[]) {
  return fieldStates.some((field) => field.state.isDirty);
}

export function isFormTouched(fieldStates: any[]) {
  return fieldStates.some((field) => field.state.isTouched);
}
