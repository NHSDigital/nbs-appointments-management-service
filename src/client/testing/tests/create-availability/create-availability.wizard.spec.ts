import { CreateAvailabilityWizardPage, LoginPage } from '@testing-page-objects';
import { test, expect } from '../../fixtures';
import { getDateInFuture } from '../../utils/date-utility';

let put: CreateAvailabilityWizardPage;

['UTC', 'Europe/London', 'Pacific/Kiritimati', 'Etc/GMT+12'].forEach(
  timezone => {
    test.describe(
      `Test in timezone: '${timezone}'`,
      { tag: [`@timezone-test-${timezone}`] },
      () => {
        test.use({ timezoneId: timezone });

        test.describe('Create Availability', () => {
          test.beforeEach(async ({ page, getTestSite }) => {
            put = await new LoginPage(page)
              .logInWithNhsMail()
              .then(oAuthPage => oAuthPage.signIn())
              .then(siteSelectionPage =>
                siteSelectionPage.selectSite(getTestSite(2)),
              )
              .then(sitePage => sitePage.clickCreateAvailabilityCard())
              .then(createAvailabilityPage =>
                createAvailabilityPage.clickCreateAvailabilityButton(),
              );
          });

          test(
            'A user creates a single date session for tomorrow',
            { tag: ['@affects:site1'] },
            async () => {
              const tomorrow = getDateInFuture(1);

              await expect(
                put.singleOrRepeatingSessionStep.title,
              ).toBeVisible();
              await put.singleOrRepeatingSessionStep.singleSessionRadio.check();
              await put.singleOrRepeatingSessionStep.continueButton.click();

              await expect(put.startAndEndDateStep.title).toBeVisible();
              await put.startAndEndDateStep.startDateDayInput.fill(
                `${tomorrow.day}`,
              );
              await put.startAndEndDateStep.startDateMonthInput.fill(
                `${tomorrow.month}`,
              );
              await put.startAndEndDateStep.startDateYearInput.fill(
                `${tomorrow.year}`,
              );
              await put.startAndEndDateStep.continueButton.click();

              await expect(put.timeAndCapacityStep.title).toBeVisible();
              await put.timeAndCapacityStep.startTimeHourInput.fill('09');
              await put.timeAndCapacityStep.startTimeMinuteInput.fill('00');
              await put.timeAndCapacityStep.endTimeHourInput.fill('10');
              await put.timeAndCapacityStep.endTimeMinuteInput.fill('00');
              await put.timeAndCapacityStep.capacityInput.fill('2');
              await put.timeAndCapacityStep.appointmentLengthInput.fill('6');
              await put.timeAndCapacityStep.continueButton.click();

              await expect(put.selectServicesStep.title).toBeVisible();
              await put.selectServicesStep.rsvCheckbox.check();
              await put.selectServicesStep.continueButton.click();

              await expect(put.summaryStep.title).toBeVisible();

              await expect(
                put.summaryStep.datesSummary.getByText(
                  `${tomorrow.day} ${tomorrow.month}`,
                  { exact: true },
                ),
              ).toBeVisible();
              await expect(
                put.summaryStep.timeSummary.getByText(`09:00 - 10:00`, {
                  exact: true,
                }),
              ).toBeVisible();
              await expect(
                put.summaryStep.capacitySummary.getByText(`2`, {
                  exact: true,
                }),
              ).toBeVisible();
              await expect(
                put.summaryStep.appointmentLengthSummary.getByText(
                  `6 minutes`,
                  {
                    exact: true,
                  },
                ),
              ).toBeVisible();
              await expect(
                put.summaryStep.servicesSummary.getByText(`RSV (Adult)`, {
                  exact: true,
                }),
              ).toBeVisible();

              const createAvailabilityPage =
                await put.summaryStep.saveSession();
              await expect(createAvailabilityPage.title).toBeVisible();
              await expect(
                createAvailabilityPage.notificationBanner,
              ).toHaveText('foobar');
            },
          );

          test(
            'A user creates a weekly repeating session on the next two days',
            { tag: ['@affects:site1'] },
            async () => {
              const tomorrow = getDateInFuture(1);
              const dayAfterTomorrowDate = getDateInFuture(2);

              await expect(
                put.singleOrRepeatingSessionStep.title,
              ).toBeVisible();
              await put.singleOrRepeatingSessionStep.weeklyRepeatingSessionRadio.check();
              await put.singleOrRepeatingSessionStep.continueButton.click();

              await expect(put.startAndEndDateStep.title).toBeVisible();
              await put.startAndEndDateStep.startDateDayInput.fill(
                `${tomorrow.day}`,
              );
              await put.startAndEndDateStep.startDateMonthInput.fill(
                `${tomorrow.month}`,
              );
              await put.startAndEndDateStep.startDateYearInput.fill(
                `${tomorrow.year}`,
              );
              await put.startAndEndDateStep.endDateDayInput.fill(
                `${dayAfterTomorrowDate.day}`,
              );
              await put.startAndEndDateStep.endDateMonthInput.fill(
                `${dayAfterTomorrowDate.month}`,
              );
              await put.startAndEndDateStep.endDateYearInput.fill(
                `${dayAfterTomorrowDate.year}`,
              );
              await put.startAndEndDateStep.continueButton.click();

              await expect(put.daysOfWeekStep.title).toBeVisible();
              await put.daysOfWeekStep.allDaysCheckbox.check();
              await put.daysOfWeekStep.continueButton.click();

              await expect(put.timeAndCapacityStep.title).toBeVisible();
              await put.timeAndCapacityStep.startTimeHourInput.fill('09');
              await put.timeAndCapacityStep.startTimeMinuteInput.fill('00');
              await put.timeAndCapacityStep.endTimeHourInput.fill('10');
              await put.timeAndCapacityStep.endTimeMinuteInput.fill('00');
              await put.timeAndCapacityStep.capacityInput.fill('1');
              await put.timeAndCapacityStep.appointmentLengthInput.fill('5');
              await put.timeAndCapacityStep.continueButton.click();

              await expect(put.selectServicesStep.title).toBeVisible();
              await put.selectServicesStep.rsvCheckbox.check();
              await put.selectServicesStep.continueButton.click();

              await expect(put.summaryStep.title).toBeVisible();

              await expect(
                put.summaryStep.datesSummary.getByText(
                  `${tomorrow.day} ${tomorrow.month}`,
                  { exact: true },
                ),
              ).toBeVisible();
              await expect(
                put.summaryStep.timeSummary.getByText(`09:00 - 10:00`, {
                  exact: true,
                }),
              ).toBeVisible();
              await expect(
                put.summaryStep.capacitySummary.getByText(`2`, {
                  exact: true,
                }),
              ).toBeVisible();
              await expect(
                put.summaryStep.appointmentLengthSummary.getByText(
                  `6 minutes`,
                  {
                    exact: true,
                  },
                ),
              ).toBeVisible();
              await expect(
                put.summaryStep.servicesSummary.getByText(`RSV (Adult)`, {
                  exact: true,
                }),
              ).toBeVisible();

              const createAvailabilityPage =
                await put.summaryStep.saveSession();
              await expect(createAvailabilityPage.title).toBeVisible();
              await expect(
                createAvailabilityPage.notificationBanner,
              ).toHaveText('foobar');
            },
          );

          test('A user uses the Change links to check each step of the Create Availability wizard before submitting - weekly session', async () => {
            const tomorrow = getDateInFuture(1);
            const dayAfterTomorrowDate = getDateInFuture(2);

            // Arrange & Act: comlete the wizard as far as the summary step
            await expect(put.singleOrRepeatingSessionStep.title).toBeVisible();
            await put.singleOrRepeatingSessionStep.weeklyRepeatingSessionRadio.check();
            await put.singleOrRepeatingSessionStep.continueButton.click();

            await expect(put.startAndEndDateStep.title).toBeVisible();
            await put.startAndEndDateStep.startDateDayInput.fill(
              `${tomorrow.day}`,
            );
            await put.startAndEndDateStep.startDateMonthInput.fill(
              `${tomorrow.month}`,
            );
            await put.startAndEndDateStep.startDateYearInput.fill(
              `${tomorrow.year}`,
            );
            await put.startAndEndDateStep.endDateDayInput.fill(
              `${dayAfterTomorrowDate.day}`,
            );
            await put.startAndEndDateStep.endDateMonthInput.fill(
              `${dayAfterTomorrowDate.month}`,
            );
            await put.startAndEndDateStep.endDateYearInput.fill(
              `${dayAfterTomorrowDate.year}`,
            );
            await put.startAndEndDateStep.continueButton.click();

            await expect(put.daysOfWeekStep.title).toBeVisible();
            await put.daysOfWeekStep.allDaysCheckbox.check();
            await put.daysOfWeekStep.continueButton.click();

            await expect(put.timeAndCapacityStep.title).toBeVisible();
            await put.timeAndCapacityStep.startTimeHourInput.fill('09');
            await put.timeAndCapacityStep.startTimeMinuteInput.fill('00');
            await put.timeAndCapacityStep.endTimeHourInput.fill('10');
            await put.timeAndCapacityStep.endTimeMinuteInput.fill('00');
            await put.timeAndCapacityStep.capacityInput.fill('1');
            await put.timeAndCapacityStep.appointmentLengthInput.fill('5');
            await put.timeAndCapacityStep.continueButton.click();

            await expect(put.selectServicesStep.title).toBeVisible();
            await put.selectServicesStep.rsvCheckbox.check();
            await put.selectServicesStep.continueButton.click();

            await expect(put.summaryStep.title).toBeVisible();

            await expect(
              put.summaryStep.datesSummary.getByText(
                `${tomorrow.day} ${tomorrow.month}`,
                { exact: true },
              ),
            ).toBeVisible();
            await expect(
              put.summaryStep.timeSummary.getByText(`09:00 - 10:00`, {
                exact: true,
              }),
            ).toBeVisible();
            await expect(
              put.summaryStep.capacitySummary.getByText(`2`, {
                exact: true,
              }),
            ).toBeVisible();
            await expect(
              put.summaryStep.appointmentLengthSummary.getByText(`6 minutes`, {
                exact: true,
              }),
            ).toBeVisible();
            await expect(
              put.summaryStep.servicesSummary.getByText(`RSV (Adult)`, {
                exact: true,
              }),
            ).toBeVisible();

            const createAvailabilityPage = await put.summaryStep.saveSession();
            await expect(createAvailabilityPage.title).toBeVisible();
            await expect(createAvailabilityPage.notificationBanner).toHaveText(
              'foobar',
            );

            // Assert: test each Change link visits the correct step and skips to summary on continue
            await put.summaryStep.changeDatesLink.click();
            await expect(put.startAndEndDateStep.title).toBeVisible();
            await put.startAndEndDateStep.continueButton.click();
            await expect(put.summaryStep.title).toBeVisible();

            await put.summaryStep.changeDaysLink.click();
            await expect(put.daysOfWeekStep.title).toBeVisible();
            await put.daysOfWeekStep.continueButton.click();
            await expect(put.summaryStep.title).toBeVisible();

            await put.summaryStep.changeTimeLink.click();
            await expect(put.timeAndCapacityStep.title).toBeVisible();
            await put.timeAndCapacityStep.continueButton.click();
            await expect(put.summaryStep.title).toBeVisible();

            await put.summaryStep.changeCapacityLink.click();
            await expect(put.timeAndCapacityStep.title).toBeVisible();
            await put.timeAndCapacityStep.continueButton.click();
            await expect(put.summaryStep.title).toBeVisible();

            await put.summaryStep.appointmentLengthSummary.click();
            await expect(put.timeAndCapacityStep.title).toBeVisible();
            await put.timeAndCapacityStep.continueButton.click();
            await expect(put.summaryStep.title).toBeVisible();

            await put.summaryStep.changeServicesLink.click();
            await expect(put.selectServicesStep.title).toBeVisible();
            await put.selectServicesStep.continueButton.click();
            await expect(put.summaryStep.title).toBeVisible();
          });

          test('A user uses the Change links to check each step of the Create Availability wizard before submitting - single session', async () => {
            const tomorrow = getDateInFuture(1);

            // Arrange & Act: comlete the wizard as far as the summary step
            await expect(put.singleOrRepeatingSessionStep.title).toBeVisible();
            await put.singleOrRepeatingSessionStep.singleSessionRadio.check();
            await put.singleOrRepeatingSessionStep.continueButton.click();

            await expect(put.startAndEndDateStep.title).toBeVisible();
            await put.startAndEndDateStep.startDateDayInput.fill(
              `${tomorrow.day}`,
            );
            await put.startAndEndDateStep.startDateMonthInput.fill(
              `${tomorrow.month}`,
            );
            await put.startAndEndDateStep.startDateYearInput.fill(
              `${tomorrow.year}`,
            );
            await put.startAndEndDateStep.continueButton.click();

            await expect(put.timeAndCapacityStep.title).toBeVisible();
            await put.timeAndCapacityStep.startTimeHourInput.fill('09');
            await put.timeAndCapacityStep.startTimeMinuteInput.fill('00');
            await put.timeAndCapacityStep.endTimeHourInput.fill('10');
            await put.timeAndCapacityStep.endTimeMinuteInput.fill('00');
            await put.timeAndCapacityStep.capacityInput.fill('2');
            await put.timeAndCapacityStep.appointmentLengthInput.fill('6');
            await put.timeAndCapacityStep.continueButton.click();

            await expect(put.selectServicesStep.title).toBeVisible();
            await put.selectServicesStep.rsvCheckbox.check();
            await put.selectServicesStep.continueButton.click();

            await expect(put.summaryStep.title).toBeVisible();

            await expect(
              put.summaryStep.datesSummary.getByText(
                `${tomorrow.day} ${tomorrow.month}`,
                { exact: true },
              ),
            ).toBeVisible();
            await expect(
              put.summaryStep.timeSummary.getByText(`09:00 - 10:00`, {
                exact: true,
              }),
            ).toBeVisible();
            await expect(
              put.summaryStep.capacitySummary.getByText(`2`, {
                exact: true,
              }),
            ).toBeVisible();
            await expect(
              put.summaryStep.appointmentLengthSummary.getByText(`6 minutes`, {
                exact: true,
              }),
            ).toBeVisible();
            await expect(
              put.summaryStep.servicesSummary.getByText(`RSV (Adult)`, {
                exact: true,
              }),
            ).toBeVisible();

            const createAvailabilityPage = await put.summaryStep.saveSession();
            await expect(createAvailabilityPage.title).toBeVisible();
            await expect(createAvailabilityPage.notificationBanner).toHaveText(
              'foobar',
            );

            // Assert: test each Change link visits the correct step and skips to summary on continue
            await put.summaryStep.changeDatesLink.click();
            await expect(put.startAndEndDateStep.title).toBeVisible();
            await put.startAndEndDateStep.continueButton.click();
            await expect(put.summaryStep.title).toBeVisible();

            await put.summaryStep.changeTimeLink.click();
            await expect(put.timeAndCapacityStep.title).toBeVisible();
            await put.timeAndCapacityStep.continueButton.click();
            await expect(put.summaryStep.title).toBeVisible();

            await put.summaryStep.changeCapacityLink.click();
            await expect(put.timeAndCapacityStep.title).toBeVisible();
            await put.timeAndCapacityStep.continueButton.click();
            await expect(put.summaryStep.title).toBeVisible();

            await put.summaryStep.appointmentLengthSummary.click();
            await expect(put.timeAndCapacityStep.title).toBeVisible();
            await put.timeAndCapacityStep.continueButton.click();
            await expect(put.summaryStep.title).toBeVisible();

            await put.summaryStep.changeServicesLink.click();
            await expect(put.selectServicesStep.title).toBeVisible();
            await put.selectServicesStep.continueButton.click();
            await expect(put.summaryStep.title).toBeVisible();
          });
        });
      },
    );
  },
);
