import { Navigate } from "react-router-dom";
import {useSiteContext} from "../ContextProviders/SiteContextProvider";

type GuardedRouteProps = {
    permission: string;
    children: React.ReactNode;
}

export const GuardedRoute = ({permission, children}: GuardedRouteProps) => {
    const {hasPermission} = useSiteContext();
    return hasPermission(permission) ? <>{children}</> : <Navigate to="/unauthorised" replace/>
}