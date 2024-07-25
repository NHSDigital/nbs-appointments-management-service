import { fetchRoles, fetchUsers, revokeUserAccess } from "@/app/lib/usersService"
import Link from "next/link";

type Props = {
    site: string;    
}

export const UsersList = async ({ site }: Props ) => {
    const users = await fetchUsers(site);
    const roles = await fetchRoles();

    const getRoleName = (role: string) => roles.find(r => r.id === role)?.displayName;

    return (
        <ul className="divide-y divide-gray-200 dark:divide-gray-700" style={{marginLeft: "12px"}}>
        {users.map(usr => (
            <li className="pb-3 sm:pb-4" style={{paddingTop: "8px"}}>
            <div className="flex items-center space-x-4 rtl:space-x-reverse">
               <div className="min-w-0">
                  <Link href={`users/manage?user=${usr.id}`} className="text-sm font-medium text-gray-900 truncate dark:text-white">
                     {usr.id}
                  </Link>
               </div>
               <div className="inline-flex items-center text-base font-semibold text-gray-900 dark:text-white">
               <ul className="flex flex-1 flex-wrap items-center justify-center text-gray-900 dark:text-white">
                {usr.roleAssignments.map(ra => (
                    <li style={{borderRight: "1px solid black", paddingRight: "6px", paddingLeft: "6px"}}>
                        {getRoleName(ra.role)}  
                    </li>
                ))}
                <li>
                    <form action={revokeUserAccess.bind(null, site, usr.id!)}>
                        <button type="submit" className="ml-3 px-3 py-2 text-xs font-medium text-center text-white bg-blue-700 rounded-lg hover:bg-blue-800 focus:ring-4 focus:outline-none focus:ring-blue-300 dark:bg-blue-600 dark:hover:bg-blue-700 dark:focus:ring-blue-800">Revoke Access</button>
                    </form>
                </li>
                </ul>
               </div>
            </div>
         </li>
        ))}
        </ul>
    )
}

export const UsersListSkeleton =() => (
    <div>Users are loading...</div>
)