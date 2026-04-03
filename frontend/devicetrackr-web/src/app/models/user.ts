export interface User {
  id: number;
  name: string;
  email: string;
  role: string;
  location: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  name: string;
  role: string;
  location: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  userId: number;
  name: string;
  email: string;
}
