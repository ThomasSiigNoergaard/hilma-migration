using Hilma.Domain.Attributes;

namespace Hilma.Domain.Configuration
{
    [Contract]
    public class TranslationsConfigurationContract
    {
        public string TranslationsEndpoint { get; set; }
        public int TranslationsCacheLifetimeMinutes { get; set; }
        public string OrganisationUrl { get; set; }
        public string NoReplyEmailAddress { get; set; }
    }
}
