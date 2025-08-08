using MediatR;

namespace NetAuth.UseCases.Commands;

public record RefreshTokenCommand(string Token) : IRequest<string>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, string>
{
    public Task<string> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Token refresh logic to be implemented
        return Task.FromResult(request.Token);
    }
}
