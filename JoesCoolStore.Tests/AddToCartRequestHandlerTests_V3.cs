using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using CSharpFunctionalExtensions;
using JoesCoolStore.ConsoleApp;
using Moq;
using NUnit.Framework;

namespace JoesCoolStore.Tests.V3
{
    /// <summary>
    /// Updated version of our tests using AutoFixture.AutoMoq to combine the powers of AutoFixture and Moq to solve our outstanding issues
    /// </summary>
    [TestFixture]
    public class AddToCartRequestHandlerTests
    {
        [Test]
        public async Task Handle_Success()
        {
            // arrange
            
            var fixture = new Fixture();

            // add AutoMoq customization to AutoFixture so that when AutoFixture sees an interface-typed field/property/constructor param/whatever it needs to satisfy, it will just ask Moq for a mock instance!
            fixture.Customize(new AutoMoqCustomization());
    
            var skuId = fixture.Create<int>();

            // use fixture.Freeze to have the immediately create a value and then save that exact value for later to satisfy future requirements of that type
            // in this case we are freezing a mock ICommandHandler which we can then configure to behave as we needed for the test
            var successCommandHandler = fixture.Freeze<Mock<ICommandHandler<AddToCartCommand>>>();
            successCommandHandler.Setup(handler => handler.Handle(It.IsAny<AddToCartCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);
                
            // now we can use AutoFixture to create our request handler, and we know that since we froze a mock ICommandHandler<AddToCartCommand>, that will be used to satisfy AddToCartRequestHandler's constructor parameter
            var requestHandler = fixture.Create<AddToCartRequestHandler>();

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
            fixture.Customize(new AutoMoqCustomization());

            var mockCommandHandler = fixture.Freeze<Mock<ICommandHandler<AddToCartCommand>>>();
            var requestHandler = fixture.Create<AddToCartRequestHandler>();

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
            fixture.Customize(new AutoMoqCustomization());

            var commandHandlerError = fixture.Create<string>();

            var failureCommandHandler = fixture.Freeze<Mock<ICommandHandler<AddToCartCommand>>>();
            failureCommandHandler
                .Setup(handler => handler.Handle(It.IsAny<AddToCartCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure(commandHandlerError));

            var requestHandler = fixture.Create<AddToCartRequestHandler>();

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