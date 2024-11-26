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
  queryType: string;
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
// TODO: Do we need to include start year & end year?
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
  bookedAppointments: bookedAppointments[];
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
  outcome?: string;
  attendeeDetails: AttendeeDetails;
  contactDetails?: ContactItem[];
  reminderSet: boolean;
  created: string;
  provisional: boolean;
  // TODO: Additional data object - any type?
};

type AttendeeDetails = {
  nhsNumber: string;
  firstName: string;
  lastName: string;
  dateOfBirth: Date;
};

type ContactItem = {
  type: string;
  value: string;
};

type bookedAppointments = {
  service: string;
  count: number;
};

type ClinicalService = {
  label: string;
  value: string;
};

// TODO: Decide where this info should live and move it there
const clinicalServices: ClinicalService[] = [
  { label: 'COVID 12-15', value: 'COVID:12_15' },
  { label: 'COVID 16-17', value: 'COVID:16_17' },
  { label: 'COVID 18-74', value: 'COVID:18_74' },
  { label: 'COVID 5-11 (10 Min)', value: 'COVID:5_11_10' },
  { label: 'COVID 75+', value: 'COVID:75' },
  { label: 'FLU 18-64', value: 'FLU:18_64' },
  { label: 'FLU 65+', value: 'FLU:65' },
  { label: 'FLU and COVID 18-64', value: 'COVID_FLU:18_64' },
  { label: 'FLU and COVID 65-74', value: 'COVID_FLU:65-74' },
  { label: 'FLU and COVID 75+', value: 'COVID_FLU:75' },
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

export { MyaError, UnauthorizedError, daysOfTheWeek, clinicalServices };
