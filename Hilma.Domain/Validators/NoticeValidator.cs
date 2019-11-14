using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using AutoMapper;
using Hilma.Domain.Configuration;
using Hilma.Domain.DataContracts;
using Hilma.Domain.Entities;
using Hilma.Domain.Enums;
using Hilma.Domain.Extensions;
using Hilma.Domain.Integrations;

namespace Hilma.Domain.Validators
{
    public class NoticeValidator
    {
        public IEnumerable<string> ValidationErrors => _validationErrors.AsReadOnly();

        private readonly Notice _notice;
        private readonly List<string> _validationErrors = new List<string>();
        private static readonly XNamespace XmlnsGeneral = "http://publications.europa.eu/resource/schema/ted/R2.0.9/reception";
        private static readonly XNamespace XmlnsDefence = "http://publications.europa.eu/resource/schema/ted/R2.0.8/reception";
        private readonly IMapper _mapper;
        private ITranslationProvider _translationProvider;

        /// <summary>
        /// The notice validator constructor
        /// </summary>
        /// <param name="notice">The notice to be validated</param>
        /// <param name="mapper">Mapper needed for TED schema validation</param>
        /// <param name="translationProvider">Translation provider needed for </param>
        public NoticeValidator(Notice notice, IMapper mapper, ITranslationProvider translationProvider)
        {
            _notice = notice ?? throw new ArgumentNullException(nameof(notice));
            _mapper = mapper;
            _translationProvider = translationProvider;
        }

        public bool Validate(bool publishToTed, out string tedXml)
        {
            var noticeValid = Valid(_notice.CreatorId != null || _notice.EtsCreatorId != null, "Creator") &&
                              Valid(_notice.Type != NoticeType.Undefined, "NoticeType");

            var communicationInformationNotRequired = _notice.Type.IsContractAward()
                || _notice.Type == NoticeType.ContractAwardUtilities
                || _notice.Type == NoticeType.Modification
                || _notice.Type == NoticeType.DefenceContractAward
                || _notice.Type == NoticeType.ExAnte
                || _notice.Type == NoticeType.NationalDirectAward;

            var hilmaValidation = ValidateAll(noticeValid,
                               Validate(_notice.Project),
                               Validate(_notice.ContactPerson),
                               (communicationInformationNotRequired || Validate(_notice.CommunicationInformation)),
                               Validate(_notice.LotsInfo),
                               Validate(_notice.ProcurementObject),
                               Validate(_notice.TenderingInformation),
                               Validate(_notice.ConditionsInformation),
                               Validate(_notice.ProcedureInformation),
                               ValidateObjectDescriptions(_notice),
                               Validate(_notice.Modifications));

            // If Hilma validation fails, return that first.
            if (!hilmaValidation || _notice.Project.Publish != PublishType.ToTed)
            {
                tedXml = null;
                return hilmaValidation;
            }

            var path = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
            var schema = new XmlSchemaSet { XmlResolver = new XmlUrlResolver() };
            var nameSpace = XmlnsGeneral;
            if (_notice.Type.IsDefence())
            {
                schema.Add(XmlnsDefence.ToString(), Path.Combine(path, "Validators", "TedSchema", "208Defence", "TED_ESENDERS.xsd"));
                nameSpace = XmlnsDefence;
            }
            else
            {
                schema.Add(XmlnsGeneral.ToString(), Path.Combine(path, "Validators", "TedSchema", "209General", "TED_ESENDERS.xsd"));
            }
            schema.Compile();
            var translations = _translationProvider.GetDynamicObject(CancellationToken.None).Result;

            var xDoc = new TedNoticeFactory(_mapper.Map<NoticeContract>(_notice), _notice.Parent != null ? _mapper.Map<NoticeContract>(_notice.Parent) : new NoticeContract(), "Validator", "validator@validator.com", "TEDEXXXX", Convert.ToString(translations), publishToTed).CreateDocument();
            xDoc.Validate(schema, (sender, e) => { _validationErrors.Add(e.Message + "\n"); });

            // We don't want to send the login part in the response.
            var descendants = xDoc.Descendants(nameSpace + "FORM_SECTION");
            tedXml = string.Join("\n", descendants);
            var teSchemaValid = Valid(!_validationErrors.Any(), "TED message formed:\n" + tedXml);

            return ValidateAll(teSchemaValid);
        }

