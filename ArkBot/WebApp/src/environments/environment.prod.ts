import { commonEnvironment } from './environment.common';

export const environment = {
  production: true,
  demo: false,
  demoDate: null,
  configJsOverride: null,
  configJsDefault: commonEnvironment.configJs,
  apiBaseUrl: "<protocol>//<hostname>:<webapi_port>/api",
  signalrBaseUrl: "<protocol>//<hostname>:<webapi_port>/signalr"
};
