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
import {
  AddServicesPage,
  AddSessionPage,
  CheckSessionDetailsPage,
  MonthViewAvailabilityPage,
  WeekViewAvailabilityPage,
} from '@e2etests/page-objects';
import CancelDayForm from './page-objects-v2/cancel-day-pages/cancel-day-form';
import ConfirmedCancellationPage from './page-objects-v2/cancel-day-pages/confirm-cancellation';

type FixtureOptions = {
  roles?: Role[];
  features?: FeatureFlag[];
  additionalUsers?: AdditionalUserOptions[];
};

type AdditionalUserOptions = {
  roles?: Role[];
};

type SetUpSingleSiteFixtureOptions = {
  siteConfig?: Partial<SiteDocument>;
} & FixtureOptions;

type MyaFixtures = {
  setUpSingleSite: (options?: SetUpSingleSiteFixtureOptions) => Promise<{
    sitePage: SitePage;
    testId: number;
    additionalUserIds: Map<string, number>;
  }>;

  monthViewAvailabilityPage: MonthViewAvailabilityPage;
  weekViewAvailabilityPage: WeekViewAvailabilityPage;
  addSessionPage: AddSessionPage;
  addServicesPage: AddServicesPage;
  checkSessionDetailsPage: CheckSessionDetailsPage;
  cancelDayForm: CancelDayForm;
  confirmedCancellationPage: ConfirmedCancellationPage;
};

export const test = base.extend<MyaFixtures>({
  monthViewAvailabilityPage: async ({ page }, use) => {
    await use(new MonthViewAvailabilityPage(page));
  },
  weekViewAvailabilityPage: async ({ page }, use) => {
    await use(new WeekViewAvailabilityPage(page));
  },
  addSessionPage: async ({ page }, use) => {
    await use(new AddSessionPage(page));
  },
  addServicesPage: async ({ page }, use) => {
    await use(new AddServicesPage(page));
  },
  checkSessionDetailsPage: async ({ page }, use) => {
    await use(new CheckSessionDetailsPage(page));
  },
  cancelDayForm: async ({ page }, use) => {
    await use(new CancelDayForm(page));
  },
  confirmedCancellationPage: async ({ page }, use) => {
    await use(new ConfirmedCancellationPage(page));
  },

  // TODO: Extend this (or create new fixtures) to cover multiple sites and multiple users per site
  setUpSingleSite: async ({ page }, use, testInfo) => {
    const cosmosDbClient = new CosmosDbClient(
      env.COSMOS_ENDPOINT,
      env.COSMOS_TOKEN,
    );
    const mockOidcClient = new MockOidcClient(env.MOCK_OIDC_SERVER_BASE_URL);
    const featureFlagClient = new FeatureFlagClient(env.NBS_API_BASE_URL);

    const testId = generateUniqueTestId(testInfo);

    const defaultFixtureOptions: SetUpSingleSiteFixtureOptions = {
      roles: [
        'canned:availability-manager',
        'canned:appointment-manager',
        'canned:site-details-manager',
        'canned:user-manager',
      ],
    };

    let featuresUsed: FeatureFlag[] = [];
    const additionalUserIds = new Map<string, number>();

    // Fixture setup. Result of use() is piped to the test
    // This currently accepts a list of roles and feature flags.
    // In the future this will be extended to accept a list of users and sites to enable tests to request extra users and sites in their setup.
    // Alternatively, we can add extra fixtures to this list and make them each responsible for setup/teardown of a different thing. Tests can import multiple fixtures as needed.
    await use(async options => {
      const {
        roles = [],
        features,
        siteConfig,
        additionalUsers = [],
      } = {
        ...defaultFixtureOptions,
        ...options,
      };

      await cosmosDbClient.createSite(testId, siteConfig);
      await cosmosDbClient.createUser(testId, roles);
      await mockOidcClient.registerTestUser(testId);

      if (additionalUsers.length > 0) {
        let index = 1;

        for (const [key, user] of Object.entries(additionalUsers)) {
          const userTestId = Number(`${testId}${index++}`);

          await cosmosDbClient.createSite(userTestId, siteConfig);
          await cosmosDbClient.createUser(userTestId, user.roles ?? []);
          await mockOidcClient.registerTestUser(userTestId);
          additionalUserIds.set(key, userTestId);
        }
      }

      await Promise.all([
        features?.map(async feature => {
          featureFlagClient.overrideFeatureFlag(feature);
        }),
      ]);

      const sitePage = await new LoginPage(page)
        .logInWithNhsMail()
        .then(mockOidcLoginPage =>
          mockOidcLoginPage.signIn(buildE2ETestUser(testId)),
        )
        .then(siteSelectionPage =>
          siteSelectionPage.selectSite(buildE2ETestSite(testId)),
        );

      featuresUsed = features ?? [];

      return { sitePage, testId, additionalUserIds };
    });

    // Clean up the fixture.
    await Promise.all([
      await cosmosDbClient.deleteSite(testId),
      await cosmosDbClient.deleteUser(testId),
      ...Array.from(additionalUserIds.values()).map(id =>
        cosmosDbClient.deleteUser(id),
      ),
      //revert all flags if they were used in the enabled state
      featuresUsed.map(async feature => {
        if (feature.enabled) {
          featureFlagClient.overrideFeatureFlag({
            name: feature.name,
            enabled: false,
          });
        }
      }),
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
