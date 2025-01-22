// We need this to avoid SSL errors when running tests locally
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

import { test as baseTest } from '@playwright/test';

export * from '@playwright/test';
export const test = baseTest.extend<object, { newUserName: string }>({
  newUserName: [
    async ({}, use) => {
      const userName = `int-test-user-${test.info().workerIndex}@nhs.net`;
      await use(userName);
    },
    { scope: 'worker' },
  ],
});

export const nonNhsEmailId = `int-test-user-${Math.floor(Math.random() * 10000)}@gmail.com`;

export const testuser6_emailId = `zzz_test_user_6@nhs.net`;
