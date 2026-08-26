using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Products.Domain.Commands.Categorys;
using Products.Domain.DTOs;
using Products.Domain.Entities;
using Products.Domain.Services.Interfaces;
using Xunit;

namespace Products.Domain.Tests.Commands
{
    public class CreateCategoryCommandHandlerTests
    {
        private readonly ICategoryService _categoryService;
        private readonly CreateCategoryCommandHandler _handler;

        public CreateCategoryCommandHandlerTests()
        {
            _categoryService = A.Fake<ICategoryService>();
            _handler = new CreateCategoryCommandHandler(_categoryService);
        }

        [Fact]
        public async Task Handle_ShouldCallCategoryServiceAndReturnResult()
        {
            // Arrange
            var command = new CreateCategoryCommand
            {
                Name = "Test Category",
                Description = "Description",
                ParentCategoryId = null
            };
            var cancellationToken = CancellationToken.None;

            var expectedResult = Result<Category>.Ok("Success", new Category { Name = "Test Category" });
            
            A.CallTo(() => _categoryService.CreateCategoryAsync(command, cancellationToken))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            Assert.Same(expectedResult, result);
            A.CallTo(() => _categoryService.CreateCategoryAsync(command, cancellationToken))
                .MustHaveHappenedOnceExactly();
        }
    }
}
