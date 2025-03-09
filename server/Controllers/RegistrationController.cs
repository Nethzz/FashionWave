using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegApi.Data;
using RegApi.DTO;
using RegApi.Migrations;
using RegApi.Model;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Numerics;
namespace RegApi.Controllers
{
    //[Route("Registration/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly ApiDbContext _apiDbContext;
        public RegistrationController(ApiDbContext apiDbContext)
        {
            _apiDbContext = apiDbContext;
        }
        //Images
        [HttpPost]
        [Route("/create")]
        public async Task<ActionResult<Registration>> Create([FromForm] Registration registration, IFormFile profilePicture)
        {
            // Check if the profile picture is provided and valid
            if (profilePicture != null && profilePicture.Length > 0)
            {
                var fileName = Path.GetFileName(profilePicture.FileName);
                var filePath = Path.Combine("RegApi/Images/", fileName); // Define the path to save the image

                // Save the image file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePicture.CopyToAsync(stream);
                }

                // Store the image path in the registration object
                registration.ImagePath = filePath;
            }

            // Save registration details in the database
            await _apiDbContext.Regs.AddAsync(registration);
            await _apiDbContext.SaveChangesAsync();

            return Ok(registration);
        }
        //Images login
        [HttpGet]
        [Route("/demo")]
        public ActionResult<Registration> demologin(string username, string password)
        {
            Registration registration = _apiDbContext.Regs.FirstOrDefault(x => x.UserName == username);

            if (registration == null)
            {
                return NotFound("User not found");
            }

            if (registration.Password == password)
            {
                // Return the user details after successful login
                return Ok(registration);
            }
            else
            {
                return BadRequest("Invalid password");
            }
        }


        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is not selected");
            }

            // Set the path where the file will be saved
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");

            // Ensure the folder exists
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Generate a unique file name to avoid overwriting
            var fileName = Path.GetFileNameWithoutExtension(file.FileName);
            var fileExtension = Path.GetExtension(file.FileName);
            var newFileName = $"{fileName}_{Guid.NewGuid()}{fileExtension}";

            // Set the full path where the file will be saved
            var filePath = Path.Combine(folderPath, newFileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { FilePath = filePath, Message = "File uploaded successfully" });
        }








        [HttpPost]
        [Route("/img")]
        public async Task<IActionResult> upload(IFormFile file)
        {
            var filename = Path.GetRandomFileName + DateTime.Now.ToString("yyyymmddhhss") + "_" + file.FileName;
            var extension = Path.GetExtension(filename);
            var filepath = Path.Combine(Directory.GetCurrentDirectory(), "Images");
            //Craete the Directory
            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }
            var completepath = Path.Combine(Directory.GetCurrentDirectory(), "Images", filename);
            using (var stream = new FileStream(completepath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(filename);
        }
        [HttpGet]
        [Route("/login")]
        public ActionResult<Registration> Get(string username, string password)
        {
            Registration registration = _apiDbContext.Regs.FirstOrDefault(x => x.UserName == username);

            if (registration == null)
            {
                return NotFound("User not found");
            }

            if (registration.Password == password)
            {
                // Return the user details after successful login
                return Ok(registration);
            }
            else
            {
                return BadRequest("Invalid password");
            }
        }

        [HttpPost("/register")]
        public async Task<IActionResult> Register([FromForm] RegistrationRequest request, IFormFile profilePicture)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var existingUser = await _apiDbContext.UserCredentials
        .FirstOrDefaultAsync(u => u.UserName == request.UserCredentials.UserName);

            if (existingUser != null)
            {
                return Conflict(new { message = "Username already exists." });
            }
            var existingPassword = await _apiDbContext.UserCredentials
        .FirstOrDefaultAsync(u => u.Password == request.UserCredentials.Password);

            if (existingPassword != null)
            {
                return Conflict(new { message = "Password already exists." });
            }
            if (profilePicture != null && profilePicture.Length > 0)
            {
                var fileName = Path.GetFileName(profilePicture.FileName);
                var filePath = Path.Combine("Persional/Images/", fileName); // Define the path to save the image

                // Save the image file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePicture.CopyToAsync(stream);
                }

                // Store the image path in the registration object
                request.PersonalDetails.Imagepath = filePath;
            }
            // Save personal details
            request.PersonalDetails.Id = Guid.NewGuid(); // Generate a new Id for registration
            await _apiDbContext.PersonalDetails.AddAsync(request.PersonalDetails);
            await _apiDbContext.SaveChangesAsync();

            // Save user credentials
            var userCredentials = request.UserCredentials;
            userCredentials.Id = request.PersonalDetails.Id; // Link to the registration
            await _apiDbContext.UserCredentials.AddAsync(userCredentials);
            await _apiDbContext.SaveChangesAsync();

            return Ok(request);
        }

        [HttpGet]
        [Route("/reglogin")]
        public ActionResult<UserDetailsDto> Gets(string username, string password)
        {
            // Fetch the user credentials based on the username
            var userCredentials = _apiDbContext.UserCredentials
                .FirstOrDefault(x => x.UserName == username);

            if (userCredentials == null)
            {
                return NotFound("User not found");
            }
            if (userCredentials.UserName != username)
            {
                return BadRequest("invalid username");
            }

            // Check if the password matches
            if (userCredentials.Password != password)
            {
                return BadRequest("Invalid password");
            }

            // If password is correct, fetch personal details using the same Id
            var personalDetails = _apiDbContext.PersonalDetails
                .FirstOrDefault(x => x.Id == userCredentials.Id);

            if (personalDetails == null)
            {
                return NotFound("Personal details not found");
            }

            // Create a DTO to return both user credentials and personal details
            var userDetails = new UserDetailsDto
            {
                Id = userCredentials.Id,
                UserName = userCredentials.UserName,
                Email = personalDetails.Email,
                Name = personalDetails.Name,
                Age = personalDetails.Age,
                Address = personalDetails.Address,
                UserType = userCredentials.UserType,
                Status = userCredentials.Status,
                Password = userCredentials.Password,

                Imagepath = personalDetails.Imagepath
            };

            return Ok(userDetails);
        }

        [HttpGet]
        [Route("/get")]
        public async Task<ActionResult<List<UserCredentials>>> GetUser()
        {
            return Ok(await _apiDbContext.UserCredentials.ToListAsync());
        }

        [HttpPut]
        [Route("/updateStatus/{id}")]
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
        [HttpPost("/shopreg")]
        public async Task<IActionResult> ShopReg([FromBody] ShopRegistration shopRegistration)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var existingUser = await _apiDbContext.UserCredentials
        .FirstOrDefaultAsync(u => u.UserName == shopRegistration.UserCredentials.UserName);

            if (existingUser != null)
            {
                return Conflict(new { message = "Username already exists." });
            }
            var existingPassword = await _apiDbContext.UserCredentials
        .FirstOrDefaultAsync(u => u.Password == shopRegistration.UserCredentials.Password);

            if (existingPassword != null)
            {
                return Conflict(new { message = "Password already exists." });
            }
            shopRegistration.ShopOwners.Id = Guid.NewGuid();
            await _apiDbContext.shopOwners.AddAsync(shopRegistration.ShopOwners);
            await _apiDbContext.SaveChangesAsync();

            var userCredentials = shopRegistration.UserCredentials;
            userCredentials.Id = shopRegistration.ShopOwners.Id;
            await _apiDbContext.UserCredentials.AddAsync(userCredentials);
            await _apiDbContext.SaveChangesAsync();
            return Ok(shopRegistration);

        }
        [HttpGet("edit/{id}")]
        public ActionResult<PersonalDetails> Edit(Guid id)
        {
            var details = _apiDbContext.PersonalDetails.FirstOrDefault(u => u.Id == id);
            if (details == null)
            {
                return NotFound();
            }
            return Ok(details);
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> update(Guid id, [FromForm] PersonalDetails personalDetails, IFormFile? imge)
        {
            // Fetch the existing details from the database
            var details = await _apiDbContext.PersonalDetails.FirstOrDefaultAsync(x => x.Id == id);

            if (details == null)
            {
                return NotFound(); // Return 404 if the record does not exist
            }
            if (!string.IsNullOrEmpty(personalDetails.Name))
            {
                details.Name = personalDetails.Name;
            }
            if (!string.IsNullOrEmpty(personalDetails.Email))
            {
                details.Email = personalDetails.Email;
            }
            if (personalDetails.Age>0 && personalDetails.Age<=100)
            {
                details.Age = personalDetails.Age;
            }
            else
            {
                return BadRequest("Please enter an age between 1 and 100.");
            }
           


            if (imge != null && imge.Length > 0)
            {
                // Define the folder structure for images
                var folderPath = Path.Combine("Persional", "Images");

                // Delete the old image if it exists
                if (!string.IsNullOrEmpty(details.Imagepath))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), details.Imagepath); // Full path for the old image
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath); // Delete the old image from the folder
                    }
                }

                // Generate a new unique file name to avoid overwriting
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(imge.FileName)}";
                var filePath = Path.Combine(folderPath, fileName); // Full file path for the new image

                // Ensure the directory exists
                Directory.CreateDirectory(folderPath);

                // Save the new image
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imge.CopyToAsync(stream); // Save the new image to the server
                }

                // Store the relative path of the new image in the database
                details.Imagepath = Path.Combine("Persional", "Images", fileName); // Update Imagepath with the relative file path
            }

            // Save the changes to the database
            _apiDbContext.PersonalDetails.Update(details);
            await _apiDbContext.SaveChangesAsync();

            return Ok(details); // Return the updated details
        }


    }
}
