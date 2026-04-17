using AI_ColdCall_Agent.Core.DTO;
using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Linq.Expressions;

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

		[Authorize(Roles = "superadmin")]
		[HttpGet("GetAllLeads")]
        public async Task<IActionResult> GetAllLeads([FromQuery] LeadRequestFilterDto filter)
        {
            Expression<Func<Models.LeadRequest, bool>> predicate = l =>
                (!filter.StatusId.HasValue || l.LeadRequestStatusId == filter.StatusId) &&
                (string.IsNullOrWhiteSpace(filter.SearchTerm) ||
                 l.BuyerName.Contains(filter.SearchTerm) ||
                 (l.Contact != null && l.Contact.Phone.Contains(filter.SearchTerm)));

            var includes = new[] { "Contact", "LeadRequestStatus" };

            var (items, totalCount) = await _unitOfWork.LeadRequests.GetPaginatedAsync(
                predicate,
                includes,
                q => q.OrderByDescending(l => l.RequestId),
                filter.PageNumber,
                filter.PageSize);

            var data = items.Select(l => new LeadRequestResponseDto
            {
                RequestId = l.RequestId,
                BuyerName = l.BuyerName ?? "عميل غير مسجل",
                BuyerPhone = l.Contact?.Phone ?? "بدون رقم هاتف",
                StatusName = l.LeadRequestStatus?.Name ?? "قيد الانتظار",
            }).ToList();

            var result = new PaginatedResult<LeadRequestResponseDto>
            {
                Data = data,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
            };

            return Ok(result);
        }

        [HttpGet("FilterLeads")]
        public async Task<IActionResult> FilterLeads([FromQuery] LeadRequestFilterDto filter)
        {
            return await GetAllLeads(filter);
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
                        message = "There are no leads to delete at the moment."
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

		[Authorize(Roles = "superadmin")]
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
                return NotFound(new { message = $"We couldn't find a lead request with ID {id}. It may have been removed or doesn't exist." });
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
