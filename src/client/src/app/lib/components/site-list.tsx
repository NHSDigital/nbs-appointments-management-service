'use client';
import Link from 'next/link';
import { Site } from '@types';
import { Table, TextInput } from '@nhsuk-frontend-components';
import { sortSitesByName } from '@sorting';
import { ChangeEvent, useState } from 'react';
import { debounce } from '../utils/debounce';

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

  const debounceSearchHandler = debounce(handleInputChange, 300);

  return (
    <>
      <TextInput
        id="site-search"
        aria-label="site-search"
        placeholder="Search"
        onChange={debounceSearchHandler}
      ></TextInput>
      <Table
        headers={['Name', 'ICB', 'ODS', 'Action']}
        rows={filteredSites.map(site => {
          return [
            site.name,
            site.integratedCareBoard,
            site.odsCode,
            <Link key={site.id} href={`/site/${site.id}`}>
              View
            </Link>,
          ];
        })}
      />
    </>
  );
};

export default SiteList;
