import NhsWarning from '@components/nhs-warning';
import { When } from '@components/when';
import { fecthPermissions } from '../../lib/auth';

export type LayoutProps = {
  params: {
    site: string;
  };
  children: React.ReactNode;
};

const Layout = async ({ params, children }: LayoutProps) => {
  const permissions = await fecthPermissions(params.site);

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
      <When condition={permissions.length > 0}>{children}</When>
    </div>
  );
};

export default Layout;
