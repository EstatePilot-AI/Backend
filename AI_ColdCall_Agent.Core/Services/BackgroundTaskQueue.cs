using IServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;

namespace Services;

public class BackgroundTaskQueue : IBackgroundTaskQueue
{
	private readonly Channel<int> _queue = Channel.CreateUnbounded<int>();
	public async ValueTask QueueCallAsync(int entityId) => await _queue.Writer.WriteAsync(entityId);
	public async ValueTask<int> DequeueAsync(CancellationToken cancellationToken) => await _queue.Reader.ReadAsync(cancellationToken);
}
