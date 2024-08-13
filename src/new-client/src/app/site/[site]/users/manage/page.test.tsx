import { render, screen } from '@testing-library/react';
import AssignRolesPage from './page';

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
    render(<AssignRolesPage params={{ site: 'TEST' }} searchParams={{}} />);
    expect(screen.getByText('Staff Role Management')).toBeVisible();
    expect(
      screen.getByText('Set the details and roles of a new user'),
    ).toBeVisible();
  });

  it('renders search form when no user in search params', () => {
    render(<AssignRolesPage params={{ site: 'TEST' }} searchParams={{}} />);
    expect(screen.getByText('Staff Role Management')).toBeVisible();
    expect(screen.getByText('Find User Form')).toBeVisible();
  });

  it('renders assign roles form when user in search params', () => {
    render(
      <AssignRolesPage
        params={{ site: 'TEST' }}
        searchParams={{ user: 'test@nhs.net' }}
      />,
    );
    expect(screen.getByText('Staff Role Management')).toBeVisible();
    expect(screen.getByText('Assign Roles Form')).toBeVisible();
  });
});
