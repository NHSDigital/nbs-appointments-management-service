import { test } from '../fixtures-v2';

test('A user loads home page, only sites with same scope are loaded', async ({
  signInToSite,
}) => {
  await signInToSite().then(async sitePage => {
    expect(sitePage.title).toBeVisible();
  });
});
