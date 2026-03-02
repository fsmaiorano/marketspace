import apiClient from '../lib/api-client';

export type UserType = 'Customer' | 'Merchant';

export interface LoginRequest {
    email: string;
    password: string;
}

export interface LoginResponse {
    accessToken: string;
    accessTokenExpiration: string;
    refreshToken: string;
    refreshTokenExpiration: string;
}

export interface RegisterRequest {
    email: string;
    password: string;
    name: string;
    username?: string;
    userType: UserType;
}

export const isAuthenticated = (): boolean => localStorage.getItem('token') !== null;

export const getToken = (): string | null => localStorage.getItem('token');

export const getUserType = (): UserType | null => {
    const userType = localStorage.getItem('userType');
    return (userType as UserType) || null;
};

export const logout = (): void => {
    const token = localStorage.getItem('token');
    const refreshToken = localStorage.getItem('refreshToken');

    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('userType');

    if (token && refreshToken) {
        const requestBody = {
            AccessToken: token,
            RefreshToken: refreshToken,
        };
        apiClient.post('/api/auth/revoke', requestBody).catch(() => {
            // Ignore errors on logout
        });
    }
};

export const login = async (credentials: LoginRequest): Promise<LoginResponse> => {
    try {
        const requestBody = {
            Email: credentials.email,
            Password: credentials.password,
        };

        const response = await apiClient.post<LoginResponse>('/api/auth/login', requestBody);
        const {accessToken, refreshToken} = response.data;

        localStorage.setItem('token', accessToken);
        localStorage.setItem('refreshToken', refreshToken);

        return response.data;
    } catch (error) {
        console.error('Login failed:', error);
        throw error;
    }
};

export const register = async (data: RegisterRequest): Promise<LoginResponse> => {
    try {
        const userTypeNumber = data.userType === 'Customer' ? 0 : 1;

        const requestBody = {
            Email: data.email,
            Password: data.password,
            Name: data.name,
            UserName: data.username || data.email,
            UserType: userTypeNumber,
        };

        const response = await apiClient.post<LoginResponse>('/api/auth/register', requestBody);
        const {accessToken, refreshToken} = response.data;

        localStorage.setItem('token', accessToken);
        localStorage.setItem('refreshToken', refreshToken);
        localStorage.setItem('userType', data.userType);

        return response.data;
    } catch (error) {
        console.error('Registration failed:', error);
        throw error;
    }
};

export const refreshToken = async (): Promise<string> => {
    try {
        const currentToken = localStorage.getItem('token');
        const currentRefreshToken = localStorage.getItem('refreshToken');

        if (!currentToken || !currentRefreshToken) {
            throw new Error('No tokens available to refresh');
        }

        const requestBody = {
            AccessToken: currentToken,
            RefreshToken: currentRefreshToken,
        };

        const response = await apiClient.post<LoginResponse>('/api/auth/refresh', requestBody);
        const {accessToken, refreshToken: newRefreshToken} = response.data;

        localStorage.setItem('token', accessToken);
        localStorage.setItem('refreshToken', newRefreshToken);
        return accessToken;
    } catch (error) {
        console.error('Token refresh failed:', error);
        localStorage.removeItem('token');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('userType');
        throw error;
    }
};




