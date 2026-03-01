using DTO;
using Identity;
using Interfaces;
using IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using Repositories;
using Services;

namespace Controllers;

[Route("api/[controller]")]
[ApiController]
public class HandleCallFromBuyerController : ControllerBase
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IBackgroundTaskQueue _queue;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly EmailSender _emailSender;

	public HandleCallFromBuyerController(IUnitOfWork unitOfWork, IBackgroundTaskQueue queue, UserManager<ApplicationUser> userManager, EmailSender emailSender)
	{
		_unitOfWork = unitOfWork;
		_queue = queue;
		_userManager = userManager;
		_emailSender = emailSender;
	}

	[HttpPost("HandleCallOutcome")]
	public async Task<IActionResult> HandleCallOutcome([FromBody] AICallResultDto resultDto)
	{
		if (ModelState.IsValid)
		{
			var leadRequest = _unitOfWork.LeadRequests.FindOneItem(lr => lr.RequestId == int.Parse(resultDto.leadID), new string[] { "Contact", "Property", "Property.PropertyType", "Property.FinishingType", "Property.ListingType", "Property.PaymentMethod", "Property.PropertyStatus", "Property.Contact", "Property.PropertiesLocation", "LeadRequestStatus" });

			if (leadRequest == null)
			{
				return NotFound(new
				{
					status="error",
					error = new
					{
						message= $"LeadRequest with ID {resultDto.leadID} not found."
					}
				});
			}

			//To get the retryCount for the current call by counting the previous call logs for the same buyer
			var callLogForSameBuyer = (await _unitOfWork.CallLogs.FindAllAsync(cl => cl.ContactId == leadRequest.BuyerContactId)).OrderByDescending(cl => cl.Timestamp).FirstOrDefault();

			int currentRetryCount = callLogForSameBuyer?.RetryCount ?? 0;

			var buyerContact = await _unitOfWork.Contacts.GetByIdAsync(leadRequest.BuyerContactId);

			var callLog = new CallLog
			{
				ContactId = leadRequest.BuyerContactId,
				ContactName = resultDto.ContactName,
				SubjectTypeId = 1, //buy
				Transcript = resultDto.summary,
				Timestamp = DateTime.UtcNow,
				Duration = (int)Math.Floor(resultDto.Duration),
				RetryCount = resultDto.CallOutcome.ToLower() == "notanswer" ? currentRetryCount+1 : currentRetryCount,
				CallIDFromAI = resultDto.callId
			};

			try
			{
				switch (resultDto.CallOutcome.ToLower())
				{
					case "interested":
						{
							callLog.CallSessionStateId = 1; //Answered
							callLog.CallOutcomeId = 1; // Interested
							leadRequest.LeadRequestStatusId = 4; //Qualified for property

							// Logic to schedule a deal
							await HandleAcceptance(leadRequest);

							buyerContact.ContactStatusId = 2; //Qualified
							break;
						}
					case "notinterested":
						{
							callLog.CallSessionStateId = 1; //Answered
							callLog.CallOutcomeId = 2; // Not Interested
							leadRequest.LeadRequestStatusId = 5; //Not interested
							buyerContact.ContactStatusId = 4; //Not Interested
							break;
						}
					case "noanswer":
					case "busy":
						{
							callLog.CallSessionStateId = 2; //Not Answered
							callLog.CallOutcomeId = 3; // Not Answered

							if (currentRetryCount >= 2) //retry count for 3 times only
							{
								leadRequest.LeadRequestStatusId = 7; //Invalid Number or failed to reach
							}
							else
							{
								leadRequest.LeadRequestStatusId = 6; //Retry pending
							}

							_unitOfWork.Save();

							await _queue.QueueCallAsync(leadRequest.RequestId);
							break;
						}
					case "failed":
						{
							callLog.CallSessionStateId = 2;//Not Answered
							callLog.CallOutcomeId = 4; // Failed
							leadRequest.LeadRequestStatusId = 7; //Invalid Number
							break;
						}
				}
				await _unitOfWork.CallLogs.AddAsync(callLog);
				_unitOfWork.Save();

				//find the next available lead and enqueue it for calling
				//enable the worker to call when The AI is ready to the next call
				var nextLead = (await _unitOfWork.LeadRequests.FindAllAsync(lr => lr.LeadRequestStatusId == 1 || lr.LeadRequestStatusId == 6)).FirstOrDefault();

				if (nextLead != null)
				{
					await _queue.QueueCallAsync(nextLead.RequestId);
				}

				return Ok(new {
					status= "success",
					data= new
					{
						message = "Outcome processed successfully"
					}	
				});
			}
			catch(Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		return BadRequest(ModelState);
	}


	//the logic of making a deal
	private async Task HandleAcceptance(LeadRequest request)
	{
		var agentId = await GetNextAgent();
		var agent = await _userManager.FindByIdAsync(agentId.ToString());

		var propertyLocation = request.Property.PropertiesLocation;
		var deal = new Deal
		{
			PropertyId = request.PropertyId,
			BuyerContactId = request.BuyerContactId,
			SellerContactId = request.Property.SellerContactId,
			AgentId = agentId,
			MeetingDate = DateTime.UtcNow.AddDays(2),
			MeetingLocation = $"{propertyLocation.Governorate}-{propertyLocation.City}-{propertyLocation.District}-{propertyLocation.Street}-Bldg{propertyLocation.BuildingNumber}-FL{propertyLocation.FloorNumber}-Apt{propertyLocation.ApartmentNumber}",
			MeetingStatusId = 1, //scheduled
			BuyerConfirmationStatusId = 1, //pending
			SellerConfirmationStatusId = 1, //pending
			DealStatusId = 1, //In Progress
			FinalSaleAmount = request.Property.Price,
			DealDate = DateTime.UtcNow
		};

		await _unitOfWork.Deals.AddAsync(deal);
		_unitOfWork.Save();

		var subject = $"Meeting Scheduled for Property #{request.PropertyId}";

		var message = HTMLMessages.MeetingInfoEmail(deal.MeetingDate, deal.MeetingLocation, request.Contact.Name, request.Contact.Phone, request.Property.Contact.Name, request.Property.Contact.Phone, agent!.Name, agent.PhoneNumber!);

		_emailSender.SendEmail(subject, request.Contact.Email, message);
		_emailSender.SendEmail(subject, request.Property.Contact.Email, message);
		_emailSender.SendEmail(subject, agent.Email!, message);
	}

	//Get the next agent for assign him into the deal to confirm the deal
	private async Task<Guid> GetNextAgent()
	{
		var agents=await _userManager.GetUsersInRoleAsync("agent");
		var notCompletedDeals = await _unitOfWork.Deals.FindAllAsync(d => d.DealStatusId != 2);

		if (!agents.Any())
		{
			//assign super admin if there isn't any agent existing
			return Guid.Parse("d8361abf-cd65-44d7-863d-dde32397149a");
		}



		var dealsCountPerAgent = notCompletedDeals.GroupBy(d => d.AgentId).Select(g => new { AgentId = g.Key, Count = g.Count() }).ToList();

		var agentWithLeastWork = agents
								.Select(a => new {
												Agent = a,
												WorkCount = dealsCountPerAgent.FirstOrDefault(w => w.AgentId == a.Id)?.Count ?? 0
												})
								.OrderBy(x => x.WorkCount)
								.Select(x => x.Agent.Id)
								.FirstOrDefault(); //ordered by the least number of deals per agent

		return agentWithLeastWork;
	}
}
