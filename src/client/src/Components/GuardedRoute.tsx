import { Navigate } from "react-router-dom";
import { useAuthContext } from "../ContextProviders/AuthContextProvider"

type GuardedRouteProps = {
    permission: string;
    children: React.ReactNode;
}

export const GuardedRoute = ({permission, children}: GuardedRouteProps) => {
    const {hasPermission} = useAuthContext();
    return hasPermission(permission) ? <>{children}</> : <Navigate to="/unauthorised" replace/>
}