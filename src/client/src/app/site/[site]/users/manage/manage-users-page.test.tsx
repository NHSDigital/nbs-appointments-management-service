import { render, screen } from '@testing-library/react';
import { ManageUsersPage } from './manage-users-page';

jest.mock('./find-user-form', () => {
  const MockFindUserForm = () => {
    return <div>Find User Form</div>;
  };
  return MockFindUserForm;
});

jest.mock('./user-details', () => {
  const MockUserDetailsForm = () => {
    return <div>User Details Form</div>;
  };
  return MockUserDetailsForm;
});

describe('User Management Page', () => {
  it('renders search form when no user in search params', () => {
    render(
      <ManageUsersPage
        params={{ site: 'TEST' }}
        searchParams={{}}
        userIsSpecified={false}
        oktaEnabled={false}
      />,
    );
    expect(screen.getByText('Find User Form')).toBeVisible();
  });

  it('renders user details form when user in search params', () => {
    render(
      <ManageUsersPage
        params={{ site: 'TEST' }}
        searchParams={{ user: 'test@nhs.net' }}
        userIsSpecified
        oktaEnabled={false}
      />,
    );
    expect(screen.getByText('User Details Form')).toBeVisible();
  });
});
