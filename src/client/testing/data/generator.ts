import {
  E2ETestSite,
  E2ETestUser,
  MockOidcUser,
  Role,
  SiteDocument,
  UserDocument,
} from '@e2etests/types';

const buildSiteDocument = (testId: number): SiteDocument => {
  return {
    id: buildSiteId(testId),
    docType: 'site',
    name: `Test Site ${testId}`,
    address: '123 Test St, Testville, TE5 7ST',
    phoneNumber: '0113 1111111',
    odsCode: buildOdsCode(testId),
    region: buildRegion(testId),
    integratedCareBoard: buildIcb(testId),
    location: {
      type: 'Point',
      coordinates: [-1.6610648, 53.795467],
    },
    accessibilities: [],
    informationForCitizens:
      'This is some placeholder information for citizens.',
  };
};

const buildE2ETestSite = (testId: number): E2ETestSite => {
  return { id: buildSiteId(testId), name: buildSiteName(testId) };
};

const buildSiteId = (testId: number): string => {
  return `site-${testId}`;
};

const buildSiteName = (testId: number): string => {
  return `Test Site ${testId}`;
};

const buildOdsCode = (testId: number): string => {
  return `ABC${testId}`;
};

const buildRegion = (testId: number): string => {
  return `R${testId}`;
};

const buildIcb = (testId: number): string => {
  return `ICB${testId}`;
};

const buildScopeForRole = (testId: number, role: Role): string => {
  switch (role) {
    case 'system:admin-user':
      return `global`;
    case 'system:icb-user':
      return buildIcb(testId);
    case 'system:regional-user':
      return buildRegion(testId);
    default:
      return buildSiteId(testId);
  }
};

const buildUserId = (testId: number): string => {
  return `test-user-${testId}@nhs.net`;
};

const buildUsername = (testId: number): string => {
  return `Test User ${testId}`;
};

const buildUserPassword = (): string => {
  return `TestUserPassword123!`;
};

const buildUserDocument = (testId: number, roles: Role[]): UserDocument => {
  return {
    id: buildUserId(testId),
    docType: 'user',
    latestAcceptedEulaVersion: '2024-12-01',
    roleAssignments: roles.map(role => {
      return { role, scope: buildScopeForRole(testId, role) };
    }),
  };
};

const buildMockOidcUser = (testId: number): MockOidcUser => {
  return {
    subjectId: buildUserId(testId),
    username: buildUsername(testId),
    password: buildUserPassword(),
    claims: [{ type: 'email', value: buildUserId(testId) }],
  };
};

const buildE2ETestUser = (testId: number): E2ETestUser => {
  return {
    id: buildUserId(testId),
    username: buildUsername(testId),
    password: buildUserPassword(),
  };
};

export {
  buildSiteDocument,
  buildUserDocument,
  buildMockOidcUser,
  buildE2ETestSite,
  buildE2ETestUser,
};
