import axios from 'axios';
import type {AxiosInstance} from 'axios';
import {envConfig} from './env-config';

const userClient: AxiosInstance = axios.create({
    baseURL: envConfig.userApiUrl,
    headers: {
        'Content-Type': 'application/json',
    },
});

const apiClient: AxiosInstance = axios.create({
    baseURL: envConfig.bffApiUrl,
    headers: {
        'Content-Type': 'application/json',
    },
});

userClient.interceptors.request.use((config) => {
    const token = localStorage.getItem('token');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

userClient.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            localStorage.removeItem('token');
            window.location.href = '/';
        }
        return Promise.reject(error);
    }
);

apiClient.interceptors.request.use((config) => {
    const token = localStorage.getItem('token');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

apiClient.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            localStorage.removeItem('token');
            window.location.href = '/';
        }
        return Promise.reject(error);
    }
);

export {apiClient, userClient};




