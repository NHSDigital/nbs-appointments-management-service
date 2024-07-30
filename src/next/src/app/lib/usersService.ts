'use server'

import { revalidatePath } from "next/cache";
import { cookies } from "next/headers";
import { redirect } from "next/navigation";

const getEndpoint = (path: string) : string => `http://localhost:7071/api/${path}`

export async function fetchUsers (site: string) {
    const tokenCookie = cookies().get("token");

    const response = await fetch(getEndpoint(`users?site=${site}`),
        {
        headers: {
          "Authorization": `Bearer ${tokenCookie?.value}`,
        }});

    const users = await response.json() as User[];
    return users.filter(usr => usr.id.includes("@"));
}

export async function fetchRoles () {
    const tokenCookie = cookies().get("token");

    if(tokenCookie === undefined) {
        revalidatePath("/");
        throw Error("It just splode");
    }

    var response = await fetch(getEndpoint("roles"),
        {
        headers: {
          "Authorization": `Bearer ${tokenCookie?.value}`,
        }});

    if(response.status === 401)
        return {status: 401, data: [] as Role[]}

    var json = await response.json();
    return {status: response.status, data: json.roles as Role[]}
}

export async function fecthPermissions (site: string) {
    const tokenCookie = cookies().get("token");

    if(tokenCookie) {
        var response = await fetch(getEndpoint(`user/permissions?site=${site}`), 
        {
            headers: {
                "Authorization": `Bearer ${tokenCookie.value}`,
        }});

        if(response.status === 200) {
            return response.json().then(data => data.permissions as string[])
        }
    }

    return [] as string[];
}

export async function fetchAccessToken(code: string) {
    var response = await fetch(getEndpoint("token"), {
        method: "POST",
        body: code
    });

    if(response.status === 200) {
        const json = await response.json();
        if(json.token) {
            cookies().set("token", json.token)
        }
    }
}

export async function saveUserRoleAssignments(site: string, user: string, formData: FormData) {
    const tokenCookie = cookies().get("token");

    const roles = formData.getAll("roles"); // Is this a string or an array?
    const payload = {
        scope: `site:${site}`,
        user: user,
        roles: roles
    }

    var response = await fetch(getEndpoint("user/roles"),
        {
            method: "POST",
            body: JSON.stringify(payload),
            headers: {
                "Authorization": `Bearer ${tokenCookie?.value}`,
              }
        }
    )

    if(response.status === 200) {
        cookies().set("notification", "user_saved")
        revalidatePath(`/site/${site}/users`);
        redirect(`/site/${site}/users`);
    }
}

export async function revokeUserAccess(site: string, user: string) {
    const tokenCookie = cookies().get("token");
    const payload = {
        scope: `site:${site}`,
        user: user,
        roles: []
    }

    await fetch(getEndpoint("user/roles"),
        {
            method: "POST",
            body: JSON.stringify(payload),
            headers: {
                "Authorization": `Bearer ${tokenCookie?.value}`,
              }
        }
    )

    cookies().set("notification", "user_revoked")
    revalidatePath(`/site/${site}/users`);
    redirect(`/site/${site}/users`);
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