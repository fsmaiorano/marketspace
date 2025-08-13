using BuildingBlocks.Exceptions;

namespace Merchant.Api.Domain.Exceptions;

public class DomainException(string message) : BaseDomainException(message);
