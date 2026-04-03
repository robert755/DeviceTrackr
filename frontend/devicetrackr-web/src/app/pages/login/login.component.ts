import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { LoginRequest } from '../../models/user';
import { DeviceApiService } from '../../services/device-api.service';
import { AuthSessionService } from '../../services/auth-session.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  errorMessage = '';

  readonly form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  constructor(
    private readonly api: DeviceApiService,
    private readonly fb: FormBuilder,
    private readonly router: Router,
    private readonly session: AuthSessionService
  ) {}

  submit(): void {
    this.errorMessage = '';
    if (this.form.invalid) {
      this.errorMessage = 'Completează email și parola.';
      this.form.markAllAsTouched();
      return;
    }

    const payload = this.form.getRawValue() as LoginRequest;
    this.api.login(payload).subscribe({
      next: (user) => {
        this.session.setUser(user);
        void this.router.navigate(['/devices']);
      },
      error: () => (this.errorMessage = 'Autentificare eșuată.')
    });
  }
}
