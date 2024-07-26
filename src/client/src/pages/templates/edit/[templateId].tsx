import { GuardedRoute } from "src/Components/GuardedRoute";
import { Permissions } from "src/Types/Permissions";
import { WeekTemplateEditorCtx } from "src/Views/WeekTemplateEditor";

export default function CreateTemplatePage() {
    return <GuardedRoute permission={Permissions.GetAvailability}>
                <WeekTemplateEditorCtx />
           </GuardedRoute>
}