using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Test.BAL.Interfaces;
using Test.BAL.Intrfaces;
using Test.BAL.Services;
using Test.Entity;
using Test.Model;
using Test.Utilities;

namespace Test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterController : ControllerBase
    {
        private readonly IMasterMgmtService service;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment hosting;

        public MasterController(IMasterMgmtService _service, Microsoft.AspNetCore.Hosting.IHostingEnvironment _hosting)
        {
            service = _service;
            hosting = _hosting;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            if (string.IsNullOrEmpty(query))
                return BadRequest("Query cannot be empty");

            var (foodItems, restaurants, menu) = await service.SearchAsync(query);

            return Ok(new
            {
                FoodItems = foodItems,
                Restaurants = restaurants,
                Menu = menu
            });
        }

        [HttpPost("UploadFile")]
        public IActionResult UploadFile(IFormFile file)
        {
            GenricResponse response = new GenricResponse();
            try
            {
                if (file == null || file.Length == 0)
                {
                    response.filename = "dummy.png";
                    response.StatusCode = 0;
                    response.StatusMessage = "File is invalid or empty.";
                    return Ok(new { result = response });
                }

                // Get the file name
                var fileNameUploaded = Path.GetFileName(file.FileName);

                if (!string.IsNullOrEmpty(fileNameUploaded))
                {
                    string filename = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                    filename += "IMG_" + RandomGenerator.RandomString(4, false);
                    string extension = Path.GetExtension(fileNameUploaded);
                    filename += extension;

                    // Define the upload folder path relative to the project root
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");
                    Console.WriteLine($"File saved to: {Path.Combine(uploadsFolder, filename)}");

                    // Check if the folder exists, if not create it
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, filename);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    response.filename = filename;
                    response.StatusCode = 1;
                    response.StatusMessage = "File uploaded successfully.";
                }
                else
                {
                    response.filename = "dummy.png";
                    response.StatusCode = 0;
                    response.StatusMessage = "File name is invalid.";
                }
            }
            catch (Exception ex)
            {
                response.filename = "dummy.png";
                response.StatusCode = 0;
                response.StatusMessage = $"Error uploading file: {ex.Message}";
            }

            return Ok(new { result = response });
        }

        [HttpGet("DownloadFile/{filename}")]
        public IActionResult DownloadFile(string filename)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles", filename);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", filename);
        }

        [HttpGet("GetImages")]
        public IActionResult GetImages()
        {
            var response = new List<object>();
            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");
                if (Directory.Exists(uploadsFolder))
                {
                    var files = Directory.GetFiles(uploadsFolder);
                    foreach (var file in files)
                    {
                        var bytes = System.IO.File.ReadAllBytes(file);
                        var base64String = Convert.ToBase64String(bytes);
                        var fileName = Path.GetFileName(file);

                        response.Add(new
                        {
                            FileName = fileName,
                            Base64Image = $"data:image/{Path.GetExtension(file).TrimStart('.')};base64,{base64String}"
                        });
                    }
                }
                else
                {
                    return NotFound("No images found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving images: {ex.Message}");
            }

            return Ok(response);
        }

        #region Restaurants
        [HttpGet("RestaurantsDD")]
        public async Task<IActionResult> RestaurantsDD(string q = "")
        {
            var res = await service.RestaurantsDD(q);
            return Ok(res);
        }

        [HttpGet("GetAllRestaurantsPagenation")]
        public IActionResult GetAllRestaurantsPagenation(string? q = "", int pageNumber = 1, int pageSize = 5)
        {
            var res = service.GetAllRestaurantsPagenation(q, pageNumber, pageSize);
            return Ok(res);
        }

        [HttpGet("GetAllRestaurants")]
        public async Task<ActionResult<List<RestaurantEntity>>> GetAllRestaurants()
        {
            var res = await service.GetAllRestaurants();
            return Ok(res);
        }

        //[HttpPost("AddOrUpdateRestaurant")]
        //public async Task<IActionResult> AddOrUpdateRestaurant([FromForm] IFormFile file, [FromForm] RestaurantEntity restaurant)
        //{
        //    GenricResponse response = new GenricResponse();
        //    try
        //    {
        //        // Handle file upload
        //        if (file != null && file.Length > 0)
        //        {
        //            // Generate filename
        //            string filename = DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "_IMG_" + RandomGenerator.RandomString(4, false) + Path.GetExtension(file.FileName);

        //            // Define the upload folder path
        //            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");

        //            // Create directory if it doesn't exist
        //            if (!Directory.Exists(uploadsFolder))
        //            {
        //                Directory.CreateDirectory(uploadsFolder);
        //            }

        //            var filePath = Path.Combine(uploadsFolder, filename);

        //            // Save the file to the disk
        //            using (var stream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await file.CopyToAsync(stream);
        //            }

        //            // Update the restaurant object with the file name (this will be saved in DB)
        //            restaurant.Rest_Image = filename;
        //        }
        //        else
        //        {
        //            // In case no file is uploaded, retain the existing image if any
        //            if (string.IsNullOrEmpty(restaurant.Rest_Image))
        //            {
        //                restaurant.Rest_Image = "dummy.png"; // Default image if no file is uploaded
        //            }
        //        }

        //        // Save or update restaurant details in the database
        //        await service.AddOrUpdateRestaurant(restaurant);

        //        response.StatusCode = 1;
        //        response.StatusMessage = "Restaurant added/updated successfully.";
        //    }
        //    catch (Exception ex)
        //    {
        //        response.StatusCode = 0;
        //        response.StatusMessage = $"Error: {ex.Message}";
        //    }

        //    return Ok(new { result = response });
        //}

        [HttpPost("AddOrUpdateRestaurant")]
        public async Task<ActionResult<GenricResponse>> AddOrUpdateRestaurant(RestaurantEntity model)
        {

            if (model == null)
            {
                return BadRequest(new GenricResponse
                {
                    StatusCode = 400,
                    StatusMessage = "Invalid request data.",
                });
            }

            else
            {
                if (model != null)
                {
                    var response = await service.AddOrUpdateRestaurant(model);
                    return StatusCode(response.StatusCode, response);
                }
                return Ok(model);
            }



        }

        [HttpGet("GetFoodItemsByRestaurantId")]
        public IActionResult GetFoodItemsByRestaurantId(int restaurantId)
        {
            try
            {
                if (restaurantId == 0)
                {
                    return BadRequest(new { StatusMessage = "Invalid RestaurantId", StatusCode = 400 });
                }

                else
                {
                    var response = service.GetFoodItemsByRestaurantId(restaurantId);
                    return Ok(new { items_1 = response });
                    //return Ok(new { StatusCode = 200, StatusMessage = "Registration successful" });
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpDelete("DeleteRestaurant")]
        public IActionResult DeleteRestaurant(int id)
        {
            var response = service.DeleteRestaurant(id); // Now returning GenricResponse

            if (response.StatusCode == 200)
            {
                return Ok(response);
            }
            else if (response.StatusCode == 404)
            {
                return NotFound(response);
            }
            else
            {
                return StatusCode(500, response);
            }
        }



        #endregion

        #region Menu
        [HttpGet("MenuItemsDD")]
        public async Task<IActionResult> MenuItemsDD(string? q = "")
        {
            var result = await service.MenuItemsDD(q);
            return Ok(result);
        }

        [HttpPost("AddOrUpdateMenu")]
        public async Task<ActionResult<GenricResponse>> AddOrUpdateMenu(MenuItemEntity model)
        {

            if (model == null)
            {
                return BadRequest(new GenricResponse
                {
                    StatusCode = 400,
                    StatusMessage = "Invalid request data.",
                });
            }

            else
            {
                if (model != null)
                {
                    var response = await service.AddOrUpdateMenu(model);
                    return StatusCode(response.StatusCode, response);
                }
                return Ok(model);
            }



        }

        [HttpGet("GetAllMenuItems")]
        public IActionResult GetAllMenuItems()
        {
            try
            {
                var res = service.GetAllMenuItems(); // Await the asynchronous method
                return Ok(res); // Return the result
            }
            catch (Exception ex)
            {
                // Log the exception (use a logging framework in production)
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("GetAllMenuItemsPagenation")]
        public IActionResult GetAllMenuItemsPagenation(string? q = "", int pageNumber = 1, int pageSize = 10)
        {
            var res = service.GetAllMenuItemsPagenation(q, pageNumber, pageSize);
            return Ok(res);
        }


        [HttpGet("GetMenuById")]
        public IActionResult GetMenuById(int id)
        {
            var res = service.GetMenuById(id);
            return Ok(res);
        }

        [HttpDelete("DeleteMenuItem")]
        public IActionResult DeleteMenuItem(int id)
        {
            var response = service.DeleteMenuItem(id); // Now returning GenricResponse

            if (response.StatusCode == 200)
            {
                return Ok(response);
            }
            else if (response.StatusCode == 404)
            {
                return NotFound(response);
            }
            else
            {
                return StatusCode(500, response);
            }
        }



        [HttpGet("GetFoodItemsByMenuId")]
        public IActionResult GetFoodItemsByMenuId([FromQuery] int menuId)
        {
            //if (menuId == 0)
            //{
            //    return BadRequest(new { StatusMessage = "Invalid menuId", StatusCode = 400 });
            //}

            var response = service.GetFoodItemsByMenuId(menuId);
            return Ok(response);
        }

        #endregion

        #region FoodItems
        [HttpGet("GetAllFoodItemsPagenation")]
        public IActionResult GetAllFoodItemsPagenation(string? q = "", int pageNumber = 1, int pageSize = 10)
        {
            var res = service.GetAllFoodItemsPagenation(q, pageNumber, pageSize);
            return Ok(res);
        }

        [HttpPost("AddOrUpdateFoodItems")]
        public async Task<ActionResult<GenricResponse>> AddOrUpdateFoodItems(FoodItemEntity model)
        {

            if (model == null)
            {
                return BadRequest(new GenricResponse
                {
                    StatusCode = 400,
                    StatusMessage = "Invalid request data.",
                });
            }

            else
            {
                if (model != null)
                {
                    var response = await service.AddOrUpdateFoodItem(model);
                    return StatusCode(response.StatusCode, response);
                }
                return Ok(model);
            }



        }


        [HttpGet("GetAllFoodItems")]
        public IActionResult GetAllFoodItems()
        {
            try
            {
                var res = service.GetAllFoodItems();
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("DeleteFoodItem")]
        public IActionResult DeleteFoodItem(int id)
        {
            GenricResponse resp = new GenricResponse();
            var menu = service.DeleteFoodItem(id);

            if (menu != null)
            {
                resp.StatusCode = 200;
                resp.StatusMessage = "Food Item deleted successfully";
                resp.CurrentId = id;
                return Ok(resp);
            }

            return NotFound(new
            {
                StatusCode = 404,
                StatusMessage = "Food Item not found"
            });
        }

        [HttpGet("MostOrdered")]
        public IActionResult GetMostOrderedFoodAndRestaurant([FromQuery] int? userId = null)
        {
            var result = service.GetMostOrderedFoodAndRestaurant(userId);

            if (result.Item1 == null || result.Item2 == null)
                return NotFound(new { message = "No order data found" });

            return Ok(new
            {
                MostOrderedFood = result.Item1,
                MostVisitedRestaurant = result.Item2
            });
        }

        #endregion

        #region Cart
        [HttpPost]
        [Route("AddandRemoveCart")]
        public async Task<IActionResult> AddandRemoveCart(CartModel cartModel)
        {
            try
            {
                var result = await service.AddandRemoveCart(cartModel);
                return result ? Ok(new { Message = "Item added to cart" }) : BadRequest(new { Message = "Failed to update cart." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Error = ex.Message });
            }
        }

        [HttpGet("OrderedFoodItems")]
        public async Task<IActionResult> OrderedFoodItems(int userId)
        {
            try
            {
                if(userId == 0 || userId == null)
                {
                    return StatusCode(400, new { Message = "User not exists." });
                }
                else
                {
                    var res = await service.OrderedFoodItems(userId);
                    return Ok(res);
                }
                
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Error = ex.Message });
            }
        }

        [HttpGet("OrdersList")]
        public async Task<IActionResult> OrdersList(int userId)
        {
            var res = await service.OrdersList(userId);
            return Ok(res);
        }

        [HttpPost("ProcessOrder")]
        public async Task<IActionResult> ProcessOrder([FromQuery] int userId)
        {
            if (userId <= 0)
            {
                return BadRequest(new { Message = "Invalid User ID" });
            }

            try
            {
                var isOrderProcessed = await service.ProcessOrder(userId);

                if (!isOrderProcessed)
                {
                    return BadRequest(new { Message = "No items in the cart to process." });
                }

                return Ok(new { Message = "Order processed successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Error = ex.Message });
            }
        }


        [HttpPost("SaveOrders")]
        public async Task<IActionResult> SaveOrder(OrdersEntity request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { Message = "Invalid order data provided." });
                }
                else
                {
                    var res = await service.SaveOrder(request);
                    return Ok(new { Message = "Order placed successfully." });

                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Error = ex.Message });
            }
            
        }

        [HttpGet("TtotalItems")]
        public async Task<ActionResult<int>> GetTotalCartItems(int userId)
        {
            var res = await service.GetTotalCartItems(userId);
            return Ok(res);
        }

        #endregion

    }
}
