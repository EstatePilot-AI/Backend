using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Controllers;

[Route("api/[controller]")]
[ApiController]
public class GetConversationData : ControllerBase
{
	private readonly IHttpClientFactory _httpClientFactory;

	public GetConversationData(IHttpClientFactory httpClientFactory)
	{
		_httpClientFactory = httpClientFactory;
	}

	[Authorize(Roles = "superadmin")]
	[HttpGet("GetDataById/{id}")]
	public async Task<IActionResult> GetDataById(string id)
	{
		var client= _httpClientFactory.CreateClient();
		var url = $"https://call-handler.vercel.app/api/v1/conv/{id}/data";

		try
		{
			var data = await client.GetFromJsonAsync<object>(url);

			if (data == null)
			{
				return NotFound(new
				{
					status = "error",
					error = new
					{
						message = $"No conversation data found for ID {id}. Please verify the ID and try again."
					}
				});
			}

			return Ok(new
			{
				status = "success",
				data = data
			});
		}
		catch(Exception ex)
		{
			return BadRequest(new
			{
				status = "error",
				error = new
				{
					message = "We couldn't retrieve the conversation data right now. Please try again later."
				}
			});
		}
	}

	[Authorize(Roles = "superadmin")]
	[HttpGet("GetAudioById/{id}")]
	public async Task<IActionResult> GetAudioById(string id)
	{
		var client = _httpClientFactory.CreateClient();
		var url = $"https://call-handler.vercel.app/api/v1/conv/{id}/audio";

		try
		{
			var response = await client.GetAsync(url);

			if (response.IsSuccessStatusCode)
			{
				var data = await response.Content.ReadAsByteArrayAsync();

				return File(data, "audio/mpeg");
			}

			return BadRequest(new
			{
				status = "error",
				error = new
				{
					message = "We couldn't retrieve the audio recording right now. Please try again later."
				}
			});
		}
		catch (Exception ex)
		{
			return BadRequest(new
			{
				status = "error",
				error = new
				{
					message = "We couldn't retrieve the audio recording right now. Please try again later."
				}
			});
		}
	}
}
