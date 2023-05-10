using DomainManager.Abstract;
using DomainManager.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace DomainManager.Commands;

public class SetPipelineStateCommandConsumer : IConsumer<SetPipelineStateCommand>, IMediatorConsumer {
    private readonly ApplicationDbContext _db;

    public SetPipelineStateCommandConsumer(ApplicationDbContext db) {
        _db = db;
    }

    public async Task Consume(ConsumeContext<SetPipelineStateCommand> context) {
        var entity = await _db.PipelineStates.FirstOrDefaultAsync(context.CancellationToken);
        if (entity is not null) {
            _db.PipelineStates.Remove(entity);
        }
        await _db.PipelineStates.AddAsync(new PipelineState {
            Command = context.Message.Command.ToString()
        }, context.CancellationToken);

        await _db.SaveChangesAsync(context.CancellationToken);
    }
}