type UserProfile = {
  emailAddress: string;
  availableSites: Site[];
};

type Site = {
  id: string;
  name: string;
  address: string;
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
  data: T;
}

interface ApiErrorResponse {
  success: false;
  httpStatusCode: number;
  errorMessage: string;
}

export type { UserProfile, Site, User, RoleAssignment, Role, ApiResponse };