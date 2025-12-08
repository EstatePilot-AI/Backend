using Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseContext;

public class AI_ColdCall_Agent_DbContext:IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
	public AI_ColdCall_Agent_DbContext(DbContextOptions<AI_ColdCall_Agent_DbContext> options):base(options)
	{
	}
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Property>()
    .HasOne(p => p.PropertiesLocation)
    .WithOne(p => p.Property)
    .HasForeignKey<Property>(p => p.LocationId);

        


        base.OnModelCreating(modelBuilder);
    }

    public DbSet<BuyerReference> BuyerReferences { get; set; }
	public DbSet<CallLog> CallLogs { get; set; }
	public DbSet<CallOutcome> CallOutcomes { get; set; }
	public DbSet<CallSessionState> CallSessionStates { get; set; }
	public DbSet<ConfirmationStatus> ConfirmationStatuses { get; set; }
	public DbSet<Contact> Contacts { get; set; }
	public DbSet<ContactStatus> ContactStatuses { get; set; }
	public DbSet<ContactType> ContactTypes { get; set; }
	public DbSet<Deal> Deals { get; set; }
	public DbSet<DealStatus> DealStatuses { get; set; }
	public DbSet<FinishingType> FinishingTypes { get; set; }
	public DbSet<LeadRequest> LeadRequests { get; set; }
	public DbSet<LeadRequestStatus> LeadRequestStatuses { get; set; }
	public DbSet<ListingType> ListingTypes { get; set; }
	public DbSet<MeetingStatus> MeetingStatuses { get; set; }
	public DbSet<Negotiation> Negotiations { get; set; }
	public DbSet<NegotiationStatus> NegotiationStatuses { get; set; }
	public DbSet<PaymentMethod> PaymentMethods { get; set; }
	public DbSet<PropertiesLocation> PropertiesLocations { get; set; }
	public DbSet<Property> Properties { get; set; }
	public DbSet<PropertyStatus> PropertyStatuses { get; set; }
	public DbSet<PropertyType> PropertyTypes { get; set; }
	public DbSet<SubjectTypeCall> SubjectTypeCalls { get; set; }
	public DbSet<UserHistory> UserHistories { get; set; }

}
