using AI_ColdCall_Agent.Core.DTO;
using Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AI_ColdCall_Agent.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeadRequestController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;


        public LeadRequestController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        [HttpGet("GetAllLeads")]
        public async Task<IActionResult> GetAllLeads()
        {
      
            var leads = await _unitOfWork.LeadRequests.GetAllWithIncludesAsync(
                l => l.Contact,
                l => l.LeadRequestStatus
            );

            if (leads == null || !leads.Any())
            {
                return Ok(new { });
            }


            var groupedLeads = leads
                .Select(l => new LeadRequestResponseDto
                {
                    RequestId = l.RequestId,
                    BuyerName = l.Contact?.Name ?? "عميل غير مسجل",
                    BuyerPhone = l.Contact?.Phone ?? "بدون رقم هاتف",
                    StatusName = l.LeadRequestStatus?.Name ?? "قيد الانتظار",
                  
                }).ToList();
               

            return Ok(groupedLeads);
        }
        [HttpGet("FilterLeads")]
        public async Task<IActionResult> FilterLeads([FromQuery] int? statusId) 
        {
            
            if (statusId == null || statusId <= 0)
            {
                return BadRequest("يجب إدخال معرف الحالة (ID) بشكل صحيح للفلترة.");
            }

            var leads = await _unitOfWork.LeadRequests.GetAllWithIncludesAsync(
                l => l.Contact,
                l => l.LeadRequestStatus
            );

          
            var filteredResults = leads
                .Where(l => l.LeadRequestStatusId == statusId)
                .Select(l => new LeadRequestResponseDto
                {
                    RequestId = l.RequestId,
                    BuyerName = l.Contact?.Name ?? "عميل غير مسجل",
                    BuyerPhone = l.Contact?.Phone ?? "بدون رقم هاتف",
                    StatusName = l.LeadRequestStatus?.Name ?? "قيد الانتظار"
                })
                .ToList();

            if (!filteredResults.Any())
            {
                return Ok(new List<LeadRequestResponseDto>());
            }

            return Ok(filteredResults);
        }

        [HttpGet("GetStatusList")]
        public async Task<IActionResult> GetStatusList()
        {
            
            var statuses = await _unitOfWork.LeadRequestStatuses.GetAllAsync();

            if (statuses == null || !statuses.Any())
            {
                return Ok(new List<StatusLookUpDto>());
            }

           
            var response = statuses.Select(s => new StatusLookUpDto
            {
                Id = s.Id,
                Name = s.Name
            }).ToList();

            return Ok(response);
        }


    }
}
