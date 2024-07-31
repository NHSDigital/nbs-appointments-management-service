import { AuthWrapper } from "@/app/components/authwrapper";
import ServerNotificationListener from "@/app/components/server-notification-listener";
import { When } from "@/app/components/when";
import { AuthContextProvider } from "@/app/context/auth";
import { fecthPermissions } from "@/app/lib/usersService"

export type LayoutProps = {
  params: {
      site: string
  }
  children: React.ReactNode
}

const Layout = async ({
    params,   
    children,
  }: LayoutProps) => {
    const permissions = await fecthPermissions(params.site);

    return (
        <div>
          <When condition={permissions.length === 0}>
            <div>You do not have permission to access this site</div>
          </When>
          <When condition={permissions.length > 0}>
            <AuthContextProvider permissions={permissions}>
              {children}
            </AuthContextProvider>
          </When>
        </div>
    )
}

export default Layout