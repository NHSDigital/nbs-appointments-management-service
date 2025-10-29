import { test, expect } from '../fixtures-v2';

test('Temporary test to prove data generation works', async ({
  setUpSingleSite,
}) => {
  const { sitePage } = await setUpSingleSite();
  await expect(sitePage.title).toBeVisible();
});
