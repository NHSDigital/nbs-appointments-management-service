import { AttributeDefinition, AttributeValue, Role, Site, User } from '@types';

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

const mockAllPermissions = [
  'site:get-config',
  'site:set-config',
  'site:get-meta-data',
  'availability:get-setup',
  'availability:set-setup',
  'availability:query',
  'booking:make',
  'booking:query',
  'booking:cancel',
  'booking:set-status',
  'users:manage',
  'users:view',
  'site:view',
  'site:manage',
];

const mockAuditerPermissions = [
  'site:get-config',
  'site:get-meta-data',
  'availability:get-setup',
  'availability:query',
  'booking:query',
  'users:view',
];

const mockNonManagerPermissions = ['booking:query', 'booking:set-status'];

const mockAttributeDefinitions: AttributeDefinition[] = [
  {
    id: 'accessibility/attr_1',
    displayName: 'Accessibility attribute 1',
  },
  {
    id: 'accessibility/attr_2',
    displayName: 'Accessibility attribute 2',
  },
  {
    id: 'different_attribute_set/attr_1',
    displayName: 'Different attribute set attribute 1',
  },
];

const mockAttributeValues: AttributeValue[] = [
  {
    id: 'accessibility/attr_1',
    value: 'true',
  },
];

export {
  getMockUserAssignments,
  mockRoles,
  mockSites,
  mockAllPermissions,
  mockAuditerPermissions,
  mockNonManagerPermissions,
  mockAttributeDefinitions,
  mockAttributeValues,
};
