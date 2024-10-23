import { render, screen } from '@testing-library/react';
import { NhsHeader } from '@components/nhs-header';

describe('NhsHeader', () => {
  it('renders', async () => {
    render(<NhsHeader>This is a header child</NhsHeader>);

    expect(screen.getByText('This is a header child')).toBeVisible();
  });
});
