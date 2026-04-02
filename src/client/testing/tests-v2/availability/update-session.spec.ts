import { timeZones } from '../../availability';
import { test, expect } from '../../fixtures-v2';
import { daysFromToday, weekHeaderText } from '../../utils/date-utility';

test.describe('Update Session', () => {
  const timezones = timeZones;

  timezones.forEach(timezone => {
    test.describe(`Timezone: ${timezone}`, () => {
      test.use({ timezoneId: timezone });

      // ==========================================
      // TEST 1: ADD SESSION FOR FUTURE DATE (KEEP UI)
      // ==========================================
      test('Verify user is able to add a session for future date', async ({
        setup,
        page,
        monthViewAvailabilityPage,
        weekViewPage,
        createAvailabilityPage,
      }) => {
        const { site } = await setup();

        const dayIncrement = 1;
        const day = daysFromToday(dayIncrement);
        const requiredDate = daysFromToday(dayIncrement, 'dddd D MMMM');
        const requiredWeekRange = weekHeaderText(day);

        await page.goto(
          `/manage-your-appointments/site/${site.id}/view-availability?date=${day}`,
        );
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/view-availability?date=${day}`,
        );

        await monthViewAvailabilityPage.verifyViewMonthDisplayed(
          requiredWeekRange,
        );
        await monthViewAvailabilityPage.openWeekViewHavingDate(
          requiredWeekRange,
        );

        await page.waitForURL('**/site/**/view-availability/week?date=**');

        await weekViewPage.verifyDateCardDisplayed(requiredDate);
        await weekViewPage.addAvailability(requiredDate);

        await page.waitForURL('**/site/**/create-availability/wizard?date=**');

        await createAvailabilityPage.enterStartTime('09', '00');
        await createAvailabilityPage.enterEndTime('10', '00');
        await createAvailabilityPage.enterNoOfVaccinators('1');
        await createAvailabilityPage.appointmentLength('5');
        await createAvailabilityPage.continueButton.click();

        await createAvailabilityPage.addService('RSV Adult');
        await createAvailabilityPage.continueButton.click();

        await createAvailabilityPage.saveSessionButton.click();
        await page.waitForURL('**/site/**/view-availability/week?date=**');

        await weekViewPage.verifySessionAdded();
      });

      // ==========================================
      // TEST 2: ADD AVAILABILITY OPTION DISPLAYED (KEEP UI)
      // ==========================================
      test('Verify add availability option displayed for future date', async ({
        setup,
        page,
        monthViewAvailabilityPage,
        weekViewPage,
      }) => {
        const { site } = await setup();

        const dayIncrement = 7;
        const day = daysFromToday(dayIncrement);
        const requiredDate = daysFromToday(dayIncrement, 'dddd D MMMM');
        const requiredWeekRange = weekHeaderText(day);

        await page.goto(
          `/manage-your-appointments/site/${site.id}/view-availability?date=${day}`,
        );
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/view-availability?date=${day}`,
        );

        await monthViewAvailabilityPage.verifyViewMonthDisplayed(
          requiredWeekRange,
        );
        await monthViewAvailabilityPage.openWeekViewHavingDate(
          requiredWeekRange,
        );

        await page.waitForURL('**/site/**/view-availability/week?date=**');

        await weekViewPage.verifyDateCardDisplayed(requiredDate);
        await weekViewPage.verifyAddAvailabilityButtonDisplayed(requiredDate);
      });

      // ==========================================
      // TEST 3: CHANGE AVAILABILITY (SEEDED)
      // ==========================================
      test('Verify user is able to change availability', async ({
        setup,
        page,
        weekViewPage,
        createAvailabilityPage,
        changeAvailabilityPage,
        editAvailabilityConfirmationPage,
        editAvailabilityConfirmedPage,
      }) => {
        const dayIncrement = 1;
        const day = daysFromToday(dayIncrement);
        const requiredDate = daysFromToday(dayIncrement, 'dddd D MMMM');

        const { site } = await setup({
          availability: [
            {
              date: day,
              sessions: [
                {
                  from: '09:00',
                  until: '10:00',
                  services: ['RSV:Adult'],
                  slotLength: 5,
                  capacity: 5,
                },
              ],
            },
          ],
        });

        await page.goto(
          `/manage-your-appointments/site/${site.id}/view-availability/week?date=${day}`,
        );
        await page.waitForURL('**/site/**/view-availability/week?date=**');

        await weekViewPage.verifyDateCardDisplayed(requiredDate);
        await weekViewPage.openChangeAvailabilityPage(requiredDate);

        await page.waitForURL(
          '**/site/**/view-availability/week/edit-session?date=**',
        );

        await changeAvailabilityPage.selectChangeType('ChangeLengthCapacity');
        await changeAvailabilityPage.saveChanges();

        await page.waitForURL('**/site/**/availability/edit?session=**');

        await createAvailabilityPage.enterEndTime('09', '30');
        await createAvailabilityPage.continueButton.click();

        await page.waitForURL(
          '**/site/**/availability/edit/confirmation?session=**',
        );

        await editAvailabilityConfirmationPage.confirmSessionChange('Yes');

        await page.waitForURL(
          '**/site/**/availability/edit/confirmed?session=**',
        );

        await editAvailabilityConfirmedPage.verifySessionUpdated();
      });

      // ==========================================
      // TEST 4: REDUCE SERVICES (SEEDED)
      // ==========================================
      test('Verify user is able to reduce services for availability', async ({
        setup,
        page,
        weekViewPage,
        changeAvailabilityPage,
        editServicesPage,
        editServicesConfirmationPage,
        editServicesConfirmedPage,
      }) => {
        const dayIncrement = 7;
        const day = daysFromToday(dayIncrement);
        const requiredDate = daysFromToday(dayIncrement, 'dddd D MMMM');
        const requiredDateFormat = daysFromToday(dayIncrement, 'DD MMMM YYYY');

        const { site } = await setup({
          availability: [
            {
              date: day,
              sessions: [
                {
                  from: '09:00',
                  until: '10:00',
                  services: [
                    'RSV:Adult',
                    'COVID:18+',
                    'FLU:18_64',
                    'COVID_FLU:18_64',
                    'FLU:2_3',
                  ],
                  slotLength: 5,
                  capacity: 5,
                },
              ],
            },
          ],
        });

        await page.goto(
          `/manage-your-appointments/site/${site.id}/view-availability/week?date=${day}`,
        );
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/view-availability/week?date=${day}`,
        );

        await weekViewPage.verifyDateCardDisplayed(requiredDate);
        await weekViewPage.openChangeAvailabilityPage(requiredDate);

        await page.waitForURL(
          '**/site/**/view-availability/week/edit-session?date=**',
        );

        await changeAvailabilityPage.selectChangeType('ReduceServices');
        await changeAvailabilityPage.saveChanges();

        await page.waitForURL(
          '**/site/**/availability/edit-services?session=**',
        );

        await editServicesPage.verifyEditServicesPageDisplayed(
          requiredDateFormat,
          [
            'RSV Adult',
            'COVID 18+',
            'Flu 18 to 64',
            'Flu and COVID 18 to 64',
            'Flu 2 to 3',
          ],
        );
        await editServicesPage.removeServices(['RSV Adult', 'Flu 2 to 3']);

        await page.waitForURL(
          '**/site/**/availability/edit-services/confirmation?removedServicesSession=**',
        );

        await Promise.all([
          page.waitForURL(
            '**/site/**/availability/edit-services/confirmed?removedServicesSession=**',
          ),
          editServicesConfirmationPage.removeServicesButton.click(),
        ]);

        await editServicesConfirmedPage.verifyServicesRemoved({
          date: requiredDateFormat,
          serviceNames: 'Flu 2-3RSV Adult',
          sessionTimeInterval: '09:00 - 10:00',
        });

        await page.goto(
          `/manage-your-appointments/site/${site.id}/view-availability/week?date=${day}`,
        );
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/view-availability/week?date=${day}`,
        );

        await weekViewPage.verifySessionDataDisplayedInTheCorrectOrder({
          header: requiredDate,
          sessions: [
            {
              serviceName: 'COVID 18+Flu 18-64Flu and COVID 18-64',
              booked: '0 booked0 booked',
              unbooked: 60,
              sessionTimeInterval: '09:00 - 10:00',
            },
          ],
          totalAppointments: 60,
          orphaned: 0,
          booked: 0,
          unbooked: 60,
        });
      });

      // ==========================================
      // TEST 5: CANCEL SESSION (SEEDED)
      // ==========================================
      test('Verify user is able to cancel session', async ({
        setup,
        page,
        weekViewPage,
        changeAvailabilityPage,
        cancelSessionDetailsPage,
      }) => {
        const dayIncrement = 2;
        const day = daysFromToday(dayIncrement);
        const requiredDate = daysFromToday(dayIncrement, 'dddd D MMMM');

        const { site } = await setup({
          availability: [
            {
              date: day,
              sessions: [
                {
                  from: '09:00',
                  until: '10:00',
                  services: ['RSV:Adult'],
                  slotLength: 5,
                  capacity: 5,
                },
              ],
            },
          ],
        });

        await page.goto(
          `/manage-your-appointments/site/${site.id}/view-availability/week?date=${day}`,
        );
        await page.waitForURL('**/site/**/view-availability/week?date=**');

        await weekViewPage.verifyDateCardDisplayed(requiredDate);
        await weekViewPage.openChangeAvailabilityPage(requiredDate);

        await page.waitForURL(
          '**/site/**/view-availability/week/edit-session?date=**',
        );

        await changeAvailabilityPage.selectChangeType('CancelSession');
        await changeAvailabilityPage.saveChanges();

        await page.waitForURL(
          '**/site/**/availability/cancel/confirmation?session=**',
        );

        await cancelSessionDetailsPage.confirmSessionCancellation('Yes');

        await page.waitForURL(
          '**/site/**/availability/cancel/confirmed?session=**',
        );

        const cancelDate = daysFromToday(dayIncrement, 'dddd DD MMMM');
        await cancelSessionDetailsPage.verifySessionCancelled(cancelDate);
      });

      // ==========================================
      // TEST 6: CANCEL SESSION (ABORT / CLICK NO) (SEEDED)
      // ==========================================
      test('Verify session not canceled if not confirmed', async ({
        setup,
        page,
        weekViewPage,
        changeAvailabilityPage,
        cancelSessionDetailsPage,
      }) => {
        const dayIncrement = 3;
        const day = daysFromToday(dayIncrement);
        const requiredDate = daysFromToday(dayIncrement, 'dddd D MMMM');

        const { site } = await setup({
          availability: [
            {
              date: day,
              sessions: [
                {
                  from: '09:00',
                  until: '10:00',
                  services: ['RSV:Adult'],
                  slotLength: 5,
                  capacity: 5,
                },
              ],
            },
          ],
        });

        await page.goto(
          `/manage-your-appointments/site/${site.id}/view-availability/week?date=${day}`,
        );
        await page.waitForURL('**/site/**/view-availability/week?date=**');

        await weekViewPage.verifyDateCardDisplayed(requiredDate);
        await weekViewPage.openChangeAvailabilityPage(requiredDate);

        await page.waitForURL(
          '**/site/**/view-availability/week/edit-session?date=**',
        );

        await changeAvailabilityPage.selectChangeType('CancelSession');
        await changeAvailabilityPage.saveChanges();

        await page.waitForURL(
          '**/site/**/availability/cancel/confirmation?session=**',
        );

        await cancelSessionDetailsPage.confirmSessionCancellation('No');
        await changeAvailabilityPage.verifyChangeAvailabilityPageDisplayed(
          daysFromToday(dayIncrement, 'DD MMMM YYYY'),
        );
        await changeAvailabilityPage.backToWeekView();

        await page.waitForURL('**/site/**/view-availability/week?date=**');

        await weekViewPage.verifyFirstSessionRecordDetail(
          requiredDate,
          '09:00 - 10:00',
          'RSV Adult',
        );
      });

      // ==========================================
      // TEST 7: VIEW DAILY APPOINTMENTS LINK (SEEDED)
      // ==========================================
      test('Verify view daily appointment link displayed', async ({
        setup,
        page,
        weekViewPage,
        dailyAppointmentDetailsPage,
      }) => {
        const dayIncrement = 4;
        const day = daysFromToday(dayIncrement);
        const requiredDate = daysFromToday(dayIncrement, 'dddd D MMMM');

        const { site } = await setup({
          availability: [
            {
              date: day,
              sessions: [
                {
                  from: '09:00',
                  until: '10:00',
                  services: ['RSV:Adult'],
                  slotLength: 5,
                  capacity: 5,
                },
              ],
            },
          ],
        });

        await page.goto(
          `/manage-your-appointments/site/${site.id}/view-availability/week?date=${day}`,
        );
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/view-availability/week?date=${day}`,
        );

        await weekViewPage.verifyDateCardDisplayed(requiredDate);
        await weekViewPage.openDailyAppointmentPage(requiredDate);
        await dailyAppointmentDetailsPage.verifyDailyAppointmentDetailsPageDisplayed();
      });

      // ==========================================
      // TEST 8: APPOINTMENT CANCELLATION ERROR (SEEDED BOOKING)
      // ==========================================
      test('Verify appointment not cancelled when not confirmed', async ({
        setup,
        page,
        weekViewPage,
        dailyAppointmentDetailsPage,
      }) => {
        const dayIncrement = 5;
        const day = daysFromToday(dayIncrement);
        const requiredDate = daysFromToday(dayIncrement, 'dddd D MMMM');

        const { site } = await setup({
          availability: [
            {
              date: day,
              sessions: [
                {
                  from: '09:00',
                  until: '10:00',
                  services: ['RSV:Adult'],
                  slotLength: 5,
                  capacity: 5,
                },
              ],
            },
          ],
          bookings: [
            {
              fromDate: day,
              fromTime: '09:00:00',
              durationMins: 10,
              service: 'RSV:Adult',
              status: 'Booked',
              availabilityStatus: 'Unknown',
            },
          ],
        });

        await page.goto(
          `/manage-your-appointments/site/${site.id}/view-availability/week?date=${day}`,
        );
        await page.waitForURL('**/site/**/view-availability/week?date=**');

        await weekViewPage.verifyDateCardDisplayed(requiredDate);
        await weekViewPage.openDailyAppointmentPage(requiredDate);

        await dailyAppointmentDetailsPage.verifyDailyAppointmentDetailsPageDisplayed();

        // Target by time since seeded data doesn't give us a physical appointment ID
        await dailyAppointmentDetailsPage.cancelAppointment('09:00');
        await dailyAppointmentDetailsPage.cancelAppointmentButton.click();

        const errorMessage = page.locator('.nhsuk-error-message');
        await expect(errorMessage).toBeVisible();
        await expect(errorMessage).toContainText(
          'Select a reason for cancelling the appointment',
        );

        await dailyAppointmentDetailsPage.verifyAppointmentNotCancelled(
          '9:00am',
        );
      });

      // ==========================================
      // TEST 9: CANCEL SESSION WITH NO BOOKINGS (SEEDED)
      // ==========================================
      test('Verify availability with no bookings is cancelled and orphaned appointments message does not exist', async ({
        setup,
        page,
        weekViewPage,
        changeAvailabilityPage,
        cancelSessionDetailsPage,
      }) => {
        const dayIncrement = 6;
        const day = daysFromToday(dayIncrement);
        const requiredDate = daysFromToday(dayIncrement, 'dddd D MMMM');

        const { site } = await setup({
          availability: [
            {
              date: day,
              sessions: [
                {
                  from: '09:00',
                  until: '10:00',
                  services: ['RSV:Adult'],
                  slotLength: 5,
                  capacity: 5,
                },
              ],
            },
          ],
        });

        await page.goto(
          `/manage-your-appointments/site/${site.id}/view-availability/week?date=${day}`,
        );
        await page.waitForURL('**/site/**/view-availability/week?date=**');

        await weekViewPage.verifyDateCardDisplayed(requiredDate);
        await weekViewPage.openChangeAvailabilityPage(requiredDate);

        await page.waitForURL(
          '**/site/**/view-availability/week/edit-session?date=**',
        );

        await changeAvailabilityPage.selectChangeType('CancelSession');
        await changeAvailabilityPage.saveChanges();

        await page.waitForURL(
          '**/site/**/availability/cancel/confirmation?session=**',
        );

        // Verify the warning isn't on the page since we seeded no bookings!
        const orphanedWarning = page.getByText(/orphaned appointment/i);
        await expect(orphanedWarning).not.toBeVisible();

        // Finish the cancellation
        await cancelSessionDetailsPage.confirmSessionCancellation('Yes');

        await page.waitForURL(
          '**/site/**/availability/cancel/confirmed?session=**',
        );

        const cancelDate = daysFromToday(dayIncrement, 'dddd DD MMMM');
        await cancelSessionDetailsPage.verifySessionCancelled(cancelDate);
      });
    });
  });
});
