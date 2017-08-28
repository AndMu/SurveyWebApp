namespace Wikiled.Survey.Data
{
    public class SurveyData
    {
        public string PostCode { get; set; }

        public string Authority { get; set; }

        public LocalAuthority[] Data { get; set; }
    }
}
