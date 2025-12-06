using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces;

public interface IUnitOfWork
{
	public IRepository<BuyerReference> BuyerReferences { get; }
	public IRepository<CallLog> CallLogs { get; }
	public IRepository<CallOutcome> CallOutcomes { get; }
	public IRepository<CallSessionState> CallSessionStates { get; }
	public IRepository<ConfirmationStatus> ConfirmationStatuses { get; }
	public IRepository<Contact> Contacts { get; }
	public IRepository<ContactStatus> ContactStatuses { get; }
	public IRepository<ContactType> ContactTypes { get; }
	public IRepository<Deal> Deals { get; }
	public IRepository<DealStatus> DealStatuses { get; }
	public IRepository<FinishingType> FinishingTypes { get; }
	public IRepository<LeadRequest> LeadRequests { get; }
	public IRepository<LeadRequestStatus> LeadRequestStatuses { get; }
	public IRepository<ListingType> ListingTypes { get; }
	public IRepository<MeetingStatus> MeetingStatuses { get; }
	public IRepository<Negotiation> Negotiations { get; }
	public IRepository<NegotiationStatus> NegotiationStatuses { get; }
	public IRepository<PaymentMethod> PaymentMethods { get; }
	public IRepository<PropertiesLocation> PropertiesLocations { get; }
	public IRepository<Property> Properties { get; }
	public IRepository<PropertyStatus> PropertyStatuses { get; }
	public IRepository<PropertyType> PropertyTypes { get; }
	public IRepository<SubjectTypeCall> SubjectTypeCalls { get; }
	public IRepository<UserHistory> UserHistories { get; }

	int Save();
}
