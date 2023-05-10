using DomainManager.Abstract;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace DomainManager.Requests;

public class GetPipelineStateRequestConsumer : IConsumer<GetPipelineStateRequest>, IMediatorConsumer {
    private readonly ApplicationDbContext _db;

    public GetPipelineStateRequestConsumer(ApplicationDbContext db) {
        _db = db;
    }

    public async Task Consume(ConsumeContext<GetPipelineStateRequest> context) {
        var cancellationToken = context.CancellationToken;
        var entity = await _db.PipelineStates.FirstOrDefaultAsync(cancellationToken);

        if (entity is not null
            && Enum.TryParse(entity.Command, out Command command)) {
            await context.RespondAsync<GetPipelineStateResponse>(new { Command = command });
            return;
        }

        await context.RespondAsync<NoPipelineStateResponse>(new { });
    }
}