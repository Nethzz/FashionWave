using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RegClient.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;

namespace RegClient
{
    public class APIGateway
    {
        // Define a single base URL
        private string baseUrl = "https://localhost:7165";
        private HttpClient httpClient = new HttpClient();

        // Method to register a new user
        public Registration Create(Registration registration, IFormFile profilePicture, out string errorMessage)
        {
            string createUrl = $"{baseUrl}/create";
            errorMessage = string.Empty;

            if (createUrl.Trim().Substring(0, 5).ToLower() == "http")
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {
                // Create a multipart form data content
                using (var form = new MultipartFormDataContent())
                {
                    // Add registration details to the form
                    form.Add(new StringContent(registration.UserName), "UserName");
                    form.Add(new StringContent(registration.Password), "Password");
                    form.Add(new StringContent(registration.Email), "Email");
                    form.Add(new StringContent(registration.Age.ToString()), "Age");
                    form.Add(new StringContent(registration.Address), "Address");

                    // Add the profile picture to the form
                    if (profilePicture != null)
                    {
                        var fileContent = new StreamContent(profilePicture.OpenReadStream());
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue(profilePicture.ContentType);
                        form.Add(fileContent, "profilePicture", profilePicture.FileName);
                    }

                    // Send registration data as multipart/form-data
                    HttpResponseMessage response = httpClient.PostAsync(createUrl, form).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        var data = JsonConvert.DeserializeObject<Registration>(result);
                        if (data != null)
                            registration = data;
                    }
                    else
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        if (result.Contains("Username already exists"))
                        {
                            errorMessage = "UsernameError: Username already exists.";
                        }
                        else if (result.Contains("Password already exists"))
                        {
                            errorMessage = "PasswordError: Password already exists.";
                        }
                        else
                        {
                            errorMessage = result;
                        }
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Error at API endpoint: " + ex.Message;
                return null;
            }

            return registration;
        }

        public Registration Demo(string username, string password)
        {
            string loginUrl = $"{baseUrl}/demo?username={username}&password={password}";

            if (loginUrl.Trim().Substring(0, 5).ToLower() == "http")
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {
                HttpResponseMessage response = httpClient.GetAsync(loginUrl).Result;
                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    var data = JsonConvert.DeserializeObject<Registration>(result);
                    return data;
                }
                else
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    throw new Exception("Login failed: " + result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error at API endpoint: " + ex.Message);
            }
        }




        // Method to login a user
        //public Registration Login(string username, string password)
        //{
        //    string loginUrl = $"{baseUrl}/login?username={username}&password={password}";

