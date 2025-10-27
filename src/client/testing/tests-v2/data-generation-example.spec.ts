import { test, expect } from '../fixtures-v2';

test('Temporary test to prove data generation works', async ({
  signInToSite,
}) => {
  await signInToSite().then(async sitePage => {
    // eslint-disable-next-line no-console
    console.log(
      'This log is coming from the example test to prove execution order',
    );
    expect(sitePage.title).toBeVisible();
  });
});
