// Environment configuration that can be overridden at runtime
export interface AppConfig {
  production: boolean;
  apiUrl: string;
  keycloakUrl?: string;
}

// Default configuration
export const environment: AppConfig = {
  production: true,
  // Try to get from window object (can be injected by server) or use default
  apiUrl: (typeof window !== 'undefined' && (window as any).ENV?.API_URL) || 'http://localhost:5001',
  keycloakUrl: (typeof window !== 'undefined' && (window as any).ENV?.KEYCLOAK_URL) || 'http://localhost:7005',
};