        public bool ValidateAll(params bool[] validationResults)
        {
            return validationResults.All(r => r);
        }

        #region Partial validators


        public bool Validate(ConditionsInformation info)
        {
            if (!Valid(info != null, "ConditionsInformation"))
            {
                return false;
            }

            if (_notice.Type != NoticeType.Contract)
            {
                return true;
            }

            return ValidateAll(Valid(!info.ExecutionOfServiceIsReservedForProfession ||
                                     (info.ReferenceToRelevantLawRegulationOrProvision).HasAnyContent(),
                                     "ConditionsInformation.ReferenceToRelevantLawRegulationOrProvision")
                              );


        }

        public bool Validate(TenderingInformation info)
        {
            if (!Valid(info != null, "TenderingInformation"))
            {
                return false;
            }

            if (_notice.Type != NoticeType.Contract)
            {
                return true;
            }

            var procedureType = _notice.ProcedureInformation.ProcedureType;
            var isOpenProcedure = procedureType == ProcedureType.ProctypeOpen;

            var openingConditions = info.TenderOpeningConditions;
            return ValidateAll(Valid(info.TendersOrRequestsToParticipateDueDateTime != null, "TenderingInformation.TendersOrRequestsToParticipateDueDateTime"),
                               Valid(info.Languages.Any(), "TenderingInformation.Languages"),
                               Valid(!isOpenProcedure || openingConditions != null, "TenderingInformation.TenderOpeningConditions"),
                               Valid(!isOpenProcedure || openingConditions?.OpeningDateAndTime != null && openingConditions?.OpeningDateAndTime > info.TendersOrRequestsToParticipateDueDateTime, "TenderingInformation.TenderOpeningConditions.OpeningDateAndTime")
                              );
        }

        public bool Validate(CommunicationInformation info) =>
            ValidateAll(Valid(info != null, "CommunicationInformation"),
                        Valid(_notice.Type != NoticeType.Contract || info?.ProcurementDocumentsAvailable != ProcurementDocumentAvailability.Undefined,
                                "CommunicationInformation.ProcurementDocumentsAvailable"),
                        Valid(info?.AdditionalInformation != AdditionalInformationAvailability.Undefined || _notice.Type == NoticeType.NationalAgricultureContract,
                                "CommunicationInformation.AdditionalInformation"),
                        Valid(info?.AdditionalInformation != AdditionalInformationAvailability.AddressAnother ||
                                Validate("CommunicationInformation.AdditionalInformationAddress", info.AdditionalInformationAddress),
                                         "CommunicationInformation.AdditionalInformation+AddressAnother"),
                        Valid(info?.SendTendersOption != TenderSendOptions.AddressSendTenders || !string.IsNullOrWhiteSpace(info.ElectronicAddressToSendTenders),
                                "CommunicationInformation.SendTendersOption+ElectronicAddressToSendTenders"),
                        Valid(_notice.Type != NoticeType.Contract || info.SendTendersOption != TenderSendOptions.Undefined,
                                "CommunicationInformation.SendTendersOption"),
                        Valid(info != null && (!info.ElectronicCommunicationRequiresSpecialTools ||
                                !string.IsNullOrWhiteSpace(info.ElectronicCommunicationInfoUrl)),
                                "CommunicationInformation: ElectronicCommunicationInfoUrl"));



