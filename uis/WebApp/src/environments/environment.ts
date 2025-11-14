export const environment = {
  production: true,
  // Use HTTP port to avoid SSL certificate issues
  // When accessed from browser: use localhost:5001
  // When running in Docker: API calls should go through the server or use internal network
  apiUrl: 'http://localhost:5001',
};
