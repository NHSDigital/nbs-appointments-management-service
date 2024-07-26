import { GuardedRoute } from "src/Components/GuardedRoute";
import { Permissions } from "src/Types/Permissions";
import { UserManagementCtx } from "src/Views/UserManagement";

export default function AvailablityPage() {
  return (
    <GuardedRoute permission={Permissions.ViewUsers}>
      <UserManagementCtx />
    </GuardedRoute>
  );
}
