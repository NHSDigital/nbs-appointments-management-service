import { test, expect } from '@playwright/test';
import { NotFoundPage, OAuthLoginPage, RootPage } from '@testing-page-objects';

test('Invalid roots yield a styled 404 page', async ({ page }) => {
  const rootPage = new RootPage(page);
  const oAuthPage = new OAuthLoginPage(page);
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();

  await page.goto('/this-route-does-not-exist');
  const notFoundPage = new NotFoundPage(page);

  await expect(notFoundPage.title).toBeVisible();
  await expect(notFoundPage.notFoundMessageText).toBeVisible();
});
