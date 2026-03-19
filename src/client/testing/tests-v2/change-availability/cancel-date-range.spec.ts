import { test, expect, BookingSetup } from '../../fixtures-v2';
import { daysFromToday, getDateInFuture } from '../../utils/date-utility';

test.describe.configure({ mode: 'serial' });

[true, false].forEach(CancelADateRangeWithBookingsFlagEnabled => {
  test.describe(`Test with CancelADateRangeWithBookingsFlag: '${CancelADateRangeWithBookingsFlagEnabled}'`, () => {
    test.describe('You are about to cancel pages', () => {
      test('Verify number of steps when bookings exist', async ({ setup }) => {
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
                cancellationImpactPage.cancelSessionsHeading(totalSessionCount),
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
                cancellationImpactPage.cancelSessionsHeading(totalSessionCount),
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
                cancellationImpactPage.cancelSessionsHeading(totalSessionCount),
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
                cancellationImpactPage.cancelSessionsHeading(totalSessionCount),
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
                cancellationImpactPage.cancelSessionsHeading(totalSessionCount),
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
  });
});
