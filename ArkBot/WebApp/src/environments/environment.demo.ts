import { commonEnvironment } from './environment.common';

export const environment = {
  production: true,
  demo: true,
  demoDate: '2017-11-10T16:30:00.0000000Z',
  configJsOverride: commonEnvironment.configJs,
  configJsDefault: commonEnvironment.configJs,
  apiBaseUrl: null,
  signalrBaseUrl: null
};
