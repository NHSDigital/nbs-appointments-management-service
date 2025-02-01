// We need this to avoid SSL errors when running tests locally
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

import { test as baseTest } from '@playwright/test';

export * from '@playwright/test';

import testUsersDataRaw from '../../../mock-oidc/users.json';

const testUsersData: UserSeedData[] = testUsersDataRaw;

export interface UserSeedData {
  Username: string;
  Password: string;
  SubjectId: string;
}

export const userBySubjectId = (testUserId = 1) => {
  const zzzTestUser = testUsersData.find(
    x => x.SubjectId === `zzz_test_user_${testUserId}@nhs.net`,
  );

  if (!zzzTestUser) {
    throw Error('Integration test user not found in users seed file');
  }

  return zzzTestUser;
};

export const test = baseTest.extend<
  object,
  {
    getTestUser: (testUserId?: number) => UserSeedData;
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
  newUserName: [
    async ({}, use) => {
      const userName = `int-test-user-${test.info().workerIndex}@nhs.net`;
      await use(userName);
    },
    { scope: 'worker' },
  ],
  externalUserName: [
    async ({}, use) => {
      const nonNHSuserName = `external-user-${test.info().workerIndex}@gmail.com`;
      await use(nonNHSuserName);
    },
    { scope: 'worker' },
  ],
});

//TODO refactor sites
export const abc01_id = `5914b64a-66bb-4ee2-ab8a-94958c1fdfcb`;
