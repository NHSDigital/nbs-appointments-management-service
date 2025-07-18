import { DayJsType } from '@services/timeService';
import MyaError, { UnauthorizedError, ErrorType } from './mya-error';

type ApiErrorResponse = {
  success: false;
  httpStatusCode: number;
  errorMessage: string;
};

type ApiResponse<T> = ApiSuccessResponse<T> | ApiErrorResponse;

interface ApiSuccessResponse<T> {
  success: true;
  data: T | null;
}

type AccessibilityDefinition = {
  id: string;
  displayName: string;
};

type Accessibility = {
  id: string;
  value: string;
};

type WellKnownOdsEntry = {
  odsCode: string;
  displayName: string;
  type: string;
};

type SetAccessibilitiesRequest = {
  accessibilities: Accessibility[];
};

type SetInformationForCitizensRequest = {
  informationForCitizens: string;
};

type SetSiteDetailsRequest = {
  name: string;
  address: string;
  phoneNumber?: string;
  latitude: string;
  longitude: string;
};

type SetSiteReferenceDetailsRequest = {
  odsCode: string;
  icb: string;
  region: string;
};

type Role = {
  displayName: string;
  id: string;
  description: string;
};

type ApplyAvailabilityTemplateRequest = {
  site: string;
  from: string;
  until: string;
  template: AvailabilityTemplate;
  mode: ApplyAvailabilityMode;
};

type AvailabilityCreatedEvent = {
  created: string;
  by: string;
  site: string;
  from: string;
  to?: string;
  template?: AvailabilityTemplate;
  sessions?: AvailabilitySession[];
};

type SetAvailabilityRequest = {
  site: string;
  date: string;
  sessions: AvailabilitySession[];
  mode: ApplyAvailabilityMode;
};

type EditSessionRequest = {
  site: string;
  date: string;
  mode: ApplyAvailabilityMode;
  sessionToEdit: AvailabilitySession;
  sessions: AvailabilitySession[];
};

type AvailabilityTemplate = {
  days: DayOfWeek[];
  sessions: AvailabilitySession[];
};

export type BookingStatus = 'Unknown' | 'Provisional' | 'Booked' | 'Cancelled';

type IdentityProvider = 'NhsMail' | 'Okta';
type UserIdentityStatus = {
  identityProvider: IdentityProvider;
  extantInIdentityProvider: boolean;
  extantInSite: boolean;
  meetsWhitelistRequirements: boolean;
};

type ApplyAvailabilityMode = 'Overwrite' | 'Additive' | 'Edit';

type EulaVersion = {
  versionDate: string;
};

const daysOfTheWeek = [
  'Monday',
  'Tuesday',
  'Wednesday',
  'Thursday',
  'Friday',
  'Saturday',
  'Sunday',
] as const;
export type DayOfWeek = (typeof daysOfTheWeek)[number];

type AvailabilitySession = {
  from: string;
  until: string;
  services: string[];
  slotLength: number;
  capacity: number;
};

type AvailabilitySlot = {
  sessionIndex: number;
  from: DayJsType;
  length: number;
  services: string[];
  capacity: number;
};

type RoleAssignment = {
  scope: string;
  role: string;
};

type NhsMyaCookieConsent = {
  consented: boolean;
  version: number;
};

type Site = {
  id: string;
  name: string;
  address: string;
  phoneNumber: string;
  odsCode: string;
  integratedCareBoard: string;
  region: string;
  location: Location;
  accessibilities: Accessibility[];
  informationForCitizens: string;
};

type Location = {
  type: string;
  coordinates: number[];
};

type User = {
  id: string;
  roleAssignments: RoleAssignment[];
  firstName?: string;
  lastName?: string;
};

type UserProfile = {
  emailAddress: string;
  latestAcceptedEulaVersion?: string;
  hasSites: boolean;
};

type DateComponents = {
  day: number | string;
  month: number | string;
  year: number | string;
};

type TimeComponents = {
  hour: number | string;
  minute: number | string;
};

type Session = {
  startTime: TimeComponents;
  endTime: TimeComponents;
  break: 'yes' | 'no';
  capacity: number;
  slotLength: number;
  services: string[];
};

type FetchAvailabilityRequest = {
  sites: string[];
  service: string;
  from: string;
  until: string;
  queryType: '*' | 'Days' | 'Hours' | 'Slots';
};

type FeatureFlag = {
  enabled: boolean;
};

type AvailabilityResponse = {
  site: string;
  service: string;
  availability: Availability[];
};

type Availability = {
  date: Date;
  blocks: AvailabilityBlock[];
};

type AvailabilityBlock = {
  from: string;
  until: string;
  count: number;
};

