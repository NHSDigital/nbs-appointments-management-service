import { test, expect } from '../fixtures-v2';
import { NotFoundPage } from '@e2etests/page-objects';

test('Invalid roots yield a styled 404 page', async ({ 
  page, 
  setUpSingleSite 
}) => {
  // Skip site selection because we just need an authenticated session.
  await setUpSingleSite({ skipSiteSelection: true });

  // Navigate to the invalid route
  await page.goto('/this-route-does-not-exist');
  const notFoundPage = new NotFoundPage(page);

  await expect(notFoundPage.title).toBeVisible();
  await expect(notFoundPage.notFoundMessageText).toBeVisible();
});