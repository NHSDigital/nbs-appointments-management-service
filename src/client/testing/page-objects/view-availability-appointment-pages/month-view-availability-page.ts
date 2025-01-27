import { type Locator, type Page, expect } from '@playwright/test';
import RootPage from '../root';

export default class MonthViewAvailabilityPage extends RootPage {
  readonly nextButton: Locator;

  constructor(page: Page) {
    super(page);
    this.nextButton = page.getByRole('link', {
      name: 'Next',
    });
  }

  async verifyViewMonthDisplayed() {
    await expect(this.nextButton).toBeEnabled();
  }

  async openWeekViewHavingDate(requiredDate: string) {
    await this.page
      .getByRole('main')
      .filter({ has: this.page.getByText(requiredDate) })
      .getByRole('link', { name: 'View week' })
      .last()
      .click();
    //const weekList= await  this.page.getByRole('main').getByRole('link',{name: 'View week'}).all();

    //  for(const week of weekList){
    //     const text: string=await week.textContent();
    //     if(text.includes(requiredDate)){
    //        await week.click();
    //     }
    //  }
  }
}
