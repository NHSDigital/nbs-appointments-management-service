import { TemplateListView } from "src/Views/TemplateListView";
import { GuardedRoute } from "src/Components/GuardedRoute";
import { Permissions } from "src/Types/Permissions";

export default function ListTemplatesPage() {
    return  <GuardedRoute permission={Permissions.GetAvailability}>
                <TemplateListView />
            </GuardedRoute>
}