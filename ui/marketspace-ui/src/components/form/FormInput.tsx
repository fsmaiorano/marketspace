import React from "react";
import { Input } from "../ui/input";
import { cn } from "../../lib/utils";

interface FormInputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  field: any;
}

export function FormInput({
  label,
  error,
  field,
  className,
  ...props
}: FormInputProps) {
  return (
    <div className="flex flex-col gap-2">
      {label && (
        <label
          htmlFor={field.name}
          className="text-sm font-medium text-gray-700"
        >
          {label}
        </label>
      )}
      <Input
        id={field.name}
        name={field.name}
        value={field.state.value}
        onChange={(e) => field.handleChange(e.target.value)}
        onBlur={field.handleBlur}
        className={cn(
          "transition-colors",
          error && "border-red-500 focus:border-red-500 focus:ring-red-200",
        )}
        {...props}
      />
      {error && <p className="text-sm text-red-600">{error}</p>}
    </div>
  );
}
