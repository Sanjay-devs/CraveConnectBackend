using System.Data;
using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Test.Context;
using Test.DAL.Interfaces;
using Test.Entity;
using Test.Model;
using static CraveConnect.Utilities.UserActions;

namespace Test.DAL.Repos
{
    public class MasterMgmtRepo : IMasterMgmtRepo
    {
        private readonly MyDbContext db;
        public MasterMgmtRepo(MyDbContext _db)
        {
            db = _db ?? throw new ArgumentNullException(nameof(_db)); 
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
        public async Task<List<DropDownList>> RestaurantsDD(string? q = "")
        {
            var items = new List<DropDownList>();
            var search = new SqlParameter("q", q == null ? "" : q);
            //var u = await db.Restaurants.FromSqlInterpolated($"EXEC dbo.sp_getRestaurantName {search}").ToListAsync();
            var u = await db.Restaurants.Where(r => r.IsDeleted == false).ToListAsync();

            foreach (var obj in u)
            {
                items.Add(new DropDownList { Name = obj.Name, Id = obj.RestaurantId });
            }
            return items;
        }
        public RestaurantspModel GetAllRestaurantsPagenation(string? q = "", int pageNumber = 1, int pageSize = 5)
        {
            RestaurantspModel response = new RestaurantspModel();
            List<RestaurantEntity> myList = new List<RestaurantEntity>();

            try
            {
                SqlParameter[] sParams =
                {
                    new SqlParameter("@q", q ?? ""),
                    new SqlParameter("@pageNumber", pageNumber),
                    new SqlParameter("@pageSize", pageSize)
                };

                string sp = "EXEC sp_getRestaurantName @q, @pageNumber, @pageSize";

                // Execute stored procedure to fetch paginated restaurant data
                myList = db.Set<RestaurantEntity>().FromSqlRaw(sp, sParams).AsEnumerable().ToList();

                // Fixing the parameter issue in count execution
                SqlParameter[] sParamsCnt =
                {
                    new SqlParameter("@q", q ?? "")
                };

                string spCnt = "EXEC sp_getRestaurantCount @q";

                // Fetch count and totalCount in one call
                DBCountResponse count = db.Set<DBCountResponse>().FromSqlRaw(spCnt, sParamsCnt).AsEnumerable().FirstOrDefault();

                response.count = count?.cnt ?? 0;
                response.totalCount = count?.totalCount ?? 0;  // This should now work
                response.result = myList;
                response.pageNumber = pageNumber;
                response.pageSize = pageSize;
                response.q = q;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching restaurant data", ex);
            }

            return response;
        }

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

        public GenricResponse DeleteRestaurant(int id)
        {
            GenricResponse resp = new GenricResponse();

            try
            {
                var restaurant = db.Restaurants.FirstOrDefault(r => !r.IsDeleted && r.RestaurantId == id);

                if (restaurant != null)
                {
                    restaurant.IsDeleted = true;
                    db.SaveChanges();

                    resp.StatusCode = 200;
                    resp.StatusMessage = "Restaurant Item Deleted Successfully";
                    resp.CurrentId = id;
                }
                else
                {
                    resp.StatusCode = 404;
                    resp.StatusMessage = "Restaurant Item not found";
                    resp.CurrentId = id;
                }
            }
            catch (Exception)
            {
                resp.StatusCode = 500;
                resp.StatusMessage = "Failed to delete";
            }

            return resp;
        }


        public List<FoodItemModel> GetFoodItemsByRestaurantId(int restaurantId)
        {
            try
            {
                var response = new GenricResponse();
                if (restaurantId <= 0)
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


        #endregion

        #region Menu
        public async Task<List<DropDownList>> MenuItemsDD(string? q = "")
        {
            var items = new List<DropDownList>();
            var search = new SqlParameter("q", q == null ? "" : q);
            //var u = await db.MenuItem.FromSqlInterpolated($"EXEC dbo.sp_getMenuName {search}").ToListAsync();
            var u = await db.MenuItem.Where(a=>a.IsDeleted==false).ToListAsync();

            foreach (var obj in u)
            {
                items.Add(new DropDownList { Name = obj.ItemName, Id = obj.MenuId });
            }
            return items;
        }
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

                var existingMenu = await db.MenuItem.Where(m => m.ItemName == model.ItemName && !m.IsDeleted)
                    .FirstOrDefaultAsync();
                if (existingMenu!=null)
                {
                    return new GenricResponse
                    {
                        StatusCode = 409,
                        StatusMessage = "Menu Item already exists.",
                    };
                }

                // Check if the restaurant exists
                var restaurant = await db.Restaurants
                    .Where(r=>r.RestaurantId == model.RestaurantId && !r.IsDeleted).FirstOrDefaultAsync();

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

        //public List<MenuItemModel> GetAllMenuItems()
        //{
        //    try
        //    {

        //        var menu = db.MenuItem.Where(a => a.IsDeleted == false).
        //               Select(a => new MenuItemModel
        //               {
        //                   MenuId = a.MenuId,
        //                   RestaurantId = a.RestaurantId,
        //                   ItemName = a.ItemName,
        //                   Price = a.Price,
        //                   MenuImage = a.MenuImage,

        //               }).ToList();

        //        return menu;
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }

        //}

        public MenuItemspModel GetAllMenuItemsPagenation(string? q = "", int pageNumber = 1, int pageSize = 10)
        {
            MenuItemspModel response = new MenuItemspModel();
            List<MenuItemModel> myList = new List<MenuItemModel>();

            try
            {
                SqlParameter[] sParams =
                {
                    new SqlParameter("@q", q ?? ""),
                    new SqlParameter("@pageNumber", pageNumber),
                    new SqlParameter("@pageSize", pageSize)
                };

                string sp = "EXEC sp_getMenuName @q, @pageNumber, @pageSize";

                // Execute stored procedure to fetch paginated restaurant data
                myList = db.Set<MenuItemModel>().FromSqlRaw(sp, sParams).AsEnumerable().ToList();

                // Fixing the parameter issue in count execution
                SqlParameter[] sParamsCnt =
                {
                    new SqlParameter("@q", q ?? "")
                };

                string spCnt = "EXEC sp_getMenuCount @q";

                // Fetch count and totalCount in one call
                DBCountResponse count = db.Set<DBCountResponse>().FromSqlRaw(spCnt, sParamsCnt).AsEnumerable().FirstOrDefault();

                response.count = count?.cnt ?? 0;
                response.totalCount = count?.totalCount ?? 0;  // This should now work
                response.result = myList;
                response.pageNumber = pageNumber;
                response.pageSize = pageSize;
                response.q = q;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching restaurant data", ex);
            }

            return response;
        }
        public List<MenuItemModel> GetAllMenuItems()
        {
            try
            {
                var menu = (from menuItem in db.MenuItem
                            join restaurant in db.Restaurants
                            on menuItem.RestaurantId equals restaurant.RestaurantId
                            where !menuItem.IsDeleted && !restaurant.IsDeleted
                            select new MenuItemModel
                            {
                                MenuId = menuItem.MenuId,
                                RestaurantId = menuItem.RestaurantId,
                                RestaurantName = restaurant.Name, // Fetching restaurant name from the Restaurants table
                                ItemName = menuItem.ItemName,
                                Price = menuItem.Price,
                                MenuImage = menuItem.MenuImage
                            }).ToList();

                return menu;
            }
            catch (Exception ex)
            {
                throw; // Consider logging the exception
            }
        }

        public MenuItemEntity GetMenuById(int id)
        {
            return db.MenuItem.Where(m => m.MenuId == id && m.IsDeleted == false).FirstOrDefault();

        }

        public GenricResponse DeleteMenuItem(int id)
        {
            GenricResponse resp = new GenricResponse();

            try
            {
                var menu = db.MenuItem.FirstOrDefault(r => !r.IsDeleted && r.MenuId == id);

                if (menu != null)
                {
                    menu.IsDeleted = true;
                    db.SaveChanges();

                    resp.StatusCode = 200;
                    resp.StatusMessage = "Menu Item Deleted Successfully";
                    resp.CurrentId = id;
                }
                else
                {
                    resp.StatusCode = 404;
                    resp.StatusMessage = "Menu Item not found";
                    resp.CurrentId = id;
                }
            }
            catch (Exception)
            {
                resp.StatusCode = 500;
                resp.StatusMessage = "Failed to delete";
            }

            return resp;
        }

        #endregion

        #region FoodItems
        public FoodItemspModel GetAllFoodItemsPagenation(string? q = "", int pageNumber = 1, int pageSize = 10)
        {
            FoodItemspModel response = new FoodItemspModel();
            List<FoodItemModel> myList = new List<FoodItemModel>();

            try
            {
                SqlParameter[] sParams =
                {
                    new SqlParameter("@q", q ?? (object)DBNull.Value),
                    new SqlParameter("@pageNumber", pageNumber),
                    new SqlParameter("@pageSize", pageSize)
                };

                string sp = "EXEC sp_getFoodItemName @q, @pageNumber, @pageSize";

                // Ensure EF Core does not expect a Discriminator column
                myList = db.Database.SqlQueryRaw<FoodItemModel>(sp, sParams).AsNoTracking().ToList();

                // Fetch count data safely
                SqlParameter[] sParamsCnt =
                {
                    new SqlParameter("@q", q ?? (object)DBNull.Value)
                };

                string spCnt = "EXEC sp_getFoodItemCount @q";
                DBCountResponse count = db.Database.SqlQueryRaw<DBCountResponse>(spCnt, sParamsCnt).AsNoTracking().AsEnumerable().FirstOrDefault();

                response.count = count?.cnt ?? 0;
                response.totalCount = count?.totalCount ?? 0;
                response.result = myList;
                response.pageNumber = pageNumber;
                response.pageSize = pageSize;
                response.q = q;
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("SQL Error fetching food item data", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("General error fetching food item data", ex);
            }

            return response;
        }

        //public FoodItemspModel GetAllFoodItemsPagenation(string? q = "", int pageNumber = 1, int pageSize = 10)
        //{
        //    FoodItemspModel response = new FoodItemspModel();
        //    List<FoodItemModel> myList = new List<FoodItemModel>();

        //    try
        //    {
        //        SqlParameter[] sParams =
        //        {
        //            new SqlParameter("@q", q ?? ""),
        //            new SqlParameter("@pageNumber", pageNumber),
        //            new SqlParameter("@pageSize", pageSize)
        //        };

        //        string sp = "EXEC sp_getFoodItemName @q, @pageNumber, @pageSize";

        //        // Execute stored procedure to fetch paginated restaurant data
        //        myList = db.Set<FoodItemModel>().FromSqlRaw(sp, sParams).AsEnumerable().ToList();

        //        // Fixing the parameter issue in count execution
        //        SqlParameter[] sParamsCnt =
        //        {
        //            new SqlParameter("@q", q ?? "")
        //        };

        //        string spCnt = "EXEC sp_getFoodItemCount @q";

        //        // Fetch count and totalCount in one call
        //        DBCountResponse count = db.Set<DBCountResponse>().FromSqlRaw(spCnt, sParamsCnt).AsEnumerable().FirstOrDefault();

        //        response.count = count?.cnt ?? 0;
        //        response.totalCount = count?.totalCount ?? 0;  // This should now work
        //        response.result = myList;
        //        response.pageNumber = pageNumber;
        //        response.pageSize = pageSize;
        //        response.q = q;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error fetching food item data", ex);
        //    }

        //    return response;
        //}
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
                var existFood = await db.FoodItems
                    .Where(f => f.FoodItem == model.FoodItem && !f.IsDeleted).FirstOrDefaultAsync();
                if (existFood != null)
                {
                    return new GenricResponse
                    {
                        StatusCode = 409,
                        StatusMessage = "Food Item already exists",
                    };
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
                    response.StatusCode = 204;
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
                ItemName = db.MenuItem.FirstOrDefault(m => m.MenuId == foodItem.MenuId)?.ItemName,
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

        public GenricResponse DeleteFoodItem(int id)
        {
            GenricResponse resp = new GenricResponse();

            try
            {
                var food = db.FoodItems.FirstOrDefault(r => !r.IsDeleted && r.FoodItemId == id);

                if (food != null)
                {
                    food.IsDeleted = true;
                    db.SaveChanges();

                    resp.StatusCode = 200;
                    resp.StatusMessage = "Food Item Deleted Successfully";
                    resp.CurrentId = id;
                }
                else
                {
                    resp.StatusCode = 404;
                    resp.StatusMessage = "Food Item not found";
                    resp.CurrentId = id;
                }
            }
            catch (Exception)
            {
                resp.StatusCode = 500;
                resp.StatusMessage = "Failed to delete";
            }

            return resp;
        }

        public (MostOrderedFood, MostVisitedRestaurant) GetMostOrderedFoodAndRestaurant(int? userId = null)
        {
            MostOrderedFood mostOrderedFood = null;
            MostVisitedRestaurant mostVisitedRestaurant = null;

            try
            {
                if (db == null) throw new Exception("Database context is null!"); // Check if `db` is null
                if (db.MostOrderedFoods == null) throw new Exception("MostOrderedFoods DbSet is null!");
                if (db.MostVisitedRestaurants == null) throw new Exception("MostVisitedRestaurants DbSet is null!");

                SqlParameter userParam = new SqlParameter("@UserId", userId ?? (object)DBNull.Value);
                string sp = "EXEC GetMostOrderedFoodAndRestaurant @UserId";

                var foods = db.MostOrderedFoods.FromSqlRaw(sp, userParam).ToList();
                var restaurants = db.MostVisitedRestaurants.FromSqlRaw(sp, userParam).ToList();

                mostOrderedFood = foods.FirstOrDefault();
                mostVisitedRestaurant = restaurants.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching most ordered food and restaurant data: " + ex.Message, ex);
            }

            return (mostOrderedFood, mostVisitedRestaurant);
        }


        #endregion

        #region Carts
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
                        PaymentStatus = "Paid Through Card",
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
                    .Where(c => c.UserId == userId && c.IsActive && !c.IsDeleted && c.PaymentStatus != "Paid Through Card")
                    .ToListAsync();

                

                if (!cartItems.Any())
                {
                    return false; // No cart items to process
                }

                foreach (var item in cartItems)
                {
                    var price = await db.FoodItems
                            .Where(f => f.FoodItemId == item.FoodItemID)
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
                        PaymentStatus = "Paid Through Card", // Update payment status to paid
                        FoodItemId = item.FoodItemID,
                        FoodItem = item.FoodItem,
                        FoodImage = foodImage,
                        CartId = item.CartId,
                        TotalQuantity = item.TotalCount ?? 1,
                        Price = price??0,
                        TotalPrice = (item.TotalCount ?? 1) * price??0,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedOn = DateTime.Now,
                    };

                    // Insert the order into the Orders table
                    await db.Order.AddAsync(order);

                    // Mark cart item as deleted after moving to orders
                    item.IsDeleted = true;
                    item.PaymentStatus = "Paid Through Card"; // Update the payment status in the cart
                    item.CreatedOn = DateTime.Now;

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

        #endregion
    }








}


