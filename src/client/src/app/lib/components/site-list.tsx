'use client';
import { Site } from '@types';
import { sortSitesByName } from '@sorting';
import { ChangeEvent, useState } from 'react';
import Link from 'next/link';
import SearchButton from './nhsuk-frontend/search-button';
import { Table, TextInput } from 'nhsuk-react-components';

type Props = {
  sites: Site[];
};

const SiteList = ({ sites }: Props) => {
  const sortedSites = sites.toSorted(sortSitesByName);
  const [filteredSites, setFilteredSites] = useState(sortedSites);
  const [searchValue, setSearchValue] = useState('');
  const [showSearchMsg, setShowSearchMsg] = useState(false);

  const handleSearchClick = () => {
    const searchQuery = searchValue.toLowerCase();
    if (searchQuery.length >= 3) {
      setFilteredSites(
        sortedSites.filter(
          s =>
            s.name.toLowerCase().includes(searchQuery) ||
            s.odsCode.toLowerCase().includes(searchQuery),
        ),
      );
      setShowSearchMsg(true);
    } else {
      setFilteredSites(sortedSites);
      setShowSearchMsg(false);
    }
  };
  const handleClearClick = () => {
    setSearchValue('');
    setFilteredSites(sortedSites);
    setShowSearchMsg(false);
  };
  const handleSearchValueChange = (e: ChangeEvent<HTMLInputElement>) => {
    setSearchValue(e.target.value);
    setShowSearchMsg(false);
  };

  return (
    <>
      <div className="search-bar">
        <div>
          <TextInput
            id="site-search"
            label="Search active sites by name or ODS code"
            aria-label="Search active sites by name or ODS code"
            value={searchValue}
            onChange={handleSearchValueChange}
          ></TextInput>
        </div>
        <SearchButton onClick={handleSearchClick}>Search</SearchButton>
        <SearchButton onClick={handleClearClick}>Clear</SearchButton>
      </div>
      {showSearchMsg && searchValue.length > 0 && (
        <p>
          {filteredSites.length > 0
            ? `Found ${filteredSites.length} site(s) matching "${searchValue}".`
            : `No sites found matching "${searchValue}"`}
        </p>
      )}
      <Table>
        <Table.Head>
          <Table.Row>
            <Table.Cell>Name</Table.Cell>
            <Table.Cell>ICB</Table.Cell>
            <Table.Cell>ODS</Table.Cell>
            <Table.Cell>Action</Table.Cell>
          </Table.Row>
        </Table.Head>
        <Table.Body>
          {filteredSites.map(site => (
            <Table.Row key={site.id}>
              <Table.Cell>{site.name}</Table.Cell>
              <Table.Cell>{site.integratedCareBoard}</Table.Cell>
              <Table.Cell>{site.odsCode}</Table.Cell>
              <Table.Cell>
                <Link
                  key={site.id}
                  aria-label={`View ${site.name}`}
                  href={`/site/${site.id}`}
                >
                  View
                </Link>
              </Table.Cell>
            </Table.Row>
          ))}
        </Table.Body>
      </Table>
    </>
  );
};

export default SiteList;
