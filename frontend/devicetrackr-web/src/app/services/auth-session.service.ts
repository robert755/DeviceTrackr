import { Injectable } from '@angular/core';
import { AuthResponse } from '../models/user';

@Injectable({ providedIn: 'root' })
export class AuthSessionService {
  private current?: AuthResponse;

  get user(): AuthResponse | undefined {
    return this.current;
  }

  setUser(user: AuthResponse | undefined): void {
    this.current = user;
  }
}
