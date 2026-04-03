import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Device, DevicePayload } from '../models/device';
import {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  User
} from '../models/user';

@Injectable({ providedIn: 'root' })
export class DeviceApiService {
  private readonly baseUrl = 'http://localhost:5035/api';

  constructor(private readonly http: HttpClient) {}

  getDevices(): Observable<Device[]> {
    return this.http.get<Device[]>(`${this.baseUrl}/devices`);
  }

  getDevice(id: number): Observable<Device> {
    return this.http.get<Device>(`${this.baseUrl}/devices/${id}`);
  }

  createDevice(payload: DevicePayload): Observable<Device> {
    return this.http.post<Device>(`${this.baseUrl}/devices`, payload);
  }

  updateDevice(id: number, payload: DevicePayload): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/devices/${id}`, payload);
  }

  deleteDevice(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/devices/${id}`);
  }

  getUsers(): Observable<User[]> {
    return this.http.get<User[]>(`${this.baseUrl}/users`);
  }

  register(payload: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/auth/register`, payload);
  }

  login(payload: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/auth/login`, payload);
  }

  assignDevice(deviceId: number, userId: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/devices/${deviceId}/assign`, { userId });
  }

  unassignDevice(deviceId: number, userId: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/devices/${deviceId}/unassign`, { userId });
  }
}
