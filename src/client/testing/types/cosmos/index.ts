type SiteDocument = {
  id: string;
  docType: 'site';
  name: string;
  address: string;
  phoneNumber: string;
  odsCode: string;
  region: string;
  integratedCareBoard: string;
  location: SiteLocation;
  status?: 'Online' | 'Offline';
  accessibilities: string[];
  informationForCitizens: string;
};

type SiteLocation = {
  type: 'Point';
  coordinates: [number, number];
};

type UserDocument = {
  id: string;
  docType: 'user';
  latestAcceptedEulaVersion: string;
  roleAssignments: RoleAssignment[];
};

type RoleAssignment = {
  role: Role;
  scope: string;
};

type Role =
  | 'canned:availability-manager'
  | 'canned:appointment-manager'
  | 'canned:site-details-manager'
  | 'canned:user-manager'
  | 'system:admin-user'
  | 'system:regional-user'
  | 'system:icb-user';

export type { SiteDocument, UserDocument, Role };
