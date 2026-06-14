using matdev.Application.DTOs;
using matdev.Application.Interfaces;
using matdev.Application.Services;
using matdev.Domain.Entities;
using matdev.Domain.Interfaces;
using Moq;

namespace matdev.UnitTests.Application
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockRepository;
        private readonly IProductService _productService;

        public ProductServiceTests()
        {
            _mockRepository = new Mock<IProductRepository>();
            _productService = new ProductService(_mockRepository.Object);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingProduct_ShouldReturnProduct()
        {
            // Arrange
            var product = new Product("Test", "Description", 19.99m);
            _mockRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(product);

            // Act
            var result = await _productService.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(product.Name, result.Name);
            Assert.Equal(product.Price, result.Price);
        }

        [Fact]
        public async Task CreateAsync_ValidProduct_ShouldSucceed()
        {
            // Arrange
            var createDto = new CreateProductDto("Test", "Description", 19.99m);
            _mockRepository.Setup(repo => repo.AddAsync(It.IsAny<Product>()))
                .ReturnsAsync((Product p) => p);

            // Act
            var result = await _productService.CreateAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createDto.Name, result.Name);
            Assert.Equal(createDto.Price, result.Price);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllProductsAsDtos()
        {
            var p1 = new Product("A", "d1", 1m) { Id = 1 };
            var p2 = new Product("B", "d2", 2m) { Id = 2 };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { p1, p2 });

            var result = (await _productService.GetAllAsync()).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal("A", result[0].Name);
            Assert.Equal(2m, result[1].Price);
        }

        [Fact]
        public async Task UpdateAsync_CallsRepositoryWithUpdatedEntity()
        {
            var product = new Product("Old", "desc", 5m) { Id = 3 };
            _mockRepository.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(product);

            await _productService.UpdateAsync(3, new UpdateProductDto("New", "nd", 10m));

            Assert.Equal("New", product.Name);
            Assert.Equal(10m, product.Price);
            _mockRepository.Verify(r => r.UpdateAsync(product), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_CallsRepositoryDeleteById()
        {
            _mockRepository.Setup(r => r.DeleteAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

            await _productService.DeleteAsync(9);

            _mockRepository.Verify(r => r.DeleteAsync(9), Times.Once);
        }
    }
}
