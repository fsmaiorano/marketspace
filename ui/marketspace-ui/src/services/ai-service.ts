import { apiClient } from "@/lib/api";

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

// userId is NOT sent from the client — the BFF injects it from the authenticated JWT token.
export const sendChatMessage = async (message: string): Promise<ChatResponse> => {
  const response = await apiClient.post<BffResult<ChatResponse>>("/api/ai/chat", { message });
  if (response.data?.data) return response.data.data;
  const err = response.data?.error ?? response.statusText ?? "Unknown error from AI chat";
  throw new Error(typeof err === "string" ? err : JSON.stringify(err));
};

export const sendAgentMessage = async (message: string): Promise<AgentResponse> => {
  const response = await apiClient.post<BffResult<AgentResponse>>("/api/ai/agent", { message });
  if (response.data?.data) return response.data.data;
  const err = response.data?.error ?? response.statusText ?? "Unknown error from AI agent";
  throw new Error(typeof err === "string" ? err : JSON.stringify(err));
};
