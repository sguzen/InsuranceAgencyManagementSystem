// src/services/authService.ts
import axios from 'axios';
import jwtDecode from 'jwt-decode';

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  permissions: string[];
  token: string;
  refreshToken: string;
}

interface LoginResponse {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  jwtToken: string;
  refreshToken: string;
  roles: string[];
  permissions: string[];
}

interface RefreshTokenRequest {
  refreshToken: string;
}

const TOKEN_KEY = 'auth_token';
const REFRESH_TOKEN_KEY = 'refresh_token';
const USER_KEY = 'user_info';

class AuthService {
  private apiUrl = '/api/auth';
  private user: User | null = null;
  private tokenExpiryTimer: any = null;

  constructor() {
    this.loadUserFromStorage();
    this.setupAxiosInterceptors();
  }

  // Load user from local storage
  private loadUserFromStorage(): void {
    const storedUser = localStorage.getItem(USER_KEY);
    const storedToken = localStorage.getItem(TOKEN_KEY);
    const storedRefreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);

    if (storedUser && storedToken && storedRefreshToken) {
      this.user = JSON.parse(storedUser);
      this.user.token = storedToken;
      this.user.refreshToken = storedRefreshToken;

      // Set up token expiry timer
      this.setupTokenExpiryTimer();
    }
  }

  // Save user to local storage
  private saveUserToStorage(user: User): void {
    localStorage.setItem(TOKEN_KEY, user.token);
    localStorage.setItem(REFRESH_TOKEN_KEY, user.refreshToken);
    
    const userInfo = { ...user };
    delete userInfo.token;
    delete userInfo.refreshToken;
    
    localStorage.setItem(USER_KEY, JSON.stringify(userInfo));
  }

  // Clear user from local storage
  private clearUserFromStorage(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  }

  // Set up axios interceptors for token refresh
  private setupAxiosInterceptors(): void {
    axios.interceptors.request.use(
      config => {
        const token = localStorage.getItem(TOKEN_KEY);
        if (token) {
          config.headers['Authorization'] = `Bearer ${token}`;
        }
        return config;
      },
      error => {
        return Promise.reject(error);
      }
    );

    axios.interceptors.response.use(
      response => response,
      async error => {
        const originalRequest = error.config;

        // If the error is not 401 or it's already retried, reject
        if (error.response?.status !== 401 || originalRequest._retry) {
          return Promise.reject(error);
        }

        originalRequest._retry = true;

        try {
          // Try to refresh the token
          const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
          if (!refreshToken) {
            this.logout();
            return Promise.reject(error);
          }

          const response = await this.refreshToken();
          
          if (response) {
            // Update the token in the current request
            originalRequest.headers['Authorization'] = `Bearer ${response.token}`;
            return axios(originalRequest);
          } else {
            this.logout();
            return Promise.reject(error);
          }
        } catch (refreshError) {
          this.logout();
          return Promise.reject(error);
        }
      }
    );
  }

  // Set up token expiry timer
  private setupTokenExpiryTimer(): void {
    if (this.tokenExpiryTimer) {
      clearTimeout(this.tokenExpiryTimer);
    }

    const token = localStorage.getItem(TOKEN_KEY);
    if (!token) return;

    try {
      const decodedToken: any = jwtDecode(token);
      const expiryTime = decodedToken.exp * 1000; // Convert to milliseconds
      const currentTime = Date.now();
      
      // Time until expiry in milliseconds
      const timeUntilExpiry = expiryTime - currentTime;
      
      // Refresh the token 5 minutes before it expires
      const refreshTime = Math.max(timeUntilExpiry - 5 * 60 * 1000, 0);
      
      this.tokenExpiryTimer = setTimeout(() => {
        this.refreshToken();
      }, refreshTime);
    } catch (error) {
      console.error('Error decoding token:', error);
    }
  }

  // Login user
  async login(email: string, password: string): Promise<User | null> {
    try {
      const response = await axios.post<LoginResponse>(`${this.apiUrl}/login`, {
        email,
        password
      });

      const userData = response.data;
      
      const user: User = {
        id: userData.userId,
        email: userData.email,
        firstName: userData.firstName,
        lastName: userData.lastName,
        roles: userData.roles,
        permissions: userData.permissions,
        token: userData.jwtToken,
        refreshToken: userData.refreshToken
      };

      this.user = user;
      this.saveUserToStorage(user);
      this.setupTokenExpiryTimer();

      return user;
    } catch (error) {
      console.error('Login error:', error);
      return null;
    }
  }

  // Refresh token
  async refreshToken(): Promise<User | null> {
    try {
      const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
      
      if (!refreshToken) {
        this.logout();
        return null;
      }

      const response = await axios.post<LoginResponse>(`${this.apiUrl}/refresh-token`, {
        refreshToken
      } as RefreshTokenRequest);

      const userData = response.data;
      
      const user: User = {
        id: userData.userId,
        email: userData.email,
        firstName: userData.firstName,
        lastName: userData.lastName,
        roles: userData.roles,
        permissions: userData.permissions,
        token: userData.jwtToken,
        refreshToken: userData.refreshToken
      };

      this.user = user;
      this.saveUserToStorage(user);
      this.setupTokenExpiryTimer();

      return user;
    } catch (error) {
      console.error('Token refresh error:', error);
      this.logout();
      return null;
    }
  }

  // Logout user
  async logout(): Promise<void> {
    if (this.user) {
      try {
        await axios.post(`${this.apiUrl}/logout`);
      } catch (error) {
        console.error('Logout error:', error);
      }
    }

    this.user = null;
    this.clearUserFromStorage();
    
    if (this.tokenExpiryTimer) {
      clearTimeout(this.tokenExpiryTimer);
      this.tokenExpiryTimer = null;
    }
    
    // Redirect to login page or reload
    window.location.href = '/login';
  }

  // Check if user has role
  hasRole(role: string): boolean {
    return this.user?.roles.includes(role) ?? false;
  }

  // Check if user has any of the roles
  hasAnyRole(roles: string[]): boolean {
    return this.user?.roles.some(role => roles.includes(role)) ?? false;
  }

  // Check if user has permission
  hasPermission(permission: string): boolean {
    return this.user?.permissions.includes(permission) ?? false;
  }

  // Check if user has any of the permissions
  hasAnyPermission(permissions: string[]): boolean {
    return this.user?.permissions.some(perm => permissions.includes(perm)) ?? false;
  }

  // Get current user
  getCurrentUser(): User | null {
    return this.user;
  }

  // Check if user is authenticated
  isAuthenticated(): boolean {
    return !!this.user;
  }
}

export const authService = new AuthService();
export default authService;