import { screen, waitFor } from '@testing-library/react';
import { Tab, Tabs } from '@nhsuk-frontend-components';
import render from '@testing/render';
import { ReadonlyURLSearchParams } from 'next/navigation';
import { SearchParamsContext } from 'next/dist/shared/lib/hooks-client-context.shared-runtime';

describe('Tabs', () => {
  it('renders', () => {
    render(
      <SearchParamsContext.Provider value={new ReadonlyURLSearchParams()}>
        <Tabs>
          <Tab title="RSV">
            <div>This is a list of RSV appointments</div>
          </Tab>
          <Tab title="Covid">
            <div>This is a list of covid appointments</div>
          </Tab>
          <Tab title="Flu">
            <div>This is a list of flu appointments</div>
          </Tab>
        </Tabs>
      </SearchParamsContext.Provider>,
    );

    expect(screen.getByRole('list')).toBeInTheDocument();

    expect(screen.getByRole('listitem', { name: 'RSV' })).toBeInTheDocument();
    expect(screen.getByRole('listitem', { name: 'Covid' })).toBeInTheDocument();
    expect(screen.getByRole('listitem', { name: 'Flu' })).toBeInTheDocument();
  });

  it('renders the first tab by default', () => {
    render(
      <SearchParamsContext.Provider value={new ReadonlyURLSearchParams()}>
        <Tabs>
          <Tab title="RSV">
            <div>This is a list of RSV appointments</div>
          </Tab>
          <Tab title="Covid">
            <div>This is a list of covid appointments</div>
          </Tab>
          <Tab title="Flu">
            <div>This is a list of flu appointments</div>
          </Tab>
        </Tabs>
      </SearchParamsContext.Provider>,
    );

    expect(
      screen.getByText('This is a list of RSV appointments'),
    ).toBeInTheDocument();
    expect(
      screen.queryByText('This is a list of covid appointments'),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByText('This is a list of flu appointments'),
    ).not.toBeInTheDocument();
  });

  it('renders a specified tab by default if specified', () => {
    render(
      <SearchParamsContext.Provider
        value={new ReadonlyURLSearchParams('tab=2')}
      >
        <Tabs>
          <Tab title="RSV">
            <div>This is a list of RSV appointments</div>
          </Tab>
          <Tab title="Covid">
            <div>This is a list of covid appointments</div>
          </Tab>
          <Tab title="Flu">
            <div>This is a list of flu appointments</div>
          </Tab>
        </Tabs>
      </SearchParamsContext.Provider>,
    );

    expect(
      screen.queryByText('This is a list of RSV appointments'),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByText('This is a list of covid appointments'),
    ).not.toBeInTheDocument();
    expect(
      screen.getByText('This is a list of flu appointments'),
    ).toBeInTheDocument();
  });

  it('toggles between tabs when labels are clicked', async () => {
    const { user } = render(
      <SearchParamsContext.Provider value={new ReadonlyURLSearchParams()}>
        <Tabs>
          <Tab title="RSV">
            <div>This is a list of RSV appointments</div>
          </Tab>
          <Tab title="Covid">
            <div>This is a list of covid appointments</div>
          </Tab>
          <Tab title="Flu">
            <div>This is a list of flu appointments</div>
          </Tab>
        </Tabs>
      </SearchParamsContext.Provider>,
    );

    expect(
      screen.getByText('This is a list of RSV appointments'),
    ).toBeInTheDocument();

    await user.click(screen.getByRole('listitem', { name: 'Covid' }));

    waitFor(() => {
      expect(
        screen.queryByText('This is a list of RSV appointments'),
      ).not.toBeInTheDocument();
      expect(
        screen.getByText('This is a list of covid appointments'),
      ).toBeInTheDocument();
    });
  });
});
