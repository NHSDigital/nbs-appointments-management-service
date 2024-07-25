export async function fetchUsers (site: string) {    
    //await new Promise((resolve) => setTimeout(resolve, 3000));
    var response = await fetch(`http://localhost:7071/api/users?site=${site}`,
        {
        headers: {
          "Authorization": "ApiKey 12345",
        }});
    return response.json().then(data => data as User[])
}

export async function fetchRoles () {
    var response = await fetch("http://localhost:7071/api/roles",
        {
        headers: {
          "Authorization": "ApiKey 12345",
        }});
    return response.json().then(data => data.roles as Role[])
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