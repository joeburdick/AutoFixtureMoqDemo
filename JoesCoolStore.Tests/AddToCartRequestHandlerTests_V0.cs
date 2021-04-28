using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using JoesCoolStore.ConsoleApp;
using NUnit.Framework;

namespace JoesCoolStore.Tests.V0
{
    [TestFixture]
    public class AddToCartRequestHandlerTests
    {
        /// <summary>
        /// Mock CommandHandler that always succeeds.
        /// </summary>
        private class SuccessMockAddToCartCommandHandler : ICommandHandler<AddToCartCommand>
        {
            public Task<Result> Handle(AddToCartCommand command, CancellationToken cancellationToken) => Task.FromResult(Result.Success());
        }

        /// <summary>
        /// Mock CommandHandler that always fails with the passed-in error message.
        ///
        /// TODO Wow this is getting annoying writing these implementations for any kind of behavior I might want to test.
        /// </summary>
        private class FailMockAddToCartCommandHandler : ICommandHandler<AddToCartCommand>
        {
            private readonly string _errorMessage;

            public FailMockAddToCartCommandHandler(string errorMessage)
            {
                _errorMessage = errorMessage;
            }

            public Task<Result> Handle(AddToCartCommand command, CancellationToken cancellationToken) =>
                Task.FromResult(Result.Failure(_errorMessage));
        }

        [Test]
        public async Task Handle_Success()
        {
            // arrange
            var skuId = 3;
            var requestHandler = new AddToCartRequestHandler(new SuccessMockAddToCartCommandHandler());

            // act
            var result = await requestHandler.Handle(new AddToCartRequest(skuId.ToString()), CancellationToken.None);

            // assert
            Assert.IsTrue(result.IsSuccess, "Call should succeed with a valid integer");
            Assert.AreEqual(result.Value, skuId, "Call result should be passed in value as integer.");
        }

        [Test]
        public async Task Handle_BadInteger()
        {
            // arrange
            var requestHandler = new AddToCartRequestHandler(new SuccessMockAddToCartCommandHandler());

            // act
            var result = await requestHandler.Handle(new AddToCartRequest("Joe"), CancellationToken.None);

            // assert
            Assert.IsTrue(result.IsFailure, "Call should fail because 'Joe' can't be parsed as an integer.");
        }

        [Test]
        public async Task Handle_AddCommandFails()
        {
            // arrange 
            var commandHandlerError = "Command handler failed";
            var requestHandler = new AddToCartRequestHandler(new FailMockAddToCartCommandHandler("Command handler failed"));

            // act
            var result = await requestHandler.Handle(new AddToCartRequest(3.ToString()), CancellationToken.None);

            // assert
            Assert.IsTrue(result.IsFailure, "Call should fail because our mock command handler fails.");
            Assert.AreEqual(commandHandlerError, result.Error, "Request handler should just pass the command handler's error message through when it fails.");
        }
    }
}