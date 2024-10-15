'use server';
import { AvailabilityPeriod } from '@types';

async function saveAvailabilityPeriod(availabilityPeriod: AvailabilityPeriod) {
  // TODO: Save the availability period to the database
  // eslint-disable-next-line no-console
  console.dir(availabilityPeriod);
}

export default saveAvailabilityPeriod;
