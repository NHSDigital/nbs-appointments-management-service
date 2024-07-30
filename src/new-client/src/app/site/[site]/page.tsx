import { SiteConfig } from '@types';

async function getData(siteId: string): Promise<SiteConfig> {
  if (siteId === '1000') {
    return { description: 'The magical mock site' };
  }

  // TODO: abstract this client and add authentication
  const res = await fetch(
    `http://localhost:7071/api/site-configuration?site=${siteId}`,
  );

  if (!res.ok) {
    // This will activate the closest `error.js` Error Boundary
    throw new Error('Failed to fetch data');
  }

  return res.json();
}

export default async function Page({ params }: { params: { site: string } }) {
  const siteConfig = await getData(params.site);

  return (
    <div>
      You are currently viewing site: {params.site} with config{' '}
      {siteConfig.description}
    </div>
  );
}
