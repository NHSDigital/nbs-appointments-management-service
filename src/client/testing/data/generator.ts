import {
  E2ETestSite,
  E2ETestUser,
  MockOidcUser,
  Role,
  SiteDocument,
  UserDocument,
} from '@e2etests/types';
import { createHash } from 'crypto';

const buildSiteDocument = (testId: number): SiteDocument => {
  return {
    id: buildSiteId(testId),
    docType: 'site',
    name: `Test Site ${testId}`,
    address: buildAddress(testId),
    phoneNumber: buildPhoneNumber(testId),
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

const buildPhoneNumber = (testId: number): string => {
  const guaranteedLongNumber = 7000000000 + (testId % 1000000000);
  return `0${guaranteedLongNumber.toString().substring(0, 3)} ${guaranteedLongNumber.toString().substring(4, 10)}`;
};

const buildAddress = (testId: number): string => {
  const hash = createHash('sha256').update(testId.toString()).digest('hex');

  const postCode = `${hash.substring(0, 3).toUpperCase()} ${hash.substring(4, 7).toUpperCase()}`;

  return `${testId.toString().substring(0, 3)} Test Street,\nTest Town,\n${postCode}`;
};

// TODO: Clean this up a bit, must be a better way of generating deterministic GUIDs
function generateDeterministicGuid(seed: number): string {
  const hash = createHash('sha256').update(seed.toString()).digest('hex');

  return [
    hash.substring(0, 8),
    hash.substring(8, 12),
    '4' + hash.substring(13, 16),
    ((parseInt(hash.substring(16, 17), 16) & 0x3) | 0x8).toString(16) +
      hash.substring(17, 20),
    hash.substring(20, 32),
  ].join('-');
}

const buildSiteId = (testId: number): string => {
  return generateDeterministicGuid(testId);
};

const buildSiteName = (testId: number): string => {
  return `Test Site ${testId}`;
};

const buildOdsCode = (testId: number): string => {
  return `ABC${testId}`;
};

// eslint-disable-next-line @typescript-eslint/no-unused-vars
const buildRegion = (testId: number): string => {
  return 'R1';

  // TODO: randomise this based on testId. Atm this needs to be a well known ODS code entry, so need to add a step which inserts the randomly generated region as a known entry.
  //return `R${testId}`;
};

// eslint-disable-next-line @typescript-eslint/no-unused-vars
const buildIcb = (testId: number): string => {
  return 'ICB1';

  // TODO: randomise this based on testId. Atm this needs to be a well known ODS code entry, so need to add a step which inserts the randomly generated region as a known entry.
  // return `ICB${testId}`;
};

const buildScopeForRole = (testId: number, role: Role): string => {
  switch (role) {
    case 'system:admin-user':
      return `global`;
    case 'system:icb-user':
      return `icb:${buildIcb(testId)}`;
    case 'system:regional-user':
      return `region:${buildRegion(testId)}`;
    default:
      return `site:${buildSiteId(testId)}`;
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
    claims: [{ Type: 'email', Value: buildUserId(testId) }],
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
  buildAddress,
  buildSiteDocument,
  buildSiteName,
  buildIcb,
  buildOdsCode,
  buildRegion,
  buildPhoneNumber,
  buildUserDocument,
  buildMockOidcUser,
  buildE2ETestSite,
  buildE2ETestUser,
};
