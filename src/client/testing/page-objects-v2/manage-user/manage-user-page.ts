import { MYALayout } from '@e2etests/types';
import UserEmailStep from './user-email-step';
import UserNamesStep from './user-names-step';
import UserRolesStep from './user-roles-step';
import UserSummaryStep from './user-summary-step';
import Users from './users';

export default class ManageUserPage extends MYALayout {
  title = this.page.getByRole('heading', {
    name: 'Manage users',
  });

  readonly emailStep: UserEmailStep = new UserEmailStep(this.page);
  readonly userNamesStep: UserNamesStep = new UserNamesStep(this.page);
  readonly userRolesStep: UserRolesStep = new UserRolesStep(this.page);
  readonly summaryStep: UserSummaryStep = new UserSummaryStep(
    this.page,
    'Confirm',
  );

  async saveUserDetails(): Promise<Users> {
    await this.summaryStep.continueButton.click();
    await this.page.waitForURL(`**/site/${this.site?.id}/users`);
    return new Users(this.page, this.site);
  }

  async saveUserEmail(): Promise<UserRolesStep> {
    await this.emailStep.continueButton.click();
    return new UserRolesStep(this.page);
  }

  async saveUserRoles(): Promise<UserSummaryStep> {
    await this.userRolesStep.continueButton.click();
    return new UserSummaryStep(this.page);
  }

  async saveUserNames(): Promise<UserSummaryStep> {
    await this.userNamesStep.continueButton.click();
    return new UserSummaryStep(this.page);
  }
}
