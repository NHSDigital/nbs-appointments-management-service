// We need this to avoid SSL errors when running tests locally
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

import { test as base, TestInfo } from '@playwright/test';
import {
  buildBookingDocument,
  buildDailyAvailabilityDocument,
  buildMockOidcUser,
  buildSiteDocument,
  buildUserDocument,
  CosmosDbClient,
  FeatureFlagClient,
  generateExtraSiteIdForSameUser,
  MockOidcClient,
} from '@e2etests/data';
export * from '@playwright/test';
import {
  LoginPage,
  SitePage,
  SiteSelectionPage,
  AddServicesPage,
  AddSessionPage,
  CheckSessionDetailsPage,
  MonthViewAvailabilityPage,
  WeekViewAvailabilityPage,
  CreateAvailabilityPage,
  ChangeAvailabilityPage,
  DailyAppointmentDetailsPage,
  EditAvailabilityConfirmationPage,
  EditAvailabilityConfirmedPage,
  CancelSessionDetailsPage,
  EditServicesPage,
  EditServicesConfirmationPage,
  EditServicesConfirmedPage,
  CancelAppointmentDetailsPage,
} from '@e2etests/page-objects';
import env from './testEnvironment';
import {
  Role,
  FeatureFlag,
  SiteDocument,
  UserDocument,
  MockOidcUser,
  BookingDocument,
  DailyAvailabilityDocument,
  AttendeeDetails,
} from '@e2etests/types';
import CancelDayForm from './page-objects-v2/cancel-day-pages/cancel-day-form';
import ConfirmedCancellationPage from './page-objects-v2/cancel-day-pages/confirm-cancellation';
import { AvailabilityStatus, BookingStatus } from '@types';

type FixtureOptions = {
  roles?: Role[];
  features?: FeatureFlag[];
  additionalUsers?: AdditionalUserOptions[];
  userConfig?: Partial<UserDocument>;
  bookings?: BookingSetup[];
  availability?: AvailabilitySetup[];
};

type AdditionalUserOptions = {
  //each array item is a different site, 0-indexed
  //and the role array is the roles for this user for that site
  siteRoles: Role[][];
};

type UserData = {
  document: UserDocument;
  oidc: MockOidcUser;
};

export type BookingSetup = {
  fromDate: string;
  fromTime: string;
  durationMins: number;
  service: string;
  status: BookingStatus;
  availabilityStatus: AvailabilityStatus;
  attendeeDetails?: AttendeeDetails;
};

export type AvailabilitySetup = {
  date: string;
  sessions: SessionSetup[];
};

export type SessionSetup = {
  from: string;
  until: string;
  services: string[];
  slotLength: number;
  capacity: number;
};

type AdditionalUserSetupData = {
  user: UserData;
  sites: SiteDocument[];
};

type setupFixtureOptions = {
  siteConfig?: Partial<SiteDocument>;
  skipSiteSelection?: boolean;
} & FixtureOptions;

