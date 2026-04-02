import { timeZones } from '../../availability';
import { test, expect } from '../../fixtures-v2';
import { getDateInFuture } from '../../utils/date-utility';
import { SummaryPage } from '@e2etests/page-objects';

test.describe('Create Availability', () => {
  const timezones = timeZones;

  timezones.forEach(timezone => {
    test.describe(`Timezone: ${timezone}`, () => {
      test.use({ timezoneId: timezone });

      test('A user can navigate to the Create Availability flow from the site page', async ({
        setup,
      }) => {
        const { sitePage } = await setup();
        const createPage = await sitePage.clickCreateAvailabilityCard();
        await expect(createPage.title).toBeVisible();
      });

      test('Create single session of RSV availability', async ({
        setup,
        page,
      }) => {
        const { sitePage, site } = await setup();
        const futureDate = getDateInFuture(1);

        const createAvailabilityPage =
          await sitePage.clickCreateAvailabilityCard();
        await createAvailabilityPage.createAvailabilityButton.click();

        await page.waitForURL(`**/site/${site.id}/create-availability/wizard`);
        await expect(createAvailabilityPage.sessionTitle).toBeVisible();

        await createAvailabilityPage.selectSession('Single date session');
        await createAvailabilityPage.continueButton.click();
        await createAvailabilityPage.enterSingleDateSessionDate(
          futureDate.day,
          futureDate.month,
          futureDate.year,
        );
        await createAvailabilityPage.continueButton.click();
        await createAvailabilityPage.enterStartTime('09', '00');
        await createAvailabilityPage.enterEndTime('10', '00');
        await createAvailabilityPage.enterNoOfVaccinators('2');
        await createAvailabilityPage.appointmentLength('6');
        await createAvailabilityPage.continueButton.click();
        await createAvailabilityPage.addService('RSV Adult');
        await createAvailabilityPage.continueButton.click();
        await createAvailabilityPage.saveSessionButton.click();
        await expect(createAvailabilityPage.sessionSuccessMsg).toBeVisible();
      });

      test('Create single session of RSV and Covid availability', async ({
        setup,
        page,
      }) => {
        const { sitePage, site } = await setup();
        const futureDate = getDateInFuture(1);

        const createAvailabilityPage =
          await sitePage.clickCreateAvailabilityCard();
        await createAvailabilityPage.createAvailabilityButton.click();
        await page.waitForURL(`**/site/${site.id}/create-availability/wizard`);

        await createAvailabilityPage.selectSession('Single date session');
        await createAvailabilityPage.continueButton.click();
        await createAvailabilityPage.enterSingleDateSessionDate(
          futureDate.day,
          futureDate.month,
          futureDate.year,
        );
        await createAvailabilityPage.continueButton.click();
        await createAvailabilityPage.enterStartTime('09', '00');
        await createAvailabilityPage.enterEndTime('10', '00');
        await createAvailabilityPage.enterNoOfVaccinators('2');
        await createAvailabilityPage.appointmentLength('6');
        await createAvailabilityPage.continueButton.click();
        await createAvailabilityPage.addService('RSV Adult');
        await createAvailabilityPage.addService('COVID 5 to 11');
        await createAvailabilityPage.addService('COVID 18+');
        await createAvailabilityPage.continueButton.click();
        await createAvailabilityPage.saveSessionButton.click();
        await expect(createAvailabilityPage.sessionSuccessMsg).toBeVisible();
      });

      test('Create weekly session of RSV availability', async ({
        setup,
        page,
      }) => {
        const { sitePage, site } = await setup();
        const startDate = getDateInFuture(1);
        const endDate = getDateInFuture(2);

        const createAvailabilityPage =
          await sitePage.clickCreateAvailabilityCard();
        await createAvailabilityPage.createAvailabilityButton.click();
        await page.waitForURL(`**/site/${site.id}/create-availability/wizard`);

        await createAvailabilityPage.selectSession('Weekly sessions');
        await createAvailabilityPage.continueButton.click();
        await createAvailabilityPage.enterWeeklySessionStartDate(
          startDate.day,
          startDate.month,
          startDate.year,
        );
        await createAvailabilityPage.enterWeeklySessionEndDate(
          endDate.day,
          endDate.month,
          endDate.year,
        );
        await createAvailabilityPage.continueButton.click();
        await createAvailabilityPage.selectDay('Select all days');
        await createAvailabilityPage.continueButton.click();
        await createAvailabilityPage.enterStartTime('09', '00');
        await createAvailabilityPage.enterEndTime('10', '00');
        await createAvailabilityPage.enterNoOfVaccinators('1');
        await createAvailabilityPage.appointmentLength('5');
        await createAvailabilityPage.continueButton.click();
        await createAvailabilityPage.addService('RSV Adult');
        await createAvailabilityPage.addService('COVID 5 to 11');
        await createAvailabilityPage.addService('COVID 18+');
        await createAvailabilityPage.continueButton.click();
        await createAvailabilityPage.saveSessionButton.click();
        await expect(createAvailabilityPage.sessionSuccessMsg).toBeVisible();
      });

      test('Create weekly session of RSV and Covid availability', async ({
        setup,
        page,
      }) => {
        const { sitePage, site } = await setup();
        const startDate = getDateInFuture(1);
        const endDate = getDateInFuture(2);

        const createAvailabilityPage =
          await sitePage.clickCreateAvailabilityCard();
        await createAvailabilityPage.createAvailabilityButton.click();
        await page.waitForURL(`**/site/${site.id}/create-availability/wizard`);

        await createAvailabilityPage.selectSession('Weekly sessions');
        await createAvailabilityPage.continueButton.click();
        await createAvailabilityPage.enterWeeklySessionStartDate(
          startDate.day,
          startDate.month,
          startDate.year,
        );
        await createAvailabilityPage.enterWeeklySessionEndDate(
          endDate.day,
          endDate.month,
          endDate.year,
        );
        await createAvailabilityPage.continueButton.click();
        await createAvailabilityPage.selectDay('Select all days');
        await createAvailabilityPage.continueButton.click();
        await createAvailabilityPage.enterStartTime('09', '00');
        await createAvailabilityPage.enterEndTime('10', '00');
        await createAvailabilityPage.enterNoOfVaccinators('1');
        await createAvailabilityPage.appointmentLength('5');
        await createAvailabilityPage.continueButton.click();
        await createAvailabilityPage.addService('RSV Adult');
        await createAvailabilityPage.continueButton.click();
        await createAvailabilityPage.saveSessionButton.click();
        await expect(createAvailabilityPage.sessionSuccessMsg).toBeVisible();
      });

      test('Validate weekly Session start date must be within the next year error', async ({
        setup,
      }) => {
        const { sitePage } = await setup();
        const moreThanAYear = getDateInFuture(366);

        const createAvailabilityPage =
          await sitePage.clickCreateAvailabilityCard();
        await createAvailabilityPage.createAvailabilityButton.click();
        await createAvailabilityPage.selectSession('Weekly sessions');
        await createAvailabilityPage.continueButton.click();
        await createAvailabilityPage.enterWeeklySessionStartDate(
          moreThanAYear.day,
          moreThanAYear.month,
          moreThanAYear.year,
        );
        await createAvailabilityPage.enterWeeklySessionEndDate(
          moreThanAYear.day,
          moreThanAYear.month,
          moreThanAYear.year,
        );
        await createAvailabilityPage.continueButton.click();
        await expect(
          createAvailabilityPage.sessionStartDateErrorMsg,
        ).toBeVisible();
        await expect(
          createAvailabilityPage.sessionEndDateErrorMsg,
        ).toBeVisible();
      });

      test('Validate weekly Session end date must be within the next year error', async ({
        setup,
      }) => {
        const { sitePage } = await setup();
        const futureDate = getDateInFuture(1);
        const aYearFutureDate = getDateInFuture(366);

        const createAvailabilityPage =
          await sitePage.clickCreateAvailabilityCard();
        await createAvailabilityPage.createAvailabilityButton.click();
        await createAvailabilityPage.selectSession('Weekly sessions');
        await createAvailabilityPage.continueButton.click();
        await createAvailabilityPage.enterWeeklySessionStartDate(
          futureDate.day,
          futureDate.month,
          futureDate.year,
        );
        await createAvailabilityPage.enterWeeklySessionEndDate(
          aYearFutureDate.day,
          aYearFutureDate.month,
          aYearFutureDate.year,
        );
        await createAvailabilityPage.continueButton.click();
        await expect(
          createAvailabilityPage.sessionEndDateErrorMsg,
        ).toBeVisible();
      });

      test('Create weekly session of RSV availability check summary page links', async ({
        setup,
        page,
      }) => {
        const { sitePage, site } = await setup();
        const startDate = getDateInFuture(1);
        const endDate = getDateInFuture(2);
        const summaryPage = new SummaryPage(page, site);

        const createAvailabilityPage =
          await sitePage.clickCreateAvailabilityCard();
        await createAvailabilityPage.createAvailabilityButton.click();
        await createAvailabilityPage.selectSession('Weekly sessions');
        await createAvailabilityPage.continueButton.click();
        await createAvailabilityPage.enterWeeklySessionStartDate(
          startDate.day,
          startDate.month,
          startDate.year,
        );
        await createAvailabilityPage.enterWeeklySessionEndDate(
          endDate.day,
          endDate.month,
          endDate.year,
        );
        await createAvailabilityPage.continueButton.click();
        await createAvailabilityPage.selectDay('Select all days');
        await createAvailabilityPage.continueButton.click();
        await createAvailabilityPage.enterStartTime('09', '00');
        await createAvailabilityPage.enterEndTime('10', '00');
        await createAvailabilityPage.enterNoOfVaccinators('1');
        await createAvailabilityPage.appointmentLength('5');
        await createAvailabilityPage.continueButton.click();
        await createAvailabilityPage.addService('RSV Adult');
        await createAvailabilityPage.continueButton.click();

        const sections = [
          'Dates',
          'Days',
          'Time',
          'Vaccinators or vaccination spaces available',
          'Appointment length',
          'Services available',
        ];

        for (const section of sections) {
          await summaryPage.changeFunctionalityLink(section);
          // Navigate back to summary
          await createAvailabilityPage.continueButton.click();
          await expect(summaryPage.title).toBeVisible();
        }
      });

      test('Create single session of RSV availability check summary page links', async ({
        setup,
        page,
      }) => {
        const { sitePage, site } = await setup();
        const futureDate = getDateInFuture(1);
        const summaryPage = new SummaryPage(page, site);

        const createAvailabilityPage =
          await sitePage.clickCreateAvailabilityCard();
        await createAvailabilityPage.createAvailabilityButton.click();
        await createAvailabilityPage.selectSession('Single date session');
        await createAvailabilityPage.continueButton.click();
        await createAvailabilityPage.enterSingleDateSessionDate(
          futureDate.day,
          futureDate.month,
          futureDate.year,
        );
        await createAvailabilityPage.continueButton.click();
        await createAvailabilityPage.enterStartTime('09', '00');
        await createAvailabilityPage.enterEndTime('10', '00');
        await createAvailabilityPage.enterNoOfVaccinators('2');
        await createAvailabilityPage.appointmentLength('6');
        await createAvailabilityPage.continueButton.click();
        await createAvailabilityPage.addService('RSV Adult');
        await createAvailabilityPage.continueButton.click();

        const sections = [
          'Date',
          'Time',
          'Vaccinators or vaccination spaces available',
          'Appointment length',
          'Services available',
        ];
        for (const section of sections) {
          await summaryPage.changeFunctionalityLink(section);
          await createAvailabilityPage.continueButton.click();
        }
      });
    });
  });
});
