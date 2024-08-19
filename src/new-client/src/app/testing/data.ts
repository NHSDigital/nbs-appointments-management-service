import { Role, Site, User } from '@types';

const getMockUserAssignments = (site: string): User[] => [
  {
    id: 'test.one@nhs.net',
    roleAssignments: [
      { role: 'role-1', scope: `site:${site}` },
      { role: 'role-2', scope: `site:${site}` },
    ],
  },
  {
    id: 'test.two@nhs.net',
    roleAssignments: [
      { role: 'role-3', scope: `site:${site}` },
      { role: 'role-4', scope: `site:${site}` },
    ],
  },
];

const mockRoles: Role[] = [
  {
    displayName: 'Role 1',
    id: 'role-1',
    description: 'This is a short description of role 1.',
  },
  {
    displayName: 'Role 2',
    id: 'role-2',
    description: 'This is a short description of role 2.',
  },
  {
    displayName: 'Role 3',
    id: 'role-3',
    description: 'This is a short description of role 3.',
  },
];

const mockSites: Site[] = [
  { id: '1001', name: 'Site Alpha', address: 'Alpha Street' },
  { id: '1002', name: 'Site Beta', address: 'Beta Street' },
  { id: '1003', name: 'Site Gamma', address: 'Gamma Street' },
];

export { getMockUserAssignments, mockRoles, mockSites };
