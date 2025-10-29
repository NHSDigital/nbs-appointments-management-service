import { MYALayout } from '@e2etests/types';
import SiteDetailsPage from '../site-details-page';
import { type Locator } from '@playwright/test';

export default class EditInformationForCitizensPage extends MYALayout {
  title = this.page.getByRole('heading', {
    name: 'Edit site reference details',
  });

  private readonly cancelButton: Locator = this.page.getByRole('button', {
    name: 'Cancel',
  });

  private readonly saveAndContinueButton: Locator = this.page.getByRole(
    'button',
    {
      name: 'Confirm site details',
    },
  );

  readonly infoTextArea: Locator = this.page.getByLabel(
    'What information would you like to include?',
  );

  async cancel(): Promise<SiteDetailsPage> {
    await this.cancelButton.click();
    await this.page.waitForURL(`**/site/${this.site?.id}/details`);

    return new SiteDetailsPage(this.page, this.site);
  }

  async saveCitizenInformation(): Promise<SiteDetailsPage> {
    await this.saveAndContinueButton.click();
    await this.page.waitForURL(`**/site/${this.site?.id}/details`);

    return new SiteDetailsPage(this.page, this.site);
  }
}
