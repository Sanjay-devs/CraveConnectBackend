using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Test.BAL.Interfaces;
using Test.BAL.Intrfaces;
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

        [HttpGet("GetAllRestaurants")]
        public async Task<ActionResult<List<RestaurantEntity>>> GetAllRestaurants()
        {
            var res = await service.GetAllRestaurants();
            return Ok(res);
        }

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


        [HttpGet("GetMenuById")]
        public IActionResult GetMenuById(int id)
        {
            var res = service.GetMenuById(id);
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
                    return Ok(new {items_1 = response});
                    //return Ok(new { StatusCode = 200, StatusMessage = "Registration successful" });
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

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

    }
}
