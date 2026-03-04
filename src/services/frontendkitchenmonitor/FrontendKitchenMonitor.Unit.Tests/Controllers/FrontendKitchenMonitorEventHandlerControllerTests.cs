using FastFood.Common;
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
    private readonly Mock<IHubClients> _hubClientsMock;
    private readonly Mock<IClientProxy> _clientProxyMock;

    public FrontendKitchenMonitorEventHandlerControllerTests()
    {
        _hubContextMock = new Mock<IHubContext<KitchenWorkUpdateHub>>();
        _loggerMock = new Mock<ILogger<FrontendKitchenMonitorEventHandlerController>>();
        _hubClientsMock = new Mock<IHubClients>();
        _clientProxyMock = new Mock<IClientProxy>();

        _hubContextMock.Setup(h => h.Clients).Returns(_hubClientsMock.Object);
        _hubClientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(_clientProxyMock.Object);
        _clientProxyMock.Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _controller = new FrontendKitchenMonitorEventHandlerController(_hubContextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task KitchenItemInPreparation_ValidEvent_BroadcastsSignalR()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var evt = new KitchenItemInPreparationEvent { OrderId = orderId, ItemId = itemId };

        // Act
        var result = await _controller.KitchenItemInPreparation(evt);

        // Assert
        Assert.IsType<OkResult>(result);
        _hubClientsMock.Verify(c => c.Group(FrontendKitchenMonitor.Constants.HubGroupKitchenMonitors), Times.Once);
        _clientProxyMock.Verify(p => p.SendCoreAsync("kitchenorderupdated", It.IsAny<object[]>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task KitchenItemInPreparation_HubThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var evt = new KitchenItemInPreparationEvent { OrderId = orderId, ItemId = itemId };

        _clientProxyMock.Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Hub error"));

        // Act
        var result = await _controller.KitchenItemInPreparation(evt);

        // Assert
        var statusResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }
}
