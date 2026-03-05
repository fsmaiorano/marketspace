import { useState } from "react";
import { useForm } from "@tanstack/react-form";
import { z } from "zod";
import { useAuthStore } from "@/stores";

const contactSchema = z.object({
  name: z.string().min(1, "Name is required"),
  email: z.string().email("Invalid email"),
  subject: z.string().min(5, "Subject must be at least 5 characters"),
  message: z.string().min(10, "Message must be at least 10 characters"),
});

export function ContactFormExample() {
  const { setLoading, setError, isLoading } = useAuthStore();
  const [success, setSuccess] = useState(false);

  const form = useForm({
    defaultValues: {
      name: "",
      email: "",
      subject: "",
      message: "",
    },
    onSubmit: async (values) => {
      setError(null);
      setSuccess(false);
      setLoading(true);

      try {
        const result = contactSchema.safeParse(values);
        if (!result.success) {
          const issue = result.error.issues[0];
          setError(issue.message);
          setLoading(false);
          return;
        }

        // Simulate API call
        await new Promise((resolve) => setTimeout(resolve, 1000));

        setSuccess(true);
        form.reset();
      } catch (error) {
        setError("Failed to send message");
      } finally {
        setLoading(false);
      }
    },
  });

  return (
    <form
      onSubmit={(e) => {
        e.preventDefault();
        form.handleSubmit();
      }}
      className="space-y-4"
    >
      {success && (
        <div className="p-4 bg-green-100 border border-green-400 text-green-700 rounded">
          Message sent successfully!
        </div>
      )}

      <form.Field
        name="name"
        children={(field) => (
          <div className="space-y-1">
            <label className="block text-sm font-medium">Name</label>
            <input
              type="text"
              value={field.state.value}
              onChange={(e) => field.handleChange(e.target.value)}
              onBlur={field.handleBlur}
              placeholder="Your name"
              className="w-full border rounded px-3 py-2"
            />
            {field.state.meta.errors.length > 0 && (
              <p className="text-red-600 text-sm">
                {String(field.state.meta.errors[0])}
              </p>
            )}
          </div>
        )}
      />

      <form.Field
        name="email"
        children={(field) => (
          <div className="space-y-1">
            <label className="block text-sm font-medium">Email</label>
            <input
              type="email"
              value={field.state.value}
              onChange={(e) => field.handleChange(e.target.value)}
              onBlur={field.handleBlur}
              placeholder="your@email.com"
              className="w-full border rounded px-3 py-2"
            />
            {field.state.meta.errors.length > 0 && (
              <p className="text-red-600 text-sm">
                {String(field.state.meta.errors[0])}
              </p>
            )}
          </div>
        )}
      />

      <form.Field
        name="subject"
        children={(field) => (
          <div className="space-y-1">
            <label className="block text-sm font-medium">Subject</label>
            <input
              type="text"
              value={field.state.value}
              onChange={(e) => field.handleChange(e.target.value)}
              onBlur={field.handleBlur}
              placeholder="Message subject"
              className="w-full border rounded px-3 py-2"
            />
            {field.state.meta.errors.length > 0 && (
              <p className="text-red-600 text-sm">
                {String(field.state.meta.errors[0])}
              </p>
            )}
          </div>
        )}
      />

      <form.Field
        name="message"
        children={(field) => (
          <div className="space-y-1">
            <label className="block text-sm font-medium">Message</label>
            <textarea
              value={field.state.value}
              onChange={(e) => field.handleChange(e.target.value)}
              onBlur={field.handleBlur}
              placeholder="Your message..."
              className="w-full border rounded px-3 py-2 h-32"
            />
            {field.state.meta.errors.length > 0 && (
              <p className="text-red-600 text-sm">
                {String(field.state.meta.errors[0])}
              </p>
            )}
          </div>
        )}
      />

      <button
        type="submit"
        disabled={isLoading}
        className="w-full bg-blue-600 text-white py-2 rounded hover:bg-blue-700 disabled:opacity-50"
      >
        {isLoading ? "Sending..." : "Send Message"}
      </button>
    </form>
  );
}
