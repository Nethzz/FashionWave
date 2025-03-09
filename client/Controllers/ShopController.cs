using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RegClient.Models;
using System.Text;

namespace RegClient.Controllers
{
    public class ShopController : Controller
    {

        private readonly APIGateway gateway;
        public ShopController(APIGateway gateway)
        {
            this.gateway = gateway;
        }
        public IActionResult List()
        {
            List<Shop_reg> userdetails;
            userdetails = gateway.ListShop();

            return View(userdetails);
        }
        [HttpPost]
        public IActionResult Edit(Shop_reg shop_reg)
        {
            if (shop_reg != null)
            {
                gateway.UpdateStatusshop(shop_reg);
                TempData["warning"] = "Data Updated";
            }
            else
            {
                TempData["error"] = "Update failed. Please try again.";
            }

            return RedirectToAction("List");
        }
        // Action for the Index view
        public IActionResult Index()
        {
            // Session value getting from login time
            var userName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = userName;

            var session = HttpContext.Session.GetString("ShopOwnerId");
            ViewBag.Session = session;
            

            // Retrieve products for the session ID
            List<Addproducts> products = gateway.ListCustomers(session); // Assuming this method takes session ID
            return View(products); // Pass the list of products to the view
        }


        // Get request for another Index view
        [HttpGet]
        public IActionResult Indexs()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Logout()
        {
           
            
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Landing");
        }
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            try
            {
                // Call the API to login the shop
                var registration = gateway.Loginshop(username, password);

                if (registration != null)
                {
                    // Check if the user is active
                    if (registration.Status == "Active")
                    {
                        // Store user details in the session
                        HttpContext.Session.SetString("UserName", registration.UserName);
                        HttpContext.Session.SetString("ShopOwnerId", registration.Id.ToString());
                        HttpContext.Session.SetString("ShopEmail", registration.ShopEmail);
                        var session = HttpContext.Session.GetString("ShopOwnerId");

                        // Redirect to the home page after successful login
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        // User is not active, show appropriate message
                        TempData["ErrorMessage"] = "Your account is not active. Please contact Admin.";
                    }
                }
                else
                {
                    // Invalid username or password
                    TempData["ErrorMessage"] = "Invalid username or password.";
                }
            }
            catch (Exception ex)
            {
                // Show error message if something goes wrong
                TempData["ErrorMessage"] = "Error: " + ex.Message;
            }

            // If there was an error, return to the login page
            return View();
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            var session = HttpContext.Session.GetString("ShopOwnerId");
            ViewBag.Session = session;
            Console.WriteLine(session);
            Console.WriteLine("hjgtyg");
            return View();
        }

        [HttpPost]
        public IActionResult AddProduct(Addproducts addproducts, IFormFile productImg)
        {
            // Set Amount to False by default
            addproducts.Payment = "False";

            // Log the incoming addproducts model
            Console.WriteLine($"ProductName: {addproducts.ProductName}, Price: {addproducts.Price}, Quantity: {addproducts.Quantity}, Id: {addproducts.Id}");

            // Capture any error message coming from the API Gateway
            string errorMessage;

            // Call the Create method in the API Gateway with the image
            var result = gateway.Addproducts(addproducts, productImg, out errorMessage);

            if (result != null)
            {
                // Registration successful, redirect to the Shop index page
                return RedirectToAction("Index");
            }
            else
            {
                // Return the registration view with existing data and the error message if any
                ViewBag.ErrorMessage = errorMessage;
                return View(addproducts);
            }
        }

        public IActionResult Details(Guid id)
        {
            Addproducts customer = new Addproducts();
            customer = gateway.GetCustomer(id);
            HttpContext.Session.SetString("productid", customer.Id.ToString());
            HttpContext.Session.SetString("customerid", customer.Pid.ToString());
            HttpContext.Session.SetString("Amount",customer.Price.ToString());
            return View(customer);
        }
        [HttpGet]
        public IActionResult Payment()
        {
            var productid = HttpContext.Session.GetString("productid");
            var customerid = HttpContext.Session.GetString("customerid");
            var ID = HttpContext.Session.GetString("ID");
            var Amount = HttpContext.Session.GetString("Amount");
            var quantity = HttpContext.Session.GetString("quantity");

            if (string.IsNullOrEmpty(productid) || string.IsNullOrEmpty(customerid) || string.IsNullOrEmpty(ID))
            {
                return BadRequest("Session values are missing");
            }

            ViewBag.productid = productid;
            ViewBag.customerid = customerid;
            ViewBag.ID = ID;
            ViewBag.Amount = Amount;
            ViewBag.quantity = quantity;
            ViewBag.tot = Convert.ToInt64( Amount) *Convert.ToInt64( quantity);
            return View();
        }

        [HttpPost]
        public IActionResult Payment(ProductPayMent productPayMent)
        {
            string errorMessage;
            productPayMent.Payment = "True"; // Default value

            var result = gateway.Payment(productPayMent, out errorMessage);

            if (result == null)
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                return View(productPayMent); // Return the view with errors
            }

            return RedirectToAction("Index","Registration");
        }
        [HttpPost]
        public IActionResult UpdateQuantity(Addproducts addproducts)
        {
            if (addproducts != null && addproducts.Quantity > 0) // Ensure quantity is valid
            {
                gateway.UpdateProductQuantity(addproducts); // Call the updated method
                TempData["warning"] = "Data Updated";
                HttpContext.Session.SetString("quantity", addproducts.Quantity.ToString());
            }
            else
            {
                TempData["error"] = "Update failed. Please try again.";
            }

            return RedirectToAction("Payment");
        }
        public IActionResult ViewAll(Guid id)
        {
            List<ProductPayMent> products = gateway.Viewall(id); // Ensure this method is available in your gateway
            return View(products);
        }


        public IActionResult DetailsProduct(Guid id)
        {
            Addproducts product = gateway.DetailsProduct(id);
            return View(product);
        }

        [HttpPost]
        public IActionResult UpdateProduct(Addproducts addProduct, IFormFile? productImg)
        {
            if (addProduct.Id == Guid.Empty)
            {
                // Handle the error: the ID is not valid
                ModelState.AddModelError("", "Invalid product ID.");
                return View(addProduct); // Return the view with the current product data
            }

            // Update the product via the gateway (send image if available)
            Addproducts updatedProduct = gateway.UpdateProduct(addProduct, productImg);

            // Redirect to DetailsProduct after successful update
            return RedirectToAction("DetailsProduct", new { id = updatedProduct.Id });
        }

       


    }
}
