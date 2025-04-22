import { RootPage, OAuthLoginPage } from '@testing-page-objects';
import { test, expect } from '../fixtures';
import env from '../testEnvironment';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;

test.beforeEach(async ({ page }) => {
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
});

test('The build number is surfaced in the footer but is not visible', async () => {
  const expectedBuildNumberText = `Build number: ${env.BUILD_NUMBER}`;

  await expect(rootPage.buildNumber).not.toBeVisible();
  await expect(rootPage.buildNumber).toHaveText(expectedBuildNumberText);
});
