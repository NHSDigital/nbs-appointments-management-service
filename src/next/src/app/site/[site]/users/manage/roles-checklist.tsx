'use client'

import { When } from "@/app/components/when"
import { useAuthContext } from "@/app/context/auth"
import { Role, RoleAssignment } from "@/app/lib/usersService"
import React from "react"

type Props = {
    roles: Role[]
    currentUserAssignments: RoleAssignment[]
}

const RolesList = ({roles, currentUserAssignments} : Props) => {
    const { hasPermission } = useAuthContext();
    const [checkedAssignments, setCheckedAssignments] = React.useState<RoleAssignment[]>(currentUserAssignments);

    const toggleRole = (role: string) => {
        checkedAssignments?.find(ca => ca.role === role) ?
            setCheckedAssignments( checkedAssignments?.filter(r => r.role !== role) ) :
            setCheckedAssignments( [...checkedAssignments, {role, scope: "site:1000"}] );
    }

    return (
        <ul className="text-sm font-medium text-gray-900 bg-white border border-gray-200 rounded-lg dark:bg-gray-700 dark:border-gray-600 dark:text-white">
        {roles.map(r => (
            <li key={r.id} className="w-full border-b border-gray-200 rounded-t-lg dark:border-gray-600">
                <div className="flex items-center ps-3">
                    <input type="checkbox" name="roles" value={r.id} onChange={(e) => toggleRole(e.target.value)} checked={checkedAssignments.find(ca => ca.role === r.id) !== undefined} className="w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500 dark:focus:ring-blue-600 dark:ring-offset-gray-700 dark:focus:ring-offset-gray-700 focus:ring-2 dark:bg-gray-600 dark:border-gray-500" />
                    <label className="w-full py-3 ms-2 text-sm font-medium text-gray-900 dark:text-gray-300">{r.displayName}</label>
                </div>
            </li>
        ))}        
        </ul>
    )
}

export default RolesList