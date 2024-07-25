import { fetchRoles, fetchUsers } from "@/app/lib/usersService"
import Link from "next/link";

type Props = {
    site: string;
    searchParams?: {
      query?: string;
      page?: string;
    }
}

export const UsersList = async ({ site, searchParams }: Props ) => {
    const users = await fetchUsers(site);
    const roles = await fetchRoles();

    const getRoleName = (role: string) => roles.find(r => r.id === role)?.displayName;

    return (
        <ul className="max-w-md divide-y divide-gray-200 dark:divide-gray-700">
        {users.map(usr => (
            <li className="pb-3 sm:pb-4">
            <div className="flex items-center space-x-4 rtl:space-x-reverse">
               <div className="flex-1 min-w-0">
                  <Link href={`users/manage?user=${usr.id}`} className="text-sm font-medium text-gray-900 truncate dark:text-white">
                     {usr.id}
                  </Link>
               </div>
               <div className="inline-flex items-center text-base font-semibold text-gray-900 dark:text-white">
               <ul className="flex flex-wrap items-center justify-center text-gray-900 dark:text-white">
                {usr.roleAssignments.map(ra => (
                    <li>
                        {getRoleName(ra.role)}
                    </li>
                ))}
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