import { useState, useRef, useEffect } from "react";
import { Bot, Send, Wrench, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { sendChatMessage, sendAgentMessage } from "@/services/ai-service";

type Mode = "chat" | "agent";

interface Message {
  role: "user" | "assistant";
  content: string;
  usedTools?: boolean;
}

interface AiChatDrawerProps {
  open: boolean;
  onClose: () => void;
}

export function AiChatDrawer({ open, onClose }: AiChatDrawerProps) {
  const [mode, setMode] = useState<Mode>("chat");
  const [messages, setMessages] = useState<Message[]>([]);
  const [input, setInput] = useState("");
  const [loading, setLoading] = useState(false);
  const bottomRef = useRef<HTMLDivElement>(null);

  // Ref used to prevent re-entrant sends (double-click / Enter firing twice)
  const isSendingRef = useRef(false);
  // Ref to dedupe identical consecutive messages within short window
  const lastSentRef = useRef<{ text: string; ts: number } | null>(null);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  const handleSend = async () => {
    const text = input.trim();
    if (!text) return;

    // Prevent re-entrancy synchronously
    if (isSendingRef.current) return;

    // Dedupe: ignore if same message sent within 2 seconds
    const now = Date.now();
    if (lastSentRef.current && lastSentRef.current.text === text && now - lastSentRef.current.ts < 2000) {
      return;
    }

    isSendingRef.current = true;
    lastSentRef.current = { text, ts: now };

    const userMessage: Message = { role: "user", content: text };
    setMessages((prev) => [...prev, userMessage]);
    setInput("");
    setLoading(true);

    try {
      if (mode === "chat") {
        const res = await sendChatMessage(text);
        setMessages((prev) => [
          ...prev,
          { role: "assistant", content: res.answer ?? "No response." },
        ]);
      } else {
        const res = await sendAgentMessage(text);
        setMessages((prev) => [
          ...prev,
          {
            role: "assistant",
            content: res.answer ?? "No response.",
            usedTools: res.usedTools,
          },
        ]);
      }
    } catch (err) {
      const message = err instanceof Error ? err.message : String(err);
      console.error("AI send error:", err);
      setMessages((prev) => [
        ...prev,
        { role: "assistant", content: `Error: ${message}` },
      ]);
    } finally {
      setLoading(false);
      isSendingRef.current = false;
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  const handleModeChange = (next: Mode) => {
    setMode(next);
    setMessages([]);
  };

  if (!open) return null;

  return (
    <div className="fixed bottom-4 right-4 z-50 flex w-[360px] flex-col rounded-xl border bg-card shadow-2xl">
      {/* Header */}
      <div className="flex items-center justify-between rounded-t-xl border-b bg-card px-4 py-3">
        <div className="flex items-center gap-2">
          <Bot className="h-5 w-5 text-primary" />
          <span className="font-semibold text-sm">AI Assistant</span>
        </div>
        <Button variant="ghost" size="icon" className="h-7 w-7" onClick={onClose}>
          <X className="h-4 w-4" />
        </Button>
      </div>

      {/* Mode tabs */}
      <div className="flex border-b">
        <button
          className={`flex-1 py-2 text-xs font-medium transition-colors ${
            mode === "chat"
              ? "border-b-2 border-primary text-primary"
              : "text-muted-foreground hover:text-foreground"
          }`}
          onClick={() => handleModeChange("chat")}
        >
          Chat
        </button>
        <button
          className={`flex-1 py-2 text-xs font-medium transition-colors ${
            mode === "agent"
              ? "border-b-2 border-primary text-primary"
              : "text-muted-foreground hover:text-foreground"
          }`}
          onClick={() => handleModeChange("agent")}
        >
          Agent
        </button>
      </div>

      {/* Messages */}
      <div className="flex h-72 flex-col gap-3 overflow-y-auto p-4">
        {messages.length === 0 && (
          <p className="text-center text-xs text-muted-foreground pt-8">
            {mode === "chat"
              ? "Ask me anything about MarketSpace."
              : "I can use tools to help you with orders, products, and more."}
          </p>
        )}
        {messages.map((msg, i) => (
          <div
            key={i}
            className={`flex flex-col gap-1 ${msg.role === "user" ? "items-end" : "items-start"}`}
          >
            <div
              className={`max-w-[80%] rounded-lg px-3 py-2 text-xs leading-relaxed ${
                msg.role === "user"
                  ? "bg-primary text-primary-foreground"
                  : "bg-muted text-foreground"
              }`}
            >
              {msg.content}
            </div>
            {msg.usedTools && (
              <span className="flex items-center gap-1 text-[10px] text-muted-foreground">
                <Wrench className="h-3 w-3" /> Used tools
              </span>
            )}
          </div>
        ))}
        {loading && (
          <div className="flex items-start">
            <div className="rounded-lg bg-muted px-3 py-2 text-xs text-muted-foreground animate-pulse">
              Thinking…
            </div>
          </div>
        )}
        <div ref={bottomRef} />
      </div>

      {/* Input */}
      <div className="flex items-center gap-2 border-t p-3">
        <input
          type="text"
          value={input}
          onChange={(e) => setInput(e.target.value)}
          onKeyDown={handleKeyDown}
          placeholder="Type a message…"
          disabled={loading}
          className="flex-1 rounded-md border bg-background px-3 py-2 text-xs outline-none focus:ring-2 focus:ring-ring disabled:opacity-50"
        />
        <Button
          size="icon"
          className="h-8 w-8 shrink-0"
          disabled={loading || !input.trim()}
          onClick={handleSend}
        >
          <Send className="h-3.5 w-3.5" />
        </Button>
      </div>
    </div>
  );
}
