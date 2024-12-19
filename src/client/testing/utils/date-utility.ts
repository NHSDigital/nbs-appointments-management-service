import { type Locator, type Page } from '@playwright/test';
import env from '../testEnvironment';

export default class DateUtils {
  getFutureDate(day: number): string[] {
    const currentDate = new Date();
    // Get current date
    currentDate.setDate(currentDate.getDate() + day);
    // Add one day
    const futureDate = currentDate.toISOString().split('T')[0];
    // Format to YYYY-MM-DD

    const year = futureDate.toString().split('-')[0];
    const month = futureDate.split('-')[1];
    const Futureday = futureDate.split('-')[2];
    const date = [year, month, Futureday];
    return date;
  }
}
