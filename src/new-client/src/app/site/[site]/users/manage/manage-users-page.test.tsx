import { render, screen } from '@testing-library/react';
import { ManageUsersPage } from './page';

jest.mock('./find-user-form', () => {
  const MockFindUserForm = () => {
    return <div>Find User Form</div>;
  };
  return MockFindUserForm;
});

jest.mock('./assign-roles', () => {
  const MockAssignRolesForm = () => {
    return <div>Assign Roles Form</div>;
  };
  return MockAssignRolesForm;
});

describe('User Management Page', () => {
  it('renders title and subtitle correctly', () => {
    render(
      <ManageUsersPage
        params={{ site: 'TEST' }}
        searchParams={{}}
        userIsSpecified={false}
      />,
    );
    expect(screen.getByText('Staff Role Management')).toBeVisible();
    expect(
      screen.getByText('Set the details and roles of a new user'),
    ).toBeVisible();
  });

  it('renders search form when no user in search params', () => {
    render(
      <ManageUsersPage
        params={{ site: 'TEST' }}
        searchParams={{}}
        userIsSpecified={false}
      />,
    );
    expect(screen.getByText('Staff Role Management')).toBeVisible();
    expect(screen.getByText('Find User Form')).toBeVisible();
  });

  it('renders assign roles form when user in search params', () => {
    render(
      <ManageUsersPage
        params={{ site: 'TEST' }}
        searchParams={{ user: 'test@nhs.net' }}
        userIsSpecified
      />,
    );
    expect(screen.getByText('Staff Role Management')).toBeVisible();
    expect(screen.getByText('Assign Roles Form')).toBeVisible();
  });
});
