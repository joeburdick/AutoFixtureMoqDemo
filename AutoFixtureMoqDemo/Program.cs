using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace JoesCoolStore.ConsoleApp
{
    public interface IRequest<out TResponse> { }

    public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }

    // ********************************************************************************************************

    public interface ICommand { }

    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        Task<Result> Handle(TCommand command, CancellationToken cancellationToken);
    }

    // ********************************************************************************************************

    public record AddToCartCommand(int SkuId) : ICommand;

    public class AddToCartCommandHandler : ICommandHandler<AddToCartCommand>
    {
        public Task<Result> Handle(AddToCartCommand command, CancellationToken cancellationToken) =>
            Task.FromResult(Result.SuccessIf(command.SkuId > 0, "SkuId must be a positive integer."));
    }

    // ********************************************************************************************************

    public record AddToCartRequest(string SkuId) : IRequest<Result<int>>;

    public class AddToCartRequestHandler : IRequestHandler<AddToCartRequest, Result<int>>
    {
        private readonly ICommandHandler<AddToCartCommand> _addToCartCommandHandler;

        public AddToCartRequestHandler(ICommandHandler<AddToCartCommand> addToCartCommandHandler)
        {
            _addToCartCommandHandler = addToCartCommandHandler;
        }

        public async Task<Result<int>> Handle(AddToCartRequest request, CancellationToken cancellationToken)
        {
            if (!int.TryParse(request.SkuId, out var skuId)) return Result.Failure<int>($"{request.SkuId} is not a valid integer.");

            var commandResult = await _addToCartCommandHandler.Handle(new AddToCartCommand(skuId), cancellationToken);
            if (commandResult.IsFailure) return Result.Failure<int>(commandResult.Error);

            return Result.Success(skuId);
        }
    }

    // ********************************************************************************************************

    internal class Program
    {
        private static IRequestHandler<AddToCartRequest, Result<int>> GetAddToCartRequestHandler()
        {
            var commandHandler = new AddToCartCommandHandler();
            return new AddToCartRequestHandler(commandHandler);
        }

        private static async Task Main()
        {
            while (true)
            {
                Console.WriteLine("Enter a SKU to add to cart or 'q' to quit.");
                var input = Console.ReadLine();
                if (input == "q") break;

                var requestHandler = GetAddToCartRequestHandler();

                var handlerResult = await requestHandler.Handle(new AddToCartRequest(input), CancellationToken.None);
                if (handlerResult.IsSuccess) Console.WriteLine($"Successfully added {handlerResult.Value} to cart.");
                else Console.WriteLine(handlerResult.Error);
            }
        }
    }
}