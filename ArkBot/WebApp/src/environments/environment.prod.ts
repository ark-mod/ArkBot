import { commonEnvironment } from './environment.common';

export const environment = {
  production: true,
  demo: false,
  demoDate: null,
  configJsOverride: null,
  configJsDefault: commonEnvironment.configJs,
  apiBaseUrl: "/api",
  signalrBaseUrl: ""
};
