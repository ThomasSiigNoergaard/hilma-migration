using System;
using Hilma.Domain.Attributes;

namespace Hilma.Domain.Enums
{
    [EnumContract]
    [Flags]
    public enum ContractType : int
    {
        Undefined = 0,
        Supplies = 1 << 0,
        Services = 1 << 1,
        Works = 1 << 2,
        SocialServices = 1 << 3,
        EducationalServices = 1 << 4
    }
}
