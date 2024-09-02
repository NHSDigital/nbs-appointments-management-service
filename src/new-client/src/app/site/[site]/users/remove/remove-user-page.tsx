import { Site } from '@types';
import ConfirmRemoveUserForm from './confirm-remove-user-form';

type Props = {
  user: string;
  site: Site;
};

export const RemoveUserPage = ({ user, site }: Props) => {
  return (
    <>
      <p>
        Are you sure you wish to remove {user} from {site.name}?
      </p>
      <p>This will:</p>
      <ul>
        <li>Revoke the user's access to this site only</li>
        <li>
          This will not affect their wider @nhs.net account - this will only
          remove them from the National Booking System
        </li>
        <li>This will not remove their access from any other sites</li>
      </ul>

      <ConfirmRemoveUserForm user={user} site={site.id} />
    </>
  );
};
