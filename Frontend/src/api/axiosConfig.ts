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
});

// Request interceptor - add auth token to requests
api.interceptors.request.use(
  (config) => {
    // Debug logging
    console.log('=== AXIOS REQUEST ===');
    console.log('BaseURL:', config.baseURL);
    console.log('URL:', config.url);
    console.log('Full URL:', `${config.baseURL}${config.url}`);
    console.log('Method:', config.method);
    console.log('Headers:', config.headers);

    // Get token from localStorage or your auth store
    const token = localStorage.getItem('auth_token');

    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
  },
  (error) => {
    console.error('Request error:', error);
    return Promise.reject(error);
  }
);

// Response interceptor - handle errors globally
api.interceptors.response.use(
  (response) => {
    console.log('=== AXIOS RESPONSE SUCCESS ===');
    console.log('Status:', response.status);
    console.log('Data:', response.data);
    return response;
  },
  (error) => {
    console.log('=== AXIOS ERROR ===');
    console.log('Error object:', error);
    console.log('Error message:', error.message);
    console.log('Error code:', error.code);
    console.log('Error config:', error.config);
    console.log('Error request:', error.request);
    console.log('Error response:', error.response);

    // Handle different error cases
    if (error.response) {
      // Server responded with error status
      const { status, data } = error.response;
      console.error('Server responded with error status:', status);

      switch (status) {
        case 401:
          // Unauthorized - clear token and redirect to login
          localStorage.removeItem('auth_token');
          window.location.href = '/login';
          break;
        case 403:
          console.error('Forbidden:', data.message);
          break;
        case 404:
          console.error('Not found:', data.message);
          break;
        case 500:
          console.error('Server error:', data.message);
          break;
        default:
          console.error('API error:', data.message || error.message);
      }
    } else if (error.request) {
      // Request made but no response received
      console.error('Network error: No response from server');
      console.error('Request details:', error.request);
    } else {
      // Something else happened
      console.error('Error:', error.message);
    }

    return Promise.reject(error);
  }
);

export default api;