type MyaFixtures = {
  setup: (options?: setupFixtureOptions) => Promise<{
    site: SiteDocument;
    bookings: BookingDocument[] | undefined;
    availability: DailyAvailabilityDocument[] | undefined;
    user: UserData;
    sitePage: SitePage;
    testId: number;
    //TODO additional user data
    additionalUserData: Map<string, AdditionalUserSetupData>;
  }>;

  monthViewAvailabilityPage: MonthViewAvailabilityPage;
  weekViewAvailabilityPage: WeekViewAvailabilityPage;
  addSessionPage: AddSessionPage;
  addServicesPage: AddServicesPage;
  checkSessionDetailsPage: CheckSessionDetailsPage;
  cancelDayForm: CancelDayForm;
  confirmedCancellationPage: ConfirmedCancellationPage;
  weekViewPage: WeekViewAvailabilityPage;
  createAvailabilityPage: CreateAvailabilityPage;
  changeAvailabilityPage: ChangeAvailabilityPage;
  dailyAppointmentDetailsPage: DailyAppointmentDetailsPage;
  cancelAppointmentDetailsPage: CancelAppointmentDetailsPage;
  editAvailabilityConfirmationPage: EditAvailabilityConfirmationPage;
  editAvailabilityConfirmedPage: EditAvailabilityConfirmedPage;
  cancelSessionDetailsPage: CancelSessionDetailsPage;
  editServicesPage: EditServicesPage;
  editServicesConfirmationPage: EditServicesConfirmationPage;
  editServicesConfirmedPage: EditServicesConfirmedPage;
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
  weekViewPage: async ({ page }, use) => {
    await use(new WeekViewAvailabilityPage(page));
  },
  createAvailabilityPage: async ({ page }, use) => {
    await use(new CreateAvailabilityPage(page));
  },
  changeAvailabilityPage: async ({ page }, use) => {
    await use(new ChangeAvailabilityPage(page));
  },
  dailyAppointmentDetailsPage: async ({ page }, use) => {
    await use(new DailyAppointmentDetailsPage(page));
  },
  cancelAppointmentDetailsPage: async ({ page }, use) => {
    await use(new CancelAppointmentDetailsPage(page));
  },
  editAvailabilityConfirmationPage: async ({ page }, use) => {
    await use(new EditAvailabilityConfirmationPage(page));
  },
  editAvailabilityConfirmedPage: async ({ page }, use) => {
    await use(new EditAvailabilityConfirmedPage(page));
  },
  cancelSessionDetailsPage: async ({ page }, use) => {
    await use(new CancelSessionDetailsPage(page));
  },
  editServicesPage: async ({ page }, use) => {
    await use(new EditServicesPage(page));
  },
  editServicesConfirmationPage: async ({ page }, use) => {
    await use(new EditServicesConfirmationPage(page));
  },
  editServicesConfirmedPage: async ({ page }, use) => {
    await use(new EditServicesConfirmedPage(page));
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
    const bookingDocuments: BookingDocument[] = [];
    const dailyAvailabilityDocuments: DailyAvailabilityDocument[] = [];

    const additionalUserData = new Map<string, AdditionalUserSetupData>();

    await use(async options => {
      const {
        roles = [],
        features,
        siteConfig,
        bookings,
        availability,
        userConfig,
        additionalUsers = [],
        skipSiteSelection = false, // Default to false so existing tests don't break
      } = {
        ...defaultFixtureOptions,
        ...options,
      };

      siteDocument = buildSiteDocument(testId, siteConfig);
      userDocument = buildUserDocument(testId, [roles], userConfig);
      const oidcUser = buildMockOidcUser(testId);

      if (bookings !== undefined) {
        for (let index = 0; index < bookings.length; index++) {
          const data = bookings[index];

          bookingDocuments.push(
            buildBookingDocument(
              testId,
              index,
              siteDocument.id,
              data?.fromDate,
              data?.fromTime,
              data.durationMins,
              data.service,
              data.status,
              data.availabilityStatus,
              data.attendeeDetails,
            ),
          );
        }
      }

      if (availability !== undefined) {
        for (let index = 0; index < availability.length; index++) {
          const data = availability[index];

          dailyAvailabilityDocuments.push(
            buildDailyAvailabilityDocument(siteDocument.id, data),
          );
        }
      }

      await cosmosDbClient.createSite(siteDocument);
      await cosmosDbClient.createUser(userDocument);
      await mockOidcClient.registerTestUser(oidcUser);

      await cosmosDbClient.createBookings(bookingDocuments);
      await cosmosDbClient.createAvailability(dailyAvailabilityDocuments);

      if (additionalUsers.length > 0) {
        let index = 1;

        for (const [key, additionalUser] of Object.entries(additionalUsers)) {
          const additionalUserTestId = Number(`${testId}${index++}`);

          const siteRoles = additionalUser.siteRoles ?? [[]];

          const additionalUserDocument = buildUserDocument(
            additionalUserTestId,
            siteRoles,
          );
          const additionalOidcUser = buildMockOidcUser(additionalUserTestId);

          const additionalSiteDocuments: SiteDocument[] = [];

          for (
            let siteRoleIndex = 0;
            siteRoleIndex < siteRoles.length;
            siteRoleIndex++
          ) {
            const siteTestId = generateExtraSiteIdForSameUser(
              additionalUserTestId,
              siteRoleIndex,
            );

            const additionalSiteDocument = buildSiteDocument(
              siteTestId,
              siteConfig,
            );

            await cosmosDbClient.createSite(additionalSiteDocument);
            additionalSiteDocuments.push(additionalSiteDocument);
          }

          await cosmosDbClient.createUser(additionalUserDocument);
          await mockOidcClient.registerTestUser(additionalOidcUser);
          additionalUserData.set(key, {
            user: {
              document: additionalUserDocument,
              oidc: additionalOidcUser,
            },
            sites: additionalSiteDocuments,
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
        bookings: bookingDocuments,
        availability: dailyAvailabilityDocuments,
        user: { document: userDocument, oidc: oidcUser },
        sitePage: sitePage as SitePage,
        testId,
        additionalUserData,
      };
    });

    // Clean up the fixture.
    await Promise.all([
      cosmosDbClient.deleteSite(siteDocument),
      cosmosDbClient.deleteUser(userDocument),
      ...Array.from(additionalUserData.values()).map(data => {
        const additionalUserCleanup = [];
        additionalUserCleanup.push(
          cosmosDbClient.deleteUser(data.user.document),
        );
        for (let siteIndex = 0; siteIndex < data.sites.length; siteIndex++) {
          additionalUserCleanup.push(
            cosmosDbClient.deleteSite(data.sites[siteIndex]),
          );
        }
        return additionalUserCleanup;
      }),

      cosmosDbClient.deleteAllBookings(bookingDocuments),
      cosmosDbClient.deleteAvailability(dailyAvailabilityDocuments),

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
