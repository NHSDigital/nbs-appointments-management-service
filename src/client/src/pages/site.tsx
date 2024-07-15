import { EditSiteServicesCtx } from "src/Views/EditSiteServices";
import { GuardedRoute } from "src/Components/GuardedRoute";
import { Permissions } from "src/Types/Permissions";

export default function EditServicesPage() {
    return <GuardedRoute permission={Permissions.SetSites}>
                <EditSiteServicesCtx />
            </GuardedRoute>
}