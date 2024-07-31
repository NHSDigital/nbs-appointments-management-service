import { Suspense } from 'react';
import { UsersList, UsersListSkeleton } from './list';
import Link from 'next/link';
import React from 'react';
import Authorized from '@/app/components/authorization';
import ServerNotificationListener from '@/app/components/server-notification-listener';

type PageProps = {
    params: {
        site: string
    }
    searchParams?: {
      action: string
    }
}

const Page = async ({params, searchParams}:PageProps) => {
    
    return (
        <>
            <h2 className="text-4xl font-extrabold dark:text-white">User Management</h2>
            <Authorized requiredPermission="users:manage" site={params.site} fallback={<div>You can't manage users</div>}>
                <AddRoleAssignmentsButton/>
            </Authorized>
            <ServerNotificationListener />
            <Suspense fallback={<UsersListSkeleton />}>
                <UsersList site={params.site} />
            </Suspense>
        </>
    )
}

const AddRoleAssignmentsButton = () => (
    <div style={{padding: "10px"}}>
                <Link href={`users/manage`} className="inline-flex items-center px-4 py-2 bg-blue-500 border border-transparent rounded-md font-semibold text-white hover:bg-blue-600 focus:outline-none focus:ring focus:border-blue-700 active:bg-blue-800">

                <svg className="svg-circleplus" viewBox="0 0 100 100" style={{stroke: "white", height: "24px", marginRight: "10px"}}>
                    <circle cx="50" cy="50" r="45" fill="none" stroke-width="7.5"></circle>
                    <line x1="32.5" y1="50" x2="67.5" y2="50" stroke-width="5"></line>
                    <line x1="50" y1="32.5" x2="50" y2="67.5" stroke-width="5"></line>
                </svg>

                Add User Role Assignments
            </Link>
            </div>
)

export default Page