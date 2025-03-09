using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegApi.Data;
using RegApi.DTO;
using RegApi.Migrations;
using RegApi.Model;

namespace RegApi.Controllers
{
    // [Route("api/[controller]")]
    [ApiController]
    public class ShopController : ControllerBase
    {
        private readonly ApiDbContext _apiDbContext;
        public ShopController(ApiDbContext apiDbContext)
        {
            _apiDbContext = apiDbContext;
        }
        [HttpGet]
        [Route("/shoplogin")]
        public ActionResult<ShopDetailsDto> Get(string username, string password)
        {
            // Fetch user credentials based on the username
            var userDetails = _apiDbContext.UserCredentials.FirstOrDefault(x => x.UserName == username);

            // Validate if the user exists
            if (userDetails == null)
            {
                return NotFound("User not found");
            }

            // Validate if the password matches
            if (userDetails.Password != password)
            {
                return BadRequest("Invalid Password");
            }

            // Fetch the shop details based on the User ID (assuming there's a foreign key)
            var shopDetails = _apiDbContext.shopOwners.FirstOrDefault(x => x.Id == userDetails.Id);

            if (shopDetails == null)
            {
                return BadRequest("No shop found for this user.");
            }

            // Map user details to a DTO object to send relevant information
            var shopOwnDetails = new ShopDetailsDto
            {
                Id = userDetails.Id,
                UserName = userDetails.UserName,
                UserType = userDetails.UserType,
                Status = userDetails.Status,
                ShopLicens = shopDetails.ShopLicens,
                ShopName = shopDetails.ShopName,
                ShopEmail = shopDetails.ShopEmail,
                ShopType = shopDetails.ShopType,

            };

            // Return OK response with the user details
            return Ok(shopOwnDetails);
        }

        [HttpGet]
        [Route("/shopget")]
        public async Task<ActionResult<List<UserCredentials>>> GetShopOwners()
        {
            // Assuming UserType is the field that distinguishes between Usert and ShopOwner
            var shopOwners = await _apiDbContext.UserCredentials
                                                .Where(u => u.UserType == "ShopOwner")
                                                .ToListAsync();

            return Ok(shopOwners);
        }
        [HttpPut]
        [Route("/updateStatusshop/{id}")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] string status)
        {
            // Find the user by Id
            var user = await _apiDbContext.UserCredentials.FindAsync(id);

            if (user == null)
            {
                return NotFound("User not found");
            }

            // Update the status field
            user.Status = status;
            // Save the changes to the database
            await _apiDbContext.SaveChangesAsync();

            return Ok("Status updated successfully");
        }

        [HttpPost]
        [Route("/addproduct")]
        public async Task<IActionResult> AddProducts([FromForm] AddingProducts addProductsDto, IFormFile productimg)
        {
            // Check if the incoming DTO is null
            if (addProductsDto == null)
            {
                return BadRequest("Invalid product data.");
            }

            // Validate the uploaded image file


            if (productimg != null && productimg.Length > 0)
            {
                // Generate a unique file name to avoid overwriting
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(productimg.FileName)}";
                var filePath = Path.Combine("Product/Images/", fileName); // Define the path to save the image

                // Ensure the directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                // Save the image file to the specified path
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await productimg.CopyToAsync(stream);
                }

