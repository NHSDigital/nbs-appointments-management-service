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

type AvailabilityTemplate = {
  days: DayOfWeek[];
  sessions: AvailabilitySession[];
};

type DayOfWeek =
  | 'Monday'
  | 'Tuesday'
  | 'Wednesday'
  | 'Thursday'
  | 'Friday'
  | 'Saturday'
  | 'Sunday';

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

type ServiceType = 'Covid' | 'Flu';

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

export type {
  ApplyAvailabilityTemplateRequest,
  ApiErrorResponse,
  ApiResponse,
  ApiSuccessResponse,
  AttributeDefinition,
  AttributeValue,
  Role,
  RoleAssignment,
  ServiceType,
  Site,
  SiteWithAttributes,
  User,
  UserProfile,
};
