import redirectToIdServer from '../../auth/redirectToIdServer';

const NhsHeaderLogIn = () => {
  return (
    <form action={redirectToIdServer.bind(null, undefined)}>
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
