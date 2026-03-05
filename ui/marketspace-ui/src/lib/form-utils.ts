export const getFieldError = (fieldMeta: any) => {
  return fieldMeta.errors?.[0] || null;
};

export const isFieldDirty = (fieldMeta: any) => {
  return fieldMeta.isDirty;
};

export const isFieldTouched = (fieldMeta: any) => {
  return fieldMeta.isTouched;
};
