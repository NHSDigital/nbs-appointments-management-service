import { test, expect } from '@playwright/test';
import { NotFoundPage, LoginPage } from '@testing-page-objects';

test('Invalid roots yield a styled 404 page', async ({ page }) => {
  (await new LoginPage(page).logInWithNhsMail()).signIn();

  await page.goto('/this-route-does-not-exist');
  const notFoundPage = new NotFoundPage(page);

  await expect(notFoundPage.title).toBeVisible();
  await expect(notFoundPage.notFoundMessageText).toBeVisible();
});
