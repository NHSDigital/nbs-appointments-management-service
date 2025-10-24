import { test } from '../fixtures-v2';

test('Temporary test to prove data generation works', async ({
  signInToSite,
}) => {
  await signInToSite().then(async sitePage => {
    expect(sitePage.title).toBeVisible();
  });
});
