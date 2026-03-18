import { apiClient } from "@/lib/api";
import axios from "axios";

interface BffResult<T> {
  isSuccess: boolean;
  error?: string;
  data?: T;
}

export interface ChatResponse {
  answer: string;
}

export interface AgentResponse {
  answer: string;
  usedTools: boolean;
}

function extractErrorMessage(err: unknown, fallback: string): string {
  if (axios.isAxiosError(err)) {
    // Try to extract detail from ProblemDetails (503) or error body
    const data = err.response?.data;
    if (typeof data === "string" && data.length > 0) return data;
    if (data?.detail) return data.detail;
    if (data?.title) return data.title;
    if (data?.error) return typeof data.error === "string" ? data.error : JSON.stringify(data.error);
    if (err.response?.statusText) return `${err.response.status}: ${err.response.statusText}`;
  }
  if (err instanceof Error) return err.message;
  return fallback;
}

// userId is NOT sent from the client — the BFF injects it from the authenticated JWT token.
export const sendChatMessage = async (message: string): Promise<ChatResponse> => {
  try {
    const response = await apiClient.post<BffResult<ChatResponse>>("/api/ai/chat", { message });
    if (response.data?.data) return response.data.data;
    const err = response.data?.error ?? response.statusText ?? "Unknown error from AI chat";
    throw new Error(typeof err === "string" ? err : JSON.stringify(err));
  } catch (err) {
    throw new Error(extractErrorMessage(err, "AI chat is currently unavailable. Please try again."));
  }
};

export const sendAgentMessage = async (message: string): Promise<AgentResponse> => {
  try {
    const response = await apiClient.post<BffResult<AgentResponse>>("/api/ai/agent", { message });
    if (response.data?.data) return response.data.data;
    const err = response.data?.error ?? response.statusText ?? "Unknown error from AI agent";
    throw new Error(typeof err === "string" ? err : JSON.stringify(err));
  } catch (err) {
    throw new Error(extractErrorMessage(err, "AI agent is currently unavailable. Please try again."));
  }
};