        public bool Validate(string name, ContractBodyContactInformation info)
        {
            return ValidateAll(Valid(info != null, $"{name}"),
                               Valid(info != null && info.NutsCodes?.Any() == true, $"{name}.NutsCodes"),
                               Valid(!string.IsNullOrWhiteSpace(info.OfficialName), $"{name}.OfficialName"),
                               Valid(!string.IsNullOrEmpty(info.MainUrl), $"{name}.MainUrl"),
                               Valid(!string.IsNullOrEmpty(info.Email), $"{name}.Email"),
                               Validate(info.PostalAddress));
        }

        public bool Validate(ContactPerson person)
        {
            return person != null && !string.IsNullOrWhiteSpace(person.Email);
        }

        public bool ValidateObjectDescriptions(Notice notice)
        {
            var expectedNumberOfDescriptions = _notice.LotsInfo.DivisionLots ? _notice.LotsInfo.QuantityOfLots : 1;

            return ValidateAll(
                Valid(_notice.ObjectDescriptions != null, "ObjectDescriptions"),
                Valid(_notice.ObjectDescriptions?.Length == expectedNumberOfDescriptions, "Object description count does not match"),
                   _notice.ObjectDescriptions?.All(Validate) ?? false);
        }

        public bool Validate(ObjectDescription objectDescription)
        {
            return ValidateAll(Valid((objectDescription.DescrProcurement).HasAnyContent() || _notice.Type.IsDefence(), "ObjectDescription.DescrProcurement"),
                             Valid(objectDescription.NutsCodes.Any()
                             || _notice.Type.IsDefence()
                             || _notice.Type == NoticeType.DesignContest
                             || _notice.Type == NoticeType.NationalDesignContest
                             || _notice.Type == NoticeType.NationalAgricultureContract, "ObjectDescription.NutsCodes"),
                             Valid(!objectDescription.TimeFrame.CanBeRenewed || (objectDescription.TimeFrame.CanBeRenewed && (objectDescription.TimeFrame?.RenewalDescription).HasAnyContent()), "ObjectDescription.TimeFrame.RenewalDescription"),
                             Valid(!(objectDescription.AwardCriteria.CriterionTypes == AwardCriterionType.DescriptiveCriteria && _notice.Type == NoticeType.ContractAward), "Award criteria cannot be descriptive for contract award."));
        }

        public bool Validate(ProcurementProjectContract project)
        {
            return ValidateAll(Valid(project != null && project.Id != 0, "ProcurementProjectContract"),
                               Valid(project?.ContractType != ContractType.Undefined, "ProcurementProjectContract.ContractType"),
                               Valid(!string.IsNullOrWhiteSpace(project?.Title), "ProcurementProjectContract.Title"),
                               Validate(project?.Organisation));
        }

        public bool Validate(ProcedureInformation info)
        {
            if (_notice.Type != NoticeType.Contract)
            {
                return true;
            }

            var canAccelerateTypes = new[]
                {ProcedureType.ProctypeOpen, ProcedureType.ProctypeRestricted, ProcedureType.ProctypeCompNegotiation};
            return ValidateAll(Valid(info != null, $"ProcedureInformation"),
                Valid(info != null && info.ProcedureType != ProcedureType.Undefined, "ProcedureInformation.ProcedureType"),
                Valid(info != null && (!info.AcceleratedProcedure || canAccelerateTypes.Contains(info?.ProcedureType ?? ProcedureType.Undefined) && (info?.JustificationForAcceleratedProcedure).HasAnyContent()), "ProcedureInformation.AcceleratedProcedure + JustificationForAcceleratedProcedure"),
                Validate(info.FrameworkAgreement, info)
                );
        }

        public bool Validate(FrameworkAgreementInformation frameworkAgreement, ProcedureInformation procedure)
        {
            if (_notice.Type != NoticeType.Contract)
            {
                return true;
            }

            return ValidateAll(
                Valid(frameworkAgreement != null, "FrameworkAgreement null"),
                Valid(!(frameworkAgreement?.IncludesDynamicPurchasingSystem ?? true) ||
                         procedure.ProcedureType == ProcedureType.ProctypeRestricted,
                      "Procedure type must be rescricted when IncludesDynamicPurchasingSystem"),
                 Valid(!(frameworkAgreement?.IncludesDynamicPurchasingSystem ?? true) ||
                         _notice.Project.CentralPurchasing || !frameworkAgreement.DynamicPurchasingSystemInvolvesAdditionalPurchasers,
                      "DynamicPurchasingSystemInvolvesAdditionalPurchasers cannot be selected when Project.CentralPurchasing is not set")


            );
        }

