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
						message = $"No data found for ID {id}."
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
					message = $"Failed to retrieve data for ID {id}: {ex.Message}"
				}
			});
		}
	}

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
					message = $"Failed to retrieve audio for ID {id}. Status code: {response.StatusCode}"
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
					message = $"Failed to retrieve audio for ID {id}: {ex.Message}"
				}
			});
		}
	}
}
