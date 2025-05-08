import {
  dateTimeFormat,
  dateFormat,
  parseToUkDatetime,
} from '@services/timeService';
import {
  AccessibilityDefinition,
  Accessibility,
  AvailabilityCreatedEvent,
  AvailabilityResponse,
  AvailabilitySession,
  AvailabilityTemplate,
  Booking,
  DailyAvailability,
  DaySummary,
  Role,
  Site,
  User,
  UserProfile,
  Week,
  WellKnownOdsEntry,
  RoleAssignment,
} from '@types';

const getMockUserAssignments = (site: string): User[] => [
  {
    id: 'test.one@nhs.net',
    roleAssignments: [
      { role: 'role-1', scope: `site:${site}` },
      { role: 'role-2', scope: `site:${site}` },
    ],
    firstName: 'first1',
    lastName: 'last1',
  },
  {
    id: 'test.two@nhs.net',
    roleAssignments: [
      { role: 'role-3', scope: `site:${site}` },
      { role: 'role-4', scope: `site:${site}` },
    ],
    firstName: 'first2',
    lastName: 'last2',
  },
];

const mockRoles: Role[] = [
  {
    displayName: 'Beta Role',
    id: 'role-1',
    description: 'This is a short description of beta role.',
  },
  {
    displayName: 'Charlie Role',
    id: 'role-2',
    description: 'This is a short description of charlie role.',
  },
  {
    displayName: 'Alpha Role',
    id: 'role-3',
    description: 'This is a short description of alpha role.',
  },
];

const mockAssignments: RoleAssignment[] = [
  {
    role: 'role-1',
    scope: 'site:TEST',
  },
  {
    role: 'role-3',
    scope: 'site:TEST',
  },
];

const mockSites: Site[] = [
  {
    id: '34e990af-5dc9-43a6-8895-b9123216d699',
    name: 'Site Alpha',
    phoneNumber: '0118 999 88199 9119 725 3',
    address: 'Alpha Street',
    odsCode: '1001',
    integratedCareBoard: 'ICB1',
    region: 'R1',
    location: {
      type: 'Point',
      coordinates: [0.5646, 56.76457],
    },
    accessibilities: [
      { id: 'site_details/info_for_citizen', value: 'Test information' },
      { id: 'accessibility/attr_1', value: 'true' },
    ],
    informationForCitizens: 'Test information 1',
  },
  {
    id: '95e4ca69-da15-45f5-9ec7-6b2ea50f07c8',
    name: 'Site Beta',
    phoneNumber: '01189998819991197253',
    address: 'Beta Street',
    odsCode: '1002',
    integratedCareBoard: 'ICB2',
    region: 'R2',
    location: {
      type: 'Point',
      coordinates: [0.5646, 56.76457],
    },
    accessibilities: [
      { id: 'site_details/info_for_citizen', value: 'Test information' },
      { id: 'accessibility/attr_2', value: 'true' },
    ],
    informationForCitizens: 'Test information 2',
  },
  {
    id: 'd79bec60-8968-4101-b553-67dec04e1019',
    name: 'Site Gamma',
    phoneNumber: '01189998819991197253',
    address: 'Gamma Street',
    odsCode: '1003',
    integratedCareBoard: 'ICB3',
    region: 'R3',
    location: {
      type: 'Point',
      coordinates: [0.5646, 56.76457],
    },
    accessibilities: [
      { id: 'site_details/info_for_citizen', value: 'Test information' },
      { id: 'accessibility/attr_3', value: 'true' },
    ],
    informationForCitizens: 'Test information 3',
  },
  {
    id: '90a9c1f2-83d0-4c40-9c7c-080d91c56e79',
    name: 'Site Delta',
    phoneNumber: '0118 999 88199 9119 725 3',
    address: 'Delta Street, London',
    odsCode: '1004',
    integratedCareBoard: 'ICB4',
    region: 'R4',
    location: {
      type: 'Point',
      coordinates: [0.5646, 56.76457],
    },
    accessibilities: [
      { id: 'site_details/info_for_citizen', value: 'Test information' },
      { id: 'accessibility/attr_4', value: 'true' },
    ],
    informationForCitizens: 'Test information 4',
  },
];

const mockSite = mockSites[0];

