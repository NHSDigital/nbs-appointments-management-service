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
          test(
            'A user creates a single date session for tomorrow',
            { tag: ['@affects:site1', `@timezone-test-${timezone}`] },
            async ({ signInToSite }) => {
              const tomorrow = getDateInFuture(1);
              test.info().title = `${test.info().title} - ${timezone}`;

              await signInToSite(1, 2)
                .then(sitePage => sitePage.clickCreateAvailabilityCard())
                .then(createAvailabilityPage =>
                  createAvailabilityPage.clickCreateAvailabilityButton(),
                )
                .then(async wizard => {
                  await expect(
                    wizard.singleOrRepeatingSessionStep.title,
                  ).toBeVisible();
                  await wizard.singleOrRepeatingSessionStep.singleSessionRadio.check();
                  await wizard.singleOrRepeatingSessionStep.continueButton.click();

                  await expect(
                    wizard.startAndEndDateStep.singleSessionTitle,
                  ).toBeVisible();
                  await wizard.startAndEndDateStep.singleDateDayInput.fill(
                    `${tomorrow.day}`,
                  );
                  await wizard.startAndEndDateStep.singleDateMonthInput.fill(
                    `${tomorrow.month}`,
                  );
                  await wizard.startAndEndDateStep.singleDateYearInput.fill(
                    `${tomorrow.year}`,
                  );
                  await wizard.startAndEndDateStep.continueButton.click();

                  await expect(wizard.timeAndCapacityStep.title).toBeVisible();
                  await wizard.timeAndCapacityStep.startTimeHourInput.fill(
                    '09',
                  );
                  await wizard.timeAndCapacityStep.startTimeMinuteInput.fill(
                    '00',
                  );
                  await wizard.timeAndCapacityStep.endTimeHourInput.fill('10');
                  await wizard.timeAndCapacityStep.endTimeMinuteInput.fill(
                    '00',
                  );
                  await wizard.timeAndCapacityStep.capacityInput.fill('2');
                  await wizard.timeAndCapacityStep.appointmentLengthInput.fill(
                    '6',
                  );
                  await wizard.timeAndCapacityStep.continueButton.click();

                  await expect(wizard.selectServicesStep.title).toBeVisible();
                  await wizard.selectServicesStep.rsvCheckbox.check();
                  await wizard.selectServicesStep.continueButton.click();

                  await expect(
                    wizard.summaryStep.singleDateSessionTitle,
                  ).toBeVisible();

                  await expect(
                    wizard.summaryStep.dateSummary.getByText(
                      `${tomorrow.day} ${tomorrow.monthName} ${tomorrow.year}`,
                    ),
                  ).toBeVisible();
                  await expect(
                    wizard.summaryStep.timeSummary.getByText(`09:00 - 10:00`),
                  ).toBeVisible();
                  await expect(
                    wizard.summaryStep.capacitySummary.getByText(`2`),
                  ).toBeVisible();
                  await expect(
                    wizard.summaryStep.appointmentLengthSummary.getByText(
                      `6 minutes`,
                    ),
                  ).toBeVisible();
                  await expect(
                    wizard.summaryStep.servicesSummary.getByText(`RSV (Adult)`),
                  ).toBeVisible();

                  return wizard.summaryStep.saveSession();
                })
                .then(async createAvailabilityPage => {
                  await expect(createAvailabilityPage.title).toBeVisible();
                  await expect(
                    createAvailabilityPage.notificationBanner.getByText(
                      'You have successfully created availability for the current site.',
                    ),
                  ).toBeVisible();
                });
            },
          );

          test(
            'A user creates a weekly repeating session on the next two days',
            { tag: ['@affects:site1'] },
            async ({ signInToSite }) => {
              const tomorrow = getDateInFuture(1);
              const dayAfterTomorrowDate = getDateInFuture(2);

              await signInToSite(1, 2)
                .then(sitePage => sitePage.clickCreateAvailabilityCard())
                .then(createAvailabilityPage =>
                  createAvailabilityPage.clickCreateAvailabilityButton(),
                )
                .then(async wizard => {
                  await expect(
                    wizard.singleOrRepeatingSessionStep.title,
                  ).toBeVisible();
                  await wizard.singleOrRepeatingSessionStep.weeklyRepeatingSessionRadio.check();
                  await wizard.singleOrRepeatingSessionStep.continueButton.click();

                  await expect(
                    wizard.startAndEndDateStep.repeatingSessionTitle,
                  ).toBeVisible();
                  await wizard.startAndEndDateStep.startDateDayInput.fill(
                    `${tomorrow.day}`,
                  );
                  await wizard.startAndEndDateStep.startDateMonthInput.fill(
                    `${tomorrow.month}`,
                  );
                  await wizard.startAndEndDateStep.startDateYearInput.fill(
                    `${tomorrow.year}`,
                  );
                  await wizard.startAndEndDateStep.endDateDayInput.fill(
                    `${dayAfterTomorrowDate.day}`,
                  );
                  await wizard.startAndEndDateStep.endDateMonthInput.fill(
                    `${dayAfterTomorrowDate.month}`,
                  );
                  await wizard.startAndEndDateStep.endDateYearInput.fill(
                    `${dayAfterTomorrowDate.year}`,
                  );
                  await wizard.startAndEndDateStep.continueButton.click();

                  await expect(wizard.daysOfWeekStep.title).toBeVisible();
                  await wizard.daysOfWeekStep.allDaysCheckbox.check();
                  await wizard.daysOfWeekStep.continueButton.click();

                  await expect(wizard.timeAndCapacityStep.title).toBeVisible();
                  await wizard.timeAndCapacityStep.startTimeHourInput.fill(
                    '09',
                  );
                  await wizard.timeAndCapacityStep.startTimeMinuteInput.fill(
                    '00',
                  );
                  await wizard.timeAndCapacityStep.endTimeHourInput.fill('10');
                  await wizard.timeAndCapacityStep.endTimeMinuteInput.fill(
                    '00',
                  );
                  await wizard.timeAndCapacityStep.capacityInput.fill('1');
                  await wizard.timeAndCapacityStep.appointmentLengthInput.fill(
                    '5',
                  );
                  await wizard.timeAndCapacityStep.continueButton.click();

                  await expect(wizard.selectServicesStep.title).toBeVisible();
                  await wizard.selectServicesStep.rsvCheckbox.check();
                  await wizard.selectServicesStep.continueButton.click();

                  await expect(
                    wizard.summaryStep.repeatingSessionTitle,
                  ).toBeVisible();

                  await expect(
                    wizard.summaryStep.datesSummary.getByText(
                      `${tomorrow.day} ${tomorrow.monthName} ${tomorrow.year} - ${dayAfterTomorrowDate.day} ${dayAfterTomorrowDate.monthName} ${dayAfterTomorrowDate.year}`,
                    ),
                  ).toBeVisible();
                  await expect(
                    wizard.summaryStep.timeSummary.getByText(`09:00 - 10:00`),
                  ).toBeVisible();
                  await expect(
                    wizard.summaryStep.capacitySummary.getByText(`1`),
                  ).toBeVisible();
                  await expect(
                    wizard.summaryStep.appointmentLengthSummary.getByText(
                      `5 minutes`,
                    ),
                  ).toBeVisible();
                  await expect(
                    wizard.summaryStep.servicesSummary.getByText(`RSV (Adult)`),
                  ).toBeVisible();

                  return wizard.summaryStep.saveSession();
                })
                .then(async createAvailabilityPage => {
                  await expect(createAvailabilityPage.title).toBeVisible();
                  await expect(
                    createAvailabilityPage.notificationBanner.getByText(
                      'You have successfully created availability for the current site.',
                    ),
                  ).toBeVisible();
                });
            },
          );

          test('A user uses the Change links to check each step of the Create Availability wizard before submitting - weekly session', async ({
            signInToSite,
          }) => {
            const tomorrow = getDateInFuture(1);
            const dayAfterTomorrowDate = getDateInFuture(2);

            // Arrange & Act: comlete the wizard as far as the summary step
            await signInToSite(1, 2)
              .then(sitePage => sitePage.clickCreateAvailabilityCard())
              .then(createAvailabilityPage =>
                createAvailabilityPage.clickCreateAvailabilityButton(),
              )
              .then(async wizard => {
                await expect(
                  wizard.singleOrRepeatingSessionStep.title,
                ).toBeVisible();
                await wizard.singleOrRepeatingSessionStep.weeklyRepeatingSessionRadio.check();
                await wizard.singleOrRepeatingSessionStep.continueButton.click();

                await expect(
                  wizard.startAndEndDateStep.repeatingSessionTitle,
                ).toBeVisible();
                await wizard.startAndEndDateStep.startDateDayInput.fill(
                  `${tomorrow.day}`,
                );
                await wizard.startAndEndDateStep.startDateMonthInput.fill(
                  `${tomorrow.month}`,
                );
                await wizard.startAndEndDateStep.startDateYearInput.fill(
                  `${tomorrow.year}`,
                );
                await wizard.startAndEndDateStep.endDateDayInput.fill(
                  `${dayAfterTomorrowDate.day}`,
                );
                await wizard.startAndEndDateStep.endDateMonthInput.fill(
                  `${dayAfterTomorrowDate.month}`,
                );
                await wizard.startAndEndDateStep.endDateYearInput.fill(
                  `${dayAfterTomorrowDate.year}`,
                );
                await wizard.startAndEndDateStep.continueButton.click();

                await expect(wizard.daysOfWeekStep.title).toBeVisible();
                await wizard.daysOfWeekStep.allDaysCheckbox.check();
                await wizard.daysOfWeekStep.continueButton.click();

                await expect(wizard.timeAndCapacityStep.title).toBeVisible();
                await wizard.timeAndCapacityStep.startTimeHourInput.fill('09');
                await wizard.timeAndCapacityStep.startTimeMinuteInput.fill(
                  '00',
                );
                await wizard.timeAndCapacityStep.endTimeHourInput.fill('10');
                await wizard.timeAndCapacityStep.endTimeMinuteInput.fill('00');
                await wizard.timeAndCapacityStep.capacityInput.fill('1');
                await wizard.timeAndCapacityStep.appointmentLengthInput.fill(
                  '5',
                );
                await wizard.timeAndCapacityStep.continueButton.click();

                await expect(wizard.selectServicesStep.title).toBeVisible();
                await wizard.selectServicesStep.rsvCheckbox.check();
                await wizard.selectServicesStep.continueButton.click();

                await expect(
                  wizard.summaryStep.repeatingSessionTitle,
                ).toBeVisible();

                await expect(
                  wizard.summaryStep.datesSummary.getByText(
                    `${tomorrow.day} ${tomorrow.monthName} ${tomorrow.year} - ${dayAfterTomorrowDate.day} ${dayAfterTomorrowDate.monthName} ${dayAfterTomorrowDate.year}`,
                  ),
                ).toBeVisible();
                await expect(
                  wizard.summaryStep.timeSummary.getByText(`09:00 - 10:00`),
                ).toBeVisible();
                await expect(
                  wizard.summaryStep.capacitySummary.getByText(`1`),
                ).toBeVisible();
                await expect(
                  wizard.summaryStep.appointmentLengthSummary.getByText(
                    `5 minutes`,
                  ),
                ).toBeVisible();
                await expect(
                  wizard.summaryStep.servicesSummary.getByText(`RSV (Adult)`),
                ).toBeVisible();

                await wizard.summaryStep.datesSummary
                  .getByRole('link', { name: 'Change' })
                  .click();
                await expect(
                  wizard.startAndEndDateStep.repeatingSessionTitle,
                ).toBeVisible();
                await wizard.startAndEndDateStep.continueButton.click();
                await expect(
                  wizard.summaryStep.repeatingSessionTitle,
                ).toBeVisible();

                await wizard.summaryStep.daysSummary
                  .getByRole('link', { name: 'Change' })
                  .click();
                await expect(wizard.daysOfWeekStep.title).toBeVisible();
                await wizard.daysOfWeekStep.continueButton.click();
                await expect(
                  wizard.summaryStep.repeatingSessionTitle,
                ).toBeVisible();

                await wizard.summaryStep.timeSummary
                  .getByRole('link', { name: 'Change' })
                  .click();
                await expect(wizard.timeAndCapacityStep.title).toBeVisible();
                await wizard.timeAndCapacityStep.continueButton.click();
                await expect(
                  wizard.summaryStep.repeatingSessionTitle,
                ).toBeVisible();

                await wizard.summaryStep.capacitySummary
                  .getByRole('link', { name: 'Change' })
                  .click();
                await expect(wizard.timeAndCapacityStep.title).toBeVisible();
                await wizard.timeAndCapacityStep.continueButton.click();
                await expect(
                  wizard.summaryStep.repeatingSessionTitle,
                ).toBeVisible();

                await wizard.summaryStep.appointmentLengthSummary
                  .getByRole('link', { name: 'Change' })
                  .click();
                await expect(wizard.timeAndCapacityStep.title).toBeVisible();
                await wizard.timeAndCapacityStep.continueButton.click();
                await expect(
                  wizard.summaryStep.repeatingSessionTitle,
                ).toBeVisible();

                await wizard.summaryStep.servicesSummary
                  .getByRole('link', { name: 'Change' })
                  .click();
                await expect(wizard.selectServicesStep.title).toBeVisible();
                await wizard.selectServicesStep.continueButton.click();
                await expect(
                  wizard.summaryStep.repeatingSessionTitle,
                ).toBeVisible();
              });

            // Assert: test each Change link visits the correct step and skips to summary on continue
          });

          test('A user uses the Change links to check each step of the Create Availability wizard before submitting - single session', async ({
            signInToSite,
          }) => {
            const tomorrow = getDateInFuture(1);

            await signInToSite(1, 2)
              .then(sitePage => sitePage.clickCreateAvailabilityCard())
              .then(createAvailabilityPage =>
                createAvailabilityPage.clickCreateAvailabilityButton(),
              )
              .then(async wizard => {
                await expect(
                  wizard.singleOrRepeatingSessionStep.title,
                ).toBeVisible();
                await wizard.singleOrRepeatingSessionStep.singleSessionRadio.check();
                await wizard.singleOrRepeatingSessionStep.continueButton.click();

                await expect(
                  wizard.startAndEndDateStep.singleSessionTitle,
                ).toBeVisible();
                await wizard.startAndEndDateStep.singleDateDayInput.fill(
                  `${tomorrow.day}`,
                );
                await wizard.startAndEndDateStep.singleDateMonthInput.fill(
                  `${tomorrow.month}`,
                );
                await wizard.startAndEndDateStep.singleDateYearInput.fill(
                  `${tomorrow.year}`,
                );
                await wizard.startAndEndDateStep.continueButton.click();

                await expect(wizard.timeAndCapacityStep.title).toBeVisible();
                await wizard.timeAndCapacityStep.startTimeHourInput.fill('09');
                await wizard.timeAndCapacityStep.startTimeMinuteInput.fill(
                  '00',
                );
                await wizard.timeAndCapacityStep.endTimeHourInput.fill('10');
                await wizard.timeAndCapacityStep.endTimeMinuteInput.fill('00');
                await wizard.timeAndCapacityStep.capacityInput.fill('2');
                await wizard.timeAndCapacityStep.appointmentLengthInput.fill(
                  '6',
                );
                await wizard.timeAndCapacityStep.continueButton.click();

                await expect(wizard.selectServicesStep.title).toBeVisible();
                await wizard.selectServicesStep.rsvCheckbox.check();
                await wizard.selectServicesStep.continueButton.click();

                await expect(
                  wizard.summaryStep.singleDateSessionTitle,
                ).toBeVisible();

                await expect(
                  wizard.summaryStep.dateSummary.getByText(
                    `${tomorrow.day} ${tomorrow.monthName} ${tomorrow.year}`,
                  ),
                ).toBeVisible();
                await expect(
                  wizard.summaryStep.timeSummary.getByText(`09:00 - 10:00`),
                ).toBeVisible();
                await expect(
                  wizard.summaryStep.capacitySummary.getByText(`2`),
                ).toBeVisible();
                await expect(
                  wizard.summaryStep.appointmentLengthSummary.getByText(
                    `6 minutes`,
                  ),
                ).toBeVisible();
                await expect(
                  wizard.summaryStep.servicesSummary.getByText(`RSV (Adult)`),
                ).toBeVisible();

                await wizard.summaryStep.dateSummary
                  .getByRole('link', { name: 'Change' })
                  .click();
                await expect(
                  wizard.startAndEndDateStep.singleSessionTitle,
                ).toBeVisible();
                await wizard.startAndEndDateStep.continueButton.click();
                await expect(
                  wizard.summaryStep.singleDateSessionTitle,
                ).toBeVisible();

                await wizard.summaryStep.timeSummary
                  .getByRole('link', { name: 'Change' })
                  .click();
                await expect(wizard.timeAndCapacityStep.title).toBeVisible();
                await wizard.timeAndCapacityStep.continueButton.click();
                await expect(
                  wizard.summaryStep.singleDateSessionTitle,
                ).toBeVisible();

                await wizard.summaryStep.capacitySummary
                  .getByRole('link', { name: 'Change' })
                  .click();
                await expect(wizard.timeAndCapacityStep.title).toBeVisible();
                await wizard.timeAndCapacityStep.continueButton.click();
                await expect(
                  wizard.summaryStep.singleDateSessionTitle,
                ).toBeVisible();

                await wizard.summaryStep.appointmentLengthSummary
                  .getByRole('link', { name: 'Change' })
                  .click();
                await expect(wizard.timeAndCapacityStep.title).toBeVisible();
                await wizard.timeAndCapacityStep.continueButton.click();
                await expect(
                  wizard.summaryStep.singleDateSessionTitle,
                ).toBeVisible();

                await wizard.summaryStep.servicesSummary
                  .getByRole('link', { name: 'Change' })
                  .click();
                await expect(wizard.selectServicesStep.title).toBeVisible();
                await wizard.selectServicesStep.continueButton.click();
                await expect(
                  wizard.summaryStep.singleDateSessionTitle,
                ).toBeVisible();
              });
          });
        });
      },
    );
  },
);
