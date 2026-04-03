import { User } from './user';

export interface Device {
  id: number;
  name: string;
  manufacturer: string;
  type: number;
  operatingSystem: string;
  osVersion: string;
  processor: string;
  ramAmountGb: number;
  description: string;
  assignedUserId?: number | null;
  assignedUser?: User | null;
}

export interface DevicePayload {
  name: string;
  manufacturer: string;
  type: number;
  operatingSystem: string;
  osVersion: string;
  processor: string;
  ramAmountGb: number;
  description: string;
}
