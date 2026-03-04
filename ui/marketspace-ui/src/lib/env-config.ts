type Environment = "local" | "aspire" | "docker";

interface EnvironmentConfig {
  env: Environment;
  userApiUrl: string;
  bffApiUrl: string;
  isDevelopment: boolean;
}

const getEnvironmentConfig = (): EnvironmentConfig => {
  const env = (import.meta.env.VITE_ENV || "local") as Environment;
  const userApiUrl =
    import.meta.env.VITE_USER_API_URL || "https://localhost:6066";
  const bffApiUrl =
    import.meta.env.VITE_BFF_API_URL || "https://localhost:5066";

  return {
    env,
    userApiUrl,
    bffApiUrl,
    isDevelopment: import.meta.env.DEV,
  };
};

export const envConfig = getEnvironmentConfig();
