using AI_ColdCall_Agent.Core.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services;

[Authorize]
public class DashboardHub:Hub
{
	public override async Task OnConnectedAsync()
	{
		// Only authenticated users join the dashboard group
		await Groups.AddToGroupAsync(Context.ConnectionId, "dashboard");
		await base.OnConnectedAsync();
	}

	public override async Task OnDisconnectedAsync(Exception? exception)
	{
		await Groups.RemoveFromGroupAsync(Context.ConnectionId, "dashboard");
		await base.OnDisconnectedAsync(exception);
	}
}
