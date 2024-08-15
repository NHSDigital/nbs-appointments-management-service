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

type AvailabilityBlock = {
  day: dayjs.Dayjs;
  start: string;
  end: string;
  sessionHolders: number;
  services: string[];
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
  Role,
  AvailabilityBlock,
  WeekInfo,
};
