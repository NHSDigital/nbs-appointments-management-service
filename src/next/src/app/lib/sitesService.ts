import { cookies } from "next/headers";

const getEndpoint = (path: string) : string => `http://localhost:7071/api/${path}`

export async function fetchSites() {
    const tokenCookie = cookies().get("token");

    if(tokenCookie) {

        var response = await fetch(getEndpoint("user/sites"),
        {
            headers: {
                "Authorization": `Bearer ${tokenCookie.value}`,
        }});

        if(response.status === 200) {
            return response.json().then(data => data as Site[])
        }
    }

    return [] as Site[];
}

export type Site = {
    id: string,
    name: string,
    address: string
}