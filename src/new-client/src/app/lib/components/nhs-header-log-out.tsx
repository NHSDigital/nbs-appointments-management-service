import { signOut } from '../../auth/signOut';

const NhsHeaderLogOut = () => {
  return (
    <form action={signOut}>
      <button
        aria-label="log out"
        className="nhsuk-header-custom__user-control nhsuk-header-custom__user-control-link"
        type="submit"
      >
        Log out
      </button>
    </form>
  );
};

export default NhsHeaderLogOut;
