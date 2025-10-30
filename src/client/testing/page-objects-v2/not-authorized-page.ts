import { MYALayout } from '@e2etests/types';

export default class NotAuthorizedPage extends MYALayout {
  title = this.page.getByRole('heading', {
    name: 'You cannot access this page',
  });
}
