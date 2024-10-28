import render from '@testing/render';
import { screen } from '@testing-library/react';
import NhsMainContainer from './nhs-main-container';

describe('NHS Main Container', () => {
  it('renders', () => {
    render(<NhsMainContainer>This is a child element</NhsMainContainer>);

    expect(screen.getByRole('main')).toBeInTheDocument();
    expect(screen.getByText('This is a child element')).toBeInTheDocument();
  });
});
