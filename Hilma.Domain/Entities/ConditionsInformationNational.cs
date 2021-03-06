using Hilma.Domain.Attributes;
using Hilma.Domain.Enums;

namespace Hilma.Domain.Entities
{
    /// <summary>
    /// Conditions for participation.
    /// National contracts only.
    /// </summary>
    [Contract]
    public class ConditionsInformationNational
    {
        /// <summary>
        /// Participation and contractor selection criteria description
        /// </summary>
        [CorrigendumLabel("suitability_requirements", "III.1")]
        public string[] ParticipantSuitabilityCriteria { get; set; }
        /// <summary>
        /// Certifications and other reports, on which suitability is assessed
        /// Todistukset ja selvitykset, joiden perusteella soveltuvuuden täyttyminen arvioidaan  
        /// </summary>
        [CorrigendumLabel("required_certifications", "III.1")]
        public string[] RequiredCertifications { get; set; }

        /// <summary>
        /// Additional information
        /// </summary>
        [CorrigendumLabel("info_additional", "III.1")]
        public string[] AdditionalInformation { get; set; }

        #region VueJS
        /// <summary>
        ///     Validation state for Vuejs application.
        /// </summary>
        public ValidationState ValidationState { get; set; }
        #endregion

        /// <summary>
        /// Procurement is reserved for sheltered workshop or program
        /// </summary>
        [CorrigendumLabel("restricted_sheltered_program", "III.1")]
        public bool ReservedForShelteredWorkshopOrProgram { get; set; }
    }
}
