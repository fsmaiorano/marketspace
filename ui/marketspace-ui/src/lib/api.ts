import axios from 'axios';
import type {AxiosInstance} from 'axios';
import {envConfig} from './env-config';

const apiClient: AxiosInstance = axios.create({
    baseURL: envConfig.bffApiUrl,
    headers: {
        'Content-Type': 'application/json',
    },
});

apiClient.interceptors.request.use((config) => {
    debugger;
    const token = localStorage.getItem('token');
    console.log('[apiClient] Token from localStorage:', token ? `${token.substring(0, 20)}...` : 'NO TOKEN');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
        console.log('[apiClient] Authorization header set:', `Bearer ${token.substring(0, 20)}...`);
    } else {
        console.warn('[apiClient] No token found in localStorage');
    }
    console.log('[apiClient] Request URL:', config.url);
    console.log('[apiClient] Request headers:', config.headers);
    return config;
});

apiClient.interceptors.response.use(
    (response) => {
        console.log('[apiClient] Response status:', response.status, 'URL:', response.config.url);
        return response.data;
    },
    (error) => {
        console.error('[apiClient] Response error:', error.response?.status, 'URL:', error.config?.url, 'Data:', error.response?.data);
        if (error.response?.status === 401) {
            localStorage.removeItem('token');
            window.location.href = '/';
        }
        return Promise.reject(error);
    }
);

export {apiClient};




