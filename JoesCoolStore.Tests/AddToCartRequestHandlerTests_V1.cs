using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using JoesCoolStore.ConsoleApp;
using Moq;
using NUnit.Framework;

namespace JoesCoolStore.Tests.V1
{
    /// <summary>
    /// Updated version of our tests using Moq to avoid writing mock implementations ourselves
    /// </summary>
    [TestFixture]
    public class AddToCartRequestHandlerTests
    {
        [Test]
        public async Task Handle_Success()
        {
            // arrange

            // TODO is 3 really an important value or do we just want any integer?
            var skuId = 3;

            // make a mock just for this test's specific needs!
            var successCommandHandler = new Mock<ICommandHandler<AddToCartCommand>>();
            successCommandHandler
                .Setup(handler => handler.Handle(It.IsAny<AddToCartCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);

            var requestHandler = new AddToCartRequestHandler(successCommandHandler.Object);

            // act
            var result = await requestHandler.Handle(new AddToCartRequest(skuId.ToString()), CancellationToken.None);
            // TODO what if we added another constructor parameter to AddToCartRequestHandler or AddToCartRequest? we could have way more tests than these and we'd have to fix the constructors in all these tests

            // assert
            Assert.IsTrue(result.IsSuccess, "Call should succeed with a valid integer");
            Assert.AreEqual(result.Value, skuId, "Call result should be passed in value as integer.");

            // we can also do this: make sure our mock handler was called with the correct value
            successCommandHandler.Verify(h => h.Handle(It.Is<AddToCartCommand>(c => c.SkuId == skuId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_BadInteger()
        {
            // arrange

            // make a mock where we don't care about functionality. we are testing other unrelated behavior and just want to avoid null references.
            var mockCommandHandler = new Mock<ICommandHandler<AddToCartCommand>>();
            var requestHandler = new AddToCartRequestHandler(mockCommandHandler.Object);

            // act
            var result = await requestHandler.Handle(new AddToCartRequest("Joe"), CancellationToken.None);

            // assert
            Assert.IsTrue(result.IsFailure, "Call should fail because 'Joe' can't be parsed as an integer.");

            // make sure we don't call our command handler when the call should fail before then
            mockCommandHandler.Verify(handler => handler.Handle(It.IsAny<AddToCartCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_AddCommandFails()
        {
            // arrange 
            var commandHandlerError = "Command handler failed";

            // make a mock just for this test's specific needs!
            var failureCommandHandler
                = new Mock<ICommandHandler<AddToCartCommand>>();
            failureCommandHandler
                .Setup(handler => handler.Handle(It.IsAny<AddToCartCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure(commandHandlerError));
            var requestHandler = new AddToCartRequestHandler(failureCommandHandler.Object);

            // act
            var result = await requestHandler.Handle(new AddToCartRequest(3.ToString()), CancellationToken.None);

            // assert
            Assert.IsTrue(result.IsFailure, "Call should fail because our mock command handler fails.");
            Assert.AreEqual(commandHandlerError, result.Error,
                "Request handler should just pass the command handler's error message through when it fails.");
        }
    }
}