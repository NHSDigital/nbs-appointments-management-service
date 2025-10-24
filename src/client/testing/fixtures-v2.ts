// We need this to avoid SSL errors when running tests locally
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

import { test as base } from '@playwright/test';
import {
  buildE2ETestSite,
  buildE2ETestUser,
  CosmosDbClient,
  FeatureFlagClient,
  MockOidcClient,
} from '@e2etests/data';
export * from '@playwright/test';
import { LoginPage, SitePage } from '@e2etests/page-objects';
import env from './testEnvironment';
import { Role, FeatureFlag } from '@e2etests/types';

type MyaFixtures = {
  siteSelectionPage: (
    roles: Role[],
    features: FeatureFlag[],
  ) => Promise<SitePage>;
};

export const test = base.extend<MyaFixtures>({
  siteSelectionPage: async ({ page }, use, testInfo) => {
    const cosmosDbClient = new CosmosDbClient(
      env.COSMOS_ENDPOINT,
      env.COSMOS_TOKEN,
    );
    const mockOidcClient = new MockOidcClient(env.MOCK_OIDC_SERVER_BASE_URL);
    const featureFlagClient = new FeatureFlagClient(env.NBS_API_BASE_URL);

    // Fixture setup. Result of use() is piped to the test
    await use(async (roles, features) => {
      const testId = Number(`${testInfo.testId}${testInfo.line}`);

      await cosmosDbClient.createSite(testId);
      await cosmosDbClient.createUser(testId, roles);
      await mockOidcClient.registerTestUser(testId);
      features.forEach(async feature =>
        featureFlagClient.overrideFeatureFlag(feature),
      );

      return await new LoginPage(page)
        .logInWithNhsMail()
        .then(mockOidcLoginPage =>
          mockOidcLoginPage.signIn(buildE2ETestUser(testId)),
        )
        .then(siteSelectionPage =>
          siteSelectionPage.selectSite(buildE2ETestSite(testId)),
        );
    });

    // // Clean up the fixture.
    await featureFlagClient.clearAllFeatureFlagOverrides();
  },
});

export { expect } from '@playwright/test';
