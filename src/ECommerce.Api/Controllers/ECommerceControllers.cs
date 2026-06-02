using System.Security.Claims;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<RegisterDto> _registerValidator;
    private readonly IValidator<LoginDto> _loginValidator;

    public AuthController(
        IAuthService authService,
        IValidator<RegisterDto> registerValidator,
        IValidator<LoginDto> loginValidator)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto, CancellationToken cancellationToken)
    {
        await _registerValidator.ValidateAndThrowAsync(dto, cancellationToken);
        var result = await _authService.RegisterAsync(dto, cancellationToken);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto, CancellationToken cancellationToken)
    {
        await _loginValidator.ValidateAndThrowAsync(dto, cancellationToken);
        var result = await _authService.LoginAsync(dto, cancellationToken);
        return Ok(result);
    }
}

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IValidator<CreateProductDto> _createValidator;

    public ProductsController(IProductService productService, IValidator<CreateProductDto> createValidator)
    {
        _productService = productService;
        _createValidator = createValidator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await _productService.GetAllAsync(cancellationToken));

    [HttpGet("category/{category}")]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetByCategory(string category, CancellationToken cancellationToken) =>
        Ok(await _productService.GetByCategoryAsync(category, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(CreateProductDto dto, CancellationToken cancellationToken)
    {
        await _createValidator.ValidateAndThrowAsync(dto, cancellationToken);
        var product = await _productService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDto>> Update(Guid id, UpdateProductDto dto, CancellationToken cancellationToken)
    {
        var product = await _productService.UpdateAsync(id, dto, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _productService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService) => _cartService = cartService;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CartItemDto>>> GetCart(CancellationToken cancellationToken) =>
        Ok(await _cartService.GetCartAsync(GetUserId(), cancellationToken));

    [HttpPost]
    public async Task<IActionResult> AddToCart(AddToCartDto dto, CancellationToken cancellationToken)
    {
        await _cartService.AddToCartAsync(GetUserId(), dto, cancellationToken);
        return Ok();
    }

    [HttpPut("{productId:guid}")]
    public async Task<IActionResult> UpdateItem(Guid productId, UpdateCartItemDto dto, CancellationToken cancellationToken)
    {
        await _cartService.UpdateCartItemAsync(GetUserId(), productId, dto, cancellationToken);
        return Ok();
    }

    [HttpDelete("{productId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid productId, CancellationToken cancellationToken)
    {
        await _cartService.RemoveFromCartAsync(GetUserId(), productId, cancellationToken);
        return NoContent();
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException();
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IValidator<CreateOrderDto> _orderValidator;

    public OrdersController(IOrderService orderService, IValidator<CreateOrderDto> orderValidator)
    {
        _orderService = orderService;
        _orderValidator = orderValidator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetMyOrders(CancellationToken cancellationToken) =>
        Ok(await _orderService.GetUserOrdersAsync(GetUserId(), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetByIdAsync(id, GetUserId(), cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<OrderDto>> Checkout(CreateOrderDto dto, CancellationToken cancellationToken)
    {
        await _orderValidator.ValidateAndThrowAsync(dto, cancellationToken);
        var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var order = await _orderService.CheckoutAsync(GetUserId(), email, dto, cancellationToken);
        return Ok(order);
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException();
}

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
}
