import { GuiAppPage } from './app.po';

describe('gui-app App', () => {
  let page: GuiAppPage;

  beforeEach(() => {
    page = new GuiAppPage();
  });

  it('should display message saying app works', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('app works!');
  });
});
