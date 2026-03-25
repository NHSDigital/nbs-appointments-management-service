import { test, expect, BookingSetup } from '../../fixtures-v2';
import {
  daysFromToday,
  getDateInFuture,
  getLongDayDateText,
} from '../../utils/date-utility';
import { ukNow, startOfUkWeek, endOfUkWeek } from '@services/timeService';

test.describe.configure({ mode: 'serial' });

['Europe/London'].forEach(timezone => {
  test.describe(`Test in timezone: '${timezone}'`, () => {
    test.use({ timezoneId: timezone });

    [true, false].forEach(CancelADateRangeFlagEnabled => {
      test.describe(`Test with CancelADateRangeFlag: '${CancelADateRangeFlagEnabled}'`, () => {
        test.describe('Cancel A Date Range Button is available', () => {
          test('Cancel a date range monthly page', async ({ setup }) => {
            const { sitePage } = await setup({
              features: [
                {
                  name: 'CancelADateRange',
                  enabled: CancelADateRangeFlagEnabled,
                },
              ],
            });

            await sitePage
              .clickSiteAvailabilityCard()
              .then(async monthViewAvailabilityPage => {
                if (!CancelADateRangeFlagEnabled) {
                  await expect(
                    monthViewAvailabilityPage.changeAvailabilityButton,
                  ).not.toBeVisible();
                  test.skip();
                }
                return await monthViewAvailabilityPage.clickChangeAvailabilityButton();
              })
              .then(async changeAvailabilityPage => {
                await expect(changeAvailabilityPage.backButton).toBeVisible();
                return await changeAvailabilityPage.clickBackToMonthViewButton();
              })
              .then(async monthViewAvailabilityPage => {
                const formattedMonthAndYear = ukNow().format('MMMM YYYY');
                await monthViewAvailabilityPage.verifyHeadingDisplayed(
                  formattedMonthAndYear,
                );
              });
          });

          test('Cancel a date range weekly page', async ({ setup }) => {
            const { sitePage } = await setup({
              features: [
                {
                  name: 'CancelADateRange',
                  enabled: CancelADateRangeFlagEnabled,
                },
              ],
            });
            const ukWeekStart = startOfUkWeek(ukNow());
            const ukWeekEnd = endOfUkWeek(ukNow());
            const currentWeekTitle = `${ukWeekStart.format('D MMMM')} to ${ukWeekEnd.format('D MMMM')}`;

            await sitePage
              .clickSiteAvailabilityCard()
              .then(async monthViewAvailabilityPage => {
                if (!CancelADateRangeFlagEnabled) {
                  await expect(
                    monthViewAvailabilityPage.changeAvailabilityButton,
                  ).not.toBeVisible();
                  test.skip();
                }

                const formattedMonthAndYear = ukNow().format('MMMM YYYY');
                await monthViewAvailabilityPage.verifyHeadingDisplayed(
                  formattedMonthAndYear,
                );
                return await monthViewAvailabilityPage.clickViewWeekInCardByDate(
                  currentWeekTitle,
                );
              })
              .then(async weekViewAvailabilityPage => {
                if (!CancelADateRangeFlagEnabled) {
                  await expect(
                    weekViewAvailabilityPage.changeAvailabilityButton,
                  ).not.toBeVisible();
                  test.skip();
                }
                return await weekViewAvailabilityPage.clickChangeAvailabilityButton();
              })
              .then(async changeAvailabilityPage => {
                await expect(changeAvailabilityPage.backButton).toBeVisible();
                return await changeAvailabilityPage.clickBackToWeekViewButton();
              })
              .then(async weekViewAvailabilityPage => {
                await expect(
                  await weekViewAvailabilityPage.getHeading(currentWeekTitle),
                ).toBeVisible();
              });
          });

          test('Cancel a date range daily page', async ({ setup }) => {
            const day = daysFromToday(10);
            const dayViewDate = daysFromToday(10, 'dddd D MMMM');
            const availability = [
              {
                date: day,
                sessions: [
                  {
                    from: '09:00',
                    until: '10:00',
                    services: ['COVID:5_11', 'COVID_FLU:65+'],
                    slotLength: 5,
                    capacity: 5,
                  },
                ],
              },
            ];
            const bookings: BookingSetup[] = [
              {
                fromDate: day,
                fromTime: '09:00:00',
                durationMins: 5,
                service: 'COVID_FLU:65+',
                status: 'Booked',
                availabilityStatus: 'Supported',
              },
              {
                fromDate: day,
                fromTime: '09:10:00',
                durationMins: 5,
                service: 'COVID_FLU:65+',
                status: 'Booked',
                availabilityStatus: 'Supported',
              },
            ];

            const { sitePage } = await setup({
              features: [
                {
                  name: 'CancelADateRange',
                  enabled: CancelADateRangeFlagEnabled,
                },
              ],
              availability: availability,
              bookings: bookings,
            });
            const ukWeekStart = startOfUkWeek(day);
            const ukWeekEnd = endOfUkWeek(day);
            const selectedWeekTitle = `${ukWeekStart.format('D MMMM')} to ${ukWeekEnd.format('D MMMM')}`;

            await sitePage
              .clickSiteAvailabilityCard()
              .then(async monthViewAvailabilityPage => {
                return await monthViewAvailabilityPage.clickViewWeekInCardByDate(
                  selectedWeekTitle,
                );
              })
              .then(async weekViewAvailabilityPage => {
                return await weekViewAvailabilityPage.clickViewDailyAppointmentsForDate(
                  dayViewDate,
                );
              })
              .then(async dayViewAvailabilityPage => {
                if (!CancelADateRangeFlagEnabled) {
                  await expect(
                    dayViewAvailabilityPage.changeAvailabilityButton,
                  ).not.toBeVisible();
                  test.skip();
                }

                return await dayViewAvailabilityPage.clickChangeAvailabilityButton();
              })
              .then(async changeAvailabilityPage => {
                await expect(changeAvailabilityPage.backButton).toBeVisible();
                return await changeAvailabilityPage.clickBackToDayViewButton();
              })
              .then(async dayViewAvailabilityPage => {
                await expect(
                  await dayViewAvailabilityPage.getHeading(dayViewDate),
                ).toBeVisible();
              });
          });
        });

        test.describe('Select Dates To Cancel', () => {
          test('Select dates to cancel error, must be in the future', async ({
            setup,
          }) => {
            const { sitePage } = await setup({
              features: [
                {
                  name: 'CancelADateRange',
                  enabled: CancelADateRangeFlagEnabled,
                },
              ],
            });

            await sitePage
              .clickSiteAvailabilityCard()
              .then(async monthViewAvailabilityPage => {
                if (!CancelADateRangeFlagEnabled) {
                  await expect(
                    monthViewAvailabilityPage.changeAvailabilityButton,
                  ).not.toBeVisible();
                  test.skip();
                }
                return await monthViewAvailabilityPage.clickChangeAvailabilityButton();
              })
              .then(
                async changeAvailabilityPage =>
                  await changeAvailabilityPage.clickContinueButton(),
              )
              .then(async selectDatePage => {
                const date = getDateInFuture(-1);
                await selectDatePage.fillDates(date, date);
                await selectDatePage.clickContinueButtonForError();

                await expect(selectDatePage.startDateError).toContainText(
                  /must be in the future/i,
                );

                await expect(selectDatePage.endDateError).toContainText(
                  /must be in the future/i,
                );

                await expect(selectDatePage.backButton).toBeVisible();
                return await selectDatePage.clickBackButton();
              })
              .then(async changeAvailabilityPage => {
                await expect(
                  changeAvailabilityPage.beforeYouContinueHeading,
                ).toBeVisible();
              });
          });

          test('Select dates to cancel error, mandatory field validation', async ({
            setup,
          }) => {
            const { sitePage } = await setup({
              features: [
                {
                  name: 'CancelADateRange',
                  enabled: CancelADateRangeFlagEnabled,
                },
              ],
            });

            await sitePage
              .clickSiteAvailabilityCard()
              .then(async monthViewAvailabilityPage => {
                if (!CancelADateRangeFlagEnabled) {
                  await expect(
                    monthViewAvailabilityPage.changeAvailabilityButton,
                  ).not.toBeVisible();
                  test.skip();
                }
                return await monthViewAvailabilityPage.clickChangeAvailabilityButton();
              })
              .then(
                async changeAvailabilityPage =>
                  await changeAvailabilityPage.clickContinueButton(),
              )
              .then(async selectDatePage => {
                await selectDatePage.clickContinueButtonForError();

                await expect(selectDatePage.startDateError).toContainText(
                  /Enter a start date/i,
                );

                await expect(selectDatePage.endDateError).toContainText(
                  /Enter an end date/i,
                );

                await expect(selectDatePage.backButton).toBeVisible();
                return await selectDatePage.clickBackButton();
              })
              .then(async changeAvailabilityPage => {
                await expect(
                  changeAvailabilityPage.beforeYouContinueHeading,
                ).toBeVisible();
              });
          });

          test('Select dates to cancel error, end date must be after the start date', async ({
            setup,
          }) => {
            const { sitePage } = await setup({
              features: [
                {
                  name: 'CancelADateRange',
                  enabled: CancelADateRangeFlagEnabled,
                },
              ],
            });

            await sitePage
              .clickSiteAvailabilityCard()
              .then(async monthViewAvailabilityPage => {
                if (!CancelADateRangeFlagEnabled) {
                  await expect(
                    monthViewAvailabilityPage.changeAvailabilityButton,
                  ).not.toBeVisible();
                  test.skip();
                }
                return await monthViewAvailabilityPage.clickChangeAvailabilityButton();
              })
              .then(
                async changeAvailabilityPage =>
                  await changeAvailabilityPage.clickContinueButton(),
              )
              .then(async selectDatePage => {
                const startDate = getDateInFuture(5);
                const endDate = getDateInFuture(2);
                await selectDatePage.fillDates(startDate, endDate);
                await selectDatePage.clickContinueButtonForError();

                await expect(selectDatePage.endDateError).toContainText(
                  /End date must be on or after the start date/i,
                );

                await expect(selectDatePage.backButton).toBeVisible();
                return await selectDatePage.clickBackButton();
              })
              .then(async changeAvailabilityPage => {
                await expect(
                  changeAvailabilityPage.beforeYouContinueHeading,
                ).toBeVisible();
              });
          });

          test('Select dates to cancel error within 3 months - 90 days or less', async ({
            setup,
          }) => {
            const { sitePage } = await setup({
              features: [
                {
                  name: 'CancelADateRange',
                  enabled: CancelADateRangeFlagEnabled,
                },
              ],
            });

            await sitePage
              .clickSiteAvailabilityCard()
              .then(async monthViewAvailabilityPage => {
                if (!CancelADateRangeFlagEnabled) {
                  await expect(
                    monthViewAvailabilityPage.changeAvailabilityButton,
                  ).not.toBeVisible();
                  test.skip();
                }
                return await monthViewAvailabilityPage.clickChangeAvailabilityButton();
              })
              .then(
                async changeAvailabilityPage =>
                  await changeAvailabilityPage.clickContinueButton(),
              )
              .then(async selectDatePage => {
                const startDate = getDateInFuture(1);
                const endDate = getDateInFuture(90);
                await selectDatePage.fillDates(startDate, endDate);
                return await selectDatePage.clickContinueButton();
              })
              .then(async cancellationImpactPage => {
                await expect(
                  cancellationImpactPage.noSessionsHeading,
                ).toBeVisible();
              });
          });

          test('Select dates to cancel error within 3 months - greater than 90 days', async ({
            setup,
          }) => {
            const { sitePage } = await setup({
              features: [
                {
                  name: 'CancelADateRange',
                  enabled: CancelADateRangeFlagEnabled,
                },
              ],
            });

            await sitePage
              .clickSiteAvailabilityCard()
              .then(async monthViewAvailabilityPage => {
                if (!CancelADateRangeFlagEnabled) {
                  await expect(
                    monthViewAvailabilityPage.changeAvailabilityButton,
                  ).not.toBeVisible();
                  test.skip();
                }
                return await monthViewAvailabilityPage.clickChangeAvailabilityButton();
              })
              .then(
                async changeAvailabilityPage =>
                  await changeAvailabilityPage.clickContinueButton(),
              )
              .then(async selectDatePage => {
                const startDate = getDateInFuture(1);
                const endDate = getDateInFuture(91);
                await selectDatePage.fillDates(startDate, endDate);
                await selectDatePage.clickContinueButtonForError();

                await expect(selectDatePage.startDateError).toContainText(
                  /Start date must be within 90 days of the end date/i,
                );

                await expect(selectDatePage.endDateError).toContainText(
                  /End date must be within 90 days of the start date/i,
                );

                await expect(selectDatePage.backButton).toBeVisible();
                return await selectDatePage.clickBackButton();
              })
              .then(async changeAvailabilityPage => {
                await expect(
                  changeAvailabilityPage.beforeYouContinueHeading,
                ).toBeVisible();
              });
          });
        });

        test.describe('Cancellation impact step MVP', () => {
          test('There are no sessions in this date range - Choose a new date range', async ({
            setup,
          }) => {
            const { sitePage } = await setup({
              features: [
                {
                  name: 'CancelADateRange',
                  enabled: CancelADateRangeFlagEnabled,
                },
              ],
            });
            const fromDate = getDateInFuture(365);
            const toDate = getDateInFuture(366);

            await sitePage
              .clickSiteAvailabilityCard()
              .then(async monthViewAvailabilityPage => {
                if (!CancelADateRangeFlagEnabled) {
                  await expect(
                    monthViewAvailabilityPage.changeAvailabilityButton,
                  ).not.toBeVisible();
                  test.skip();
                }
                return await monthViewAvailabilityPage.clickChangeAvailabilityButton();
              })
              .then(
                async changeAvailabilityPage =>
                  await changeAvailabilityPage.clickContinueButton(),
              )
              .then(async selectDatePage => {
                await selectDatePage.fillDates(fromDate, toDate);
                return await selectDatePage.clickContinueButton();
              })
              .then(async cancellationImpactPage => {
                await expect(
                  cancellationImpactPage.noSessionsHeading,
                ).toBeVisible();
                await expect(
                  cancellationImpactPage.newDateRangeButton,
                ).toBeVisible();
                return await cancellationImpactPage.clickNewDateRangeButton();
              })
              .then(async selectDatePage => {
                await expect(selectDatePage.startDateDayInput).toHaveValue(
                  parseInt(fromDate.day.toString(), 10).toString(),
                );
                await expect(selectDatePage.startDateMonthInput).toHaveValue(
                  parseInt(fromDate.month.toString(), 10).toString(),
                );
                await expect(selectDatePage.startDateYearInput).toHaveValue(
                  fromDate.year.toString(),
                );

                await expect(selectDatePage.endDateDayInput).toHaveValue(
                  parseInt(toDate.day.toString(), 10).toString(),
                );
                await expect(selectDatePage.endDateMonthInput).toHaveValue(
                  parseInt(toDate.month.toString(), 10).toString(),
                );
                await expect(selectDatePage.endDateYearInput).toHaveValue(
                  toDate.year.toString(),
                );
              });
          });

          test('You are about to cancel X sessions - without bookings', async ({
            setup,
          }) => {
            const day = daysFromToday(180);
            const availability = [
              {
                date: day,
                sessions: [
                  {
                    from: '09:00',
                    until: '10:00',
                    services: ['COVID:5_11', 'COVID_FLU:65+'],
                    slotLength: 5,
                    capacity: 5,
                  },
                ],
              },
            ];

            const { sitePage } = await setup({
              features: [
                {
                  name: 'CancelADateRange',
                  enabled: CancelADateRangeFlagEnabled,
                },
              ],
              availability: availability,
            });
            const fromDate = getDateInFuture(180);
            const toDate = getDateInFuture(181);
            const totalSessionCount = availability.reduce(
              (acc, sessionDay) => acc + sessionDay.sessions.length,
              0,
            );

            await sitePage
              .clickSiteAvailabilityCard()
              .then(async monthViewAvailabilityPage => {
                if (!CancelADateRangeFlagEnabled) {
                  await expect(
                    monthViewAvailabilityPage.changeAvailabilityButton,
                  ).not.toBeVisible();
                  test.skip();
                }
                return await monthViewAvailabilityPage.clickChangeAvailabilityButton();
              })
              .then(
                async changeAvailabilityPage =>
                  await changeAvailabilityPage.clickContinueButton(),
              )
              .then(async selectDatePage => {
                await selectDatePage.fillDates(fromDate, toDate);
                return await selectDatePage.clickContinueButton();
              })
              .then(async cancellationImpactPage => {
                await expect(
                  cancellationImpactPage.cancelSessionsHeading(
                    totalSessionCount,
                  ),
                ).toBeVisible();
                await expect(
                  cancellationImpactPage.cancelSessionsNoBookingsText,
                ).toBeVisible();
              });
          });
        });

        test.describe('Check your answers', () => {
          test('Single session to remove - table content', async ({
            setup,
          }) => {
            const dayIncrement = 1;
            const day = daysFromToday(dayIncrement);
            const startDate = getDateInFuture(dayIncrement);
            const availability = [
              {
                date: day,
                sessions: [
                  {
                    from: '09:00',
                    until: '10:00',
                    services: ['COVID:5_11', 'COVID_FLU:65+'],
                    slotLength: 5,
                    capacity: 5,
                  },
                ],
              },
            ];

            const { sitePage } = await setup({
              features: [
                {
                  name: 'CancelADateRange',
                  enabled: CancelADateRangeFlagEnabled,
                },
              ],
              availability: availability,
            });
            const totalSessionCount = availability.reduce(
              (acc, sessionDay) => acc + sessionDay.sessions.length,
              0,
            );

            await sitePage
              .clickSiteAvailabilityCard()
              .then(async monthViewAvailabilityPage => {
                if (!CancelADateRangeFlagEnabled) {
                  await expect(
                    monthViewAvailabilityPage.changeAvailabilityButton,
                  ).not.toBeVisible();
                  test.skip();
                }
                return await monthViewAvailabilityPage.clickChangeAvailabilityButton();
              })
              .then(
                async changeAvailabilityPage =>
                  await changeAvailabilityPage.clickContinueButton(),
              )
              .then(async selectDatePage => {
                await selectDatePage.fillDates(startDate, startDate);
                return await selectDatePage.clickContinueButton();
              })
              .then(async cancellationImpactPage => {
                await expect(
                  cancellationImpactPage.cancelSessionsHeading(
                    totalSessionCount,
                  ),
                ).toBeVisible();
                await expect(
                  cancellationImpactPage.cancelSessionsNoBookingsText,
                ).toBeVisible();
                return await cancellationImpactPage.clickContinueButton();
              })
              .then(async checkYourAnswersPage => {
                await expect(
                  checkYourAnswersPage.checkYourAnswersHeading,
                ).toBeVisible();

                const expectedDates = `${daysFromToday(dayIncrement, 'D MMMM')} to ${daysFromToday(dayIncrement, 'D MMMM YYYY')}`;
                await expect(
                  checkYourAnswersPage.listItemValue('Dates'),
                ).toHaveText(expectedDates);
                await expect(
                  checkYourAnswersPage.listItemValue('Number of sessions'),
                ).toHaveText(totalSessionCount.toString());
              });
          });

          test('Multiple sessions to remove across multiple days - table content', async ({
            setup,
          }) => {
            const startDayIncrement = 1;
            const endDayIncrement = 2;
            const startDay = daysFromToday(startDayIncrement);
            const endDay = daysFromToday(endDayIncrement);
            const startDateComponent = getDateInFuture(startDayIncrement);
            const endDateComponent = getDateInFuture(endDayIncrement);

            const availability = [
              {
                date: startDay,
                sessions: [
                  {
                    from: '09:00',
                    until: '10:00',
                    services: ['COVID:5_11', 'COVID_FLU:65+'],
                    slotLength: 5,
                    capacity: 5,
                  },
                ],
              },
              {
                date: endDay,
                sessions: [
                  {
                    from: '09:00',
                    until: '10:00',
                    services: ['COVID:5_11', 'COVID_FLU:65+'],
                    slotLength: 5,
                    capacity: 5,
                  },
                ],
              },
            ];

            const { sitePage } = await setup({
              features: [
                {
                  name: 'CancelADateRange',
                  enabled: CancelADateRangeFlagEnabled,
                },
              ],
              availability: availability,
            });
            const totalSessionCount = availability.reduce(
              (acc, sessionDay) => acc + sessionDay.sessions.length,
              0,
            );

            await sitePage
              .clickSiteAvailabilityCard()
              .then(async monthViewAvailabilityPage => {
                if (!CancelADateRangeFlagEnabled) {
                  await expect(
                    monthViewAvailabilityPage.changeAvailabilityButton,
                  ).not.toBeVisible();
                  test.skip();
                }
                return await monthViewAvailabilityPage.clickChangeAvailabilityButton();
              })
              .then(
                async changeAvailabilityPage =>
                  await changeAvailabilityPage.clickContinueButton(),
              )
              .then(async selectDatePage => {
                await selectDatePage.fillDates(
                  startDateComponent,
                  endDateComponent,
                );
                return await selectDatePage.clickContinueButton();
              })
              .then(async cancellationImpactPage => {
                await expect(
                  cancellationImpactPage.cancelSessionsHeading(
                    totalSessionCount,
                  ),
                ).toBeVisible();
                await expect(
                  cancellationImpactPage.cancelSessionsNoBookingsText,
                ).toBeVisible();
                return await cancellationImpactPage.clickContinueButton();
              })
              .then(async checkYourAnswersPage => {
                await expect(
                  checkYourAnswersPage.checkYourAnswersHeading,
                ).toBeVisible();

                const expectedDates = `${daysFromToday(startDayIncrement, 'D MMMM')} to ${daysFromToday(endDayIncrement, 'D MMMM YYYY')}`;
                await expect(
                  checkYourAnswersPage.listItemValue('Dates'),
                ).toHaveText(expectedDates);
                await expect(
                  checkYourAnswersPage.listItemValue('Number of sessions'),
                ).toHaveText(totalSessionCount.toString());
              });
          });

          test('Selected dates are persisted on Change link click', async ({
            setup,
          }) => {
            const startDayIncrement = 4;
            const endDayIncrement = 5;
            const startDay = daysFromToday(startDayIncrement);
            const endDay = daysFromToday(endDayIncrement);
            const startDateComponent = getDateInFuture(startDayIncrement);
            const endDateComponent = getDateInFuture(endDayIncrement);

            const availability = [
              {
                date: startDay,
                sessions: [
                  {
                    from: '09:00',
                    until: '10:00',
                    services: ['COVID:5_11', 'COVID_FLU:65+'],
                    slotLength: 5,
                    capacity: 5,
                  },
                ],
              },
              {
                date: endDay,
                sessions: [
                  {
                    from: '09:00',
                    until: '10:00',
                    services: ['COVID:5_11', 'COVID_FLU:65+'],
                    slotLength: 5,
                    capacity: 5,
                  },
                ],
              },
            ];

            const { sitePage } = await setup({
              features: [
                {
                  name: 'CancelADateRange',
                  enabled: CancelADateRangeFlagEnabled,
                },
              ],
              availability: availability,
            });
            const totalSessionCount = availability.reduce(
              (acc, sessionDay) => acc + sessionDay.sessions.length,
              0,
            );

            await sitePage
              .clickSiteAvailabilityCard()
              .then(async monthViewAvailabilityPage => {
                if (!CancelADateRangeFlagEnabled) {
                  await expect(
                    monthViewAvailabilityPage.changeAvailabilityButton,
                  ).not.toBeVisible();
                  test.skip();
                }
                return await monthViewAvailabilityPage.clickChangeAvailabilityButton();
              })
              .then(
                async changeAvailabilityPage =>
                  await changeAvailabilityPage.clickContinueButton(),
              )
              .then(async selectDatePage => {
                await selectDatePage.fillDates(
                  startDateComponent,
                  endDateComponent,
                );
                return await selectDatePage.clickContinueButton();
              })
              .then(async cancellationImpactPage => {
                await expect(
                  cancellationImpactPage.cancelSessionsHeading(
                    totalSessionCount,
                  ),
                ).toBeVisible();
                await expect(
                  cancellationImpactPage.cancelSessionsNoBookingsText,
                ).toBeVisible();
                return await cancellationImpactPage.clickContinueButton();
              })
              .then(async checkYourAnswersPage => {
                await expect(
                  checkYourAnswersPage.checkYourAnswersHeading,
                ).toBeVisible();

                const expectedDates = `${daysFromToday(startDayIncrement, 'D MMMM')} to ${daysFromToday(endDayIncrement, 'D MMMM YYYY')}`;
                await expect(
                  checkYourAnswersPage.listItemValue('Dates'),
                ).toHaveText(expectedDates);
                await expect(
                  checkYourAnswersPage.listItemValue('Number of sessions'),
                ).toHaveText(totalSessionCount.toString());

                return await checkYourAnswersPage.clickChangeDatesButton();
              })
              .then(async selectDatePage => {
                await expect(selectDatePage.pageHeading).toBeVisible();
                await expect(selectDatePage.startDateDayInput).toHaveValue(
                  parseInt(startDateComponent.day.toString(), 10).toString(),
                );
                await expect(selectDatePage.startDateMonthInput).toHaveValue(
                  parseInt(startDateComponent.month.toString(), 10).toString(),
                );
                await expect(selectDatePage.startDateYearInput).toHaveValue(
                  startDateComponent.year.toString(),
                );

                await expect(selectDatePage.endDateDayInput).toHaveValue(
                  parseInt(endDateComponent.day.toString(), 10).toString(),
                );
                await expect(selectDatePage.endDateMonthInput).toHaveValue(
                  parseInt(endDateComponent.month.toString(), 10).toString(),
                );
                await expect(selectDatePage.endDateYearInput).toHaveValue(
                  endDateComponent.year.toString(),
                );
              });
          });
        });

        test.describe('Cancellation Confirmation', () => {
          test('Cancel sessions, verify sessions have been cancelled', async ({
            setup,
          }) => {
            const fromDayIncrement = 170;
            const toDayIncrement = 171;
            const startDay = daysFromToday(fromDayIncrement);
            const endDay = daysFromToday(toDayIncrement);
            const startDate = getDateInFuture(fromDayIncrement);
            const endDate = getDateInFuture(toDayIncrement);
            const availability = [
              {
                date: startDay,
                sessions: [
                  {
                    from: '09:00',
                    until: '10:00',
                    services: ['COVID:5_11', 'COVID_FLU:65+'],
                    slotLength: 5,
                    capacity: 5,
                  },
                ],
              },
              {
                date: endDay,
                sessions: [
                  {
                    from: '09:00',
                    until: '10:00',
                    services: ['COVID:5_11', 'COVID_FLU:65+'],
                    slotLength: 5,
                    capacity: 5,
                  },
                ],
              },
            ];

            const { sitePage } = await setup({
              features: [
                {
                  name: 'CancelADateRange',
                  enabled: CancelADateRangeFlagEnabled,
                },
              ],
              availability: availability,
            });
            const totalSessionCount = availability.reduce(
              (acc, sessionDay) => acc + sessionDay.sessions.length,
              0,
            );

            await sitePage
              .clickSiteAvailabilityCard()
              .then(async monthViewAvailabilityPage => {
                if (!CancelADateRangeFlagEnabled) {
                  await expect(
                    monthViewAvailabilityPage.changeAvailabilityButton,
                  ).not.toBeVisible();
                  test.skip();
                }
                return await monthViewAvailabilityPage.clickChangeAvailabilityButton();
              })
              .then(
                async changeAvailabilityPage =>
                  await changeAvailabilityPage.clickContinueButton(),
              )
              .then(async selectDatePage => {
                await selectDatePage.fillDates(startDate, endDate);
                return await selectDatePage.clickContinueButton();
              })
              .then(async cancellationImpactPage => {
                await expect(
                  cancellationImpactPage.cancelSessionsHeading(
                    totalSessionCount,
                  ),
                ).toBeVisible();
                await expect(
                  cancellationImpactPage.cancelSessionsNoBookingsText,
                ).toBeVisible();
                return await cancellationImpactPage.clickContinueButton();
              })
              .then(async checkYourAnswersPage => {
                await expect(
                  checkYourAnswersPage.checkYourAnswersHeading,
                ).toBeVisible();

                const expectedDates = `${daysFromToday(fromDayIncrement, 'D MMMM')} to ${daysFromToday(toDayIncrement, 'D MMMM YYYY')}`;
                await expect(
                  checkYourAnswersPage.listItemValue('Dates'),
                ).toHaveText(expectedDates);
                await expect(
                  checkYourAnswersPage.listItemValue('Number of sessions'),
                ).toHaveText(totalSessionCount.toString());
                return await checkYourAnswersPage.clickCancelSessionsButton();
              })
              .then(async confirmationPage => {
                await expect(
                  await confirmationPage.getSessionOnlyCancellationHeading(
                    totalSessionCount,
                  ),
                ).toBeVisible();

                return await confirmationPage.clickGoBackToViewAvailabilityLink();
              })
              .then(async monthViewAvailabilityPage => {
                const formattedMonthAndYear = ukNow().format('MMMM YYYY');
                await monthViewAvailabilityPage.verifyHeadingDisplayed(
                  formattedMonthAndYear,
                );
                return await monthViewAvailabilityPage.goToWeekForDate(
                  startDate,
                );
              })
              .then(async weekViewAvailabilityPage => {
                const date = getLongDayDateText(startDate);
                await expect(
                  await weekViewAvailabilityPage.getDailyCard(date),
                ).toBeVisible();
                await expect(
                  await weekViewAvailabilityPage.getAddAvailabilityLinkForDailyCard(
                    date,
                  ),
                ).toBeVisible();
                expect(
                  (await weekViewAvailabilityPage.getDailyCard(date)).getByText(
                    'No availability',
                  ),
                ).toBeVisible();
                await expect(
                  (await weekViewAvailabilityPage.getDailyCard(date)).getByText(
                    'Total appointments: 0',
                  ),
                ).toBeVisible();
              });
          });

          test('Cancel session, verify session has been cancelled', async ({
            setup,
          }) => {
            const dayIncrement = 170;
            const day = daysFromToday(dayIncrement);
            const startDate = getDateInFuture(dayIncrement);
            const availability = [
              {
                date: day,
                sessions: [
                  {
                    from: '09:00',
                    until: '10:00',
                    services: ['COVID:5_11', 'COVID_FLU:65+'],
                    slotLength: 5,
                    capacity: 5,
                  },
                ],
              },
            ];

            const { sitePage } = await setup({
              features: [
                {
                  name: 'CancelADateRange',
                  enabled: CancelADateRangeFlagEnabled,
                },
              ],
              availability: availability,
            });
            const totalSessionCount = availability.reduce(
              (acc, sessionDay) => acc + sessionDay.sessions.length,
              0,
            );

            await sitePage
              .clickSiteAvailabilityCard()
              .then(async monthViewAvailabilityPage => {
                if (!CancelADateRangeFlagEnabled) {
                  await expect(
                    monthViewAvailabilityPage.changeAvailabilityButton,
                  ).not.toBeVisible();
                  test.skip();
                }
                return await monthViewAvailabilityPage.clickChangeAvailabilityButton();
              })
              .then(
                async changeAvailabilityPage =>
                  await changeAvailabilityPage.clickContinueButton(),
              )
              .then(async selectDatePage => {
                await selectDatePage.fillDates(startDate, startDate);
                return await selectDatePage.clickContinueButton();
              })
              .then(async cancellationImpactPage => {
                await expect(
                  cancellationImpactPage.cancelSessionsHeading(
                    totalSessionCount,
                  ),
                ).toBeVisible();
                await expect(
                  cancellationImpactPage.cancelSessionsNoBookingsText,
                ).toBeVisible();
                return await cancellationImpactPage.clickContinueButton();
              })
              .then(async checkYourAnswersPage => {
                await expect(
                  checkYourAnswersPage.checkYourAnswersHeading,
                ).toBeVisible();

                const expectedDates = `${daysFromToday(dayIncrement, 'D MMMM')} to ${daysFromToday(dayIncrement, 'D MMMM YYYY')}`;
                await expect(
                  checkYourAnswersPage.listItemValue('Dates'),
                ).toHaveText(expectedDates);
                await expect(
                  checkYourAnswersPage.listItemValue('Number of sessions'),
                ).toHaveText(totalSessionCount.toString());
                return await checkYourAnswersPage.clickCancelSessionsButton();
              })
              .then(async confirmationPage => {
                await expect(
                  await confirmationPage.getSessionOnlyCancellationHeading(
                    totalSessionCount,
                  ),
                ).toBeVisible();

                return await confirmationPage.clickGoBackToViewAvailabilityLink();
              })
              .then(async monthViewAvailabilityPage => {
                const formattedMonthAndYear = ukNow().format('MMMM YYYY');
                await monthViewAvailabilityPage.verifyHeadingDisplayed(
                  formattedMonthAndYear,
                );
                return await monthViewAvailabilityPage.goToWeekForDate(
                  startDate,
                );
              })
              .then(async weekViewAvailabilityPage => {
                const date = getLongDayDateText(startDate);
                await expect(
                  await weekViewAvailabilityPage.getDailyCard(date),
                ).toBeVisible();
                await expect(
                  await weekViewAvailabilityPage.getAddAvailabilityLinkForDailyCard(
                    date,
                  ),
                ).toBeVisible();
                expect(
                  (await weekViewAvailabilityPage.getDailyCard(date)).getByText(
                    'No availability',
                  ),
                ).toBeVisible();
                await expect(
                  (await weekViewAvailabilityPage.getDailyCard(date)).getByText(
                    'Total appointments: 0',
                  ),
                ).toBeVisible();
              });
          });
        });
      });
    });

    [true, false].forEach(CancelADateRangeWithBookingsFlagEnabled => {
      test.describe(`Test with CancelADateRangeWithBookingsFlag: '${CancelADateRangeWithBookingsFlagEnabled}'`, () => {
        test.describe('You are about to cancel pages', () => {
          test('Verify number of steps when bookings exist', async ({
            setup,
          }) => {
            const { sitePage } = await setup({
              features: [
                {
                  name: 'CancelADateRangeWithBookings',
                  enabled: CancelADateRangeWithBookingsFlagEnabled,
                },
                {
                  name: 'CancelADateRange',
                  enabled: true,
                },
              ],
            });

            await sitePage
              .clickSiteAvailabilityCard()
              .then(
                async monthViewAvailabilityPage =>
                  await monthViewAvailabilityPage.clickChangeAvailabilityButton(),
              )
              .then(async changeAvailabilityPage => {
                if (CancelADateRangeWithBookingsFlagEnabled) {
                  await expect(changeAvailabilityPage.listItems).toHaveText([
                    /Cancel the sessions you want to change/,
                    /Choose to keep existing bookings/,
                    /Create new sessions with the updated details/,
                  ]);
                } else {
                  await expect(changeAvailabilityPage.listItems).toHaveText([
                    /Cancel the sessions you want to change/,
                    /Create new sessions with the updated details/,
                  ]);
                }
              });
          });

          test('You are about to cancel X sessions - with bookings', async ({
            setup,
          }) => {
            const day = daysFromToday(1);
            const availability = [
              {
                date: day,
                sessions: [
                  {
                    from: '09:00',
                    until: '10:00',
                    services: ['COVID:5_11', 'COVID_FLU:65+'],
                    slotLength: 5,
                    capacity: 5,
                  },
                ],
              },
            ];
            const bookings: BookingSetup[] = [
              {
                fromDate: day,
                fromTime: '09:00:00',
                durationMins: 5,
                service: 'COVID_FLU:65+',
                status: 'Booked',
                availabilityStatus: 'Supported',
              },
              {
                fromDate: day,
                fromTime: '09:10:00',
                durationMins: 5,
                service: 'COVID_FLU:65+',
                status: 'Booked',
                availabilityStatus: 'Supported',
              },
            ];

            const { sitePage } = await setup({
              features: [
                {
                  name: 'CancelADateRangeWithBookings',
                  enabled: CancelADateRangeWithBookingsFlagEnabled,
                },
                {
                  name: 'CancelADateRange',
                  enabled: true,
                },
              ],
              availability: availability,
              bookings: bookings,
            });
            const totalSessionCount = availability.reduce(
              (acc, sessionDay) => acc + sessionDay.sessions.length,
              0,
            );

            await sitePage
              .clickSiteAvailabilityCard()
              .then(
                async monthViewAvailabilityPage =>
                  await monthViewAvailabilityPage.clickChangeAvailabilityButton(),
              )
              .then(
                async changeAvailabilityPage =>
                  await changeAvailabilityPage.clickContinueButton(),
              )
              .then(async selectDatePage => {
                const fromDate = getDateInFuture(1);
                const toDate = getDateInFuture(1);
                await selectDatePage.fillDates(fromDate, toDate);
                return await selectDatePage.clickContinueButton();
              })
              .then(async cancellationImpactPage => {
                if (!CancelADateRangeWithBookingsFlagEnabled) {
                  await expect(
                    cancellationImpactPage.canNotCancelHeading,
                  ).toBeVisible();
                  await expect(
                    cancellationImpactPage.canNotCancelReturnButton,
                  ).toBeVisible();
                  await expect(
                    cancellationImpactPage.canNotCancelDifferentDatesButton,
                  ).toBeVisible();
                } else {
                  await expect(
                    cancellationImpactPage.cancelSessionsHeading(
                      totalSessionCount,
                    ),
                  ).toBeVisible();
                  await expect(
                    cancellationImpactPage.keepBookingsRadio,
                  ).toBeVisible();
                  await expect(
                    cancellationImpactPage.cancelBookingsRadio,
                  ).toBeVisible();
                }
              });
          });
        });

        test.describe('Check your answers pagess', () => {
          ['Keep bookings', 'Cancel bookings'].forEach(cancellationDecision => {
            test(`Single session to remove - table content: '${cancellationDecision}'`, async ({
              setup,
            }) => {
              const day = daysFromToday(1);
              const availability = [
                {
                  date: day,
                  sessions: [
                    {
                      from: '09:00',
                      until: '10:00',
                      services: ['COVID:5_11', 'COVID_FLU:65+'],
                      slotLength: 5,
                      capacity: 5,
                    },
                  ],
                },
              ];
              const bookings: BookingSetup[] = [
                {
                  fromDate: day,
                  fromTime: '09:00:00',
                  durationMins: 5,
                  service: 'COVID_FLU:65+',
                  status: 'Booked',
                  availabilityStatus: 'Supported',
                },
                {
                  fromDate: day,
                  fromTime: '09:10:00',
                  durationMins: 5,
                  service: 'COVID_FLU:65+',
                  status: 'Booked',
                  availabilityStatus: 'Supported',
                },
              ];

              const { sitePage } = await setup({
                features: [
                  {
                    name: 'CancelADateRangeWithBookings',
                    enabled: CancelADateRangeWithBookingsFlagEnabled,
                  },
                  {
                    name: 'CancelADateRange',
                    enabled: true,
                  },
                ],
                availability: availability,
                bookings: bookings,
              });
              const totalSessionCount = availability.reduce(
                (acc, sessionDay) => acc + sessionDay.sessions.length,
                0,
              );

              await sitePage
                .clickSiteAvailabilityCard()
                .then(
                  async monthViewAvailabilityPage =>
                    await monthViewAvailabilityPage.clickChangeAvailabilityButton(),
                )
                .then(
                  async changeAvailabilityPage =>
                    await changeAvailabilityPage.clickContinueButton(),
                )
                .then(async selectDatePage => {
                  const fromDate = getDateInFuture(1);
                  const toDate = getDateInFuture(1);
                  await selectDatePage.fillDates(fromDate, toDate);
                  return await selectDatePage.clickContinueButton();
                })
                .then(async cancellationImpactPage => {
                  if (!CancelADateRangeWithBookingsFlagEnabled) {
                    await expect(
                      cancellationImpactPage.canNotCancelHeading,
                    ).toBeVisible();
                    test.skip();
                  }

                  await expect(
                    cancellationImpactPage.cancelSessionsHeading(
                      totalSessionCount,
                    ),
                  ).toBeVisible();

                  if (cancellationDecision === 'Keep bookings') {
                    await cancellationImpactPage.keepBookingsRadio.check();
                    await expect(
                      cancellationImpactPage.keepBookingsRadio,
                    ).toBeChecked();
                    await expect(
                      cancellationImpactPage.cancelBookingsRadio,
                    ).not.toBeChecked();
                  } else {
                    await cancellationImpactPage.cancelBookingsRadio.check();
                    await expect(
                      cancellationImpactPage.keepBookingsRadio,
                    ).not.toBeChecked();
                    await expect(
                      cancellationImpactPage.cancelBookingsRadio,
                    ).toBeChecked();
                  }

                  return await cancellationImpactPage.clickContinueButton();
                })
                .then(async checkYourAnswersPage => {
                  await expect(
                    checkYourAnswersPage.checkYourAnswersHeading,
                  ).toBeVisible();
                  const expectedDates = `${daysFromToday(1, 'D MMMM')} to ${daysFromToday(1, 'D MMMM YYYY')}`;
                  const cancellationDecisionText =
                    cancellationDecision === 'Keep bookings'
                      ? 'Keep bookings'
                      : `Cancel ${bookings.length} ${bookings.length > 1 ? 'bookings' : 'booking'}`;

                  await expect(
                    checkYourAnswersPage.listItemValue('Dates'),
                  ).toHaveText(expectedDates);
                  await expect(
                    checkYourAnswersPage.listItemValue('Number of sessions'),
                  ).toHaveText(totalSessionCount.toString());
                  await expect(
                    checkYourAnswersPage.listItemValue(
                      'What you have chosen to do with the bookings',
                    ),
                  ).toHaveText(cancellationDecisionText);
                });
            });
          });

          ['Keep bookings', 'Cancel bookings'].forEach(cancellationDecision => {
            test(`Multiple sessions to remove across multiple days - table content: '${cancellationDecision}'`, async ({
              setup,
            }) => {
              const day = daysFromToday(1);
              const day2 = daysFromToday(2);
              const availability = [
                {
                  date: day,
                  sessions: [
                    {
                      from: '09:00',
                      until: '10:00',
                      services: ['COVID:5_11', 'COVID_FLU:65+'],
                      slotLength: 5,
                      capacity: 5,
                    },
                  ],
                },
                {
                  date: day2,
                  sessions: [
                    {
                      from: '10:00',
                      until: '11:00',
                      services: ['COVID:5_11', 'COVID_FLU:65+'],
                      slotLength: 5,
                      capacity: 5,
                    },
                  ],
                },
              ];
              const bookings: BookingSetup[] = [
                {
                  fromDate: day,
                  fromTime: '09:00:00',
                  durationMins: 5,
                  service: 'COVID_FLU:65+',
                  status: 'Booked',
                  availabilityStatus: 'Supported',
                },
                {
                  fromDate: day,
                  fromTime: '09:10:00',
                  durationMins: 5,
                  service: 'COVID_FLU:65+',
                  status: 'Booked',
                  availabilityStatus: 'Supported',
                },
                {
                  fromDate: day2,
                  fromTime: '10:00:00',
                  durationMins: 5,
                  service: 'COVID_FLU:65+',
                  status: 'Booked',
                  availabilityStatus: 'Supported',
                },
                {
                  fromDate: day2,
                  fromTime: '10:10:00',
                  durationMins: 5,
                  service: 'COVID_FLU:65+',
                  status: 'Booked',
                  availabilityStatus: 'Supported',
                },
              ];

              const { sitePage } = await setup({
                features: [
                  {
                    name: 'CancelADateRangeWithBookings',
                    enabled: CancelADateRangeWithBookingsFlagEnabled,
                  },
                  {
                    name: 'CancelADateRange',
                    enabled: true,
                  },
                ],
                availability: availability,
                bookings: bookings,
              });
              const totalSessionCount = availability.reduce(
                (acc, sessionDay) => acc + sessionDay.sessions.length,
                0,
              );

              await sitePage
                .clickSiteAvailabilityCard()
                .then(
                  async monthViewAvailabilityPage =>
                    await monthViewAvailabilityPage.clickChangeAvailabilityButton(),
                )
                .then(
                  async changeAvailabilityPage =>
                    await changeAvailabilityPage.clickContinueButton(),
                )
                .then(async selectDatePage => {
                  const fromDate = getDateInFuture(1);
                  const toDate = getDateInFuture(2);
                  await selectDatePage.fillDates(fromDate, toDate);
                  return await selectDatePage.clickContinueButton();
                })
                .then(async cancellationImpactPage => {
                  if (!CancelADateRangeWithBookingsFlagEnabled) {
                    await expect(
                      cancellationImpactPage.canNotCancelHeading,
                    ).toBeVisible();
                    test.skip();
                  }

                  await expect(
                    cancellationImpactPage.cancelSessionsHeading(
                      totalSessionCount,
                    ),
                  ).toBeVisible();

                  if (cancellationDecision === 'Keep bookings') {
                    await cancellationImpactPage.keepBookingsRadio.check();
                    await expect(
                      cancellationImpactPage.keepBookingsRadio,
                    ).toBeChecked();
                    await expect(
                      cancellationImpactPage.cancelBookingsRadio,
                    ).not.toBeChecked();
                  } else {
                    await cancellationImpactPage.cancelBookingsRadio.check();
                    await expect(
                      cancellationImpactPage.keepBookingsRadio,
                    ).not.toBeChecked();
                    await expect(
                      cancellationImpactPage.cancelBookingsRadio,
                    ).toBeChecked();
                  }

                  return await cancellationImpactPage.clickContinueButton();
                })
                .then(async checkYourAnswersPage => {
                  await expect(
                    checkYourAnswersPage.checkYourAnswersHeading,
                  ).toBeVisible();
                  const expectedDates = `${daysFromToday(1, 'D MMMM')} to ${daysFromToday(2, 'D MMMM YYYY')}`;
                  const cancellationDecisionText =
                    cancellationDecision === 'Keep bookings'
                      ? 'Keep bookings'
                      : `Cancel ${bookings.length} ${bookings.length > 1 ? 'bookings' : 'booking'}`;

                  await expect(
                    checkYourAnswersPage.listItemValue('Dates'),
                  ).toHaveText(expectedDates);
                  await expect(
                    checkYourAnswersPage.listItemValue('Number of sessions'),
                  ).toHaveText(totalSessionCount.toString());
                  await expect(
                    checkYourAnswersPage.listItemValue(
                      'What you have chosen to do with the bookings',
                    ),
                  ).toHaveText(cancellationDecisionText);
                });
            });
          });

          ['Keep bookings', 'Cancel bookings'].forEach(cancellationDecision => {
            test(`Cancellation decision is persisted on Change link click: ${cancellationDecision}`, async ({
              setup,
            }) => {
              const day = daysFromToday(1);
              const availability = [
                {
                  date: day,
                  sessions: [
                    {
                      from: '09:00',
                      until: '10:00',
                      services: ['COVID:5_11', 'COVID_FLU:65+'],
                      slotLength: 5,
                      capacity: 5,
                    },
                  ],
                },
              ];
              const bookings: BookingSetup[] = [
                {
                  fromDate: day,
                  fromTime: '09:00:00',
                  durationMins: 5,
                  service: 'COVID_FLU:65+',
                  status: 'Booked',
                  availabilityStatus: 'Supported',
                },
                {
                  fromDate: day,
                  fromTime: '09:10:00',
                  durationMins: 5,
                  service: 'COVID_FLU:65+',
                  status: 'Booked',
                  availabilityStatus: 'Supported',
                },
              ];

              const { sitePage } = await setup({
                features: [
                  {
                    name: 'CancelADateRangeWithBookings',
                    enabled: CancelADateRangeWithBookingsFlagEnabled,
                  },
                  {
                    name: 'CancelADateRange',
                    enabled: true,
                  },
                ],
                availability: availability,
                bookings: bookings,
              });
              const totalSessionCount = availability.reduce(
                (acc, sessionDay) => acc + sessionDay.sessions.length,
                0,
              );

              await sitePage
                .clickSiteAvailabilityCard()
                .then(
                  async monthViewAvailabilityPage =>
                    await monthViewAvailabilityPage.clickChangeAvailabilityButton(),
                )
                .then(
                  async changeAvailabilityPage =>
                    await changeAvailabilityPage.clickContinueButton(),
                )
                .then(async selectDatePage => {
                  const fromDate = getDateInFuture(1);
                  const toDate = getDateInFuture(1);
                  await selectDatePage.fillDates(fromDate, toDate);
                  return await selectDatePage.clickContinueButton();
                })
                .then(async cancellationImpactPage => {
                  if (!CancelADateRangeWithBookingsFlagEnabled) {
                    await expect(
                      cancellationImpactPage.canNotCancelHeading,
                    ).toBeVisible();
                    test.skip();
                  }

                  await expect(
                    cancellationImpactPage.cancelSessionsHeading(
                      totalSessionCount,
                    ),
                  ).toBeVisible();

                  if (cancellationDecision === 'Keep bookings') {
                    await cancellationImpactPage.keepBookingsRadio.check();
                    await expect(
                      cancellationImpactPage.keepBookingsRadio,
                    ).toBeChecked();
                    await expect(
                      cancellationImpactPage.cancelBookingsRadio,
                    ).not.toBeChecked();
                  } else {
                    await cancellationImpactPage.cancelBookingsRadio.check();
                    await expect(
                      cancellationImpactPage.keepBookingsRadio,
                    ).not.toBeChecked();
                    await expect(
                      cancellationImpactPage.cancelBookingsRadio,
                    ).toBeChecked();
                  }

                  return await cancellationImpactPage.clickContinueButton();
                })
                .then(async checkYourAnswersPage => {
                  await expect(
                    checkYourAnswersPage.checkYourAnswersHeading,
                  ).toBeVisible();

                  return await checkYourAnswersPage.clickChangeCancellationDecitionButton(
                    'What you have chosen to do with the bookings',
                  );
                })
                .then(async cancellationImpactPage => {
                  await expect(
                    cancellationImpactPage.cancelSessionsHeading(
                      totalSessionCount,
                    ),
                  ).toBeVisible();

                  if (cancellationDecision === 'Keep bookings') {
                    await expect(
                      cancellationImpactPage.keepBookingsRadio,
                    ).toBeChecked();
                    await expect(
                      cancellationImpactPage.cancelBookingsRadio,
                    ).not.toBeChecked();
                  } else {
                    await expect(
                      cancellationImpactPage.keepBookingsRadio,
                    ).not.toBeChecked();
                    await expect(
                      cancellationImpactPage.cancelBookingsRadio,
                    ).toBeChecked();
                  }
                });
            });
          });
        });

        test.describe('Cannot Cancel', () => {
          test('Cannot cancel these sessions - Return to view availability', async ({
            setup,
          }) => {
            const day = daysFromToday(1);
            const availability = [
              {
                date: day,
                sessions: [
                  {
                    from: '09:00',
                    until: '10:00',
                    services: ['COVID:5_11', 'COVID_FLU:65+'],
                    slotLength: 5,
                    capacity: 5,
                  },
                ],
              },
            ];
            const bookings: BookingSetup[] = [
              {
                fromDate: day,
                fromTime: '09:00:00',
                durationMins: 5,
                service: 'COVID_FLU:65+',
                status: 'Booked',
                availabilityStatus: 'Supported',
              },
              {
                fromDate: day,
                fromTime: '09:10:00',
                durationMins: 5,
                service: 'COVID_FLU:65+',
                status: 'Booked',
                availabilityStatus: 'Supported',
              },
            ];

            const { sitePage } = await setup({
              features: [
                {
                  name: 'CancelADateRangeWithBookings',
                  enabled: CancelADateRangeWithBookingsFlagEnabled,
                },
                {
                  name: 'CancelADateRange',
                  enabled: true,
                },
              ],
              availability: availability,
              bookings: bookings,
            });
            const totalSessionCount = availability.reduce(
              (acc, sessionDay) => acc + sessionDay.sessions.length,
              0,
            );

            await sitePage
              .clickSiteAvailabilityCard()
              .then(
                async monthViewAvailabilityPage =>
                  await monthViewAvailabilityPage.clickChangeAvailabilityButton(),
              )
              .then(
                async changeAvailabilityPage =>
                  await changeAvailabilityPage.clickContinueButton(),
              )
              .then(async selectDatePage => {
                const fromDate = getDateInFuture(1);
                const toDate = getDateInFuture(1);
                await selectDatePage.fillDates(fromDate, toDate);
                return await selectDatePage.clickContinueButton();
              })
              .then(async cancellationImpactPage => {
                if (CancelADateRangeWithBookingsFlagEnabled) {
                  await expect(
                    cancellationImpactPage.cancelSessionsHeading(
                      totalSessionCount,
                    ),
                  ).toBeVisible();
                  test.skip();
                }

                await expect(
                  cancellationImpactPage.canNotCancelHeading,
                ).toBeVisible();

                return await cancellationImpactPage.clickCanNotCancelReturnButton();
              })
              .then(async monthViewAvailabilityPage => {
                const formattedMonthAndYear = ukNow().format('MMMM YYYY');
                await monthViewAvailabilityPage.verifyHeadingDisplayed(
                  formattedMonthAndYear,
                );
              });
          });

          test('Cannot cancel these sessions - Select different dates', async ({
            setup,
          }) => {
            const day = daysFromToday(1);
            const availability = [
              {
                date: day,
                sessions: [
                  {
                    from: '09:00',
                    until: '10:00',
                    services: ['COVID:5_11', 'COVID_FLU:65+'],
                    slotLength: 5,
                    capacity: 5,
                  },
                ],
              },
            ];
            const bookings: BookingSetup[] = [
              {
                fromDate: day,
                fromTime: '09:00:00',
                durationMins: 5,
                service: 'COVID_FLU:65+',
                status: 'Booked',
                availabilityStatus: 'Supported',
              },
              {
                fromDate: day,
                fromTime: '09:10:00',
                durationMins: 5,
                service: 'COVID_FLU:65+',
                status: 'Booked',
                availabilityStatus: 'Supported',
              },
            ];

            const { sitePage } = await setup({
              features: [
                {
                  name: 'CancelADateRangeWithBookings',
                  enabled: CancelADateRangeWithBookingsFlagEnabled,
                },
                {
                  name: 'CancelADateRange',
                  enabled: true,
                },
              ],
              availability: availability,
              bookings: bookings,
            });
            const totalSessionCount = availability.reduce(
              (acc, sessionDay) => acc + sessionDay.sessions.length,
              0,
            );
            const fromDate = getDateInFuture(1);
            const toDate = getDateInFuture(1);

            await sitePage
              .clickSiteAvailabilityCard()
              .then(
                async monthViewAvailabilityPage =>
                  await monthViewAvailabilityPage.clickChangeAvailabilityButton(),
              )
              .then(
                async changeAvailabilityPage =>
                  await changeAvailabilityPage.clickContinueButton(),
              )
              .then(async selectDatePage => {
                await selectDatePage.fillDates(fromDate, toDate);
                return await selectDatePage.clickContinueButton();
              })
              .then(async cancellationImpactPage => {
                if (CancelADateRangeWithBookingsFlagEnabled) {
                  await expect(
                    cancellationImpactPage.cancelSessionsHeading(
                      totalSessionCount,
                    ),
                  ).toBeVisible();
                  test.skip();
                }

                await expect(
                  cancellationImpactPage.canNotCancelDifferentDatesButton,
                ).toBeVisible();

                return await cancellationImpactPage.clickCanNotCancelDifferentDatesButton();
              })
              .then(async selectDatePage => {
                await expect(selectDatePage.pageHeading).toBeVisible();
                await expect(selectDatePage.startDateDayInput).toHaveValue(
                  parseInt(fromDate.day.toString(), 10).toString(),
                );
                await expect(selectDatePage.startDateMonthInput).toHaveValue(
                  parseInt(fromDate.month.toString(), 10).toString(),
                );
                await expect(selectDatePage.startDateYearInput).toHaveValue(
                  fromDate.year.toString(),
                );

                await expect(selectDatePage.endDateDayInput).toHaveValue(
                  parseInt(toDate.day.toString(), 10).toString(),
                );
                await expect(selectDatePage.endDateMonthInput).toHaveValue(
                  parseInt(toDate.month.toString(), 10).toString(),
                );
                await expect(selectDatePage.endDateYearInput).toHaveValue(
                  toDate.year.toString(),
                );
              });
          });
        });
      });
    });
  });
});
