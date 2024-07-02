import { Navigate } from "react-router-dom";
import { useAuthContext } from "../ContextProviders/AuthContextProvider"

export const GuardedRoute = ({permission, children}: {permission: string, children: React.ReactNode}) => {
    const {hasPermission} = useAuthContext();
    // TODO create page for when permissions arent present
    return hasPermission(permission) ? <>{children}</> : <Navigate to="/unauthorised" replace/>
}