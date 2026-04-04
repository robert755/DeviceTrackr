import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged, map } from 'rxjs/operators';
import { Device, DevicePayload } from '../../models/device';
import { User } from '../../models/user';
import { DeviceApiService } from '../../services/device-api.service';
import { AuthSessionService } from '../../services/auth-session.service';

@Component({
  selector: 'app-devices',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './devices.component.html'
})
export class DevicesComponent implements OnInit, OnDestroy {
  devices: Device[] = [];
  users: User[] = [];
  searchQuery = '';
  selectedDevice?: Device;
  loading = false;
  errorMessage = '';
  successMessage = '';
  isEditMode = false;
  generatingDescription = false;

  private readonly searchTerms = new Subject<string>();
  private searchSub?: Subscription;

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
    this.searchSub = this.searchTerms
      .pipe(
        debounceTime(300),
        map((s) => s.trim()),
        distinctUntilChanged()
      )
      .subscribe(() => this.loadDevicesOnly());

    this.loadAll();
  }

  ngOnDestroy(): void {
    this.searchSub?.unsubscribe();
  }

  onSearchQueryChange(value: string): void {
    this.searchTerms.next(value);
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
      error: () => (this.errorMessage = 'Could not load users.')
    });

    this.api.searchDevices(this.searchQuery.trim() || undefined).subscribe({
      next: (devices) => {
        this.applyDevicesResponse(devices);
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.errorMessage = 'Could not load devices. Start the API.';
      }
    });
  }

  private loadDevicesOnly(): void {
    this.api.searchDevices(this.searchQuery.trim() || undefined).subscribe({
      next: (devices) => this.applyDevicesResponse(devices),
      error: () => (this.errorMessage = 'Search failed. Check the API.')
    });
  }

  private applyDevicesResponse(devices: Device[]): void {
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

  deviceTypeLabel(type: number): string {
    switch (type) {
      case 1:
        return 'Phone';
      case 2:
        return 'Tablet';
      case 3:
        return 'Laptop';
      default:
        return 'Unknown';
    }
  }

  /** Only devices without an assigned user can be modified or deleted. */
  canModifyDevice(device: Device): boolean {
    return !device.assignedUserId;
  }

  startEdit(device: Device): void {
    if (!this.canModifyDevice(device)) {
      this.errorMessage = 'You cannot edit a device that is already assigned.';
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
      this.errorMessage =
        'Fill in the required fields (description is optional; you can generate it with Gemini after saving).';
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
      this.errorMessage = 'A device with this name already exists.';
      return;
    }

    if (!this.isEditMode) {
      this.api.createDevice(payload).subscribe({
        next: () => {
          this.successMessage = 'Device added.';
          this.startCreate();
          this.loadAll();
        },
        error: () => (this.errorMessage = 'Create failed.')
      });
      return;
    }

    if (!this.selectedDevice) {
      this.errorMessage = 'Select a device.';
      return;
    }

    if (this.selectedDevice.assignedUserId) {
      this.errorMessage = 'The device is assigned and cannot be updated.';
      return;
    }

    this.api.updateDevice(this.selectedDevice.id, payload).subscribe({
      next: () => {
        this.successMessage = 'Device updated.';
        this.loadAll();
      },
      error: () => (this.errorMessage = 'Update failed.')
    });
  }

  deleteDevice(device: Device): void {
    if (!this.canModifyDevice(device)) {
      this.errorMessage = 'You cannot delete an assigned device; unassign it first.';
      return;
    }
    this.successMessage = '';
    this.errorMessage = '';
    this.api.deleteDevice(device.id).subscribe({
      next: () => {
        this.successMessage = 'Device deleted.';
        if (this.selectedDevice?.id === device.id) {
          this.selectedDevice = undefined;
        }
        this.loadAll();
      },
      error: () => (this.errorMessage = 'Delete failed.')
    });
  }

  assignToMe(device: Device): void {
    this.successMessage = '';
    this.errorMessage = '';
    const user = this.currentUser;
    if (!user) {
      this.errorMessage = 'You must be signed in.';
      return;
    }

    this.api.assignDevice(device.id, user.userId).subscribe({
      next: () => {
        this.successMessage = 'Device assigned to you.';
        if (this.selectedDevice?.id === device.id && this.isEditMode) {
          this.isEditMode = false;
          this.resetDeviceFormEmpty();
        }
        this.loadAll();
      },
      error: () => (this.errorMessage = 'Assignment failed.')
    });
  }

  unassignFromMe(device: Device): void {
    this.successMessage = '';
    this.errorMessage = '';
    const user = this.currentUser;
    if (!user) {
      this.errorMessage = 'You must be signed in.';
      return;
    }

    this.api.unassignDevice(device.id, user.userId).subscribe({
      next: () => {
        this.successMessage = 'Assignment removed.';
        this.loadAll();
      },
      error: () => (this.errorMessage = 'Operation failed.')
    });
  }

  getAssignedUserText(device: Device): string {
    if (!device.assignedUser) {
      return 'Unassigned';
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
        this.successMessage = 'Description generated with Gemini and saved.';
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
              ? (body?.message ?? 'Invalid request (usually Gemini:ApiKey is missing on the server).')
              : `HTTP error ${err.status}. Check the API console and set Gemini__ApiKey on the server.`;
      }
    });
  }
}