        public bool Valid(bool isValid, string objectName)
        {
            if (!isValid)
            {
                _validationErrors.Add(objectName);
            }

            return isValid;
        }

        public bool Validate(ProcurementObject procurementObject)
        {
            return ValidateAll(Valid(procurementObject != null, "ProcurementObject"),
                               Valid(!string.IsNullOrEmpty(procurementObject?.MainCpvCode?.Code) || _notice.Type == NoticeType.NationalAgricultureContract, "ProcurementObject.MainCpvCode"),
                               Valid(string.IsNullOrEmpty(_notice.PreviousNoticeOjsNumber) || Regex.Match(_notice.PreviousNoticeOjsNumber, @"(19|20)\d{2}\/S (((00)?[1-9])|([0]?[1-9][0-9])|(1[0-9][0-9])|(2[0-5][0-9]))-\d{6}").Success, "PreviousNoticeOjsNumber format incorrect"),
                               Valid(procurementObject?.ShortDescription.HasAnyContent() == true
                                      || _notice.Type.IsDefence()
                                      || _notice.Type == NoticeType.DesignContest
                                      || _notice.Type == NoticeType.NationalDesignContest
                                      || _notice.Type == NoticeType.NationalAgricultureContract, "ProcurementObject.ShortDescription"));
        }

        public bool Validate(OrganisationContract organisation)
        {
            return ValidateAll(Valid(organisation != null, "Organisation"),
                               Valid(organisation?.ContractingAuthorityType != ContractingAuthorityType.Undefined, "Organisation.ContractingAuthorityType"),
                               Valid((organisation?.MainActivity != MainActivity.Undefined || organisation?.ContractingAuthorityType == ContractingAuthorityType.MaintypeFarmer) ||
                                organisation?.MainActivityUtilities != MainActivityUtilities.Undefined, "Organisation.MainActivity"),
                               Valid(organisation?.Id != Guid.Empty, "Organisation.Id"),
                               Validate(organisation?.Information));

        }

        public bool Validate(PostalAddress postalAddress)
        {
            return ValidateAll(Valid(postalAddress != null, "PostalAddress"),
                               Valid(!string.IsNullOrWhiteSpace(postalAddress?.Country), "PostalAddress.Country"),
                               Valid(!string.IsNullOrWhiteSpace(postalAddress?.Town), "PostalAddress.Town"));
        }

        public bool Validate(ContractBodyContactInformation info)
        {
            return ValidateAll(Valid(info != null, "Information"),
                Valid(!string.IsNullOrWhiteSpace(info?.NationalRegistrationNumber), "Information.NationalRegistrationNumber"),
                Valid(info != null && info.NutsCodes.Any(), "Information.NutsCodes"),
                Valid(!string.IsNullOrWhiteSpace(info?.OfficialName), "Information.OfficialName"),
                Valid(!string.IsNullOrWhiteSpace(info?.MainUrl), "ContactInformation.MainUrl"),
                Validate(info?.PostalAddress));
        }

        public bool Validate(LotsInfo lotsInfo)
        {
            return Valid(lotsInfo != null &&
                   (!lotsInfo.DivisionLots || lotsInfo.QuantityOfLots >= 2), "LotsInfo.QuantityOfLots");
        }


        private bool Validate(Modifications modifications)
        {
            if (_notice.Type != NoticeType.Modification)
            {
                return true;
            }

            return ValidateAll(Valid(modifications.Description != null, "Modifications.Description"),
                    Valid(modifications.Reason != ModificationReason.Undefined, "Modifications.Reason"));
        }
        #endregion
    }
}