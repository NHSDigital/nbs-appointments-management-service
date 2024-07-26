import { RoleAssignment } from "./RoleAssignment";

export type User = {
  id: string;
  roleAssignments?: RoleAssignment[];
  displayName?: string;
};
