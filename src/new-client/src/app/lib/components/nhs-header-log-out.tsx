import { signOut } from '@services/appointmentsService';

const NhsHeaderLogOut = () => {
  return (
    <form action={signOut}>
      <button
        aria-label="log out"
        className="header__user-control-link"
        type="submit"
      >
        Log out
      </button>
    </form>
  );
};

export default NhsHeaderLogOut;
