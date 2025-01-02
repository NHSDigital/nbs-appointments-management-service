'use client';
import {
  Children,
  cloneElement,
  FunctionComponentElement,
  ReactNode,
} from 'react';
import Link from 'next/link';
import { useSearchParams } from 'next/navigation';

type TabProps = {
  title: string;
  children: ReactNode;
};

type InjectedTabProps = {
  tabIndex: number;
  isActive: boolean;
};

type TabsProps = {
  title?: string;
  children: ReactNode;
  initialTab?: number;
  paramsToSetOnTabChange?: { key: string; value: string }[];
};

/**
 * A tabs component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * @see https://service-manual.nhs.uk/design-system/components/tabs
 */
const Tabs = ({
  title,
  children,
  initialTab = 0,
  paramsToSetOnTabChange,
}: TabsProps) => {
  const searchParams = useSearchParams();
  const params = new URLSearchParams(searchParams.toString());
  const activeTab = Number(params.get('tab')) ?? initialTab;

  const filteredChildren = Children.toArray(
    children,
  ) as FunctionComponentElement<TabProps & InjectedTabProps>[];

  return (
    <div className="nhsuk-tabs" data-module="nhsuk-tabs">
      {title && <h2 className="nhsuk-tabs__title">{title}</h2>}

      <ul className="nhsuk-tabs__list">
        {filteredChildren.map((tab, index) => {
          return (
            <li
              key={`tab-header-${index}`}
              className={`nhsuk-tabs__list-item ${index === activeTab ? 'nhsuk-tabs__list-item--selected' : ''}`}
              aria-label={tab.props.title}
            >
              <Link
                className="nhsuk-tabs__tab"
                href={''}
                onClick={() => {
                  params.set('tab', String(index));
                  if (paramsToSetOnTabChange) {
                    paramsToSetOnTabChange.forEach(p => {
                      params.set(p.key, p.value);
                    });
                  }
                  window.history.pushState(null, '', `?${params.toString()}`);
                }}
              >
                {tab.props.title}
              </Link>
            </li>
          );
        })}
      </ul>

      {filteredChildren.map((tab, index) => {
        return cloneElement<TabProps & InjectedTabProps>(tab, {
          tabIndex: index,
          isActive: index === activeTab,
        });
      })}
    </div>
  );
};

// eslint-disable-next-line @typescript-eslint/no-unused-vars
export const Tab = ({ title, children, ...rest }: TabProps) => {
  const injectedTabProps = rest as InjectedTabProps;
  const { isActive } = injectedTabProps;

  if (!isActive) {
    return null;
  }

  return (
    <div
      key={`tab-content-panel-${injectedTabProps.tabIndex}`}
      className="nhsuk-tabs__panel"
      id={`tab-content-panel-${injectedTabProps.tabIndex}`}
    >
      {children}
    </div>
  );
};

export default Tabs;
