using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using CSharpFunctionalExtensions;
using JoesCoolStore.ConsoleApp;
using Moq;
using NUnit.Framework;

namespace JoesCoolStore.Tests.V2
{
    /// <summary>
    /// Updated version of our tests using AutoFixture to create test data for us and let us avoid creating our own test objects
    /// </summary>
    [TestFixture]
    public class AddToCartRequestHandlerTests
    {
        [Test]
        public async Task Handle_Success()
        {
            // arrange

            // make our AutoFixture fixture
            var fixture = new Fixture();

            // let AutoFixture make us some test integer. now it's clear there was nothing special about 3.
            // by default, AutoFixture makes natural 1,2,3,4,5 numbers but we could customize for any other special rules we want (https://blog.ploeh.dk/2010/10/19/Convention-basedCustomizationswithAutoFixture/)
            var skuId = fixture.Create<int>();

            var successCommandHandler = new Mock<ICommandHandler<AddToCartCommand>>();
            successCommandHandler
                .Setup(handler => handler.Handle(It.IsAny<AddToCartCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);

            // TODO we still have to call AddToCartRequestHandler's constructor manually because AutoFixture wouldn't know how to satisfy ICommandHandler<AddToCartCommand> constructor parameter

            fixture.Create<AddToCartRequestHandler>();
            var requestHandler = new AddToCartRequestHandler(successCommandHandler.Object);

            // use AutoFixture builder pattern to make an AddToCartRequest object and setup properties we care about without calling the constructor ourselves
            // if AddToCartRequest got another constructor parameter, the test might not need to be changed at all
            var addToCartRequest = fixture.Build<AddToCartRequest>().With(request => request.SkuId, skuId.ToString).Create();

            // act
            var result = await requestHandler.Handle(addToCartRequest, CancellationToken.None);

            // assert
            Assert.IsTrue(result.IsSuccess, "Call should succeed with a valid integer");
            Assert.AreEqual(result.Value, skuId, "Call result should be passed in value as integer.");
            successCommandHandler.Verify(h => h.Handle(It.Is<AddToCartCommand>(c => c.SkuId == skuId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_BadInteger()
        {
            // arrange
            var fixture = new Fixture();

            var mockCommandHandler = new Mock<ICommandHandler<AddToCartCommand>>();
            var requestHandler = new AddToCartRequestHandler(mockCommandHandler.Object);

            // let AutoFixture create our AddToCartRequest
            var addToCartRequest = fixture.Build<AddToCartRequest>().With(request => request.SkuId, "Joe").Create();

            // act
            var result = await requestHandler.Handle(addToCartRequest, CancellationToken.None);

            // assert
            Assert.IsTrue(result.IsFailure, "Call should fail because a 'Joe' can't be parsed as an integer.");
            mockCommandHandler.Verify(handler => handler.Handle(It.IsAny<AddToCartCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_AddCommandFails()
        {
            // arrange 
            var fixture = new Fixture();
            var commandHandlerError = fixture.Create<string>();

            var failureCommandHandler
                = new Mock<ICommandHandler<AddToCartCommand>>();
            failureCommandHandler
                .Setup(handler => handler.Handle(It.IsAny<AddToCartCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure(commandHandlerError));
            var requestHandler = new AddToCartRequestHandler(failureCommandHandler.Object);

            var addToCartRequest = fixture.Build<AddToCartRequest>().With(request => request.SkuId, fixture.Create<int>().ToString).Create();

            // act
            var result = await requestHandler.Handle(addToCartRequest, CancellationToken.None);

            // assert
            Assert.IsTrue(result.IsFailure, "Call should fail because our mock command handler fails.");
            Assert.AreEqual(commandHandlerError, result.Error,
                "Request handler should just pass the command handler's error message through when it fails.");
        }
    }
}