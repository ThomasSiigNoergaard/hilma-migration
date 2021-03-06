using System;
using System.ComponentModel.DataAnnotations;
using AutoMapper.Attributes;
using Hilma.Domain.Attributes;
using Hilma.Domain.Entities;
using Hilma.Domain.Enums;
using Hilma.Domain.Extensions;

namespace Hilma.Domain.DataContracts
{
    /// <summary>
    ///     Describes an organisation to vuejs app.
    /// </summary>
    [MapsFrom(typeof(Organisation))]
    [Contract]
    public class OrganisationContract
    {
        /// <summary>
        ///     Hilma assigned primary key for the organisation.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        ///     Main form information bunch of the organisation.
        /// </summary>
        public ContractBodyContactInformation Information { get; set; }

        /// <summary>
        ///     Type of the contracting authority
        /// </summary>
        [Required]
        [CorrigendumLabel("ca_type", "I.4")]
        public ContractingAuthorityType ContractingAuthorityType { get; set; }

        /// <summary>
        ///     Asked if ContractingAuthorityType is "Other"
        /// </summary>
        [MaxLength(1000)]
        [CorrigendumLabel("other_type", "I.4")]
        public string OtherContractingAuthorityType { get; set; }

        /// <summary>
        /// Used in F15, F24 and F25 to determine type of main activity:
        ///  (in the case of a notice published by a contracting authority -> MainActivity)
        ///  or
        ///  (in the case of a notice published by a contracting entity -> MainActivityUtilities )
        /// </summary>
        public ContractingType ContractingType { get; set; }

        /// <summary>
        ///     Primary field of operations of the organisation.
        /// </summary>
        [CorrigendumLabel("mainactivity", "I.5")]
        public MainActivity MainActivity { get; set; }

        /// <summary>
        ///     Asked if MainActivity is "Other"
        /// </summary>
        [CorrigendumLabel("other_activity", "I.5")]
        public string OtherMainActivity { get; set; }

        /// <summary>
        ///     Main activity utilities.
        /// </summary>
        [CorrigendumLabel("mainactivity", "I.6")]
        public MainActivityUtilities MainActivityUtilities { get; set; }

        /// <summary>
        ///     Vuejs application validation state for organisation section.
        /// </summary>
        public ValidationState ValidationState { get; set; }

        /// <summary>
        ///     Currently selected department for this organisation.
        /// </summary>
        public Guid? DepartmentId { get; set; }
        
        public void Trim()
        {
            if (ContractingAuthorityType != ContractingAuthorityType.OtherType)
            {
                OtherContractingAuthorityType = string.Empty;
            }

            if (MainActivity != MainActivity.OtherActivity && MainActivityUtilities != MainActivityUtilities.OtherActivity)
            {
                OtherMainActivity = string.Empty;
            }

            if (Information != null)
            {
                Information.MainUrl = Information.MainUrl.CleanUrl();
            }
        }
    }
}
