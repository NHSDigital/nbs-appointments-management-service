import { BookingStatus } from '@types';

type SiteDocument = {
  id: string;
  docType: 'site';
  name: string;
  address: string;
  phoneNumber: string;
  odsCode: string;
  region: string;
  integratedCareBoard: string;
  location: SiteLocation;
  status?: 'Online' | 'Offline';
  accessibilities: string[];
  informationForCitizens: string;
};

type BookingDocument = {
  docType: 'booking';
  id: string;
  reference: string;
  site: string;
  from: string;
  duration: number;
  service: string;
  status: BookingStatus;
  availabilityStatus: string;
  attendeeDetails: AttendeeDetails;
  contactDetails: string[];
  additionalData: AdditionalData;
  reminderSent: boolean;
  created: string;
  statusUpdated: string;
};

type BookingIndexDocument = {
  docType: 'booking_index';
  id: string;
  reference: string;
  site: string;
  from: string;
  status: BookingStatus;
  nhsNumber: string;
  created: string;
  statusUpdated: string;
};

type AttendeeDetails = {
  nhsNumber: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
};

type AdditionalData = {
  isCallCentreBooking: boolean;
  callCentreHandlerEmail: string | undefined;
  isAppBooking: boolean;
  selfReferralOccupation: string;
  decisionReason: string;
};

type SiteLocation = {
  type: 'Point';
  coordinates: [number, number];
};

type UserDocument = {
  id: string;
  docType: 'user';
  latestAcceptedEulaVersion: string;
  roleAssignments: RoleAssignment[];
};

type RoleAssignment = {
  role: Role;
  scope: string;
};

type Role =
  | 'canned:availability-manager'
  | 'canned:appointment-manager'
  | 'canned:site-details-manager'
  | 'canned:user-manager'
  | 'system:admin-user'
  | 'system:regional-user'
  | 'system:icb-user';

export type {
  SiteDocument,
  UserDocument,
  BookingDocument,
  BookingIndexDocument,
  Role,
};
