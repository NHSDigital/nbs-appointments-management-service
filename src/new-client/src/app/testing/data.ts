import {
  AttributeDefinition,
  AttributeValue,
  AvailabilityCreatedEvent,
  AvailabilitySession,
  AvailabilityTemplate,
  Role,
  Site,
  SiteWithAttributes,
  User,
  UserProfile,
} from '@types';

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

const mockSite = mockSites[0];

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

const mockUserProfile: UserProfile = {
  emailAddress: 'test.one@nhs.net',
  availableSites: mockSites,
};

const mockSession1: AvailabilitySession = {
  from: '09:00',
  until: '12:00',
  services: ['RSV:Adult'],
  capacity: 2,
  slotLength: 5,
};

const mockSession2: AvailabilitySession = {
  from: '13:00',
  until: '17:30',
  services: ['RSV:Adult'],
  capacity: 2,
  slotLength: 5,
};

const mockSession3: AvailabilitySession = {
  from: '09:00',
  until: '17:30',
  services: ['RSV:Adult'],
  capacity: 3,
  slotLength: 10,
};

const mockTemplate1: AvailabilityTemplate = {
  days: ['Monday', 'Tuesday'],
  sessions: [mockSession1],
};

const mockTemplate2: AvailabilityTemplate = {
  days: ['Thursday', 'Friday'],
  sessions: [mockSession3],
};

const mockAvailabilityCreatedEvents: AvailabilityCreatedEvent[] = [
  {
    created: '2024-11-20T13:36:43.4680585Z',
    by: mockUserProfile.emailAddress,
    site: mockSite.id,
    from: '2025-01-01',
    to: '2024-02-28',
    template: mockTemplate1,
    sessions: undefined,
  },
  {
    created: '2024-11-20T13:36:43.4680585Z',
    by: mockUserProfile.emailAddress,
    site: mockSite.id,
    from: '2025-01-01',
    to: undefined,
    template: undefined,
    sessions: [mockSession2],
  },
  {
    created: '2024-11-20T13:36:43.4680585Z',
    by: mockUserProfile.emailAddress,
    site: mockSite.id,
    from: '2025-03-01',
    to: '2024-04-30',
    template: mockTemplate2,
    sessions: undefined,
  },
  {
    created: '2024-11-20T13:36:43.4680585Z',
    by: mockUserProfile.emailAddress,
    site: mockSite.id,
    from: '2025-02-16',
    to: undefined,
    template: undefined,
    sessions: [mockSession3],
  },
];

const mockSiteWithAttributes: SiteWithAttributes = {
  id: mockSites[0].id,
  address: mockSites[0].address,
  name: mockSites[0].name,
  attributeValues: [
    { id: 'site_details/info_for_citizen', value: 'Test information' },
    { id: 'accessibility/attr_1', value: 'true' },
  ],
};

export {
  getMockUserAssignments,
  mockAvailabilityCreatedEvents,
  mockRoles,
  mockSite,
  mockSites,
  mockAllPermissions,
  mockAuditerPermissions,
  mockNonManagerPermissions,
  mockAttributeDefinitions,
  mockAttributeValues,
  mockUserProfile,
  mockSiteWithAttributes,
};
