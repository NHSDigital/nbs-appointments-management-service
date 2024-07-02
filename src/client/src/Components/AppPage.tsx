import { useAuthContext } from "../ContextProviders/AuthContextProvider";
import { Permissions } from "../Types/Permissions";
import { NhsFooter } from "./NhsFooter";
import { NhsHeader } from "./NhsHeader";
import { SiteIndicator } from "./SiteIndicator";
import { useLocation } from 'react-router-dom'

type AppPageProps = {
    navLinks: {name:string, route:string}[]
    children: React.ReactNode;
  };
  
  export const AppPage = ({ navLinks, children}: AppPageProps) => {
    const { getUserEmail, signOut, hasPermission } = useAuthContext();
    const currentRoute = useLocation()

    if(!hasPermission(Permissions.CANGETAVAILABILITY)){
      navLinks = navLinks.filter(link => link.route != "/templates")
    }
    return(
    <>
        <NhsHeader navLinks={navLinks} userEmail={getUserEmail()} signOut={signOut}  />
        <SiteIndicator title={navLinks.find(x => x.route === currentRoute.pathname)?.name ?? ""} />
        <div className="nhsuk-width-container-fluid">
          <main className="nhsuk-main-wrapper " id="maincontent" role="main">
                {children}
          </main>
        </div>
        <NhsFooter/>
    </>)
  }
  