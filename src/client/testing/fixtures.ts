// We need this to avoid SSL errors when running tests locally
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

import { test as baseTest, request } from '@playwright/test';
import { Site } from '@types';

export * from '@playwright/test';

import testUsersDataRaw from '../../../mock-oidc/users.json';
import testSite1DataRaw from '../../../data/CosmosDbSeeder/items/local/core_data/site_ABC01.json';
import testSite2DataRaw from '../../../data/CosmosDbSeeder/items/local/core_data/site_ABC02.json';

const testUsersData: UserSeedDataRaw[] = testUsersDataRaw;
const testSite1Data: Site = testSite1DataRaw;
const testSite2Data: Site = testSite2DataRaw;

interface UserSeedDataRaw {
  Username: string;
  Password: string;
  SubjectId: string;
}

export interface UserSeedData {
  username: string;
  password: string;
  subjectId: string;
}

export const userBySubjectId = (testUserId = 1): UserSeedData => {
  const zzzTestUser = testUsersData.find(
    x => x.SubjectId === `zzz_test_user_${testUserId}@nhs.net`,
  );

  if (!zzzTestUser) {
    throw Error('Test user not found in users seed file');
  }

  return {
    subjectId: zzzTestUser.SubjectId,
    username: zzzTestUser.Username,
    password: zzzTestUser.Password,
  };
};

export async function overrideFeatureFlag(
  featureFlag: string,
  enabled: boolean,
) {
  const api = await request.newContext();
  await api.patch(
    `${process.env.NBS_API_BASE_URL}/api/feature-flag-override/${featureFlag}?enabled=${enabled}`,
  );
}

export async function clearAllFeatureFlagOverrides() {
  const api = await request.newContext();
  await api.patch(
    `${process.env.NBS_API_BASE_URL}/api/feature-flag-overrides-clear`,
  );
}

const siteById = (testSiteId = 1) => {
  switch (testSiteId) {
    case 1:
      return testSite1Data;
    case 2:
      return testSite2Data;
    default:
      throw Error('Test site not found in local sites seed files');
  }
};

export const test = baseTest.extend<
  object,
  {
    getTestUser: (testUserId?: number) => UserSeedData;
    getTestSite: (testSiteId?: number) => Site;
    newUserName: string;
    externalUserName: string;
  }
>({
  getTestUser: [
    ({}, use) => {
      use(userBySubjectId);
    },
    { scope: 'worker' },
  ],
  getTestSite: [
    ({}, use) => {
      use(siteById);
    },
    { scope: 'worker' },
  ],
  newUserName: [
    async ({}, use) => {
      const userName = `int-test-user-${test.info().workerIndex}@nhs.net`;
      await use(userName);
    },
    { scope: 'worker' },
  ],
  externalUserName: [
    async ({}, use) => {
      const nonNhsUserName = `external-user-${test.info().workerIndex}@boots.com`;
      await use(nonNhsUserName);
    },
    { scope: 'worker' },
  ],
});