// TODO: Rename this type to something more meaningful
type Week = {
  start: number;
  end: number;
  startMonth: number;
  endMonth: number;
  startYear: number;
  endYear: number;
  unbooked?: number;
  totalAppointments?: number;
  booked?: number;
  bookedAppointments: BookedAppointments[];
  startDate: DayJsType;
  endDate: DayJsType;
};

type FetchBookingsRequest = {
  from: string;
  to: string;
  site: string;
};

type Booking = {
  reference: string;
  from: string;
  duration: number;
  service: string;
  site: string;
  status: BookingStatus;
  availabilityStatus: 'Unknown' | 'Supported' | 'Orphaned';
  attendeeDetails: AttendeeDetails;
  contactDetails?: ContactItem[];
  reminderSet: boolean;
  created: string;
  // TODO: Additional data object - any type?
};

type AttendeeDetails = {
  nhsNumber: string;
  firstName: string;
  lastName: string;
  dateOfBirth: Date;
};

type ContactItem = {
  type: 'Phone' | 'Email' | 'Landline';
  value: string;
};

type BookedAppointments = {
  service: string;
  count: number;
};

type ClinicalService = {
  label: string;
  value: string;
};

type DailyAvailability = {
  date: string;
  sessions: AvailabilitySession[];
};

type DayAvailabilityDetails = {
  fullDate: string;
  date: string;
  serviceInformation?: ServiceInformation[];
  totalAppointments?: number;
  booked: number;
  unbooked?: number;
};

type SessionSummary = {
  ukStartDatetime: string;
  ukEndDatetime: string;
  maximumCapacity: number;
  totalBookings: number;
  bookings: Record<string, number>;
  capacity: number;
  slotLength: number;
};

type DaySummary = {
  ukDate: DayJsType;
  sessions: SessionSummary[];
  maximumCapacity: number;
  bookedAppointments: number;
  cancelledAppointments: number;
  orphanedAppointments: number;
  remainingCapacity: number;
};

type WeekSummary = {
  startDate: DayJsType;
  endDate: DayJsType;
  daySummaries: DaySummary[];
  maximumCapacity: number;
  bookedAppointments: number;
  orphanedAppointments: number;
  remainingCapacity: number;
};

type WeekSummaryV2 = {
  daySummaries: DaySummaryV2[];
  maximumCapacity: number;
  remainingCapacity: number;
  bookedAppointments: number;
  orphanedAppointments: number;
};

type DaySummaryV2 = {
  date: string;
  sessions: SessionSummary[];
  maximumCapacity: number;
  remainingCapacity: number;
  bookedAppointments: number;
  orphanedAppointments: number;
  cancelledAppointments: number;
};

type ServiceInformation = {
  time: string;
  serviceDetails: ServiceBookingDetails[];
  unbooked?: number;
  capacity: number;
};

type ServiceBookingDetails = {
  service: string;
  booked: number;
};

type CancelSessionRequest = {
  site: string;
  date: string;
  from: string;
  until: string;
  services: string[];
  slotLength: number;
  capacity: number;
};

// TODO: Decide where this info should live and move it there
const clinicalServices: ClinicalService[] = [
  { label: 'RSV Adult', value: 'RSV:Adult' },
];

export type {
  ApplyAvailabilityTemplateRequest,
  ApiErrorResponse,
  ApiResponse,
  ApiSuccessResponse,
  AttendeeDetails,
  AccessibilityDefinition,
  Accessibility,
  Availability,
  AvailabilityBlock,
  AvailabilityResponse,
  AvailabilityCreatedEvent,
  AvailabilitySession,
  AvailabilitySlot,
  AvailabilityTemplate,
  Booking,
  CancelSessionRequest,
  ContactItem,
  DaySummary,
  DailyAvailability,
  DateComponents,
  DayAvailabilityDetails,
  EditSessionRequest,
  ErrorType,
  FetchAvailabilityRequest,
  FetchBookingsRequest,
  EulaVersion,
  IdentityProvider,
  NhsMyaCookieConsent,
  Role,
  RoleAssignment,
  ServiceInformation,
  ServiceBookingDetails,
  Session,
  SessionSummary,
  SetAccessibilitiesRequest,
  SetAvailabilityRequest,
  SetInformationForCitizensRequest,
  Site,
  TimeComponents,
  User,
  UserIdentityStatus,
  UserProfile,
  Week,
  WeekSummary,
  WellKnownOdsEntry,
  SetSiteDetailsRequest,
  SetSiteReferenceDetailsRequest,
  FeatureFlag,
  WeekSummaryV2,
  DaySummaryV2,
  ClinicalService,
};

export { MyaError, UnauthorizedError, daysOfTheWeek, clinicalServices };
