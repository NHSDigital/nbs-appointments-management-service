import redirectToIdServer from '../../auth/redirectToIdServer';

const NhsHeaderLogIn = ({ redirectUrl }: { redirectUrl: string }) => {
  return (
    <form action={redirectToIdServer.bind(null, redirectUrl, 'nhs-mail')}>
      <button
        aria-label="log in with NHS Mail"
        className="nhsuk-header-custom__user-control nhsuk-header-custom__user-control-link"
        type="submit"
      >
        Log in with NHS Mail
      </button>
    </form>
  );
};

export default NhsHeaderLogIn;
