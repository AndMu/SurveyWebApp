import { WikledSurveyAppPage } from './app.po';

describe('wikiled-survey-app App', () => {
  let page: WikledSurveyAppPage;

  beforeEach(() => {
    page = new WikledSurveyAppPage();
  });

  it('should display welcome message', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('Welcome to app!');
  });
});