const mockWellKnownOdsCodeEntries: WellKnownOdsEntry[] = [
  {
    odsCode: 'R1',
    displayName: 'Region One',
    type: 'region',
  },
  {
    odsCode: 'R3',
    displayName: 'Region Three',
    type: 'region',
  },
  {
    odsCode: 'ICB1',
    displayName: 'Integrated Care Board One',
    type: 'icb',
  },
  {
    odsCode: 'ICB3',
    displayName: 'Integrated Care Board Three',
    type: 'icb',
  },
];
const mockAllPermissions = [
  'site:get-meta-data',
  'availability:setup',
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
  'site:get-meta-data',
  'availability:query',
  'booking:query',
  'users:view',
];

const mockNonManagerPermissions = ['booking:query', 'booking:set-status'];

const mockAccessibilityDefinitions: AccessibilityDefinition[] = [
  {
    id: 'accessibility/attr_1',
    displayName: 'Accessibility attribute 1',
  },
  {
    id: 'accessibility/attr_2',
    displayName: 'Accessibility attribute 2',
  },
  {
    id: 'different_accessibility_set/attr_1',
    displayName: 'Different attribute set attribute 1',
  },
];

const mockAccessibilities: Accessibility[] = [
  {
    id: 'accessibility/attr_1',
    value: 'true',
  },
];

