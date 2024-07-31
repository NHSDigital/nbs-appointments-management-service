import { fetchUsers, fetchRoles, saveUserRoleAssignments, RoleAssignment } from "@/app/lib/usersService";
import { UserPageProps } from "./page";
import { When } from "@/app/components/when";
import { RolesForm } from "./roles-form";

const AssignRoles = async ({params, searchParams} : UserPageProps) => {
    const user = searchParams?.user;
    const {status, data} = await fetchRoles();
    const users = await fetchUsers(params.site);

    const currentUserAssignments = users.find(usr => usr.id === user)?.roleAssignments ?? [] as RoleAssignment[];

    return (
        <>
            <div style={{marginBottom: "20px", marginTop: "20px"}}>Assign roles to {user}</div>
            <When condition={currentUserAssignments.length === 0}>
                <div className="p-4 mb-4 text-sm text-yellow-800 rounded-lg bg-yellow-50 dark:bg-gray-800 dark:text-yellow-300" role="alert">
                    <span className="font-medium">New User!</span> This user is not currently assigned roles to the this site. This will invite them. Or we can have a check box or some other UI to allow certain options to be done
                    <div className="flex items-center ps-3">
                        <input id="vue-checkbox" type="checkbox" className="w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500 dark:focus:ring-blue-600 dark:ring-offset-gray-700 dark:focus:ring-offset-gray-700 focus:ring-2 dark:bg-gray-600 dark:border-gray-500" />
                        <label className="w-full py-3 ms-2 text-sm font-medium text-gray-900 dark:text-gray-300">Send an invite</label>
                    </div>
                </div>
            </When>
            <RolesForm site={params.site} user={user!} roles={data} assignments={currentUserAssignments} />
        </>
    )
}

export default AssignRoles