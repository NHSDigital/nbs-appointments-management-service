import { MonthViewPage } from '@testing-page-objects';
import { expect } from '../../fixtures';

type WeekOverview = {
  weekTitle: string;
  services: ServiceOverview[];
  totalAppointments: number;
  booked: number;
  unbooked: number;
};

type ServiceOverview = {
  serviceName: string;
  bookedAppointments: number;
};

async function expectEmptyWeek(put: MonthViewPage, weekTitle: string) {
  const weekCard = await put.weekCard(weekTitle);
  await expect(weekCard).toBeVisible();

  await expect(
    weekCard.getByText('No availability', { exact: true }),
  ).toBeVisible();
  await expect(
    weekCard.getByText('Total appointments: 0', { exact: true }),
  ).toBeVisible();
  await expect(weekCard.getByText('Booked: 0', { exact: true })).toBeVisible();
  await expect(
    weekCard.getByText('Unbooked: 0', { exact: true }),
  ).toBeVisible();
}

async function expectWeekSummary(put: MonthViewPage, props: WeekOverview) {
  const { weekTitle, services, totalAppointments, booked, unbooked } = props;

  const weekCard = await put.weekCard(weekTitle);
  await expect(weekCard).toBeVisible();

  //assert no availability not visible
  expect(weekCard.getByText('No availability')).not.toBeVisible();

  //only do for a single service for now!!
  const serviceCell = weekCard.getByRole('cell', {
    name: services[0].serviceName,
  });
  const bookedAppointmentsCell = weekCard.getByRole('cell', {
    name: services[0].bookedAppointments.toString(),
  });

  await expect(serviceCell).toBeVisible();
  await expect(bookedAppointmentsCell).toBeVisible();

  //totals
  await expect(
    weekCard.getByText(`Total appointments: ${totalAppointments}`),
  ).toBeVisible();
  await expect(
    weekCard.getByText(`Booked: ${booked}`, {
      exact: true,
    }),
  ).toBeVisible();
  await expect(
    weekCard.getByText(`Unbooked: ${unbooked}`, {
      exact: true,
    }),
  ).toBeVisible();
}

export { expectEmptyWeek, expectWeekSummary };
