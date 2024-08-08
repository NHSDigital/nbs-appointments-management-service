'use client';
import { usePathname, useRouter, useSearchParams } from 'next/navigation';
import React from 'react';

const FindUserForm = ({ site }: { site: string }) => {
  const [search, setSearch] = React.useState<string>();
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const { replace } = useRouter();

  const findUser = () => {
    const params = new URLSearchParams(searchParams);
    if (search) {
      params.set('user', search);
    } else {
      params.delete('user');
    }
    replace(`${pathname}?${params.toString()}`);
  };

  const cancel = () => {
    replace(`/site/${site}/users`);
  };

  return (
    <>
      <div className="nhsuk-form-group">
        <label htmlFor="email" className="nhsuk-label">
          Email
        </label>
        <input
          id="email"
          className="nhsuk-input nhsuk-input--width-20"
          type="text"
          onChange={e => setSearch(e.target.value)}
        />
      </div>
      <div className="nhsuk-navigation">
        <button
          type="button"
          aria-label="save user"
          className="nhsuk-button nhsuk-u-margin-bottom-0"
          onClick={findUser}
        >
          Search user
        </button>
        <button
          type="button"
          aria-label="cancel"
          className="nhsuk-button nhsuk-button--secondary nhsuk-u-margin-left-3 nhsuk-u-margin-bottom-0"
          onClick={cancel}
        >
          Cancel
        </button>
      </div>
    </>
  );
};

export default FindUserForm;
