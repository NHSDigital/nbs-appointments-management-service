import render from '@testing/render';
import { screen } from '@testing-library/react';
import FeedbackBanner from './feedback-banner';

describe('Feedback Banner', () => {
  it('renders', () => {
    const originPage = 'view-availability-week';
    render(<FeedbackBanner originPage={originPage} />);

    //Tag
    const tagElement = screen.getByText('Feedback');
    expect(tagElement).toBeInTheDocument();
    expect(tagElement.tagName).toBe('STRONG');

    //Link
    const linkElement = screen.getByRole('link', {
      name: /Give page or site feedback \(opens in a new tab\)/i,
    });
    expect(linkElement).toBeInTheDocument();
    expect(linkElement).toHaveAttribute(
      'href',
      `https://feedback.digital.nhs.uk/jfe/form/SV_0I2qLDukSOJtvjU?origin=${originPage}`,
    );
    expect(linkElement).toHaveAttribute('target', '_blank');
    expect(linkElement).toHaveAttribute('rel', 'noopener noreferrer');

    //Remaining text
    const spanElement = screen.getByText(
      '- this is a new service, your feedback will help us improve it.',
    );
    expect(spanElement).toBeInTheDocument();
  });
});