        //    if (loginUrl.Trim().Substring(0, 5).ToLower() == "http")
        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        //    try
        //    {
        //        HttpResponseMessage response = httpClient.GetAsync(loginUrl).Result;
        //        if (response.IsSuccessStatusCode)
        //        {
        //            string result = response.Content.ReadAsStringAsync().Result;
        //            var data = JsonConvert.DeserializeObject<Registration>(result);
        //            return data;
        //        }
        //        else
        //        {
        //            string result = response.Content.ReadAsStringAsync().Result;
        //            throw new Exception("Login failed: " + result);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error at API endpoint: " + ex.Message);
        //    }
        //}
        public async Task<(Registration_Modify registrationModify, string errorMessage)> Creates(Registration_Modify registration_Modify, IFormFile profilePicture)
        {
            string createUrl = $"{baseUrl}/register";
            string errorMessage = string.Empty;

            // Set security protocol if the URL starts with HTTP
            if (createUrl.Trim().StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }

            try
            {
                using (var form = new MultipartFormDataContent())
                {
                    // Add Personal Details
                    form.Add(new StringContent(registration_Modify.Name, Encoding.UTF8), "PersonalDetails.Name");
                    form.Add(new StringContent(registration_Modify.Email, Encoding.UTF8), "PersonalDetails.Email");
                    form.Add(new StringContent(registration_Modify.Age.ToString(), Encoding.UTF8), "PersonalDetails.Age");
                    form.Add(new StringContent(registration_Modify.Address, Encoding.UTF8), "PersonalDetails.Address");

                    // Add User Credentials
                    form.Add(new StringContent(registration_Modify.UserName, Encoding.UTF8), "UserCredentials.UserName");
                    form.Add(new StringContent(registration_Modify.Password, Encoding.UTF8), "UserCredentials.Password");
                    form.Add(new StringContent("User", Encoding.UTF8), "UserCredentials.UserType");
                    form.Add(new StringContent("Active", Encoding.UTF8), "UserCredentials.Status");

                    // Add profile picture if provided
                    if (profilePicture != null && profilePicture.Length > 0)
                    {
                        var fileContent = new StreamContent(profilePicture.OpenReadStream());
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue(profilePicture.ContentType);
                        form.Add(fileContent, "profilePicture", profilePicture.FileName);
                    }

                    // Send the multipart form data
                    HttpResponseMessage response = await httpClient.PostAsync(createUrl, form);

                    if (response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();
                        return (JsonConvert.DeserializeObject<Registration_Modify>(result), errorMessage);
                    }
                    else
                    {
                        // Handle possible error responses
                        string result = await response.Content.ReadAsStringAsync();
                        errorMessage = result.Contains("Username already exists")
                            ? "UsernameError: Username already exists."
                            : result.Contains("Password already exists")
                            ? "PasswordError: Password already exists."
                            : result;

                        return (null, errorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Error at API endpoint: " + ex.Message;
                return (null, errorMessage);
            }
        }






        public Registration_Modify Login(string username, string password)
        {
            string loginUrl = $"{baseUrl}/reglogin?username={username}&password={password}";

            if (loginUrl.Trim().Substring(0, 5).ToLower() == "http")
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {
                HttpResponseMessage response = httpClient.GetAsync(loginUrl).Result;
                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    var data = JsonConvert.DeserializeObject<Registration_Modify>(result);
                    return data;
                }
                else
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    throw new Exception("Login failed: " + result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error at API endpoint: " + ex.Message);
            }
        }
        public List<Registration_Modify> ListUser()
        {
            List<Registration_Modify> userdetails = new List<Registration_Modify>();
            string createUrl = $"{baseUrl}/get";
            if (createUrl.Trim().Substring(0, 5).ToLower() == "https")
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            try
            {
                HttpResponseMessage response = httpClient.GetAsync(createUrl).Result;
                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    var dactol = JsonConvert.DeserializeObject<List<Registration_Modify>>(result);
                    if (dactol != null)
                        userdetails = dactol;
                }
                else
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    throw new Exception("Erroe occure at the API end point,ERROR INFO " + result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erroe occure at the API end point,ERROR INFO " + ex.Message);
            }
            finally
            {

            }
            return userdetails;
        }

        public void UpdateStatus(Registration_Modify registrationModify)
        {
            // Use the appropriate endpoint for updating the user status
            string updateUrl = $"{baseUrl}/updateStatus/{registrationModify.Id}";

            // Check if the URL starts with "http" and set security protocol
            if (updateUrl.Trim().Substring(0, 5).ToLower() == "http")
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }

            // Serialize the status string to JSON
            string json = JsonConvert.SerializeObject(registrationModify.Status); // Assuming Status is a string in Registration_Modify

            try
            {
                // Make the PUT request to the API
                HttpResponseMessage response = httpClient.PutAsync(updateUrl, new StringContent(json, Encoding.UTF8, "application/json")).Result;

                // Check if the response was successful
                if (!response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    throw new Exception("Error occurred at API endpoint. Error info: " + result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred at API endpoint. Error info: " + ex.Message);
            }
        }

        public Shop_reg Create_shop(Shop_reg shop_Reg,out string errorMessage)
        {
            string url = $"{baseUrl}/shopreg";
            errorMessage = string.Empty;
            if(url.Trim().Substring(0, 5).ToLower() != "http")
                ServicePointManager.SecurityProtocol= SecurityProtocolType.Tls12;
            var combinedata = new
            {
                ShopOwners = new
                {
                    Id = shop_Reg.Id,
                    ShopLicens = shop_Reg.ShopLicens,
                    ShopName = shop_Reg.ShopName,
                    ShopType = shop_Reg.ShopType,
                    ShopEmail = shop_Reg.ShopEmail,
                },
                UserCredentials = new
                {
                    Id = shop_Reg.Id,
                    UserName = shop_Reg.UserName,
                    Password = shop_Reg.Password,
                    UserType = "ShopOwner",
                    Status = "Active"
                }
            };

            string json = JsonConvert.SerializeObject(combinedata);
            try
            {
                HttpResponseMessage response = httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json")).Result;
                if(response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    var data = JsonConvert.DeserializeObject<Shop_reg>(result);
                    if (data != null)
                        shop_Reg = data;
                }
                else
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    if (result.Contains("Username already exists"))
                    {
                        errorMessage = "UsernameError: Username already exists.";
                    }
                    else if (result.Contains("Password already exists"))
                    {
                        errorMessage = "PasswordError: Password already exists.";
                    }
                    else
                    {
                        errorMessage = result;
                    }
                    return null;
                }

            }
            catch (Exception ex)
            {
                errorMessage = "Error at API endpoint: " + ex.Message;
                return null;
            }
            return shop_Reg;
        }
        public Shop_reg Loginshop(string username, string password)
        {
            string loginUrl = $"{baseUrl}/shoplogin?username={username}&password={password}";

            if (loginUrl.Trim().Substring(0, 5).ToLower() == "http")
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {
                HttpResponseMessage response = httpClient.GetAsync(loginUrl).Result;

                if (response.IsSuccessStatusCode)
                {
                    // If login is successful, deserialize the response content into Shop_reg
                    string result = response.Content.ReadAsStringAsync().Result;
                    var data = JsonConvert.DeserializeObject<Shop_reg>(result);
                    return data;
                }
                else
                {
                    // If login fails, get the error response message
                    string result = response.Content.ReadAsStringAsync().Result;
                    throw new Exception("Login failed: " + result);
                }
            }
            catch (Exception ex)
            {
                // Catch and throw any exceptions during the API call
                throw new Exception("Error at API endpoint: " + ex.Message);
            }
        }

        public List<Shop_reg> ListShop()
        {
            List<Shop_reg> userdetails = new List<Shop_reg>();
            string createUrl = $"{baseUrl}/shopget";
            if (createUrl.Trim().Substring(0, 5).ToLower() == "https")
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            try
            {
                HttpResponseMessage response = httpClient.GetAsync(createUrl).Result;
                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    var dactol = JsonConvert.DeserializeObject<List<Shop_reg>>(result);
                    if (dactol != null)
                        userdetails = dactol;
                }
                else
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    throw new Exception("Erroe occure at the API end point,ERROR INFO " + result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erroe occure at the API end point,ERROR INFO " + ex.Message);
            }
            finally
            {

            }
            return userdetails;
        }
        public void UpdateStatusshop(Shop_reg shop_reg)
        {
            // Use the appropriate endpoint for updating the user status
            string updateUrl = $"{baseUrl}/updateStatusshop/{shop_reg.Id}";

            // Check if the URL starts with "http" and set security protocol
            if (updateUrl.Trim().Substring(0, 5).ToLower() == "http")
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }

            // Serialize the status string to JSON
            string json = JsonConvert.SerializeObject(shop_reg.Status); // Assuming Status is a string in Registration_Modify

            try
            {
                // Make the PUT request to the API
                HttpResponseMessage response = httpClient.PutAsync(updateUrl, new StringContent(json, Encoding.UTF8, "application/json")).Result;

                // Check if the response was successful
                if (!response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    throw new Exception("Error occurred at API endpoint. Error info: " + result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred at API endpoint. Error info: " + ex.Message);
            }
        }

        public Addproducts Addproducts(Addproducts addproducts, IFormFile productImg, out string errorMessage)
        {
            string createUrl = $"{baseUrl}/addproduct";
            errorMessage = string.Empty;

            if (createUrl.Trim().Substring(0, 5).ToLower() == "http")
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (var formContent = new MultipartFormDataContent())
            {
                // Add the product details
                formContent.Add(new StringContent(addproducts.ProductName), "ProductName");
                formContent.Add(new StringContent(addproducts.Price.ToString()), "Price");
                formContent.Add(new StringContent(addproducts.Quantity.ToString()), "Quantity");
                formContent.Add(new StringContent(addproducts.ProductDescription), "ProductDescription");
                formContent.Add(new StringContent(addproducts.Pid.ToString()), "Pid");
                formContent.Add(new StringContent(addproducts.Payment.ToString()), "Payment"); // Add Amount field (False)

                // Add the image file
                if (productImg != null)
                {
                    var imageContent = new StreamContent(productImg.OpenReadStream());
                    imageContent.Headers.ContentType = new MediaTypeHeaderValue(productImg.ContentType);
                    formContent.Add(imageContent, "productimg", productImg.FileName); // Use "productimg" as the key
                }

                try
                {
                    HttpResponseMessage response = httpClient.PostAsync(createUrl, formContent).Result;

                    // Log the outgoing request
                    Console.WriteLine($"Sending to API: {createUrl}");

                    if (response.IsSuccessStatusCode)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        var data = JsonConvert.DeserializeObject<Addproducts>(result);
                        if (data != null)
                            addproducts = data;
                    }
                    else
                    {
                        // Capture the error response message
                        string result = response.Content.ReadAsStringAsync().Result;
                        errorMessage = $"API Error: {result}";
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = "Error at API endpoint: " + ex.Message;
                    return null;
                }
            }

            return addproducts;
        }

        public List<Addproducts> ListCustomers()
        {
            List<Addproducts> customers = new List<Addproducts>();
            string createUrl = $"{baseUrl}/gets";
            if (createUrl.Trim().Substring(0, 5).ToLower() == "https")
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            try
            {
                HttpResponseMessage response = httpClient.GetAsync(createUrl).Result;
                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    var dactol = JsonConvert.DeserializeObject<List<Addproducts>>(result);
                    if (dactol != null)
                        customers = dactol;
                }
                else
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    throw new Exception("Erroe occure at the API end point,ERROR INFO " + result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erroe occure at the API end point,ERROR INFO " + ex.Message);
            }
            finally
            {

            }
            return customers;
        }
        public List<Addproducts> ListCustomers(string sessionId)
        {
            List<Addproducts> customers = new List<Addproducts>();
            string createUrl = $"{baseUrl}/gets/{sessionId}"; // Make sure to use sessionId here
            if (createUrl.Trim().Substring(0, 5).ToLower() == "https")
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {
                HttpResponseMessage response = httpClient.GetAsync(createUrl).Result;
                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    var dactol = JsonConvert.DeserializeObject<List<Addproducts>>(result);
                    if (dactol != null)
                        customers = dactol;
                }
                else
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    throw new Exception("Error occurred at the API endpoint, ERROR INFO: " + result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred at the API endpoint, ERROR INFO: " + ex.Message);
            }
            finally
            {
                // Clean up if needed
            }
            return customers;
        }
        public Addproducts GetCustomer(Guid id)
        {
            Addproducts customer = new Addproducts();
            string createUrl = $"{baseUrl}/product_details/{id}";
            
            if (createUrl.Trim().Substring(0, 5).ToLower() == ("http"))
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            try
            {
                HttpResponseMessage reponse = httpClient.GetAsync(createUrl).Result;
                if (reponse.IsSuccessStatusCode)
                {
                    string result = reponse.Content.ReadAsStringAsync().Result;
                    var data = JsonConvert.DeserializeObject<Addproducts>(result);
                    if (data != null)
                        customer = data;
                }
                else
                {
                    string result = reponse.Content.ReadAsStringAsync().Result;
                    throw new Exception("Error occuer at API End point,Error info" + result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erroe occuer at API endpoint" + ex.Message);
            }
            finally { }
            return customer;
        }

        public ProductPayMent Payment(ProductPayMent productPayMent, out string errorMessage)
        {
            string url = $"{baseUrl}/addamount";
            errorMessage = string.Empty;

            if (url.Trim().Substring(0, 5).ToLower() != "http")
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }

            var combinedata = new
            {
                Pid = productPayMent.Pid,
                Uid = productPayMent.Cid,
                Cid = productPayMent.Uid,
                Payment = productPayMent.Payment, // Always "True"
                Address= productPayMent.Address,
                Amount =productPayMent.Amount
            };

            string json = JsonConvert.SerializeObject(combinedata);

            try
            {
                HttpResponseMessage response = httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json")).Result;

                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    var data = JsonConvert.DeserializeObject<ProductPayMent>(result);

                    if (data != null)
                        productPayMent = data;
                }
                else
                {
                    errorMessage = "API call failed: " + response.ReasonPhrase;
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Error at API endpoint: " + ex.Message;
                return null;
            }

            return productPayMent;
        }
        public void UpdateProductQuantity(Addproducts addproducts)
        {
            // Use the appropriate endpoint for updating the product quantity
            string updateUrl = $"{baseUrl}/quantity/{addproducts.Id}";

            // Check if the URL starts with "http" and set security protocol
            if (updateUrl.Trim().Substring(0, 5).ToLower() == "http")
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }

            // Serialize the quantity to JSON
            string json = JsonConvert.SerializeObject(addproducts.Quantity);

            try
            {
                // Make the PUT request to the API
                HttpResponseMessage response = httpClient.PutAsync(updateUrl, new StringContent(json, Encoding.UTF8, "application/json")).Result;

                // Check if the response was successful
                if (!response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    throw new Exception("Error occurred at API endpoint. Error info: " + result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred at API endpoint. Error info: " + ex.Message);
            }
        }

        public Addproducts DetailsProduct(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new Exception("The provided ID is invalid.");
            }

            Addproducts addProduct = new Addproducts();
            string createUrl = $"{baseUrl}/ownerviewproduct/{id}";

            Console.WriteLine("Calling URL: " + createUrl);

            if (createUrl.Trim().Substring(0, 5).ToLower() == "http")
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }

            try
            {
                HttpResponseMessage response = httpClient.GetAsync(createUrl).Result;

                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    var data = JsonConvert.DeserializeObject<Addproducts>(result);

                    if (data != null)
                        addProduct = data;
                }
                else
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine("API Error: " + result);
                    throw new Exception("Error occurred at API endpoint. Error info: " + result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred at API endpoint: " + ex.Message);
            }

            return addProduct;
        }


        public Addproducts UpdateProduct(Addproducts addProduct, IFormFile? productImg = null)
        {
            // Ensure the product ID is set
            if (addProduct.Id == Guid.Empty)
            {
                throw new Exception("Invalid product ID.");
            }

            string updateUrl = $"{baseUrl}/updateproduct/{addProduct.Id}";

            if (updateUrl.Trim().Substring(0, 5).ToLower() == "http")
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }

            try
            {
                using (var formContent = new MultipartFormDataContent())
                {
                    // Add individual fields to the form data (instead of serializing the whole object)
                    formContent.Add(new StringContent(addProduct.ProductName ?? string.Empty), nameof(addProduct.ProductName));
                    formContent.Add(new StringContent(addProduct.ProductDescription ?? string.Empty), nameof(addProduct.ProductDescription));
                    formContent.Add(new StringContent(addProduct.Price.ToString()), nameof(addProduct.Price));
                    formContent.Add(new StringContent(addProduct.Quantity.ToString()), nameof(addProduct.Quantity));

                    // If there's an image, add it to the form data
                    if (productImg != null)
                    {
                        var imageContent = new StreamContent(productImg.OpenReadStream());
                        imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(productImg.ContentType);
                        formContent.Add(imageContent, "productimg", productImg.FileName);
                    }

                    // Send the PUT request
                    HttpResponseMessage response = httpClient.PutAsync(updateUrl, formContent).Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        throw new Exception("Error occurred at API endpoint. Error info: " + result);
                    }

                    return addProduct;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred at API endpoint. Error info: " + ex.Message);
            }
        }




        public List<ProductPayMent> Viewall(Guid id)
        {
            List<ProductPayMent> productList = new List<ProductPayMent>();
            string createUrl = $"{baseUrl}/product_buyed_user/{id}";

            if (createUrl.Trim().Substring(0, 5).ToLower() == "http")
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {
                HttpResponseMessage response = httpClient.GetAsync(createUrl).Result;
                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    productList = JsonConvert.DeserializeObject<List<ProductPayMent>>(result);
                }
                else
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    throw new Exception("Error occurred at API End point, Error info: " + result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred at API endpoint: " + ex.Message);
            }

            return productList;
        }


    }
}
