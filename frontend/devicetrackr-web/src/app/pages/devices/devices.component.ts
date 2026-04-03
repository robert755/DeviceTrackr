import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Device, DevicePayload } from '../../models/device';
import { User } from '../../models/user';
import { DeviceApiService } from '../../services/device-api.service';
import { AuthSessionService } from '../../services/auth-session.service';

@Component({
  selector: 'app-devices',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './devices.component.html'
})
export class DevicesComponent implements OnInit {
  devices: Device[] = [];
  users: User[] = [];
  selectedDevice?: Device;
  loading = false;
  errorMessage = '';
  successMessage = '';
  isEditMode = false;
  generatingDescription = false;

  readonly deviceForm = this.fb.group({
    name: ['', Validators.required],
    manufacturer: ['', Validators.required],
    type: [1, Validators.required],
    operatingSystem: ['', Validators.required],
    osVersion: ['', Validators.required],
    processor: ['', Validators.required],
    ramAmountGb: [4, [Validators.required, Validators.min(1)]],
    description: ['']
  });

  constructor(
    private readonly api: DeviceApiService,
    private readonly fb: FormBuilder,
    private readonly router: Router,
    readonly session: AuthSessionService
  ) {}

  get currentUser() {
    return this.session.user;
  }

  ngOnInit(): void {
    this.loadAll();
  }

  logout(): void {
    this.session.setUser(undefined);
    void this.router.navigate(['/login']);
  }

  loadAll(): void {
    this.loading = true;
    this.errorMessage = '';
    this.api.getUsers().subscribe({
      next: (users) => (this.users = users),
      error: () => (this.errorMessage = 'Nu s-au putut încărca utilizatorii.')
    });

    this.api.getDevices().subscribe({
      next: (devices) => {
        this.devices = devices;
        const selId = this.selectedDevice?.id;
        if (selId !== undefined) {
          const fresh = devices.find((d) => d.id === selId);
          if (fresh) {
            this.selectedDevice = fresh;
            if (fresh.assignedUserId && this.isEditMode) {
              this.isEditMode = false;
              this.resetDeviceFormEmpty();
            }
          }
        }
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.errorMessage = 'Nu s-au putut încărca dispozitivele. Pornește API-ul.';
      }
    });
  }

  selectDevice(device: Device): void {
    this.selectedDevice = device;
    this.successMessage = '';
    this.errorMessage = '';
  }

  startCreate(): void {
    this.isEditMode = false;
    this.selectedDevice = undefined;
    this.successMessage = '';
    this.errorMessage = '';
    this.resetDeviceFormEmpty();
  }

  private resetDeviceFormEmpty(): void {
    this.deviceForm.reset({
      name: '',
      manufacturer: '',
      type: 1,
      operatingSystem: '',
      osVersion: '',
      processor: '',
      ramAmountGb: 4,
      description: ''
    });
  }

  /** Doar dispozitivele fără utilizator alocat pot fi modificate sau șterse. */
  canModifyDevice(device: Device): boolean {
    return !device.assignedUserId;
  }

  startEdit(device: Device): void {
    if (!this.canModifyDevice(device)) {
      this.errorMessage = 'Nu poți edita un dispozitiv deja alocat.';
      return;
    }
    this.isEditMode = true;
    this.selectedDevice = device;
    this.successMessage = '';
    this.errorMessage = '';
    this.deviceForm.patchValue({
      name: device.name,
      manufacturer: device.manufacturer,
      type: device.type,
      operatingSystem: device.operatingSystem,
      osVersion: device.osVersion,
      processor: device.processor,
      ramAmountGb: device.ramAmountGb,
      description: device.description
    });
  }

