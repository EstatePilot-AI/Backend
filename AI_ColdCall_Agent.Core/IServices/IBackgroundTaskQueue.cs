using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;

namespace IServices;

public interface IBackgroundTaskQueue
{
	ValueTask QueueCallAsync(int entityId);
	ValueTask<int> DequeueAsync(CancellationToken cancellationToken);
}