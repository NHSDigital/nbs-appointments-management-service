import {GuardedRoute} from "../../Components/GuardedRoute";
import {Permissions} from "../../Types/Permissions";
import {EditUserCtx} from "../../Views/EditUser";

export default function EditUserPage() {
    return <GuardedRoute permission={Permissions.ManageUsers}>
        <EditUserCtx />
    </GuardedRoute>
}