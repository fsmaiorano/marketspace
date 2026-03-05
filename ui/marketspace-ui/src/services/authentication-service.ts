import {apiClient} from "@/lib/api.ts";

export type UserType = "Customer" | "Merchant";

export interface LoginRequest {
    email: string;
    password: string;
    userType?: number;
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
    userType: number;
}

interface LoginRequestBody {
    Email: string;
    Password: string;
    UserType?: number;
}

interface RegisterRequestBody {
    Email: string;
    Password: string;
    Name: string;
    UserName: string;
    UserType: number;
}

interface RefreshRequestBody {
    AccessToken: string;
    RefreshToken: string;
}

export const isAuthenticated = (): boolean =>
    localStorage.getItem("token") !== null;

export const getToken = (): string | null => localStorage.getItem("token");

export const getUserType = (): UserType | null => {
    const userType = localStorage.getItem("userType");
    return (userType as UserType) || null;
};

export const logout = (): void => {
    const token = localStorage.getItem("token");
    const refreshToken = localStorage.getItem("refreshToken");

    localStorage.removeItem("token");
    localStorage.removeItem("refreshToken");
    localStorage.removeItem("userType");

    if (token && refreshToken) {
        const requestBody: RefreshRequestBody = {
            AccessToken: token,
            RefreshToken: refreshToken,
        };
        apiClient.post("/api/user/revoke", requestBody).catch(() => {
            // Ignore errors on logout
        });
    }
};

export const login = async (
    credentials: LoginRequest,
): Promise<LoginResponse> => {
    try {
        const requestBody: LoginRequestBody = {
            Email: credentials.email,
            Password: credentials.password,
        };

        if (credentials.userType !== undefined) {
            requestBody.UserType = credentials.userType;
        }

        const response = await apiClient.post<LoginResponse>(
            "/api/auth/login",
            requestBody,
        );
        const {accessToken, refreshToken} = response.data;

        console.log('[Login] Storing token:', accessToken ? `${accessToken.substring(0, 20)}...` : 'NO TOKEN');
        localStorage.setItem("token", accessToken);
        localStorage.setItem("refreshToken", refreshToken);
        console.log('[Login] Token stored successfully');
        console.log('[Login] Token in localStorage:', localStorage.getItem('token') ? `${localStorage.getItem('token')!.substring(0, 20)}...` : 'NO TOKEN');

        return response.data;
    } catch (error) {
        console.error("Login failed:", error);
        throw error;
    }
};

export const register = async (
    data: RegisterRequest,
): Promise<LoginResponse> => {
    try {
        const requestBody: RegisterRequestBody = {
            Email: data.email,
            Password: data.password,
            Name: data.name,
            UserName: data.username || data.email,
            UserType: data.userType,
        };

        const response = await apiClient.post<LoginResponse>(
            "/api/auth/register",
            requestBody,
        );
        const {accessToken, refreshToken} = response.data;

        console.log('[Register] Storing token:', accessToken ? `${accessToken.substring(0, 20)}...` : 'NO TOKEN');
        localStorage.setItem("token", accessToken);
        localStorage.setItem("refreshToken", refreshToken);
        console.log('[Register] Token stored successfully');
        console.log('[Register] Token in localStorage:', localStorage.getItem('token') ? `${localStorage.getItem('token')!.substring(0, 20)}...` : 'NO TOKEN');

        const userTypeStr = data.userType === 0 ? "Customer" : "Merchant";
        localStorage.setItem("userType", userTypeStr);

        return response.data;
    } catch (error) {
        console.error("Registration failed:", error);
        throw error;
    }
};

export const refreshToken = async (): Promise<string> => {
    try {
        const currentToken = localStorage.getItem("token");
        const currentRefreshToken = localStorage.getItem("refreshToken");

        if (!currentToken || !currentRefreshToken) {
            throw new Error("No tokens available to refresh");
        }

        const requestBody: RefreshRequestBody = {
            AccessToken: currentToken,
            RefreshToken: currentRefreshToken,
        };

        const response = await apiClient.post<LoginResponse>(
            "/api/auth/refresh",
            requestBody,
        );
        const {accessToken, refreshToken: newRefreshToken} = response.data;

        localStorage.setItem("token", accessToken);
        localStorage.setItem("refreshToken", newRefreshToken);
        return accessToken;
    } catch (error) {
        console.error("Token refresh failed:", error);
        localStorage.removeItem("token");
        localStorage.removeItem("refreshToken");
        localStorage.removeItem("userType");
        throw error;
    }
};

export const getMe = async (): Promise<any> => {
    try {
        const token = localStorage.getItem('token');
        console.log('[getMe] Token from localStorage:', token ? `${token.substring(0, 20)}...` : 'NO TOKEN');
        console.log('[getMe] Calling userClient.get("/api/auth/me")');
        const response = await apiClient.get("/api/auth/me");
        console.log('[getMe] Response received:', response.data);
        return response.data;
    } catch (error) {
        console.error("Failed to fetch user profile:", error);
        throw error;
    }
};
