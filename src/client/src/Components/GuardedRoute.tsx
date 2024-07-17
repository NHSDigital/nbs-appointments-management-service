"use client"

import { useRouter } from "next/navigation";
import {useSiteContext} from "../ContextProviders/SiteContextProvider";

type GuardedRouteProps = {
    permission: string;
    children: React.ReactNode;
}

export const GuardedRoute = ({permission, children}: GuardedRouteProps) => {
    const {hasPermission} = useSiteContext();
    const router = useRouter();
    if(!hasPermission(permission)) {
        router.replace("/unauthorised");
        return null;
    }
    return <>{children}</>;
}