  saveDevice(): void {
    this.successMessage = '';
    this.errorMessage = '';
    if (this.deviceForm.invalid) {
      this.errorMessage = 'Completează câmpurile obligatorii (descrierea e opțională; o poți genera cu Gemini după salvare).';
      this.deviceForm.markAllAsTouched();
      return;
    }

    const formValue = this.deviceForm.getRawValue();
    const payload: DevicePayload = {
      name: (formValue.name ?? '').trim(),
      manufacturer: (formValue.manufacturer ?? '').trim(),
      type: Number(formValue.type),
      operatingSystem: (formValue.operatingSystem ?? '').trim(),
      osVersion: (formValue.osVersion ?? '').trim(),
      processor: (formValue.processor ?? '').trim(),
      ramAmountGb: Number(formValue.ramAmountGb),
      description: (formValue.description ?? '').trim()
    };
    const duplicate = this.devices.some(
      (d) =>
        d.name.trim().toLowerCase() === payload.name.trim().toLowerCase() &&
        (!this.isEditMode || d.id !== this.selectedDevice?.id)
    );
    if (duplicate) {
      this.errorMessage = 'Există deja un dispozitiv cu acest nume.';
      return;
    }

    if (!this.isEditMode) {
      this.api.createDevice(payload).subscribe({
        next: () => {
          this.successMessage = 'Dispozitiv adăugat.';
          this.startCreate();
          this.loadAll();
        },
        error: () => (this.errorMessage = 'Crearea a eșuat.')
      });
      return;
    }

    if (!this.selectedDevice) {
      this.errorMessage = 'Selectează un dispozitiv.';
      return;
    }

    if (this.selectedDevice.assignedUserId) {
      this.errorMessage = 'Dispozitivul este alocat; nu poate fi actualizat.';
      return;
    }

    this.api.updateDevice(this.selectedDevice.id, payload).subscribe({
      next: () => {
        this.successMessage = 'Dispozitiv actualizat.';
        this.loadAll();
      },
      error: () => (this.errorMessage = 'Actualizarea a eșuat.')
    });
  }

  deleteDevice(device: Device): void {
    if (!this.canModifyDevice(device)) {
      this.errorMessage = 'Nu poți șterge un dispozitiv alocat; dealocă-l mai întâi.';
      return;
    }
    this.successMessage = '';
    this.errorMessage = '';
    this.api.deleteDevice(device.id).subscribe({
      next: () => {
        this.successMessage = 'Dispozitiv șters.';
        if (this.selectedDevice?.id === device.id) {
          this.selectedDevice = undefined;
        }
        this.loadAll();
      },
      error: () => (this.errorMessage = 'Ștergerea a eșuat.')
    });
  }

  assignToMe(device: Device): void {
    this.successMessage = '';
    this.errorMessage = '';
    const user = this.currentUser;
    if (!user) {
      this.errorMessage = 'Trebuie să fii autentificat.';
      return;
    }

    this.api.assignDevice(device.id, user.userId).subscribe({
      next: () => {
        this.successMessage = 'Dispozitiv alocat ție.';
        if (this.selectedDevice?.id === device.id && this.isEditMode) {
          this.isEditMode = false;
          this.resetDeviceFormEmpty();
        }
        this.loadAll();
      },
      error: () => (this.errorMessage = 'Alocarea a eșuat.')
    });
  }

  unassignFromMe(device: Device): void {
    this.successMessage = '';
    this.errorMessage = '';
    const user = this.currentUser;
    if (!user) {
      this.errorMessage = 'Trebuie să fii autentificat.';
      return;
    }

    this.api.unassignDevice(device.id, user.userId).subscribe({
      next: () => {
        this.successMessage = 'Alocare anulată.';
        this.loadAll();
      },
      error: () => (this.errorMessage = 'Operațiunea a eșuat.')
    });
  }

  getAssignedUserText(device: Device): string {
    if (!device.assignedUser) {
      return 'Nealocat';
    }
    return `${device.assignedUser.name} (${device.assignedUser.location})`;
  }

  canUnassign(device: Device): boolean {
    return !!this.currentUser && device.assignedUserId === this.currentUser.userId;
  }

  canAssign(device: Device): boolean {
    return !!this.currentUser && !device.assignedUserId;
  }

  generateAiDescription(): void {
    const d = this.selectedDevice;
    if (!d) {
      return;
    }

    this.errorMessage = '';
    this.successMessage = '';
    this.generatingDescription = true;
    this.api.generateDeviceDescription(d.id).subscribe({
      next: (updated) => {
        this.generatingDescription = false;
        this.successMessage = 'Descriere generată cu Gemini și salvată.';
        this.selectedDevice = updated;
        if (this.isEditMode) {
          this.deviceForm.patchValue({ description: updated.description });
        }
        this.loadAll();
      },
      error: (err: HttpErrorResponse) => {
        this.generatingDescription = false;
        const body = err.error as { message?: string; detail?: string } | null;
        const fromApi = body?.detail || body?.message;
        this.errorMessage =
          typeof fromApi === 'string' && fromApi.length > 0
            ? fromApi
            : err.status === 400
              ? (body?.message ?? 'Cerere invalidă (de obicei lipsește Gemini:ApiKey pe server).')
              : `Eroare HTTP ${err.status}. Verifică consola API și setează Gemini__ApiKey pe server.`;
      }
    });
  }
}
