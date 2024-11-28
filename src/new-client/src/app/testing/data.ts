import {
  AppointmentStatus,
  AttributeDefinition,
  AttributeValue,
  AvailabilityCreatedEvent,
  AvailabilityResponse,
  AvailabilitySession,
  AvailabilityTemplate,
  Booking,
  Role,
  Site,
  SiteWithAttributes,
  User,
  UserProfile,
  Week,
} from '@types';
import dayjs from 'dayjs';

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
  days: [
    'Monday',
    'Tuesday',
    'Wednesday',
    'Thursday',
    'Friday',
    'Saturday',
    'Sunday',
  ],
  sessions: [mockSession3],
};

const mockAvailabilityCreatedEvents: AvailabilityCreatedEvent[] = [
  {
    created: '2024-11-20T13:36:43.4680585Z',
    by: mockUserProfile.emailAddress,
    site: mockSite.id,
    from: '2024-01-01',
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
    from: '2024-03-01',
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

const mockAvailability: AvailabilityResponse[] = [
  {
    site: 'TEST01',
    service: '*',
    availability: [
      {
        date: new Date(2024, 10, 1),
        blocks: [
          { from: '00:00', until: '12:00', count: 12 },
          { from: '12:00', until: '16:00', count: 4 },
        ],
      },
      {
        date: new Date(2024, 10, 8),
        blocks: [
          { from: '00:00', until: '12:00', count: 12 },
          { from: '12:00', until: '16:00', count: 4 },
        ],
      },
      {
        date: new Date(2024, 10, 16),
        blocks: [
          { from: '00:00', until: '12:00', count: 12 },
          { from: '12:00', until: '16:00', count: 4 },
        ],
      },
      {
        date: new Date(2024, 10, 26),
        blocks: [
          { from: '00:00', until: '12:00', count: 12 },
          { from: '12:00', until: '16:00', count: 4 },
        ],
      },
      {
        date: new Date(2024, 10, 1),
        blocks: [
          { from: '00:00', until: '12:00', count: 12 },
          { from: '12:00', until: '16:00', count: 4 },
        ],
      },
    ],
  },
];

const mockBookings: Booking[] = [
  {
    reference: '1234',
    from: '2024-11-10T14:05:00',
    duration: 5,
    service: 'RSV:Adult',
    site: 'TEST01',
    attendeeDetails: {
      nhsNumber: '9999999990',
      firstName: 'John',
      lastName: 'Smith',
      dateOfBirth: new Date(1979, 1, 1),
    },
    created: '2024-11-05T10:35:08.0477062',
    status: AppointmentStatus.Booked,
    reminderSet: false,
  },
  {
    reference: '4321',
    from: '2024-11-19T14:05:00',
    duration: 5,
    service: 'COVID:75',
    site: 'TEST01',
    attendeeDetails: {
      nhsNumber: '9999999991',
      firstName: 'Sarah',
      lastName: 'Smith',
      dateOfBirth: new Date(1945, 1, 1),
    },
    created: '2024-11-15T10:35:08.0477062',
    status: AppointmentStatus.Booked,
    reminderSet: false,
  },
  {
    reference: '2468',
    from: '2024-11-27T14:05:00',
    duration: 5,
    service: 'FLU:18_64',
    site: 'TEST01',
    attendeeDetails: {
      nhsNumber: '9999999995',
      firstName: 'Brian',
      lastName: 'Smith',
      dateOfBirth: new Date(1984, 1, 1),
    },
    created: '2024-11-05T10:35:08.0477062',
    status: AppointmentStatus.Booked,
    reminderSet: false,
  },
];

const mockDetailedWeeks: Week[] = [
  {
    start: 1,
    startMonth: 11,
    startYear: 2024,
    endYear: 2025,
    end: 7,
    endMonth: 11,
    startDate: dayjs().year(2024).month(11).date(1),
    endDate: dayjs().year(2024).month(11).date(7),
    bookedAppointments: [
      { service: 'COVID 75+', count: 10 },
      { service: 'FLU 18-64', count: 5 },
      { service: 'RSV (Adult)', count: 2 },
    ],
    booked: 17,
    unbooked: 13,
    totalAppointments: 30,
  },
  {
    start: 8,
    startMonth: 11,
    startYear: 2024,
    endYear: 2025,
    end: 15,
    endMonth: 11,
    startDate: dayjs().year(2024).month(11).date(8),
    endDate: dayjs().year(2024).month(11).date(15),
    bookedAppointments: [
      { service: 'COVID 75+', count: 5 },
      { service: 'FLU 18-64', count: 1 },
      { service: 'RSV (Adult)', count: 12 },
    ],
    booked: 18,
    unbooked: 12,
    totalAppointments: 30,
  },
  {
    start: 16,
    startMonth: 11,
    startYear: 2024,
    endYear: 2025,
    end: 23,
    endMonth: 11,
    startDate: dayjs().year(2024).month(11).date(16),
    endDate: dayjs().year(2024).month(11).date(23),
    bookedAppointments: [
      { service: 'COVID 75+', count: 5 },
      { service: 'FLU 18-64', count: 10 },
      { service: 'RSV (Adult)', count: 10 },
    ],
    booked: 25,
    unbooked: 5,
    totalAppointments: 30,
  },
];

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
  mockAvailability,
  mockBookings,
  mockDetailedWeeks,
};
