export type TabsChildren = {
  isSelected: boolean;
  url: string;
  tabTitle: string;
  content: React.ReactNode;
};

type Props = {
  children: TabsChildren[];
};

const Tabs = ({ children }: Props) => {
  return (
    <div className="nhsuk-tabs" data-module="nhsuk-tabs">
      {renderTabs(children)}
      {renderTabContents(children)}
    </div>
  );
};

const renderTabs = (tabs: TabsChildren[]) => {
  return (
    <ul className="nhsuk-tabs__list">
      {tabs.map((tab, key) => (
        <li
          key={key}
          className={`nhsuk-tabs__list-item ${
            tab.isSelected ? 'nhsuk-tabs__list-item--selected' : ''
          }`}
        >
          <a href={tab.url} className="nhsuk-tabs__tab">
            {tab.tabTitle}
          </a>
        </li>
      ))}
    </ul>
  );
};

const renderTabContents = (tabs: TabsChildren[]) => {
  return tabs.map((tab, key) => (
    <div
      className={`nhsuk-tabs__panel ${
        !tab.isSelected ? 'nhsuk-tabs__panel--hidden' : ''
      }`}
      key={key}
    >
      {tab.content} {tab.isSelected ? 'selected' : 'not selected'}
    </div>
  ));
};

export default Tabs;
