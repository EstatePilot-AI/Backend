using AI_ColdCall_Agent.Core.DTO;
using Interfaces;
using Microsoft.AspNetCore.Authorization;
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
                    BuyerName = l.BuyerName ?? "عميل غير مسجل",
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
                    BuyerName = l.BuyerName ?? "عميل غير مسجل",
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

		[Authorize(Roles = "superadmin")]
		[HttpDelete("DeleteAllLeads")]
        public async Task<IActionResult> DeleteAllLeads()
        {
            var leads=await _unitOfWork.LeadRequests.GetAllAsync();

            if (leads == null)
            {
                return NotFound(new
                {
                    status = "error",
                    error = new
                    {
                        message = "No leads found to delete."
                    }
                });
			}

            foreach(var lead in leads)
            {
                _unitOfWork.LeadRequests.Delete(lead);
            }

            _unitOfWork.Save();

            return Ok(new
            {
                status = "success",
                message = "All leads have been deleted successfully."
            });
		}

        [HttpGet("GetLeadRequestById/{id}")]
        public async Task<IActionResult> GetLeadRequestById(int id)
        {
            
            var includes = new string[]
            {
        "Contact",               
        "LeadRequestStatus",    
        "Property",             
        "Property.Contact",      
        "Property.PropertyType", 
        "Property.PropertiesLocation" 
            };

            var lead = await _unitOfWork.LeadRequests.GetFirstOrDefaultWithStringsAsync(
                l => l.RequestId == id,
                includes
            );

            if (lead == null)
            {
                
                return NotFound(new { message = $"Lead request with ID {id} was not found." });
            }

            var response = new LeadRequestDto
            {
                RequestId = lead.RequestId,
                BuyerName = lead.Contact?.Name ?? "غير مسجل",
                BuyerPhone = lead.Contact?.Phone ?? "لا يوجد رقم",

             
                SellerName = lead.Property?.Contact?.Name ?? "غير محدد",
                SellerPhone = lead.Property?.Contact?.Phone ?? "غير محدد",

                PropertyId = lead.Property?.PropertyId ?? 0,
                Price = lead.Property?.Price ?? 0,
                Area = lead.Property?.Area ?? 0,

                Location = lead.Property?.PropertiesLocation?.City ?? "غير محدد",

         
                PropertyType = lead.Property?.PropertyType?.Name ?? "غير محدد",

                StatusName = lead.LeadRequestStatus?.Name ?? "قيد الانتظار"
            };

            return Ok(response);
        }
    }
}
