import { RoleAssignment } from "./Role";

export type User = {
  id: string;
  roleAssignments?: RoleAssignment[];
  displayName?: string;
};
