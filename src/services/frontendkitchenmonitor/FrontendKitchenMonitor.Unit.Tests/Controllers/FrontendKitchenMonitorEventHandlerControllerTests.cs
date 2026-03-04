using FrontendKitchenMonitor.Controllers;
using KitchenService.Common.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;

namespace FrontendKitchenMonitor.Unit.Tests.Controllers;

public class FrontendKitchenMonitorEventHandlerControllerTests
{
    private readonly Mock<IHubContext<KitchenWorkUpdateHub>> _hubContextMock;
    private readonly Mock<ILogger<FrontendKitchenMonitorEventHandlerController>> _loggerMock;
    private readonly FrontendKitchenMonitorEventHandlerController _controller;

    public FrontendKitchenMonitorEventHandlerControllerTests()
    {
        _hubContextMock = new Mock<IHubContext<KitchenWorkUpdateHub>>();
        _loggerMock = new Mock<ILogger<FrontendKitchenMonitorEventHandlerController>>();

        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        _hubContextMock.Setup(h => h.Clients).Returns(mockClients.Object);
        mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
        mockClientProxy.Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _controller = new FrontendKitchenMonitorEventHandlerController(_hubContextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task KitchenItemInPreparation_ValidEvent_BroadcastsSignalRUpdate()
    {
        // Arrange
        var itemEvent = new KitchenItemInPreparationEvent { OrderId = Guid.NewGuid(), ItemId = Guid.NewGuid() };

        // Act
        var result = await _controller.KitchenItemInPreparation(itemEvent);

        // Assert
        Assert.IsType<OkResult>(result);
        _hubContextMock.Verify(h => h.Clients.Group(FrontendKitchenMonitor.Constants.HubGroupKitchenMonitors), Times.Once);
    }

    [Fact]
    public async Task KitchenItemInPreparation_HubThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var itemEvent = new KitchenItemInPreparationEvent { OrderId = Guid.NewGuid(), ItemId = Guid.NewGuid() };

        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        _hubContextMock.Setup(h => h.Clients).Returns(mockClients.Object);
        mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
        mockClientProxy.Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("SignalR error"));

        // Act
        var result = await _controller.KitchenItemInPreparation(itemEvent);

        // Assert
        var statusResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }
}
