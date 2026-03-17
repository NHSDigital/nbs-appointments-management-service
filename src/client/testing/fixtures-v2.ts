// We need this to avoid SSL errors when running tests locally
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

import { test as base, TestInfo } from '@playwright/test';
import {
  buildMockOidcUser,
  buildSiteDocument,
  buildUserDocument,
  CosmosDbClient,
  FeatureFlagClient,
  MockOidcClient,
} from '@e2etests/data';
export * from '@playwright/test';
import { LoginPage, SitePage, SiteSelectionPage } from '@e2etests/page-objects';
import env from './testEnvironment';
import {
  Role,
  FeatureFlag,
  SiteDocument,
  UserDocument,
  MockOidcUser,
} from '@e2etests/types';
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
  userConfig?: Partial<UserDocument>;
};

type AdditionalUserOptions = {
  roles?: Role[];
};

type UserData = {
  document: UserDocument;
  oidc: MockOidcUser;
};

type AdditionalUserSetupData = {
  user: UserData;
  site: SiteDocument;
};

type setupFixtureOptions = {
  siteConfig?: Partial<SiteDocument>;
  skipSiteSelection?: boolean;
} & FixtureOptions;

type MyaFixtures = {
  setup: (options?: setupFixtureOptions) => Promise<{
    site: SiteDocument;
    user: UserData;
    sitePage: SitePage;
    //TODO restructure
    additionalUserData: Map<string, AdditionalUserSetupData>;
    testId: number;
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
  setup: async ({ page }, use, testInfo) => {
    const cosmosDbClient = new CosmosDbClient(
      env.COSMOS_ENDPOINT,
      env.COSMOS_TOKEN,
    );
    const mockOidcClient = new MockOidcClient(env.MOCK_OIDC_SERVER_BASE_URL);
    const featureFlagClient = new FeatureFlagClient(env.NBS_API_BASE_URL);

    const testId = generateUniqueTestId(testInfo);

    const defaultFixtureOptions: setupFixtureOptions = {
      roles: [
        'canned:availability-manager',
        'canned:appointment-manager',
        'canned:site-details-manager',
        'canned:user-manager',
      ],
    };

    let featuresUsed: FeatureFlag[] = [];
    let siteDocument: SiteDocument | undefined = undefined;
    let userDocument: UserDocument | undefined = undefined;
    const additionalUserData = new Map<string, AdditionalUserSetupData>();

    // Fixture setup. Result of use() is piped to the test
    // This currently accepts a list of roles and feature flags.
    // In the future this will be extended to accept a list of users and sites to enable tests to request extra users and sites in their setup.
    // Alternatively, we can add extra fixtures to this list and make them each responsible for setup/teardown of a different thing. Tests can import multiple fixtures as needed.
    await use(async options => {
      const {
        roles = [],
        features,
        siteConfig,
        userConfig, // Extract userConfig
        additionalUsers = [],
        skipSiteSelection = false, // Default to false so existing tests don't break
      } = {
        ...defaultFixtureOptions,
        ...options,
      };

      siteDocument = buildSiteDocument(testId, siteConfig);
      userDocument = buildUserDocument(testId, roles, userConfig);
      const oidcUser = buildMockOidcUser(testId);

      await cosmosDbClient.createSite(siteDocument);
      // Pass userConfig here!
      await cosmosDbClient.createUser(userDocument);
      await mockOidcClient.registerTestUser(oidcUser);

      if (additionalUsers.length > 0) {
        let index = 1;

        for (const [key, additionalUser] of Object.entries(additionalUsers)) {
          const userTestId = Number(`${testId}${index++}`);

          const newUserDocument = buildUserDocument(
            userTestId,
            additionalUser.roles ?? [],
          );
          const newOidcUser = buildMockOidcUser(userTestId);
          const newSiteDocument = buildSiteDocument(userTestId, siteConfig);

          await cosmosDbClient.createSite(newSiteDocument);
          await cosmosDbClient.createUser(newUserDocument);
          await mockOidcClient.registerTestUser(newOidcUser);
          additionalUserData.set(key, {
            user: { document: userDocument, oidc: newOidcUser },
            site: newSiteDocument,
          });
        }
      }

      await Promise.all([
        features?.map(async feature => {
          featureFlagClient.overrideFeatureFlag(feature);
        }),
      ]);

      const loginPage = new LoginPage(page);
      const mockOidcLoginPage = await loginPage.logInWithNhsMail();

      // 1. Manually fill fields since we can't use .signIn()
      await mockOidcLoginPage.usernameField.fill(oidcUser.username);
      await mockOidcLoginPage.passwordField.fill(oidcUser.password);
      await mockOidcLoginPage.passwordField.press('Enter');

      // 2. The Flexible Wait: This is the secret sauce.
      // It allows the app to go to either the EULA or the Sites page.
      await page.waitForURL(/\/sites|\/eula/);

      let sitePage: SitePage | undefined;

      // 3. Logic: Only try to select a site if we aren't on the EULA page
      if (page.url().includes('/eula')) {
        sitePage = undefined;
      } else if (skipSiteSelection) {
        sitePage = undefined;
      } else {
        // We use the existing SiteSelectionPage logic if we landed on /sites
        const selectionPage = new SiteSelectionPage(page);
        sitePage = await selectionPage.selectSite(siteDocument);
      }

      featuresUsed = features ?? [];

      // Type cast sitePage to satisfy tests that expect sitePage to be non-optional
      return {
        site: siteDocument,
        user: { document: userDocument, oidc: oidcUser },
        sitePage: sitePage as SitePage,
        testId,
        additionalUserData,
      };
    });

    // Clean up the fixture.
    await Promise.all([
      await cosmosDbClient.deleteSite(siteDocument),
      await cosmosDbClient.deleteUser(userDocument),
      ...Array.from(additionalUserData.values()).map(data =>
        cosmosDbClient.deleteUser(data.user.document),
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
