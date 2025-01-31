// We need this to avoid SSL errors when running tests locally
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

import { test as baseTest } from '@playwright/test';

export * from '@playwright/test';

export interface UserSeedData {
  Username: string;
  Password: string;
  SubjectId: string;
}

export const test = baseTest.extend<
  object,
  { newUserName: string; externalUserName: string }
>({
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

export const testuser8_emailId = `zzz_test_user_8@nhs.net`;
export const testuser9_emailId = `zzz_test_user_9@nhs.net`;
export const testuser10_emailId = `zzz_test_user_10@nhs.net`;
export const testuser11_emailId = `zzz_test_user_11@nhs.net`;
export const abc01_id = `5914b64a-66bb-4ee2-ab8a-94958c1fdfcb`;
