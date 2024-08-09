import { fetchPermissions } from '@services/appointmentsService';
import NhsWarning from '@components/nhs-warning';
import { When } from '@components/when';

export type LayoutProps = {
  params: {
    site: string;
  };
  children: React.ReactNode;
};

const Layout = async ({ params, children }: LayoutProps) => {
  const permissions = await fetchPermissions(params.site);

  return (
    <div>
      <When condition={permissions.length === 0}>
        <div className="nhsuk-width-container nhsuk-grid-row nhsuk-main-wrapper">
          <NhsWarning title="You cannot access this site">
            <p>
              You do not have any roles assigned to you for this site. If you
              need access to this site you will need to contact the site
              administrator.
            </p>
          </NhsWarning>
        </div>
      </When>
      <When condition={permissions.length > 0}>
        <main
          className="nhsuk-main-wrapper nhsuk-main-wrapper--s nhsuk-width-container-fluid"
          role="main"
        >
          {children}
        </main>
      </When>
    </div>
  );
};

export default Layout;
