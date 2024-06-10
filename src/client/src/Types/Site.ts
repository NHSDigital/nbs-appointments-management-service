export type Site = {
  id: string;
  name: string;
  address: string;
};

export type SiteInformation = {
  site: Site,
  siteConfiguration: SiteConfiguration | null
}

export type SiteConfiguration = {
  siteId: string,
  informationForCitizen: string,
  serviceConfiguration: ServiceConfiguration[]
}

export type ServiceConfiguration = {
  code: string,
  displayName: string,
  duration: number,
  enabled: boolean
}

export const defaultServiceConfigurationTypes:ServiceConfiguration[] = [
  { code: "COVID:5_11_10", displayName: "Covid 5-11", duration: 10 , enabled: true},
  { code: "COVID:12_15", displayName: "Covid 12-15", duration: 10, enabled: true},
  { code: "COVID:16_17", displayName: "Covid 16-17", duration: 10, enabled: true},
  { code: "COVID:18_74", displayName: "Covid 18-74", duration: 10, enabled: true},
  { code: "COVID:75", displayName: "Covid 75+", duration: 10, enabled: true},
  { code: "FLU:18_64", displayName: "Flu 18-64", duration: 10, enabled: true},
  { code: "FLU:65", displayName: "Flu 65+", duration: 10, enabled: true},
  { code: "COVID_FLU:18_64", displayName: "Flu and Covid 18-64", duration: 10, enabled: true},
  { code: "COVID_FLU:65_74", displayName: "Flu and Covid 65-74", duration: 10, enabled: true},
  { code: "COVID_FLU:75", displayName: "Flu and Covid 75+", duration: 10, enabled: true},
];