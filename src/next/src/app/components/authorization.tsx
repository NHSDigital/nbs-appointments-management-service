import { ReactNode } from "react";
import { fecthPermissions } from "../lib/usersService";
import { When } from "./when";

type Props = {
    requiredPermission: string;
    site: string;
    fallback?: ReactNode;
    children: ReactNode;
}

const Authorized = async ({requiredPermission, site, fallback, children}: Props) => {
    const permissions = await fecthPermissions(site);

    return (
        <>
            <When condition={permissions.includes(requiredPermission)}>
                {children}
            </When>
            <When condition={!permissions.includes(requiredPermission)}>
                {fallback}
            </When>
    </>)
}

export default Authorized