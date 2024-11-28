import dayjs from 'dayjs';
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

type AttributeDefinition = {
  id: string;
  displayName: string;
};

type AttributeValue = {
  id: string;
  value: string;
};

type SetAttributesRequest = {
  scope: string;
  attributeValues: AttributeValue[];
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
};

type AvailabilityTemplate = {
  days: DayOfWeek[];
  sessions: AvailabilitySession[];
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

type RoleAssignment = {
  scope: string;
  role: string;
};

type Site = {
  id: string;
  name: string;
  address: string;
};

type SiteWithAttributes = Site & {
  attributeValues: AttributeValue[];
};

type User = {
  id: string;
  roleAssignments: RoleAssignment[];
};

type UserProfile = {
  emailAddress: string;
  availableSites: Site[];
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
  startDate: dayjs.Dayjs;
  endDate: dayjs.Dayjs;
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
  status: AppointmentStatus;
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
  type: ContactItemType;
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

enum AppointmentStatus {
  Unknown,
  Provisional,
  Booked,
  Cancelled,
}

enum ContactItemType {
  Phone,
  Email,
  Landline,
}

// TODO: Decide where this info should live and move it there
const clinicalServices: ClinicalService[] = [
  { label: 'RSV (Adult)', value: 'RSV:Adult' },
];

export type {
  ApplyAvailabilityTemplateRequest,
  ApiErrorResponse,
  ApiResponse,
  ApiSuccessResponse,
  AttributeDefinition,
  AttributeValue,
  Availability,
  AvailabilityBlock,
  AvailabilityResponse,
  AvailabilityCreatedEvent,
  AvailabilitySession,
  AvailabilityTemplate,
  Booking,
  DateComponents,
  ErrorType,
  FetchAvailabilityRequest,
  FetchBookingsRequest,
  Role,
  RoleAssignment,
  Session,
  SetAttributesRequest,
  SetAvailabilityRequest,
  Site,
  SiteWithAttributes,
  TimeComponents,
  User,
  UserProfile,
  Week,
};

export {
  AppointmentStatus,
  ContactItemType,
  MyaError,
  UnauthorizedError,
  daysOfTheWeek,
  clinicalServices,
};
