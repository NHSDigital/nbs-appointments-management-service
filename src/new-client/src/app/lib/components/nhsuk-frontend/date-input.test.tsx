import render from '@testing/render';
import { screen } from '@testing-library/react';
import { DateInput, TextInput } from '@nhsuk-frontend-components';

describe('Date Input', () => {
  it('renders', () => {
    render(
      <DateInput
        heading="What is your date of birth?"
        hint="For example, 15 3 1984"
        id="date-of-birth-input"
      >
        <TextInput
          label="Day"
          id="date-of-birth-input-day"
          inputMode="numeric"
        />
        <TextInput
          label="Month"
          id="date-of-birth-input-month"
          inputMode="numeric"
        />
        <TextInput
          label="Year"
          id="date-of-birth-input-year"
          inputMode="numeric"
        />
      </DateInput>,
    );

    expect(screen.getByText('What is your date of birth?')).toBeInTheDocument();
  });

  it('renders a heading and hint', () => {
    render(
      <DateInput
        heading="What is your date of birth?"
        hint="For example, 15 3 1984"
        id="date-of-birth-input"
      >
        <TextInput
          label="Day"
          id="date-of-birth-input-day"
          inputMode="numeric"
        />
        <TextInput
          label="Month"
          id="date-of-birth-input-month"
          inputMode="numeric"
        />
        <TextInput
          label="Year"
          id="date-of-birth-input-year"
          inputMode="numeric"
        />
      </DateInput>,
    );

    expect(
      screen.getByRole('heading', { name: 'What is your date of birth?' }),
    ).toBeInTheDocument();
    expect(screen.getByText('For example, 15 3 1984')).toBeInTheDocument();
  });

  it('permits data entry to each input', async () => {
    const { user } = render(
      <DateInput
        heading="What is your date of birth?"
        hint="For example, 15 3 1984"
        id="date-of-birth-input"
      >
        <TextInput
          label="Day"
          id="date-of-birth-input-day"
          inputMode="numeric"
        />
        <TextInput
          label="Month"
          id="date-of-birth-input-month"
          inputMode="numeric"
        />
        <TextInput
          label="Year"
          id="date-of-birth-input-year"
          inputMode="numeric"
        />
      </DateInput>,
    );

    await user.type(screen.getByLabelText('Day'), '15');
    await user.type(screen.getByLabelText('Month'), '3');
    await user.type(screen.getByLabelText('Year'), '1984');

    expect(screen.getByLabelText('Day')).toHaveDisplayValue('15');
    expect(screen.getByLabelText('Month')).toHaveDisplayValue('3');
    expect(screen.getByLabelText('Year')).toHaveDisplayValue('1984');
  });
});
