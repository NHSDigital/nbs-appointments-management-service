"use client"

import { useNotifications } from "@/app/context/notifications"
import { Role, RoleAssignment, saveUserRoleAssignments } from "@/app/lib/usersService"
import Link from "next/link"
import React from "react"
import { SubmitHandler, useForm } from "react-hook-form"

type FormFields = {
    roles: string[]
}

export const RolesForm = ({site, user, roles, assignments}: {site: string, user:string, roles: Role[], assignments: RoleAssignment[]}) => {

    const {addNotification} = useNotifications();

    const { register, handleSubmit } = useForm<FormFields>({defaultValues: {
        roles: assignments.map(a => a.role)
    }});
    
    const submitForm: SubmitHandler<FormFields> = async (form) => {
        const result = await saveUserRoleAssignments(site, user, form.roles);
        if(result !== undefined) {
            addNotification(result!);
        }
    }

    return (
        <form onSubmit={handleSubmit(submitForm)}>
        <ul className="text-sm font-medium text-gray-900 bg-white border border-gray-200 rounded-lg dark:bg-gray-700 dark:border-gray-600 dark:text-white">
            {roles.map(r => (
                <li key={r.id} className="w-full border-b border-gray-200 rounded-t-lg dark:border-gray-600">
                    <div className="flex items-center ps-3">
                        <input type="checkbox" value={r.id} {...register("roles")} />
                        <label className="w-full py-3 ms-2 text-sm font-medium text-gray-900 dark:text-gray-300">{r.displayName}</label>
                    </div>
                </li>
            ))}
        </ul>
        <div style={{marginTop: "20px"}}>
        <button type="submit" className="text-white bg-blue-700 hover:bg-blue-800 focus:ring-4 focus:ring-blue-300 font-medium rounded-lg text-sm px-5 py-2.5 me-2 mb-2 dark:bg-blue-600 dark:hover:bg-blue-700 focus:outline-none dark:focus:ring-blue-800">Apply Roles</button>
        <Link href="./" className="py-2.5 px-5 me-2 mb-2 text-sm font-medium text-gray-900 focus:outline-none bg-white rounded-lg border border-gray-200 hover:bg-gray-100 hover:text-blue-700 focus:z-10 focus:ring-4 focus:ring-gray-100 dark:focus:ring-gray-700 dark:bg-gray-800 dark:text-gray-400 dark:border-gray-600 dark:hover:text-white dark:hover:bg-gray-700">Cancel</Link>
        </div>
    </form>)
}