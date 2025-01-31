'use client';
import Link from 'next/link';
import { Site } from '@types';
import { Card, TextInput } from '@nhsuk-frontend-components';
import { sortSitesByName } from '@sorting';
import { ChangeEvent, useState } from 'react';

type Props = {
  sites: Site[];
};

const SiteList = ({ sites }: Props) => {
  const sortedSites = sites.toSorted(sortSitesByName);
  const [filteredSites, setFilteredSites] = useState(sortedSites);

  const handleInputChange = (e: ChangeEvent<HTMLInputElement>) => {
    const searchInput = e.target.value.toLowerCase();
    if (searchInput.length >= 3) {
      setFilteredSites(
        sortedSites.filter(
          s =>
            s.name.toLowerCase().includes(searchInput) ||
            s.odsCode.toLowerCase() === searchInput,
        ),
      );
    } else {
      setFilteredSites(sortedSites);
    }
  };

  return (
    <Card title="Choose a site">
      <TextInput
        id="site-search"
        aria-label="site-search"
        placeholder="Search"
        onChange={handleInputChange}
      ></TextInput>
      <ul className="nhsuk-list nhsuk-list--border">
        {filteredSites.map(s => (
          <li key={s.id}>
            <Link
              aria-label={s.name}
              className="nhsuk-back-link__link"
              href={`/site/${s.id}`}
            >
              {s.name}
            </Link>
          </li>
        ))}
      </ul>
    </Card>
  );
};

export default SiteList;
