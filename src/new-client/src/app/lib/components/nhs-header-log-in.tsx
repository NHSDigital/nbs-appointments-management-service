import redirectToIdServer from '../../auth/redirectToIdServer';

const NhsHeaderLogIn = ({ redirectUrl }: { redirectUrl: string }) => {
  return (
    <form action={redirectToIdServer.bind(null, redirectUrl)}>
      <button
        aria-label="log in"
        className="nhsuk-header-custom__user-control nhsuk-header-custom__user-control-link"
        type="submit"
      >
        Log in
      </button>
    </form>
  );
};

export default NhsHeaderLogIn;