const mockUserProfile: UserProfile = {
  emailAddress: 'test.one@nhs.net',
  hasSites: true,
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
    status: 'Booked',
    availabilityStatus: 'Supported',
    reminderSet: false,
  },
  {
    reference: '4321',
    from: '2024-11-19T14:05:00',
    duration: 5,
    service: 'RSV:Adult',
    site: 'TEST01',
    attendeeDetails: {
      nhsNumber: '9999999991',
      firstName: 'Sarah',
      lastName: 'Smith',
      dateOfBirth: new Date(1945, 1, 1),
    },
    created: '2024-11-15T10:35:08.0477062',
    status: 'Booked',
    availabilityStatus: 'Supported',
    reminderSet: false,
  },
  {
    reference: '2468',
    from: '2024-11-27T14:05:00',
    duration: 5,
    service: 'RSV:Adult',
    site: 'TEST01',
    attendeeDetails: {
      nhsNumber: '9999999995',
      firstName: 'Brian',
      lastName: 'Smith',
      dateOfBirth: new Date(1984, 1, 1),
    },
    created: '2024-11-05T10:35:08.0477062',
    status: 'Booked',
    availabilityStatus: 'Supported',
    reminderSet: false,
  },
  {
    reference: '8642',
    from: '2024-12-02T10:35:00',
    duration: 5,
    service: 'RSV:Adult',
    site: 'TEST01',
    attendeeDetails: {
      nhsNumber: '9999999995',
      firstName: 'Brian',
      lastName: 'Smith',
      dateOfBirth: new Date(1984, 1, 1),
    },
    created: '2024-11-05T10:35:08.0477062',
    status: 'Booked',
    availabilityStatus: 'Supported',
    reminderSet: false,
  },
  {
    reference: '6823',
    from: '2024-12-05T09:34:00',
    duration: 5,
    service: 'RSV:Adult',
    site: 'TEST01',
    attendeeDetails: {
      nhsNumber: '9999999995',
      firstName: 'Ian',
      lastName: 'Goldsmith',
      dateOfBirth: new Date(1973, 2, 3),
    },
    created: '2024-08-29T03:21:08.0477062',
    status: 'Booked',
    availabilityStatus: 'Supported',
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
    startDate: parseToUkDatetime('2024-12-01'),
    endDate: parseToUkDatetime('2024-12-07'),
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
    startDate: parseToUkDatetime('2024-12-08'),
    endDate: parseToUkDatetime('2024-12-15'),
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
    startDate: parseToUkDatetime('2024-12-16'),
    endDate: parseToUkDatetime('2024-12-23'),
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

const mockDaySummaries: DaySummary[] = [
  {
    ukDate: parseToUkDatetime('2024-12-02'),
    sessions: [
      {
        ukStartDatetime: '2024-11-02T09:00:00',
        ukEndDatetime: '2024-11-02T17:00:00',
        maximumCapacity: 123,
        totalBookings: 5,
        bookings: {
          'RSV:Adult': mockBookings.length,
        },
        capacity: 2,
        slotLength: 5,
      },
    ],
    maximumCapacity: 123,
    bookedAppointments: 5,
    cancelledAppointments: 0,
    orphanedAppointments: 0,
    remainingCapacity: 118,
  },
  {
    ukDate: parseToUkDatetime('2024-12-04'),
    sessions: [
      {
        ukStartDatetime: '2024-11-04T09:00:00',
        ukEndDatetime: '2024-11-04T17:00:00',
        maximumCapacity: 200,
        totalBookings: 15,
        bookings: {
          'COVID:75+': mockBookings.length,
        },
        capacity: 2,
        slotLength: 5,
      },
    ],
    maximumCapacity: 200,
    bookedAppointments: 15,
    cancelledAppointments: 0,
    orphanedAppointments: 0,
    remainingCapacity: 185,
  },
  {
    ukDate: parseToUkDatetime('2024-12-05'),
    sessions: [
      {
        ukStartDatetime: '2024-11-05T09:00:00',
        ukEndDatetime: '2024-11-05T17:00:00',
        maximumCapacity: 200,
        totalBookings: 20,
        bookings: {
          'FLU:18_64': mockBookings.length,
        },
        capacity: 2,
        slotLength: 5,
      },
    ],
    maximumCapacity: 160,
    bookedAppointments: 20,
    cancelledAppointments: 0,
    orphanedAppointments: 0,
    remainingCapacity: 140,
  },
  {
    ukDate: parseToUkDatetime('2024-12-06'),
    sessions: [],
    maximumCapacity: 0,
    bookedAppointments: 0,
    cancelledAppointments: 0,
    orphanedAppointments: 0,
    remainingCapacity: 0,
  },
];

const mockEmptyDays: DaySummary[] = [
  {
    ukDate: parseToUkDatetime('2024-12-02'),
    sessions: [],
    maximumCapacity: 0,
    bookedAppointments: 0,
    cancelledAppointments: 0,
    orphanedAppointments: 0,
    remainingCapacity: 0,
  },
  {
    ukDate: parseToUkDatetime('2024-12-04'),
    sessions: [],
    maximumCapacity: 0,
    bookedAppointments: 0,
    cancelledAppointments: 0,
    orphanedAppointments: 0,
    remainingCapacity: 0,
  },
  {
    ukDate: parseToUkDatetime('2024-12-05'),
    sessions: [],
    maximumCapacity: 0,
    bookedAppointments: 0,
    cancelledAppointments: 0,
    orphanedAppointments: 0,
    remainingCapacity: 0,
  },
];

const mockWeekAvailabilityStart = parseToUkDatetime('2024-12-02');
const mockWeekAvailabilityEnd = parseToUkDatetime(
  '2024-12-08T23:59:59',
  dateTimeFormat,
);

const mockWeekAvailability: DailyAvailability[] = [
  {
    date: mockWeekAvailabilityStart.format(dateFormat),
    sessions: [
      {
        capacity: 2,
        from: '10:00',
        until: '16:00',
        slotLength: 5,
        services: ['RSV:Adult'],
      },
      {
        capacity: 1,
        from: '12:00',
        until: '16:00',
        slotLength: 5,
        services: ['RSV:Adult'],
      },
    ],
  },
  {
    date: mockWeekAvailabilityEnd.format(dateFormat),
    sessions: [
      {
        capacity: 2,
        from: '09:00',
        until: '14:00',
        slotLength: 5,
        services: ['RSV:Adult'],
      },
    ],
  },
];

export {
  getMockUserAssignments,
  mockAvailabilityCreatedEvents,
  mockRoles,
  mockAssignments,
  mockSite,
  mockSites,
  mockAllPermissions,
  mockAuditerPermissions,
  mockNonManagerPermissions,
  mockAccessibilityDefinitions,
  mockAccessibilities,
  mockUserProfile,
  mockAvailability,
  mockBookings,
  mockDetailedWeeks,
  mockDaySummaries,
  mockWeekAvailabilityStart,
  mockWeekAvailabilityEnd,
  mockEmptyDays,
  mockWeekAvailability,
  mockWellKnownOdsCodeEntries,
};
