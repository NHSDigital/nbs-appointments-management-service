import { MYALayout } from '@e2etests/types';
import { type Locator } from '@playwright/test';
import SiteDetailsPage from '../site-details-page';

export default class EditDetailsPage extends MYALayout {
  title = this.page.getByRole('heading', {
    name: 'Edit site reference details',
  });

  private readonly backLink: Locator = this.page.getByRole('link', {
    name: 'Go back',
  });

  private readonly saveAndContinueButton: Locator = this.page.getByRole(
    'button',
    {
      name: 'Save and continue',
    },
  );

  readonly nameInput: Locator = this.page.getByRole('textbox', {
    name: 'Site name',
  });

  readonly addressInput: Locator = this.page.getByRole('textbox', {
    name: 'Site address',
  });

  readonly latitudeInput: Locator = this.page.getByRole('textbox', {
    name: 'Latitude',
  });

  readonly longitudeInput: Locator = this.page.getByRole('textbox', {
    name: 'Longitude',
  });

  readonly phoneNumberInput: Locator = this.page.getByRole('textbox', {
    name: 'Phone Number',
  });

  async goBack(): Promise<SiteDetailsPage> {
    await this.backLink.click();
    await this.page.waitForURL(`**/site/${this.site?.id}/details`);

    return new SiteDetailsPage(this.page, this.site);
  }

  async saveSiteDetails(): Promise<SiteDetailsPage> {
    await this.saveAndContinueButton.click();
    await this.page.waitForURL(`**/site/${this.site?.id}/details`);

    return new SiteDetailsPage(this.page, this.site);
  }
}
