import { test, expect } from '../fixtures';
import RootPage from '../page-objects/root';
import env from '../testEnvironment';

let rootPage: RootPage;

test.beforeEach(async ({ page }) => {
  rootPage = new RootPage(page);

  await rootPage.goto();
});

test('The build number is surfaced in the footer but is not visible', async () => {
  const expectedBuildNumberText = `Build number: ${env.BUILD_NUMBER}`;

  await expect(rootPage.buildNumber).not.toBeVisible();
  await expect(rootPage.buildNumber).toHaveText(expectedBuildNumberText);
});
