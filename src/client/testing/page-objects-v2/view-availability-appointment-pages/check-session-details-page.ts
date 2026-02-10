import { MYALayout } from '@e2etests/types';
import { expect } from '../../fixtures-v2';

export default class CheckSessionDetailsPage extends MYALayout {
  readonly title = this.page.getByRole('heading', {
    name: 'Check your answers',
  });

	readonly goBackButton = this.page.getByRole('link', {
    name: 'Go back',
	});

	readonly saveSessionButton = this.page.getByRole('button', {
		name: 'Save and publish availability',
	});

  async verifyCheckSessionDetailsPageDisplayed() {
    await expect(this.page.getByText('Check your answers')).toBeVisible();
  }

  async saveSession() {
    await this.saveSessionButton.click();
  }
}
