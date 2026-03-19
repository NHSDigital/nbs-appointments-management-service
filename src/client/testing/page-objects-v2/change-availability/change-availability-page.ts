import { MYALayout } from '@e2etests/types';
import SelectDatePage from './select-date-page';

export default class ChangeAvailabilityPage extends MYALayout {
  title = this.page.getByRole('heading', {
    name: this.site?.name,
  });

  readonly continueButton = this.page.getByRole('button', {
    name: 'Continue to cancel',
  });

  readonly listItems = this.page
    .locator('ol.nhsuk-list--number')
    .getByRole('listitem');

  async clickContinueButton(): Promise<SelectDatePage> {
    await this.continueButton.click();

    return new SelectDatePage(this.page, this.site);
  }
}
