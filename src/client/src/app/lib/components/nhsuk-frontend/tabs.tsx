'use client';
import React, { useState } from 'react';

export type TabsChildren = {
  id: string;
  isSelected: boolean;
  tabTitle: string;
  content: React.ReactNode;
};

type Props = {
  title: string;
  children: TabsChildren[];
};

/**
 * An Tag component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * @see https://service-manual.nhs.uk/design-system/components/tag
 */
const Tabs = ({ title, children }: Props) => {
  const [tabs, setTabs] = useState(children);
  const handleTabClick = (id: string) => {
    setTabs(prevTabs =>
      prevTabs.map(tab => ({
        ...tab,
        isSelected: tab.id === id,
      })),
    );
  };
  return (
    <div className="nhsuk-tabs" data-module="nhsuk-tabs">
      <h2 className="nhsuk-tabs__title">{title}</h2>
      {renderTabs(tabs, handleTabClick)}
      {renderTabContents(tabs)}
    </div>
  );
};

const renderTabs = (
  tabs: TabsChildren[],
  handleTabClick: (id: string) => void,
) => {
  return (
    <ul className="nhsuk-tabs__list">
      {tabs.map(tab => (
        <button
          key={tab.id}
          className={`nhsuk-tabs__list-item ${
            tab.isSelected ? 'nhsuk-tabs__list-item--selected' : ''
          }`}
          onClick={() => handleTabClick(tab.id)}
        >
          <a className="nhsuk-tabs__tab">{tab.tabTitle}</a>
        </button>
      ))}
    </ul>
  );
};

const renderTabContents = (tabs: TabsChildren[]) => {
  return tabs.map(tab => (
    <div
      className={`nhsuk-tabs__panel ${
        !tab.isSelected ? 'nhsuk-tabs__panel--hidden' : ''
      }`}
      id={tab.id}
      key={tab.id}
    >
      {tab.content} {tab.isSelected ? 'selected' : 'not selected'}
    </div>
  ));
};

export default Tabs;
