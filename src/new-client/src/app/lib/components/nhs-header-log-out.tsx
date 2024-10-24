'use server';
import { fetchUserProfile } from '@services/appointmentsService';
import { signOut } from '../../auth/signOut';

const NhsHeaderLogOut = async () => {
  const userProfile = await fetchUserProfile();

  return (
    <>
      <span className="nhsuk-header-custom__user-control">
        {userProfile.emailAddress}
      </span>
      <form action={signOut}>
        <button
          aria-label="log out"
          className="nhsuk-header-custom__user-control nhsuk-header-custom__user-control-link"
          type="submit"
        >
          Log out
        </button>
      </form>
    </>
  );
};

export default NhsHeaderLogOut;
