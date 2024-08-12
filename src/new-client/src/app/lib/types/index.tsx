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

type AttributeDefinition = {
  id: string;
  displayName: string;
};

type AttributeValue = {
  id: string;
  value: string;
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

type AvailabilityBlock = {
  day: dayjs.Dayjs;
  start: string;
  end: string;
  sessionHolders: number;
  services: number;
  isPreview?: boolean;
};

type WeekInfo = {
  weekNumber: number;
  month: string;
  commencing: dayjs.Dayjs;
};

export type {
  UserProfile,
  Site,
  User,
  RoleAssignment,
  SiteWithAttributes,
  Role,
  ApiResponse,
  AttributeDefinition,
  AvailabilityBlock,
  WeekInfo,
};
