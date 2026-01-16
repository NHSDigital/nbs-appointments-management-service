import { MYALayout } from '@e2etests/types';
import { expect } from '@playwright/test';
import Users from './users';

export default class RemoveUser extends MYALayout {
  title = this.page.getByRole('heading', {
    name: 'Remove User',
  });

  readonly removeButton = this.page.getByRole('button', {
    name: 'Remove this account',
  });

  readonly cancelButton = this.page.getByRole('button', {
    name: 'Cancel',
  });

  async verifyUserNavigatedToRemovePage(userName: string, siteName: string) {
    await this.page.waitForURL(`**/users/remove?user=${userName}`);
    await expect(
      this.page.getByText(
        `Are you sure you wish to remove ${userName} from ${siteName}?`,
      ),
    ).toBeVisible();
  }

  async clickButton(
    buttonName: 'Remove this account' | 'Cancel',
  ): Promise<Users> {
    await this.page.getByRole('button', { name: `${buttonName}` }).click();
    return new Users(this.page, this.site);
  }
}
