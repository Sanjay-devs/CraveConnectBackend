using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Test.Context;
using Test.DAL.Interfaces;
using Test.Entity;
using Test.Model;

namespace Test.DAL.Repos
{
    public class MasterMgmtRepo : IMasterMgmtRepo
    {
        private readonly MyDbContext db;
        public MasterMgmtRepo(MyDbContext _db)
        {
            db = _db;
        }

        public async Task<(IEnumerable<FoodItemEntity>, IEnumerable<RestaurantEntity>, IEnumerable<MenuItemEntity>)> SearchAsync(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return (Enumerable.Empty<FoodItemEntity>(), Enumerable.Empty<RestaurantEntity>(), Enumerable.Empty<MenuItemEntity>());
            }

            query = query.ToLower();

            var menu = await db.MenuItem
               .Where(f => f.IsActive && !f.IsDeleted && EF.Functions.Like(f.ItemName.ToLower(), $"%{query}%"))
               .ToListAsync();

            var foodItems = await db.FoodItems
                .Where(f => f.IsActive && !f.IsDeleted && EF.Functions.Like(f.FoodItem.ToLower(), $"%{query}%"))
                .ToListAsync();

            var restaurants = await db.Restaurants
                .Where(r => r.IsActive && !r.IsDeleted && EF.Functions.Like(r.Name.ToLower(), $"%{query}%"))
                .ToListAsync();

            return (foodItems, restaurants, menu);
        }

