using FastFood.Common;
using FrontendKitchenMonitor.Controllers;
using KitchenService.Common.Events;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;

namespace FrontendKitchenMonitor.Unit.Tests.Controllers;

public class FrontendKitchenMonitorEventHandlerControllerTests
{
    [Fact]
    public async Task KitchenItemInPreparation_BroadcastsKitchenOrderUpdated()
    {
        var orderId = Guid.NewGuid();
        var clientProxyMock = new Mock<IClientProxy>();
        clientProxyMock.Setup(proxy => proxy.SendCoreAsync(
                "kitchenorderupdated",
                It.Is<object?[]>(args => args.Length == 1 && (Guid)args[0]! == orderId),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var clientsMock = new Mock<IHubClients>();
        clientsMock.Setup(clients => clients.Group(Constants.HubGroupKitchenMonitors))
            .Returns(clientProxyMock.Object);
        var hubContextMock = new Mock<IHubContext<KitchenWorkUpdateHub>>();
        hubContextMock.Setup(context => context.Clients).Returns(clientsMock.Object);
        var controller = new FrontendKitchenMonitorEventHandlerController(
            hubContextMock.Object,
            Mock.Of<ILogger<FrontendKitchenMonitorEventHandlerController>>());

        var result = await controller.KitchenItemInPreparation(new KitchenItemInPreparationEvent
        {
            OrderId = orderId,
            ItemId = Guid.NewGuid()
        });

        Assert.IsType<Microsoft.AspNetCore.Mvc.OkResult>(result);
        clientProxyMock.Verify(proxy => proxy.SendCoreAsync(
            "kitchenorderupdated",
            It.Is<object?[]>(args => args.Length == 1 && (Guid)args[0]! == orderId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
