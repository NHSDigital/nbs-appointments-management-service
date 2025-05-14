import { test, expect } from '../fixtures';
import env from '../testEnvironment';

test('The build number is surfaced in the footer but is not visible', async ({
  signInToSite,
}) => {
  await signInToSite().then(async siteSelectionPage => {
    const expectedBuildNumberText = `Build number: ${env.BUILD_NUMBER}`;

    await expect(siteSelectionPage.buildNumber).not.toBeVisible();
    await expect(siteSelectionPage.buildNumber).toHaveText(
      expectedBuildNumberText,
    );
  });
});