                // Store the image path in the DTO object
                addProductsDto.ProductImg = filePath; // Assuming this property exists in AddingProducts
            }
            else
            {
                return BadRequest("No image file provided."); // Return an error if no image is uploaded
            }

            // Map the DTO to the Entity Model
            var newProduct = new AddProducts
            {
                Id = Guid.NewGuid(), // Assign a new GUID
                Pid = addProductsDto.Pid,
                ProductName = addProductsDto.ProductName,
                Price = addProductsDto.Price,
                Quantity = addProductsDto.Quantity,
                ProductDescription = addProductsDto.ProductDescription,
                ProductImg = addProductsDto.ProductImg, // Store the image path in the database
                Payment = addProductsDto.Payment
            };

            // Save the new product in the database
            await _apiDbContext.AddProducts.AddAsync(newProduct);
            await _apiDbContext.SaveChangesAsync();

            return Ok(newProduct); // Return the newly added product
        }

        //get Products In False
        [HttpGet]
        [Route("/gets")]
        public async Task<ActionResult<List<AddProducts>>> getproduct()
        {
            var addproducts = await _apiDbContext.AddProducts
                                               .Where(u => u.Payment == "False")
                                               .ToListAsync();

            return Ok(addproducts);
        }
        [HttpGet]
        [Route("/gets/{sessionid}")]
        public async Task<ActionResult<List<AddProducts>>> GetProduct(string sessionid)
        {
            // Convert sessionid (string) to Guid
            if (!Guid.TryParse(sessionid, out Guid sessionGuid))
            {
                // Return BadRequest if the sessionid is not a valid Guid
                return BadRequest("Invalid session ID format.");
            }

            // Fetch product based on Pid (which is of type Guid)
            var products = await _apiDbContext.AddProducts
                                    .Where(x => x.Pid == sessionGuid)
                                    .ToListAsync();

            return Ok(products);
        }
        [HttpGet]
        [Route("/product_details/{id}")]
        public async Task<IActionResult> GetProductDetails(Guid id)
        {
            var addProduct = await _apiDbContext.AddProducts.FirstOrDefaultAsync(x => x.Id == id);

            if (addProduct == null)
            {
                return NotFound(); // Return 404 if the product is not found
            }

            return Ok(addProduct); // Return 200 OK with the product details
        }
        [HttpPut] // Use HttpPut since you're updating a resource
        [Route("/quantity/{id}")]
        public async Task<IActionResult> UpdateProductQuantity(Guid id, [FromBody] int quantity)
        {
            var addProduct = await _apiDbContext.AddProducts.FirstOrDefaultAsync(x => x.Id == id);

            if (addProduct == null)
            {
                return NotFound(); // Return 404 if the product is not found
            }

            // Subtract the quantity from the current product quantity
            if (addProduct.Quantity >= quantity)
            {
                addProduct.Quantity -= quantity;
            }
            else
            {
                return BadRequest("Insufficient product quantity."); // If quantity to subtract is greater than available
            }

            // Save changes to the database
            await _apiDbContext.SaveChangesAsync();

            return Ok(addProduct); // Return 200 OK with the updated product details
        }

        [HttpPost]
        [Route("/addamount")]
        public async Task<IActionResult> Payment(ProductPayMent productPayMent)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid data.");
            }

            try
            {
                await _apiDbContext.productPayMents.AddAsync(productPayMent);
                await _apiDbContext.SaveChangesAsync();
                return Ok(productPayMent);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
       
        [HttpGet]
        [Route("/product_buyed_user/{id}")]
        public async Task<ActionResult<List<ProductPayMent>>> product_buyed_user(Guid id)
        {
            var product = await _apiDbContext.productPayMents.Where(x => x.Pid == id).ToListAsync();
            return Ok(product);

        }


        [HttpGet]
        [Route("/ownerviewproduct/{id}")]
        public async Task<IActionResult> ProductDetails(Guid id)
        {
            var addProduct = await _apiDbContext.AddProducts.FirstOrDefaultAsync(x => x.Id == id);

            if (addProduct == null)
            {
                return NotFound(); // Return 404 if the product is not found
            }

            return Ok(addProduct); // Return 200 OK with the product details
        }

        [HttpPut]
        [Route("updateproduct/{id}")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromForm] AddProducts updatedProduct, IFormFile? productimg)
        {
            // Retrieve the existing product from the database
            var existingProduct = await _apiDbContext.AddProducts.FirstOrDefaultAsync(x => x.Id == id);

            // Check if the product exists
            if (existingProduct == null)
            {
                return NotFound($"Product with Id = {id} not found.");
            }

            // Update product fields if provided
            if (!string.IsNullOrWhiteSpace(updatedProduct.ProductName))
            {
                existingProduct.ProductName = updatedProduct.ProductName;
            }

            if (updatedProduct.Price > 0)
            {
                existingProduct.Price = updatedProduct.Price;
            }

            if (updatedProduct.Quantity > 0)
            {
                existingProduct.Quantity = updatedProduct.Quantity;
            }

            if (!string.IsNullOrWhiteSpace(updatedProduct.ProductDescription))
            {
                existingProduct.ProductDescription = updatedProduct.ProductDescription;
            }

            // Handle image update only if a new image is provided
            if (productimg != null && productimg.Length > 0)
            {
                // Define the folder structure for images
                var folderPath = Path.Combine("product/images");

                // Delete the old image if it exists
                if (!string.IsNullOrEmpty(existingProduct.ProductImg))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), existingProduct.ProductImg); // Full path for the old image
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath); // Delete the old image from the folder
                    }
                }

                // Generate a new unique file name to avoid overwriting
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(productimg.FileName)}";
                var filePath = Path.Combine(folderPath, fileName); // Full file path for the new image

                // Ensure the directory exists
                Directory.CreateDirectory(folderPath);

                // Save the new image
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await productimg.CopyToAsync(stream); // Save the new image to the server
                }

                // Store the relative path of the new image in the database
                existingProduct.ProductImg = Path.Combine("product/images", fileName); // Update the ProductImg with the relative file path
            }

            // Save changes to the database
            _apiDbContext.AddProducts.Update(existingProduct);
            await _apiDbContext.SaveChangesAsync();

            return Ok(existingProduct); // Return the updated product
        }












    }
}







