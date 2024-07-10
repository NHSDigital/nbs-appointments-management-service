export interface AppConfiguration {
    apiUrl: string;
  }
  
  const defaultConfiguration: AppConfiguration = {
    apiUrl: process.env.NEXT_PUBLIC_API_URL || "http://localhost:7071/api"
  };
  
  export const getConfig = (): AppConfiguration => {
    return defaultConfiguration;
  };

  export const getApiUrl = (path: string): string => {
    const { apiUrl } = getConfig();
    return `${apiUrl}/${path}`;
  };