import { ScheduleEditor } from "src/Views/ScheduleEditor";
import { GuardedRoute } from "src/Components/GuardedRoute";
import { Permissions } from "src/Types/Permissions";

export default function AvailablityPage() {
    return <GuardedRoute permission={Permissions.GetAvailability}>
                <ScheduleEditor />
            </GuardedRoute>
}