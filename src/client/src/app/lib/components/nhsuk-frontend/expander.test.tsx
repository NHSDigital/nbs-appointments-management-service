import render from '@testing/render';
import { screen } from '@testing-library/react';
import Expander from './expander';

describe('Expander', () => {
  it('renders', () => {
    render(
      <Expander summary={'How to find your NHS number'}>
        <p>An NHS number is a 10 digit number, like 485 777 3456.</p>
        <p>
          You can find your NHS number by logging in to a GP online service or
          on any document the NHS has sent you, such as your:
        </p>
        <ul>
          <li>prescriptions</li>
          <li>test results</li>
          <li>hospital referral letters</li>
          <li>appointment letters</li>
        </ul>
        <p>Ask your GP surgery for help if you can't find your NHS number.</p>
      </Expander>,
    );

    expect(screen.getByText('How to find your NHS number')).toBeInTheDocument();
  });

  it('renders in a collapsed state by default', () => {
    render(
      <Expander summary={'How to find your NHS number'}>
        <p>An NHS number is a 10 digit number, like 485 777 3456.</p>
        <p>
          You can find your NHS number by logging in to a GP online service or
          on any document the NHS has sent you, such as your:
        </p>
        <ul>
          <li>prescriptions</li>
          <li>test results</li>
          <li>hospital referral letters</li>
          <li>appointment letters</li>
        </ul>
        <p>Ask your GP surgery for help if you can't find your NHS number.</p>
      </Expander>,
    );

    expect(
      screen.getByText(
        'An NHS number is a 10 digit number, like 485 777 3456.',
      ),
    ).toBeInTheDocument();
    expect(
      screen.getByText(
        'An NHS number is a 10 digit number, like 485 777 3456.',
      ),
    ).not.toBeVisible();
  });

  it('expands when clicked', async () => {
    const { user } = render(
      <Expander summary={'How to find your NHS number'}>
        <p>An NHS number is a 10 digit number, like 485 777 3456.</p>
        <p>
          You can find your NHS number by logging in to a GP online service or
          on any document the NHS has sent you, such as your:
        </p>
        <ul>
          <li>prescriptions</li>
          <li>test results</li>
          <li>hospital referral letters</li>
          <li>appointment letters</li>
        </ul>
        <p>Ask your GP surgery for help if you can't find your NHS number.</p>
      </Expander>,
    );

    expect(
      screen.getByText(
        'An NHS number is a 10 digit number, like 485 777 3456.',
      ),
    ).not.toBeVisible();

    await user.click(screen.getByText('How to find your NHS number'));

    expect(
      screen.getByText(
        'An NHS number is a 10 digit number, like 485 777 3456.',
      ),
    ).toBeVisible();
  });

  it('expands when clicked then collapses when clicked again', async () => {
    const { user } = render(
      <Expander summary={'How to find your NHS number'}>
        <p>An NHS number is a 10 digit number, like 485 777 3456.</p>
        <p>
          You can find your NHS number by logging in to a GP online service or
          on any document the NHS has sent you, such as your:
        </p>
        <ul>
          <li>prescriptions</li>
          <li>test results</li>
          <li>hospital referral letters</li>
          <li>appointment letters</li>
        </ul>
        <p>Ask your GP surgery for help if you can't find your NHS number.</p>
      </Expander>,
    );

    await user.click(screen.getByText('How to find your NHS number'));
    expect(
      screen.getByText(
        'An NHS number is a 10 digit number, like 485 777 3456.',
      ),
    ).toBeVisible();

    await user.click(screen.getByText('How to find your NHS number'));
    expect(
      screen.getByText(
        'An NHS number is a 10 digit number, like 485 777 3456.',
      ),
    ).not.toBeVisible();
  });
});
