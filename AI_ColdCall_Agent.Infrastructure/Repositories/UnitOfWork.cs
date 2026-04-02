using DatabaseContext;
using Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repositories;

public class UnitOfWork :IUnitOfWork
{
	private readonly AI_ColdCall_Agent_DbContext _context;

	public UnitOfWork(AI_ColdCall_Agent_DbContext context)
	{
		_context = context;
		BuyerReferences = new Repository<BuyerReference>(context);
		CallLogs = new Repository<CallLog>(context);
		CallOutcomes = new Repository<CallOutcome>(context);
		CallSessionStates = new Repository<CallSessionState>(context);
		ConfirmationStatuses = new Repository<ConfirmationStatus>(context);
		Contacts = new Repository<Contact>(context);
		ContactStatuses = new Repository<ContactStatus>(context);
		ContactTypes = new Repository<ContactType>(context);
		Deals = new Repository<Deal>(context);
		DealStatuses = new Repository<DealStatus>(context);
		FinishingTypes = new Repository<FinishingType>(context);
		LeadRequests = new Repository<LeadRequest>(context);
		LeadRequestStatuses = new Repository<LeadRequestStatus>(context);
		ListingTypes = new Repository<ListingType>(context);
		MeetingStatuses = new Repository<MeetingStatus>(context);
		Negotiations = new Repository<Negotiation>(context);
		NegotiationStatuses = new Repository<NegotiationStatus>(context);
		PaymentMethods = new Repository<PaymentMethod>(context);
		PropertiesLocations = new Repository<PropertiesLocation>(context);
		Properties = new Repository<Property>(context);
		PropertyStatuses = new Repository<PropertyStatus>(context);
		PropertyTypes = new Repository<PropertyType>(context);
		SubjectTypeCalls = new Repository<SubjectTypeCall>(context);
		UserHistories = new Repository<UserHistory>(context);
		PropertyImages=new Repository<PropertyImages>(context);
	}

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
	public IRepository<PropertyImages> PropertyImages { get; }

	public void Dispose()
	{
		_context.Dispose();
	}

	public int Save()
	{
		return _context.SaveChanges();
	}
}
