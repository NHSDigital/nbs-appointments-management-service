// eslint-disable-next-line import/no-extraneous-dependencies
import dayjs from 'dayjs';

type UserProfile = {
  emailAddress: string;
  availableSites: Site[];
};

type Site = {
  id: string;
  name: string;
  address: string;
};

type SiteWithAttributes = Site & {
  attributeValues: AttributeValue[];
};

type AttributeValue = {
  id: string;
  value: string;
};

type AttributeDefinition = {
  id: string;
  displayName: string;
};

type Service = {
  id: string;
  displayName: string;
};

type User = {
  id: string;
  roleAssignments: RoleAssignment[];
};

type RoleAssignment = {
  scope: string;
  role: string;
};

type Role = {
  displayName: string;
  id: string;
  description: string;
};

type ApiResponse<T> = ApiSuccessResponse<T> | ApiErrorResponse;

interface ApiSuccessResponse<T> {
  success: true;
  data: T | null;
}

interface ApiErrorResponse {
  success: false;
  httpStatusCode: number;
  errorMessage: string;
}

type TimePeriod = {
  day: dayjs.Dayjs;
  start: string;
  end: string;
};

type AvailabilityBlock = TimePeriod & {
  appointmentLength: number;
  sessionHolders: number;
  services: string[];
  isPreview?: boolean;
  isBreak?: boolean;
};

type WeekInfo = {
  weekNumber: number;
  month: string;
  commencing: dayjs.Dayjs;
};

const daysOfTheWeek = [
  'Monday',
  'Tuesday',
  'Wednesday',
  'Thursday',
  'Friday',
  'Saturday',
  'Sunday',
];

const monthsOfTheYear = [
  'January',
  'February',
  'March',
  'April',
  'May',
  'June',
  'July',
  'August',
  'September',
  'October',
  'November',
  'December',
];

export { daysOfTheWeek, monthsOfTheYear };
export type {
  AttributeValue,
  AttributeDefinition,
  UserProfile,
  Site,
  Service,
  User,
  RoleAssignment,
  SiteWithAttributes,
  Role,
  ApiResponse,
  AvailabilityBlock,
  WeekInfo,
};
