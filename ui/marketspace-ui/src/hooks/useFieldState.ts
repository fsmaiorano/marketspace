export function getFieldError(field: any) {
  return field.state.meta.errors?.[0] || null;
}

export function getFieldStatus(field: any) {
  return {
    isTouched: field.state.meta.isTouched,
    isDirty: field.state.meta.isDirty,
    isValidating: field.state.meta.isValidating,
  };
}

export function shouldShowError(field: any) {
  return field.state.meta.isTouched && field.state.meta.errors.length > 0;
}