        #region Restaurants
        public async Task<GenricResponse> AddOrUpdateRestaurant(RestaurantEntity model)
        {
            try
            {
                // Check if the restaurant exists
                var existingRestaurant = await db.Restaurants
                    .FirstOrDefaultAsync(r => r.Name == model.Name && r.IsDeleted == false);

                if (existingRestaurant != null)
                {
                    return new GenricResponse
                    {
                        StatusCode = 409,
                        StatusMessage = "Restaurant already exists.",
                    };
                }

                var rest = await db.Restaurants.FirstOrDefaultAsync(r => r.RestaurantId == model.RestaurantId);

                if (rest != null)
                {
                    // Update existing restaurant
                    db.Entry(rest).CurrentValues.SetValues(model);
                    await db.SaveChangesAsync();
                    return new GenricResponse
                    {
                        StatusCode = 200,
                        StatusMessage = "Restaurant updated successfully."

                    };
                }

                // Add new restaurant
                db.Restaurants.Add(model);
                await db.SaveChangesAsync();
                return new GenricResponse
                {
                    StatusCode = 201,
                    StatusMessage = "Restaurant added successfully."
                };
            }
            catch (Exception ex)
            {
                return new GenricResponse
                {
                    StatusCode = 500,
                    StatusMessage = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<List<RestaurantEntity>> GetAllRestaurants()
        {
            return await db.Restaurants.Where(a => a.IsActive == true && a.IsDeleted == false).ToListAsync();
        }


        #endregion

        #region Menu
        public async Task<GenricResponse> AddOrUpdateMenu(MenuItemEntity model)
        {
            var response = new GenricResponse();

            try
            {
                // Validate input model
                if (model == null)
                {
                    response.StatusMessage = "Data can not be empty";
                    response.StatusCode = 400; // Bad Request
                    return response;
                }


                // Check if the restaurant exists
                var restaurant = await db.Restaurants.FindAsync(model.RestaurantId);
                if (restaurant == null)
                {
                    response.StatusMessage = "Restaurant not found.";
                    response.StatusCode = 404; // Not Found
                    return response;
                }

                // Check if the menu item already exists (for update)
                var existingMenuItem = await db.MenuItem.FirstOrDefaultAsync(m => m.MenuId == model.MenuId);

                if (existingMenuItem != null)
                {
                    // Update existing menu item
                    existingMenuItem.ItemName = model.ItemName;
                    existingMenuItem.Price = model.Price;
                    existingMenuItem.IsDeleted = model.IsDeleted;
                    existingMenuItem.IsActive = model.IsActive;
                    existingMenuItem.RestaurantId = model.RestaurantId;
                    existingMenuItem.MenuImage = model.MenuImage;

                    db.MenuItem.Update(existingMenuItem);
                    response.StatusMessage = "Menu item updated successfully.";
                }
                else
                {
                    // Add a new menu item
                    var newMenuItem = new MenuItemEntity
                    {
                        RestaurantId = model.RestaurantId,
                        ItemName = model.ItemName,
                        Price = model.Price,
                        IsDeleted = model.IsDeleted,
                        IsActive = model.IsActive,
                        MenuImage = model.MenuImage
                    };

                    await db.MenuItem.AddAsync(newMenuItem);
                    response.StatusMessage = "Menu item added successfully.";
                }

                // Save changes to the database
                await db.SaveChangesAsync();

                // Include the restaurant name in the response data
                response.Data = $"Menu item belongs to restaurant: {restaurant.Name}";
                response.StatusCode = 200; // Success

            }
            catch (Exception ex)
            {
                // Handle exceptions
                response.StatusMessage = $"An error occurred: {ex.Message}";
                response.StatusCode = 500; // Internal Server Error
            }

            return response;
        }

        public List<MenuItemModel> GetAllMenuItems()
        {
            try
            {
                var menu = db.MenuItem.Where(a => a.IsDeleted == false).
                       Select(a => new MenuItemModel
                       {
                           MenuId = a.MenuId,
                           RestaurantId = a.RestaurantId,
                           ItemName = a.ItemName,
                           Price = a.Price,
                           MenuImage = a.MenuImage,

                       }).ToList();

                return menu;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public MenuItemEntity GetMenuById(int id)
        {
            return db.MenuItem.Where(m => m.MenuId == id && m.IsDeleted == false).FirstOrDefault();

        }
        #endregion

        public async Task<GenricResponse> AddOrUpdateFoodItem(FoodItemEntity model)
        {
            var response = new GenricResponse();

            try
            {
                // Validate input model
                if (model == null)
                {
                    response.StatusMessage = "Empty data.";
                    response.StatusCode = 400; // Bad Request
                    return response;
                }


                // Check if the restaurant exists
                var restaurant = await db.Restaurants.FindAsync(model.RestaurantId);
                if (restaurant == null)
                {
                    response.StatusMessage = "Restaurant not found.";
                    response.StatusCode = 404;
                    return response;
                }

                var menu = await db.MenuItem.FindAsync(model.MenuId);
                if (menu == null)
                {
                    response.StatusCode = 404;
                    response.StatusMessage = "Menu not found";
                    return response;
                }

                // Check if the menu food item already exists (for update)
                var existingFoodItem = await db.FoodItems.FirstOrDefaultAsync(m => m.FoodItemId == model.FoodItemId);

                if (existingFoodItem != null)
                {
                    // Update existing menu item
                    existingFoodItem.FoodItem = model.FoodItem;
                    existingFoodItem.Price = model.Price;
                    existingFoodItem.IsDeleted = model.IsDeleted;
                    existingFoodItem.IsActive = model.IsActive;
                    existingFoodItem.RestaurantId = model.RestaurantId;
                    existingFoodItem.MenuId = model.MenuId;
                    existingFoodItem.FoodImage = model.FoodImage;

                    db.FoodItems.Update(existingFoodItem);
                    response.StatusMessage = "Food item updated successfully.";
                }
                else
                {
                    // Add a new food item
                    var newMenuItem = new FoodItemEntity
                    {
                        FoodItemId = model.FoodItemId,
                        RestaurantId = model.RestaurantId,
                        MenuId = model.MenuId,
                        FoodItem = model.FoodItem,
                        Price = model.Price,
                        IsDeleted = model.IsDeleted,
                        IsActive = model.IsActive,
                        FoodImage = model.FoodImage
                    };

                    await db.FoodItems.AddAsync(newMenuItem);
                    response.StatusMessage = "Food item added successfully.";
                }

                // Save changes to the database
                await db.SaveChangesAsync();

                // Include the restaurant name in the response data
                response.Data = $"Menu item belongs to restaurant: {restaurant.Name}, and Food item  belongs to menu: {menu.ItemName}";
                response.StatusCode = 200; // Success

            }
            catch (Exception ex)
            {
                // Handle exceptions
                response.StatusMessage = $"An error occurred: {ex.Message}";
                response.StatusCode = 500; // Internal Server Error
            }

            return response;
        }

        public List<FoodItemModel> GetAllFoodItems()
        {

            var fooditems = db.FoodItems.Where(f => !f.IsDeleted).ToList();

            if (fooditems == null || !fooditems.Any())
            {
                return new List<FoodItemModel>();
            }


            var result = fooditems.Select(foodItem => new FoodItemModel
            {
                FoodItemId = foodItem.FoodItemId,
                RestaurantId = foodItem.RestaurantId,
                RestaurantName = db.Restaurants.FirstOrDefault(r => r.RestaurantId == foodItem.RestaurantId)?.Name,
                MenuId = foodItem.MenuId,
                MenuName = db.MenuItem.FirstOrDefault(m => m.MenuId == foodItem.MenuId)?.ItemName,
                FoodItem = foodItem.FoodItem,
                Price = foodItem.Price,
                FoodImage = foodItem.FoodImage
            }).ToList();

            return result;
        }

        public List<FoodItemModel> GetFoodItemsByMenuId(int menuId)
        {
            try
            {
                var response = new GenricResponse();

                var menu = db.MenuItem.Where(m => m.MenuId == menuId).Select(m => m.ItemName).ToList();

                var foodItems = db.FoodItems
                                        .Where(food => food.MenuId == menuId && food.IsActive && !food.IsDeleted)
                                        .Select(food => new FoodItemModel
                                        {
                                            RestaurantId = food.RestaurantId,
                                            MenuId = food.MenuId,
                                            FoodItemId = food.FoodItemId,
                                            FoodItem = food.FoodItem,
                                            Price = food.Price,
                                            FoodImage = food.FoodImage,
                                        })
                                   .ToList();
                return foodItems;


            }
            catch (Exception)
            {

                throw;
            }



        }

        public List<FoodItemModel> GetFoodItemsByRestaurantId(int restaurantId)
        {
            try
            {
                var response = new GenricResponse();
                if (restaurantId <=0 )
                {
                    response.StatusCode = 0;
                    response.StatusMessage = "Id is zero or less than zero";
                }

                //var menu = db.MenuItem.Where(m => m.MenuId == menuId).Select(m => m.ItemName).ToList();

                var foodItems = db.FoodItems
                                        .Where(food => food.RestaurantId == restaurantId && food.IsActive && !food.IsDeleted)
                                        .Select(food => new FoodItemModel
                                        {
                                            RestaurantId = food.RestaurantId,
                                            MenuId = food.MenuId,
                                            FoodItemId = food.FoodItemId,
                                            FoodItem = food.FoodItem,
                                            Price = food.Price,
                                            FoodImage = food.FoodImage,
                                        })
                                   .ToList();
                return foodItems;


            }
            catch (Exception)
            {

                throw;
            }



        }
        public async Task<bool> AddandRemoveCart(CartModel cartModel)
        {
            try
            {
                var foodImage = await db.FoodItems
                            .Where(f => f.FoodItemId == f.FoodItemId)
                            .Select(f => f.FoodImage).FirstOrDefaultAsync();

                var existingCart = await db.Cart.FirstOrDefaultAsync(c => c.UserId == cartModel.UserId  && c.FoodItemID == cartModel.FoodItemID);
                if (!db.Users.Any(u => u.UserId == cartModel.UserId))
                {
                    throw new Exception("Invalid UserId. User does not exist.");
                }
                if (existingCart != null)
                {
                    if (cartModel.TotalCount <= 0)
                    {
                        existingCart.IsDeleted = true;
                        db.Cart.Update(existingCart);
                    }
                    else
                    {
                        existingCart.FoodItem = cartModel.FoodItem;
                        existingCart.TotalCount = cartModel.TotalCount;
                        existingCart.IsDeleted = false;
                        existingCart.IsActive = true;
                        existingCart.CreatedOn = DateTime.Now;
                        db.Cart.Update(existingCart);
                    }
                }
                else if (cartModel.TotalCount > 0)
                {
                    var newCart = new CartEntity
                    {
                        CartId =cartModel.CartId,
                        FoodItemID = cartModel.FoodItemID,
                        MenuId = cartModel.MenuId,
                        RestaurantId = cartModel.RestaurantId,
                        UserId = cartModel.UserId,
                        TotalCount = cartModel.TotalCount,
                        FoodItem = cartModel.FoodItem,
                        FoodImage = foodImage,
                        IsDeleted = false,
                        IsActive = true,
                        CreatedOn = DateTime.Now,
                    };
                    await db.Cart.AddAsync(newCart);
                }

                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }



        public async Task<List<CartEntity>> OrderedFoodItems(int userId)
        {
            GenricResponse rsp = new GenricResponse();
            try
            {
                var cartItems = await db.Cart
                    .Where(c => c.UserId == userId && c.IsActive && !c.IsDeleted)
                    .ToListAsync();

                if (cartItems == null)
                {
                    rsp.StatusCode = 400;
                    rsp.StatusMessage = "User not exists";
                }

                var orderedFoodItems = new List<CartEntity>();

                foreach (var c in cartItems)
                {
                    var foodImage = await db.FoodItems
                        .Where(f => f.FoodItemId == c.FoodItemID)
                        .Select(f => f.FoodImage)
                        .FirstOrDefaultAsync();

                    var price = await db.FoodItems
                        .Where(f => f.FoodItemId == c.FoodItemID)
                        .Select(f => (decimal?)f.Price)
                        .FirstOrDefaultAsync();

                    var restaurant = await db.Restaurants.FindAsync(c.RestaurantId);

                    orderedFoodItems.Add(new CartEntity
                    {
                        UserId = c.UserId,
                        MenuId = c.MenuId,
                        RestaurantId = c.RestaurantId,
                        RestaurantName = restaurant?.Name,
                        FoodItemID = c.FoodItemID,
                        FoodItem = c.FoodItem,
                        CartId = c.CartId,
                        TotalCount = c.TotalCount,
                        FoodImage = foodImage,
                        Price = price,
                        TotalPrice = (c.TotalCount) * price,
                        PaymentStatus = "Paid",
                    });
                }

                return orderedFoodItems;
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                throw new Exception("An error occurred: " + ex.Message);
            }
        }

        public async Task<bool> ProcessOrder(int userId)
        {
            try
            {
                // Fetch active items in the cart for the user
                var cartItems = await db.Cart
                    .Where(c => c.UserId == userId && c.IsActive && !c.IsDeleted && c.PaymentStatus != "Paid")
                    .ToListAsync();

                

                if (!cartItems.Any())
                {
                    return false; // No cart items to process
                }

                foreach (var item in cartItems)
                {
                    var price = await db.FoodItems
                        .Where(f => f.FoodItemId == f.FoodItemId)
                        .Select(f => (decimal?)f.Price)
                        .FirstOrDefaultAsync();

                    var foodImage = await db.FoodItems
                            .Where(f => f.FoodItemId == item.FoodItemID)
                            .Select(f => f.FoodImage).FirstOrDefaultAsync();

                    var restaurant = await db.Restaurants
                            .Where(f => f.RestaurantId == f.RestaurantId)
                            .Select(f => f.Name)
                            .FirstOrDefaultAsync();
                    // Create new order for each item
                    var order = new OrdersEntity
                    {
                        UserId = item.UserId,
                        MenuId = item.MenuId,
                        RestaurantId = item.RestaurantId,
                        RestaurantName = restaurant,
                        PaymentStatus = "Paid", // Update payment status to paid
                        FoodItemId = item.FoodItemID,
                        FoodItem = item.FoodItem,
                        FoodImage = foodImage,
                        CartId = item.CartId,
                        TotalQuantity = item.TotalCount ?? 1,
                        Price = price??0,
                        TotalPrice = (item.TotalCount ?? 1) * price??0,
                        IsActive = true,
                        IsDeleted = false
                    };

                    // Insert the order into the Orders table
                    await db.Order.AddAsync(order);

                    // Mark cart item as deleted after moving to orders
                    item.IsDeleted = true;
                    item.PaymentStatus = "Paid"; // Update the payment status in the cart

                    db.Cart.Update(item);
                }

                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                throw new Exception("An error occurred while moving cart items to orders: " + ex.Message);
            }
        }

        public async Task<List<OrdersEntity>> OrdersList(int userId)
        {
            try
            {
                var order = await db.Order
                    .Where(a => a.UserId == userId && !a.IsDeleted && a.IsActive)
                    .OrderByDescending(a => a.OrderId) // Order by OrderId descending
                    .ToListAsync();
                return order;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        //public async Task<List<OrdersEntity>> OrdersList(int userId)
        //{
        //    try
        //    {
        //        var cartItems = await db.Cart
        //            .Where(c => c.UserId == userId && c.IsActive && !c.IsDeleted)
        //            .ToListAsync();

        //        var orderedFoodItems = new List<OrdersEntity>();

        //        foreach (var c in cartItems)
        //        {
        //            var foodItem = await db.FoodItems.FindAsync(c.FoodItemID);
        //            var restaurant = await db.Restaurants.FindAsync(c.RestaurantId);

        //            orderedFoodItems.Add(new OrdersEntity
        //            {
        //                UserId = c.UserId,
        //                FoodItem = foodItem?.FoodItem, // Safe navigation in case foodItem is null
        //                FoodImage = foodItem?.FoodImage, // Safe navigation for FoodImage
        //                TotalQuantity = c.TotalCount ?? 0, // Provide a default value for nullable int
        //                Price = c.Price ?? 0, // Provide a default value for nullable decimal
        //                TotalPrice = (c.TotalCount ?? 0) * (c.Price ?? 0), // Handle nullable values
        //                RestaurantName = restaurant?.Name, // Safe navigation for restaurant name
        //                PaymentStatus = "Paid",
        //                RestaurantId = restaurant?.RestaurantId ?? 0, // Assign restaurant's Id, fallback to 0 if null
        //                FoodItemId = c.FoodItemID,
        //                CartId = c.CartId,
        //            });
        //        }

        //        return orderedFoodItems;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("An error occurred: " + ex.Message);
        //    }
        //}

        public async Task<OrdersEntity> SaveOrder(OrdersEntity request)
        {
                GenricResponse resp = new GenricResponse();
            try
            {
                var orders = new OrdersEntity {

                    UserId = request.UserId,
                    MenuId = request.MenuId,
                    RestaurantId = request.RestaurantId,
                    FoodItemId = request.FoodItemId,
                    FoodItem = request.FoodItem,
                    FoodImage = request.FoodImage,
                    CartId = request.CartId,
                    TotalQuantity = request.TotalQuantity,
                    Price = request.Price,
                    TotalPrice = request.TotalPrice,
                    IsActive = true,
                    IsDeleted = false
                };

                   db.Order.Add(orders);
                

                await db.SaveChangesAsync();
                resp.StatusCode = 200;
                resp.StatusMessage = "Order placed successfully";
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return request;
        }
        public async Task<int> GetTotalCartItems(int userId)
        {
            var cartItems = await db.Cart
                .Where(c => c.UserId == userId && !c.IsDeleted && c.IsActive)
                .ToListAsync();

            if (cartItems == null || cartItems.Count == 0)
            {
                return 0; // If no items are found, return 0
            }

            var totalItems = cartItems.Sum(c => c.TotalCount ?? 0);
            return totalItems;
        }


    }








}


