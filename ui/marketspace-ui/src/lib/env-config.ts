type Environment = 'local' | 'aspire' | 'docker';

interface EnvironmentConfig {
  env: Environment;
  bffApiUrl: string;
  isDevelopment: boolean;
}

const getEnvironmentConfig = (): EnvironmentConfig => {
  const env = (import.meta.env.VITE_ENV || 'local') as Environment;
  const bffApiUrl = import.meta.env.VITE_BFF_API_URL || 'http://localhost:4000';
  
  return {
    env,
    bffApiUrl,
    isDevelopment: import.meta.env.DEV,
  };
};

export const envConfig = getEnvironmentConfig();


