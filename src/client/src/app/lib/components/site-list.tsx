'use client';
import { Site } from '@types';
import { Button, Table, TextInput } from '@nhsuk-frontend-components';
import { sortSitesByName } from '@sorting';
import { ChangeEvent, useState } from 'react';
import Link from 'next/link';

type Props = {
  sites: Site[];
};

const SiteList = ({ sites }: Props) => {
  const sortedSites = sites.toSorted(sortSitesByName);
  const [filteredSites, setFilteredSites] = useState(sortedSites);
  const [searchValue, setSearchValue] = useState('');
  const handleSearchClick = () => {
    const searchQuery = searchValue.toLowerCase();
    if (searchQuery.length >= 3) {
      setFilteredSites(
        sortedSites.filter(
          s =>
            s.name.toLowerCase().includes(searchQuery) ||
            s.odsCode.toLowerCase() === searchQuery,
        ),
      );
    } else {
      setFilteredSites(sortedSites);
    }
  };
  const handleClearClick = () => {
    setSearchValue('');
    setFilteredSites(sortedSites);
  };
  const handleSearchValueChange = (e: ChangeEvent<HTMLInputElement>) => {
    setSearchValue(e.target.value);
  };

  return (
    <>
      <div className="search-bar">
        <div>
          <TextInput
            id="site-search"
            label="Search active sites by name, ICB or ODS code"
            aria-label="Search active sites by name, ICB or ODS code"
            value={searchValue}
            onChange={handleSearchValueChange}
          ></TextInput>
        </div>
        <Button styleType="secondary" onClick={handleSearchClick}>
          Search
        </Button>
        <Button styleType="secondary" onClick={handleClearClick}>
          Clear
        </Button>
      </div>
      <Table
        headers={['Name', 'ICB', 'ODS', 'Action']}
        rows={filteredSites.map(site => {
          return [
            site.name,
            site.integratedCareBoard,
            site.odsCode,
            <Link
              key={site.id}
              aria-label={`View ${site.name}`}
              href={`/site/${site.id}`}
            >
              View
            </Link>,
          ];
        })}
      />
    </>
  );
};

export default SiteList;
