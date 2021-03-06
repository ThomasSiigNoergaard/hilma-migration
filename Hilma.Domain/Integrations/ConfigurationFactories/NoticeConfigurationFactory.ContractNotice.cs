using Hilma.Domain.Integrations.Configuration;

namespace Hilma.Domain.Integrations.ConfigurationFactories
{
    public partial class NoticeConfigurationFactory
    {
        private static NoticeContractConfiguration ContractNotice => new NoticeContractConfiguration {
            PreviousNoticeOjsNumber = true,
            Project = BasicProjectConfiguration,
            LotsInfo = LotsInfoConfigurationDefault,
            ObjectDescriptions = new ObjectDescriptionConfiguration {
                Title = true,
                LotNumber = true,
                AdditionalCpvCodes = new CpvCodeConfiguration { Code = true, VocCodes = new VocCodeConfiguration { Code = true } },
                NutsCodes = true,
                MainsiteplaceWorksDelivery = true,
                DescrProcurement = true,
                EstimatedValue = new ValueRangeContractConfiguration { Currency = true, Value = true },
                AwardCriteria = new AwardCriteriaConfiguration {
                    CriterionTypes = true,
                    QualityCriteria = new AwardCriterionDefinitionConfiguration { Criterion = true, Weighting = true } ,
                    CostCriteria = new AwardCriterionDefinitionConfiguration { Criterion = true, Weighting = true },
                    PriceCriterion = new AwardCriterionDefinitionConfiguration { Weighting = true }
                },
                TimeFrame = new TimeFrameConfiguration {
                    Type = true,
                    BeginDate = true,
                    EndDate = true,
                    CanBeRenewed = true,
                    Days = true,
                    Months = true,
                    RenewalDescription = true
                },
                CandidateNumberRestrictions = new CandidateNumberRestrictionsConfiguration {
                    EnvisagedNumber = true,
                    EnvisagedMinimumNumber = true,
                    EnvisagedMaximumNumber = true,
                    ObjectiveCriteriaForChoosing = true,
                    Selected = true
                },
                OptionsAndVariants = new OptionsAndVariantsConfiguration {
                    Options = true,
                    OptionsDescription = true,
                    VariantsWillBeAccepted = true
                },
                TendersMustBePresentedAsElectronicCatalogs = true,
                EuFunds = new EuFundsConfiguration {
                    ProcurementRelatedToEuProgram = true,
                    ProjectIdentification = true
                },
                AdditionalInformation = true
            },
            ConditionsInformation = new ConditionsInformationConfiguration {
                ProfessionalSuitabilityRequirements = true,
                EconomicCriteriaToParticipate = true,
                EconomicCriteriaDescription = true,
                EconomicRequiredStandards = true,
                TechnicalCriteriaToParticipate = true,
                TechnicalCriteriaDescription = true,
                TechnicalRequiredStandards = true,
                RestrictedToShelteredProgram = true,
                RestrictedToShelteredWorkshop = true,
                ExecutionOfServiceIsReservedForProfession = true,
                ReferenceToRelevantLawRegulationOrProvision = true,
                ContractPerformanceConditions = true,
                ObligationToIndicateNamesAndProfessionalQualifications = true
            },
            CommunicationInformation = new CommunicationInformationConfiguration {
                ProcurementDocumentsAvailable = true,
                ProcurementDocumentsUrl = true,
                AdditionalInformation = true,
                AdditionalInformationAddress = new ContractBodyContactInformationConfiguration {
                    OfficialName = true,
                    NationalRegistrationNumber = true,
                    Email = true,
                    NutsCodes = true,
                    MainUrl = true,
                    PostalAddress = new PostalAddressConfiguration {
                        Town = true,
                        Country = true,
                        PostalCode = true,
                        StreetAddress = true
                    },
                    TelephoneNumber = true
                },
                ElectronicCommunicationRequiresSpecialTools = true,
                ElectronicCommunicationInfoUrl = true,
                SendTendersOption = true,
                AddressToSendTenders = ContractBodyContactInformationConfigurationDefault,
            },
            ContactPerson = new ContactPersonConfiguration {
                Name = true,
                Email = true,
                Phone = true
            },
            ProcurementObject = new ProcurementObjectConfiguration {
                ShortDescription = true,
                EstimatedValue = new ValueRangeContractConfiguration { Currency = true, Value = true },
                MainCpvCode = new CpvCodeConfiguration { Code = true, VocCodes = new VocCodeConfiguration { Code = true } }
            },
            ProcedureInformation = new ProcedureInformationConfiguration {
                ProcedureType = true,
                AcceleratedProcedure = true,
                JustificationForAcceleratedProcedure = true,
                ElectronicAuctionWillBeUsed = true,
                AdditionalInformationAboutElectronicAuction = true,
                ProcurementGovernedByGPA = true,
                ReductionRecourseToReduceNumberOfSolutions = true,
                ReserveRightToAwardWithoutNegotiations = true,
                FrameworkAgreement = new FrameworkAgreementInformationConfiguration {
                    DynamicPurchasingSystemInvolvesAdditionalPurchasers = true,
                    EnvisagedNumberOfParticipants = true,
                    FrameworkAgreementType = true,
                    IncludesDynamicPurchasingSystem = true,
                    IncludesFrameworkAgreement = true,
                    JustificationForDurationOverFourYears = true
                }
            },
            TenderingInformation = new TenderingInformationConfiguration {
                TendersMustBeValidForMonths = true,
                TendersMustBeValidUntil = true,
                TendersOrRequestsToParticipateDueDateTime = true,
                TendersMustBeValidOption = true, 
                EstimatedDateOfInvitations = true,
                Languages = true,
                TenderOpeningConditions = new TenderOpeningConditionsConfiguration {
                    OpeningDateAndTime = true,
                    Place = true,
                    InformationAboutAuthorisedPersons = true
                }
            },
            ComplementaryInformation = new ComplementaryInformationConfiguration {
                AdditionalInformation = true,
                IsRecurringProcurement = true,
                EstimatedTimingForFurtherNoticePublish = true,
                ElectronicOrderingUsed = true,
                ElectronicInvoicingUsed = true,
                ElectronicPaymentUsed = true
            },
            ProceduresForReview = new ProceduresForReviewInformationConfiguration {
                ReviewBody = ContractBodyContactInformationConfigurationDefault,
                ReviewProcedure = true

            },
            AttachmentInformation = new AttachmentInformationConfiguration {
                Description = true,
                Links = new LinkConfiguration {
                    Description = true,
                    Url = true
                }
            }
        };
    }

}
