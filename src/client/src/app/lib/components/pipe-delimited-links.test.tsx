import render from '@testing/render';
import { screen } from '@testing-library/react';
import PipeDelimitedLinks from './pipe-delimited-links';

describe('Pipe Delimited Links', () => {
  it('renders', () => {
    const actionLinks = [{ text: 'Link 1', href: '/link1' }];
    render(<PipeDelimitedLinks actionLinks={actionLinks} />);
  });

  it('renders a single link correctly', () => {
    const actionLinks = [{ text: 'Link 1', href: '/link1' }];
    render(<PipeDelimitedLinks actionLinks={actionLinks} />);

    expect(screen.getByRole('link', { name: 'Link 1' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Link 1' })).toHaveAttribute(
      'href',
      '/link1',
    );
  });

  it('renders multiple links with pipe delimiters', () => {
    const actionLinks = [
      { text: 'Link 1', href: '/link1' },
      { text: 'Link 2', href: '/link2' },
      { text: 'Link 3', href: '/link3' },
    ];
    render(<PipeDelimitedLinks actionLinks={actionLinks} />);

    expect(screen.getByRole('link', { name: 'Link 1' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Link 1' })).toHaveAttribute(
      'href',
      '/link1',
    );
    expect(screen.getByRole('link', { name: 'Link 2' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Link 2' })).toHaveAttribute(
      'href',
      '/link2',
    );
    expect(screen.getByRole('link', { name: 'Link 3' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Link 3' })).toHaveAttribute(
      'href',
      '/link3',
    );

    expect(screen.getByText(/Link 1 \| Link 2 \| Link 3/)).toBeInTheDocument();
    const pipeDelimiters = screen.getAllByText('\|');
    expect(pipeDelimiters).toHaveLength(2);
  });

  it('does not render pipe delimiters for a single link', () => {
    const actionLinks = [{ text: 'Link 1', href: '/link1' }];
    render(<PipeDelimitedLinks actionLinks={actionLinks} />);

    expect(screen.queryByText(' | ')).not.toBeInTheDocument();
  });
});
