import { useState } from "react";
import { useForm } from "@tanstack/react-form";
import { z } from "zod";
import { Input } from "../components/ui/input";
import { Button } from "../components/ui/button";
import {
  login,
  register,
  getMe,
  normalizeUserType,
  storeUserType,
  type LoginRequest,
  type RegisterRequest,
} from "../services/authentication-service";
import { useAuthStore } from "../stores/auth-store";
import { envConfig } from "../lib/env-config";

type AuthMode = "login" | "register";

const loginSchema = z.object({
  email: z.string().email("Invalid email address"),
  password: z.string().min(1, "Password is required"),
});

const registerSchema = z
  .object({
    email: z.string().email("Invalid email address"),
    name: z.string().min(1, "Full name is required"),
    password: z.string().min(6, "Password must be at least 6 characters"),
    confirmPassword: z.string(),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: "Passwords do not match",
    path: ["confirmPassword"],
  });

export default function AuthPage() {
  const [mode, setMode] = useState<AuthMode>("login");
  const { setTokens, setLoading, setError, error, isLoading, setUser } = useAuthStore();
  const [success, setSuccess] = useState<string | null>(null);

  const schema = mode === "login" ? loginSchema : registerSchema;

  const form = useForm({
    defaultValues: {
      email: "",
      password: "",
      name: "",
      confirmPassword: "",
    } as any,
    onSubmit: async () => {
      setError(null);
      setSuccess(null);
      setLoading(true);

      try {
        const emailValue = form.state.values.email ?? "";
        const passwordValue = form.state.values.password ?? "";
        const nameValue = form.state.values.name ?? "";
        const confirmPasswordValue = form.state.values.confirmPassword ?? "";

        const collectedValues = {
          email: emailValue,
          password: passwordValue,
          ...(mode === "register" && {
            name: nameValue,
            confirmPassword: confirmPasswordValue,
          }),
        };

        const result = schema.safeParse(collectedValues);
        if (!result.success) {
          const issues = result.error.issues;
          if (issues.length > 0) {
            setError(issues[0].message);
          }
          setLoading(false);
          return;
        }

        const values = collectedValues;

        const syncCurrentUser = async () => {
          const me = await getMe();
          const resolvedUserType = normalizeUserType(me.userType) ?? "Customer";
          storeUserType(resolvedUserType);
          setUser({
            id: me.userId,
            email: me.email ?? values.email,
            userType: resolvedUserType,
          });
        };

        if (mode === "login") {
          const credentials: LoginRequest = {
            email: values.email,
            password: values.password,
          };

          const response = await login(credentials);
          setTokens(response.accessToken, response.refreshToken);
          await syncCurrentUser();

          window.location.href = "/home";
        } else {
          const data: RegisterRequest = {
            email: values.email,
            password: values.password,
            name: values.name,
            username: values.email,
            userType: 0,
          };

          const response = await register(data);
          setTokens(response.accessToken, response.refreshToken);
          await syncCurrentUser();
          setSuccess(`Account created successfully! Welcome, ${values.name}`);

          setTimeout(() => {
            window.location.href = "/home";
          }, 1500);
        }
      } catch (err: unknown) {
        const errorMessage =
          err instanceof Error
            ? err.message
            : `${mode === "login" ? "Login" : "Registration"} failed. Please try again.`;
        setError(errorMessage);
        console.error(`${mode} error:`, err);
      } finally {
        setLoading(false);
      }
    },
  });

  const handleModeChange = (newMode: AuthMode) => {
    setMode(newMode);
    setError(null);
    setSuccess(null);
    form.reset();
  };

  return (
    <div className="flex flex-col items-center justify-center min-h-screen w-screen bg-background">
      <div className="mb-4 text-sm text-gray-500">
        Environment: <span className="font-semibold">{envConfig.env}</span> |
        API: <span className="font-semibold">{envConfig.bffApiUrl}</span>
      </div>

      <h1 className="text-3xl font-bold mb-2">MarketSpace</h1>
      <p className="text-gray-600 mb-8">
        {mode === "login" ? "Login to your account" : "Create a new account"}
      </p>

      <form
        onSubmit={(e) => {
          e.preventDefault();
          form.handleSubmit();
        }}
        className="space-y-6 w-full max-w-sm bg-card p-8 rounded-lg shadow-lg"
      >
        {error && (
          <div className="p-4 bg-red-100 border border-red-400 text-red-700 rounded-md text-sm">
            {error}
          </div>
        )}

        {success && (
          <div className="p-4 bg-green-100 border border-green-400 text-green-700 rounded-md text-sm">
            {success}
          </div>
        )}

        <form.Field
          name="email"
          children={(field) => (
            <div className="space-y-2">
              <label htmlFor={field.name} className="block text-sm font-medium">
                Email
              </label>
              <Input
                id={field.name}
                type="email"
                value={field.state.value}
                onChange={(e) => field.handleChange(e.target.value)}
                onBlur={field.handleBlur}
                placeholder="Enter your email"
                autoComplete="email"
                disabled={isLoading}
                className={
                  field.state.meta.errors.length > 0
                    ? "border-red-500 focus:border-red-500"
                    : ""
                }
              />
              {field.state.meta.errors.length > 0 && (
                <p className="text-sm text-red-600">
                  {String(field.state.meta.errors[0])}
                </p>
              )}
            </div>
          )}
        />

        {mode === "register" && (
          <form.Field
            name="name"
            children={(field) => (
              <div className="space-y-2">
                <label
                  htmlFor={field.name}
                  className="block text-sm font-medium"
                >
                  Full Name
                </label>
                <Input
                  id={field.name}
                  type="text"
                  value={field.state.value}
                  onChange={(e) => field.handleChange(e.target.value)}
                  onBlur={field.handleBlur}
                  placeholder="Enter your full name"
                  autoComplete="name"
                  disabled={isLoading}
                  className={
                    field.state.meta.errors.length > 0
                      ? "border-red-500 focus:border-red-500"
                      : ""
                  }
                />
                {field.state.meta.errors.length > 0 && (
                  <p className="text-sm text-red-600">
                    {String(field.state.meta.errors[0])}
                  </p>
                )}
              </div>
            )}
          />
        )}

        <form.Field
          name="password"
          children={(field) => (
            <div className="space-y-2">
              <label htmlFor={field.name} className="block text-sm font-medium">
                Password
              </label>
              <Input
                id={field.name}
                type="password"
                value={field.state.value}
                onChange={(e) => field.handleChange(e.target.value)}
                onBlur={field.handleBlur}
                placeholder="Enter your password"
                autoComplete={
                  mode === "login" ? "current-password" : "new-password"
                }
                disabled={isLoading}
                className={
                  field.state.meta.errors.length > 0
                    ? "border-red-500 focus:border-red-500"
                    : ""
                }
              />
              {field.state.meta.errors.length > 0 && (
                <p className="text-sm text-red-600">
                  {String(field.state.meta.errors[0])}
                </p>
              )}
            </div>
          )}
        />

        {mode === "register" && (
          <form.Field
            name="confirmPassword"
            children={(field) => (
              <div className="space-y-2">
                <label
                  htmlFor={field.name}
                  className="block text-sm font-medium"
                >
                  Confirm Password
                </label>
                <Input
                  id={field.name}
                  type="password"
                  value={field.state.value}
                  onChange={(e) => field.handleChange(e.target.value)}
                  onBlur={field.handleBlur}
                  placeholder="Confirm your password"
                  autoComplete="new-password"
                  disabled={isLoading}
                  className={
                    field.state.meta.errors.length > 0
                      ? "border-red-500 focus:border-red-500"
                      : ""
                  }
                />
                {field.state.meta.errors.length > 0 && (
                  <p className="text-sm text-red-600">
                    {String(field.state.meta.errors[0])}
                  </p>
                )}
              </div>
            )}
          />
        )}

        <Button type="submit" className="w-full" disabled={isLoading}>
          {isLoading
            ? "Loading..."
            : mode === "login"
              ? "Login"
              : "Create Account"}
        </Button>

        <div className="text-center text-sm">
          {mode === "login" ? (
            <>
              Don't have an account?{" "}
              <button
                type="button"
                onClick={() => handleModeChange("register")}
                className="text-blue-600 hover:underline font-medium"
                disabled={isLoading}
              >
                Register here
              </button>
            </>
          ) : (
            <>
              Already have an account?{" "}
              <button
                type="button"
                onClick={() => handleModeChange("login")}
                className="text-blue-600 hover:underline font-medium"
                disabled={isLoading}
              >
                Login here
              </button>
            </>
          )}
        </div>
      </form>
    </div>
  );
}
