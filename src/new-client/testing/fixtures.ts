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
