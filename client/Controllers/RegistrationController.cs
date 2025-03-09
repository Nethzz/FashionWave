using Microsoft.AspNetCore.Mvc;
using PagedList.Core;
using RegClient.Models;
using X.PagedList;

namespace RegClient.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly APIGateway gateway;
        public RegistrationController(APIGateway gateway)
        {
            this.gateway = gateway;
        }
        public IActionResult List()
        {
            List<Registration_Modify> userdetails;
            userdetails = gateway.ListUser();

            return View(userdetails);
        }
        [HttpPost]
        public IActionResult Edit(Registration_Modify registrationModify)
        {
            if (registrationModify != null)
            {
                gateway.UpdateStatus(registrationModify);
                TempData["warning"] = "Data Updated";
            }
            else
            {
                TempData["error"] = "Update failed. Please try again.";
            }

            return RedirectToAction("List");
        }



        public IActionResult Index(string searchTerm = "")
        {
            // Fetch all products from the gateway
            List<Addproducts> allProducts = gateway.ListCustomers();

            // Ensure the list is not null
            if (allProducts == null)
            {
                allProducts = new List<Addproducts>();
            }

            // Filter products based on the search term (if provided)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                allProducts = allProducts
                    .Where(p => p.ProductName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Set session values in ViewBag for display
            var name = HttpContext.Session.GetString("Name");
            var img = HttpContext.Session.GetString("img");
            ViewBag.Name = name;
            ViewBag.img = img;
            ViewBag.SearchTerm = searchTerm;

            // Return the products list to the view
            return View(allProducts);
        }








        [HttpGet]
        public IActionResult Create()
        {
            Registration registration = new Registration();
            return View();
        }


        [HttpPost]
        public IActionResult Create(Registration registration, IFormFile profilePicture)
        {
            string errorMessage;
            var result = gateway.Create(registration, profilePicture, out errorMessage); // Pass the profile picture

            if (result == null) // If API call fails, handle the error
            {
                if (errorMessage.Contains("UsernameError"))
                {
                    ViewData["UserNameError"] = "Username already exists.";
                }
                else if (errorMessage.Contains("PasswordError"))
                {
                    ViewData["PasswordError"] = "Password already exists.";
                }
                else
                {
                    ViewData["GeneralError"] = errorMessage;
                }

                return View(registration);
            }

            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult DemoLogin()
        {
            var userName = HttpContext.Session.GetString("UserName");
            var email = HttpContext.Session.GetString("Email");
            var img = HttpContext.Session.GetString("ImagePath");

            // Pass the user details to the view
            ViewBag.UserName = userName;
            ViewBag.Email = email;
            ViewBag.ImagePath = img;

            return View();
        }
        [HttpPost]
        public IActionResult DemoLogin(string username, string password)
        {
            try
            {
                var demo = gateway.Demo(username, password);
                HttpContext.Session.SetString("UserName", demo.UserName);
                HttpContext.Session.SetString("Email", demo.Email);
                HttpContext.Session.SetString("ImagePath", demo.ImagePath);
            }
            catch
            {

            }
            return RedirectToAction("DemoView");
        }
        [HttpGet]
        public IActionResult DemoView()
        {
            var userName = HttpContext.Session.GetString("UserName");
            var email = HttpContext.Session.GetString("Email");
            var img = HttpContext.Session.GetString("ImagePath");

            // Pass the user details to the view
            ViewBag.UserName = userName;
            ViewBag.Email = email;
            ViewBag.ImagePath = img;
            return View();
        }



        [HttpGet]
        public IActionResult Login()
        {
            var userName = HttpContext.Session.GetString("UserName");
            var email = HttpContext.Session.GetString("Email");
            var pss = HttpContext.Session.GetString("Password");
            var img = HttpContext.Session.GetString("ProfilePicture");

            var age = HttpContext.Session.GetString("Age");
            var address = HttpContext.Session.GetString("Address");

            var ID = HttpContext.Session.GetString("ID");
            // Pass the user details to the view
            ViewBag.ID = ID;
            ViewBag.UserName = userName;
            ViewBag.Email = email;
            ViewBag.Password = pss;
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            try
            {
                var registration = gateway.Login(username, password);

                if (registration != null)
                {
                    // Store user details in session
                    HttpContext.Session.SetString("UserName", registration.UserName);
                    HttpContext.Session.SetString("Email", registration.Email);
                    HttpContext.Session.SetString("Name", registration.Name);
                    HttpContext.Session.SetString("Age",Convert.ToString( registration.Age));
                    HttpContext.Session.SetString("Address",registration.Address);
                    if (!string.IsNullOrEmpty(registration.ImagePath))
                    {
                        HttpContext.Session.SetString("img", registration.ImagePath);
                    }
                    else
                    {
                        // Optionally, handle the case where the image path is null
                        HttpContext.Session.SetString("img", "default-image.jpg"); // Example fallback
                    }

                    HttpContext.Session.SetString("ID", registration.Id.ToString());

                    // Redirect to the home page after successful login
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.ErrorMessage = "Invalid username or password.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error: " + ex.Message;
            }

            return View(); // Return to the login view if there's an error
        }

        public IActionResult Logout()
        {
            // Clear the session
            HttpContext.Session.Clear();

            // Redirect to the login page
            return RedirectToAction("Index","Landing");
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Handle form submission for registration
        [HttpPost]
        public async Task<IActionResult> Register(Registration_Modify registration, IFormFile profilePicture)
        {
            // Call the Create method in the API Gateway
            var (result, errorMessage) = await gateway.Creates(registration, profilePicture);

            if (result != null)
            {
                // Registration successful, redirect to login page or any other page
                return RedirectToAction("login", "Registration");
            }
            else
            {
                // Determine the type of error and set appropriate ViewData messages
                if (errorMessage.Contains("UsernameError"))
                {
                    ViewData["UserNameError"] = "Username already exists.";
                }
                else if (errorMessage.Contains("PasswordError"))
                {
                    ViewData["PasswordError"] = "Password already exists.";
                }
                else
                {
                    ViewData["GeneralError"] = errorMessage; // Handle other generic errors
                }

                // Return the registration view with the existing data
                return View(registration);
            }
        }

        [HttpGet]
        public IActionResult Shop_reg()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Shop_reg(Shop_reg shop_Reg)
        {
            if (!ModelState.IsValid)
            {
                // If model state is invalid, re-render the view with validation errors.
                return View(shop_Reg);
            }

            string errorMessage;
            var result = gateway.Create_shop(shop_Reg, out errorMessage);

            if (result != null)
            {
                // Success - redirect to the desired page.
                return RedirectToAction("Login", "Shop");
            }
            else
            {
                // Determine the type of error and set appropriate ViewData messages.
                if (errorMessage.Contains("UsernameError"))
                {
                    ViewData["UserNameError"] = "Username already exists.";
                }
                else if (errorMessage.Contains("PasswordError"))
                {
                    ViewData["PasswordError"] = "Password already exists.";
                }
                else
                {
                    ViewData["GeneralError"] = errorMessage; // Handle other generic errors.
                }

                // Return the registration view with the existing data and error messages.
                return View(shop_Reg);
            }
        }

        [HttpGet]
        public IActionResult ProductList()
        {
            return View();
        }
        [HttpPost]
        public IActionResult ProductList(Addproducts addproducts)
        {
            return View();
        }
        [HttpGet]
        public IActionResult updateprofile()
        {
            var email = HttpContext.Session.GetString("Email");

            var name = HttpContext.Session.GetString("Name");
            var age = HttpContext.Session.GetString("Age");
            var address = HttpContext.Session.GetString("Address");
            var img = HttpContext.Session.GetString("img");
            ViewBag.Name = name;
            ViewBag.img = img;
            ViewBag.address = address;
            ViewBag.age = age;
            ViewBag.Email = email;
            return View();
        }

    }

}

