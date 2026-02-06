import env from '../testEnvironment';
import { test, expect } from '../fixtures-v2';

test('The build number is surfaced in the footer but is not visible', async ({
  setUpSingleSite,
}) => {
  await setUpSingleSite()
  .then(async page => {
    const expectedBuildNumberText = `Build number: ${env.BUILD_NUMBER}`;
    
    await expect(page.sitePage.buildNumber).not.toBeVisible();
    await expect(page.sitePage.buildNumber).toHaveText(expectedBuildNumberText);
  });
});
