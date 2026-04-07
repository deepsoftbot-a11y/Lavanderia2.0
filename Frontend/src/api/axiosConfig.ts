import axios from 'axios';
import { API_BASE_URL } from '@/shared/utils/constants';

// Create axios instance with default configuration
const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000,
  withCredentials: false,
  headers: {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
  },
  // ASP.NET Core espera arrays sin corchetes: paymentStatuses=a&paymentStatuses=b
  // Axios por defecto usa paymentStatuses[]=a, que el model binder no reconoce
  paramsSerializer: (params) => {
    const parts: string[] = [];
    for (const key of Object.keys(params)) {
      const value = params[key];
      if (value === undefined || value === null) continue;
      if (Array.isArray(value)) {
        for (const item of value) {
          parts.push(`${encodeURIComponent(key)}=${encodeURIComponent(item)}`);
        }
      } else {
        parts.push(`${encodeURIComponent(key)}=${encodeURIComponent(value)}`);
      }
    }
    return parts.join('&');
  },
});

const isDev = import.meta.env.DEV;

// Request interceptor - add auth token to requests
api.interceptors.request.use(
  (config) => {
    if (isDev) {
      console.log(`[API] ${config.method?.toUpperCase()} ${config.baseURL}${config.url}`);
    }

    const token = localStorage.getItem('auth_token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
  },
  (error) => {
    if (isDev) console.error('[API] Request error:', error);
    return Promise.reject(error);
  }
);

// Response interceptor - handle errors globally
api.interceptors.response.use(
  (response) => {
    if (isDev) {
      console.log(`[API] ${response.status} ${response.config.url}`);
    }
    return response;
  },
  (error) => {
    if (error.response) {
      const { status, data } = error.response;

      switch (status) {
        case 401:
          localStorage.removeItem('auth_token');
          window.location.href = '/login';
          break;
        case 403:
          if (isDev) console.warn('[API] 403 Forbidden:', data?.message);
          break;
        case 429:
          if (isDev) console.warn('[API] 429 Too Many Requests');
          break;
        case 500:
          if (isDev) console.error('[API] 500 Server error:', data?.message);
          break;
        default:
          if (isDev) console.error(`[API] ${status}:`, data?.message || error.message);
      }
    } else if (error.request) {
      if (isDev) console.error('[API] Network error: no response from server');
    } else {
      if (isDev) console.error('[API] Error:', error.message);
    }

    // Extraer el mensaje del backend si existe, si no usar el mensaje de Axios
    const backendMessage = error.response?.data?.message ?? error.message;
    return Promise.reject(new Error(backendMessage));
  }
);

export default api;
