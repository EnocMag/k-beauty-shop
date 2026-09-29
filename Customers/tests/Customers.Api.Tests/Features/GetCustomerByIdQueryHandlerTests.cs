using System.Net;
using AutoMapper;
using Customers.Domain.Entities;
using Customers.Features.Customers.Common;
using Customers.Features.Customers.GetById;
using Customers.Infrastructure.DbContexts;
using Customers.Utils;
using FakeItEasy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Customers.Api.Tests.Features;

public class GetCustomerByIdQueryHandlerTests
{
    private readonly CustomersDbContext _dbContext;
    private readonly AutoMapper.IMapper _mapper;
    private readonly ILogger<GetCustomerByIdQueryHandler> _logger;

    public GetCustomerByIdQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<CustomersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new CustomersDbContext(options);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddProfile<CustomerMappingProfile>());
        var provider = services.BuildServiceProvider();
        _mapper = provider.GetRequiredService<AutoMapper.IMapper>();

        _logger = A.Fake<ILogger<GetCustomerByIdQueryHandler>>();
    }

    private async Task<Customer> SeedCustomerAsync(string email = "jane.doe@example.com")
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Doe",
            Email = email,
            PhoneNumber = "+1-555-123-4567",
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();
        return customer;
    }

    [Fact]
    public async Task Handle_WithExistingId_ReturnsSuccessWithCustomer()
    {
        // Arrange
        var customer = await SeedCustomerAsync();
        var handler = new GetCustomerByIdQueryHandler(_dbContext, _mapper, _logger);

        // Act
        var result = await handler.Handle(new GetCustomerByIdQuery(customer.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(customer.Id);
        result.Data.FirstName.Should().Be("Jane");
        result.Data.LastName.Should().Be("Doe");
        result.Data.Email.Should().Be("jane.doe@example.com");
        result.Data.PhoneNumber.Should().Be("+1-555-123-4567");
    }

    [Fact]
    public async Task Handle_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var handler = new GetCustomerByIdQueryHandler(_dbContext, _mapper, _logger);

        // Act
        var result = await handler.Handle(new GetCustomerByIdQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.State.Should().Be(HttpStatusCode.NotFound);
        result.Message.Should().Be(MsgConstants.CUSTOMER_NOT_FOUND);
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithMultipleCustomers_ReturnsOnlyRequestedCustomer()
    {
        // Arrange
        await SeedCustomerAsync("first@example.com");
        var target = await SeedCustomerAsync("second@example.com");
        var handler = new GetCustomerByIdQueryHandler(_dbContext, _mapper, _logger);

        // Act
        var result = await handler.Handle(new GetCustomerByIdQuery(target.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Id.Should().Be(target.Id);
        result.Data.Email.Should().Be("second@example.com");
    }

    [Fact]
    public async Task Handle_WhenCustomerFound_UsesAutoMapperToMapResponse()
    {
        // Arrange
        var customer = await SeedCustomerAsync();
        var fakeMapper = A.Fake<AutoMapper.IMapper>();
        var expectedResponse = new CustomerResponse
        {
            Id = customer.Id,
            FirstName = "Mocked",
            LastName = "User",
            Email = "mocked@example.com",
            PhoneNumber = "12345",
            Status = CustomerStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        A.CallTo(() => fakeMapper.Map<CustomerResponse>(A<Customer>._)).Returns(expectedResponse);
        var handler = new GetCustomerByIdQueryHandler(_dbContext, fakeMapper, _logger);

        // Act
        var result = await handler.Handle(new GetCustomerByIdQuery(customer.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(expectedResponse);
        A.CallTo(() => fakeMapper.Map<CustomerResponse>(A<Customer>.That.Matches(c => c.Id == customer.Id)))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenCustomerNotFound_DoesNotInvokeMapper()
    {
        // Arrange
        var fakeMapper = A.Fake<AutoMapper.IMapper>();
        var handler = new GetCustomerByIdQueryHandler(_dbContext, fakeMapper, _logger);

        // Act
        await handler.Handle(new GetCustomerByIdQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        A.CallTo(fakeMapper).MustNotHaveHappened();
    }
}
