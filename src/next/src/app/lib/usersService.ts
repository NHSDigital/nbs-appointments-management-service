'use server'

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";

export async function fetchUsers (site: string) {    
    //await new Promise((resolve) => setTimeout(resolve, 3000));
    const response = await fetch(`http://localhost:7071/api/users?site=${site}`,
        {
        headers: {
          "Authorization": "ApiKey 12345",
        }});
    const users = await response.json() as User[];
    return users.filter(usr => usr.id.includes("@"));
}

export async function fetchRoles () {
    var response = await fetch("http://localhost:7071/api/roles",
        {
        headers: {
          "Authorization": "ApiKey 12345",
        }});
    return response.json().then(data => data.roles as Role[])
}

export async function saveUserRoleAssignments(site: string, user: string, formData: FormData) {
    const roles = formData.getAll("roles"); // Is this a string or an array?
    const payload = {
        scope: `site:${site}`,
        user: user,
        roles: roles
    }

    await fetch("http://localhost:7071/api/user/roles",
        {
            method: "POST",
            body: JSON.stringify(payload),
            headers: {
                "Authorization": "ApiKey 12345",
              }
        }
    )

    revalidatePath(`/site/${site}/users`);
    redirect(`/site/${site}/users?action=saved`);
}

export async function revokeUserAccess(site: string, user: string) {
    const payload = {
        scope: `site:${site}`,
        user: user,
        roles: []
    }

    await fetch("http://localhost:7071/api/user/roles",
        {
            method: "POST",
            body: JSON.stringify(payload),
            headers: {
                "Authorization": "ApiKey 12345",
              }
        }
    )

    revalidatePath(`/site/${site}/users`);
    redirect(`/site/${site}/users?action=revoked`);
}

export type User = {
    id: string;
    roleAssignments: RoleAssignment[]
}

export type RoleAssignment = {
    scope: string
    role: string
}

export type Role = {
    id: string;
    displayName: string;
}