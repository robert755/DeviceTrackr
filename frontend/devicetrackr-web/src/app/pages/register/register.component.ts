import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { RegisterRequest } from '../../models/user';
import { DeviceApiService } from '../../services/device-api.service';
import { AuthSessionService } from '../../services/auth-session.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html'
})
export class RegisterComponent {
  errorMessage = '';

  readonly form = this.fb.group({
    name: [''],
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
    role: ['', Validators.required],
    location: ['', Validators.required]
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
      this.errorMessage = 'Fill in all required fields.';
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const payload: RegisterRequest = {
      email: (raw.email ?? '').trim(),
      password: (raw.password ?? '').trim(),
      name: (raw.name ?? '').trim(),
      role: (raw.role ?? '').trim(),
      location: (raw.location ?? '').trim()
    };

    this.api.register(payload).subscribe({
      next: (user) => {
        this.session.setUser(user);
        void this.router.navigate(['/devices']);
      },
      error: () => (this.errorMessage = 'Registration failed.')
    });
  }
}
