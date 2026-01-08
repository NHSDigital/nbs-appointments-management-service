// We need this to avoid SSL errors when running tests locally
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

import { test as base, TestInfo } from '@playwright/test';
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
import { Role, FeatureFlag, SiteDocument } from '@e2etests/types';

type FixtureOptions = {
  roles?: Role[];
  features?: FeatureFlag[];
  additionalUsers?: AdditionalUserOptions[];
};

type SiteSetupOptions = {
  siteConfig?: Partial<SiteDocument>;
  roles?: Role[];
  additionalUsers?: AdditionalUserOptions[];
};

type AdditionalUserOptions = {
  roles?: Role[];
};

type SetUpSingleSiteFixtureOptions = {
  siteConfig?: Partial<SiteDocument>;
} & FixtureOptions;

type SetUpSitesFixtureOptions = {
  sites?: SiteSetupOptions[];
  features?: FeatureFlag[];
  additionalUsers?: AdditionalUserOptions[];
};

type MyaFixtures = {
  setUpSingleSite: (options?: SetUpSingleSiteFixtureOptions) => Promise<{
    sitePage: SitePage;
    testId: number;
    additionalUserIds: Map<string, number>;
  }>;
  setUpSites: (options?: SetUpSitesFixtureOptions) => Promise<{
    sitePages: SitePage[];
    testIds: number[];
    additionalUserIds: Map<string, number>;
  }>;
};

export const test = base.extend<MyaFixtures>({
  setUpSingleSite: async ({ setUpSites }, use) => {
    await use(async options => {
      const {
        roles = [
          'canned:availability-manager',
          'canned:appointment-manager',
          'canned:site-details-manager',
          'canned:user-manager',
        ],
        features,
        siteConfig,
        additionalUsers = [],
      } = options ?? {};

      const { sitePages, testIds, additionalUserIds } = await setUpSites({
        sites: [
          {
            siteConfig,
            roles,
            additionalUsers,
          },
        ],
        features,
      });

      return {
        sitePage: sitePages[0],
        testId: testIds[0],
        additionalUserIds,
      };
    });
  },
  setUpSites: async ({ page }, use, testInfo) => {
    const cosmosDbClient = new CosmosDbClient(
      env.COSMOS_ENDPOINT,
      env.COSMOS_TOKEN,
    );
    const mockOidcClient = new MockOidcClient(env.MOCK_OIDC_SERVER_BASE_URL);
    const featureFlagClient = new FeatureFlagClient(env.NBS_API_BASE_URL);

    const testIds: number[] = [];
    const sitePages: SitePage[] = [];
    let featuresUsed: FeatureFlag[] = [];

    await use(async options => {
      const {
        sites = [{}],
        features = [],
        additionalUsers = [],
      } = options ?? {};

      featuresUsed = features;
      const additionalUserIds = new Map<string, number>();

      // Enable feature flags
      await Promise.all(
        features.map(feature => featureFlagClient.overrideFeatureFlag(feature)),
      );

      for (const site of sites) {
        const testId = generateUniqueTestId(testInfo);
        testIds.push(testId);

        await cosmosDbClient.createSite(testId, site.siteConfig);

        if (additionalUsers.length > 0) {
          let index = 1;

          for (const [key, user] of Object.entries(additionalUsers)) {
            const userTestId = Number(`${testId}${index++}`);

            await cosmosDbClient.createSite(userTestId, site.siteConfig);
            await cosmosDbClient.createUser(userTestId, user.roles ?? []);
            await mockOidcClient.registerTestUser(userTestId);
            additionalUserIds.set(key, userTestId);
          }
        }

        const sitePage = await new LoginPage(page)
          .logInWithNhsMail()
          .then(mockOidcLoginPage =>
            mockOidcLoginPage.signIn(buildE2ETestUser(testId)),
          )
          .then(siteSelectionPage =>
            siteSelectionPage.selectSite(buildE2ETestSite(testId)),
          );

        sitePages.push(sitePage);
      }

      return { sitePages, testIds, additionalUserIds };
    });

    // Cleanup
    await Promise.all([
      ...testIds.map(id => cosmosDbClient.deleteSite(id)),
      ...testIds.map(id => cosmosDbClient.deleteUser(id)),
      ...featuresUsed.map(feature =>
        feature.enabled
          ? featureFlagClient.overrideFeatureFlag({
              name: feature.name,
              enabled: false,
            })
          : Promise.resolve(),
      ),
    ]);
  },
});

// TODO: Improve this in the future
const generateUniqueTestId = (testInfo: TestInfo): number => {
  const testId = Number(
    `${testInfo.workerIndex}${Date.now()}${Math.floor(Math.random() * 1000)}`,
  );

  return testId;
};

export { expect } from '@playwright/test';
