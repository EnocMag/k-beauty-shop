using AutoMapper;
using Customers.Domain.Entities;
using Customers.Features.Customers.Common;
using Customers.Features.Customers.Create;
using Customers.Infrastructure.DbContexts;
using Customers.Utils;
using FakeItEasy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Customers.Api.Tests.Features;

public class CreateCustomerCommandHandlerTests
{
    private readonly CustomersDbContext _dbContext;
    private readonly AutoMapper.IMapper _mapper;
    private readonly ILogger<CreateCustomerCommandHandler> _logger;

    public CreateCustomerCommandHandlerTests()
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

        _logger = A.Fake<ILogger<CreateCustomerCommandHandler>>();
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesCustomerAndReturnsSuccess()
    {
        // Arrange
        var handler = new CreateCustomerCommandHandler(_dbContext, _mapper, _logger);
        var command = new CreateCustomerCommand("Jane", "Doe", "jane.doe@example.com", "+1-555-123-4567");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.FirstName.Should().Be("Jane");
        result.Data.LastName.Should().Be("Doe");
        result.Data.Email.Should().Be("jane.doe@example.com");
        result.Data.PhoneNumber.Should().Be("+1-555-123-4567");
        result.Data.Id.Should().NotBeEmpty();

        var savedCustomer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.Email == "jane.doe@example.com");
        savedCustomer.Should().NotBeNull();
        savedCustomer!.FirstName.Should().Be("Jane");
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ReturnsFailureResult()
    {
        // Arrange
        var existingCustomer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = "Existing",
            LastName = "User",
            Email = "duplicate@example.com",
            PhoneNumber = "1234567890",
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Customers.Add(existingCustomer);
        await _dbContext.SaveChangesAsync();

        var handler = new CreateCustomerCommandHandler(_dbContext, _mapper, _logger);
        var command = new CreateCustomerCommand("New", "User", "duplicate@example.com", "9876543210");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be(MsgConstants.CUSTOMER_ALREADY_EXISTS);
    }

    [Fact]
    public async Task Handle_WithValidCommand_NormalizesEmailAndTrimsFields()
    {
        // Arrange
        var handler = new CreateCustomerCommandHandler(_dbContext, _mapper, _logger);
        var command = new CreateCustomerCommand("  Jane  ", "  Doe  ", "  Jane.Doe@EXAMPLE.COM  ", "  +1-555-123-4567  ");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Email.Should().Be("jane.doe@example.com");
        result.Data.FirstName.Should().Be("Jane");
        result.Data.LastName.Should().Be("Doe");
        result.Data.PhoneNumber.Should().Be("+1-555-123-4567");

        var savedCustomer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.Email == "jane.doe@example.com");
        savedCustomer.Should().NotBeNull();
        savedCustomer!.Email.Should().Be("jane.doe@example.com");
    }

    [Fact]
    public async Task Handle_WhenCustomerCreated_UsesAutoMapperToMapResponse()
    {
        // Arrange
        var fakeMapper = A.Fake<AutoMapper.IMapper>();
        var expectedResponse = new CustomerResponse
        {
            Id = Guid.NewGuid(),
            FirstName = "Mocked",
            LastName = "User",
            Email = "mocked@example.com"
        };
        A.CallTo(() => fakeMapper.Map<CustomerResponse>(A<Customer>._)).Returns(expectedResponse);

        var handler = new CreateCustomerCommandHandler(_dbContext, fakeMapper, _logger);
        var command = new CreateCustomerCommand("Mocked", "User", "mocked@example.com", "12345");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(expectedResponse);
        A.CallTo(() => fakeMapper.Map<CustomerResponse>(A<Customer>.That.Matches(c => c.Email == "mocked@example.com")))
            .MustHaveHappenedOnceExactly();
    }
}
