using Test.Entity;
using Test.Model;
using static CraveConnect.Utilities.UserActions;

namespace Test.BAL.Interfaces
{
    public interface IMasterMgmtService
    {
       Task<(IEnumerable<FoodItemEntity>, IEnumerable<RestaurantEntity>, IEnumerable<MenuItemEntity>)> SearchAsync(string query);
       Task<List<RestaurantEntity>> GetAllRestaurants();
       Task<List<DropDownList>> RestaurantsDD(string? q = "");
       RestaurantspModel GetAllRestaurantsPagenation(string? q = "", int pageNumber = 1, int pageSize = 5);
       Task<GenricResponse> AddOrUpdateRestaurant(RestaurantEntity model);
       GenricResponse DeleteRestaurant(int id);
       Task<List<DropDownList>> MenuItemsDD(string? q = "");
       Task<GenricResponse> AddOrUpdateMenu(MenuItemEntity model);
       List<MenuItemModel> GetAllMenuItems();
       MenuItemspModel GetAllMenuItemsPagenation(string? q = "", int pageNumber = 1, int pageSize = 10);
       MenuItemEntity GetMenuById(int id);
       GenricResponse DeleteMenuItem(int id);
       Task<GenricResponse> AddOrUpdateFoodItem(FoodItemEntity model);
       List<FoodItemModel> GetAllFoodItems();
       GenricResponse DeleteFoodItem(int id);
       FoodItemspModel GetAllFoodItemsPagenation(string? q = "", int pageNumber = 1, int pageSize = 10);
       (MostOrderedFood, MostVisitedRestaurant) GetMostOrderedFoodAndRestaurant(int? userId = null);
       Task<bool> AddandRemoveCart(CartModel cartModel);
       List<FoodItemModel> GetFoodItemsByMenuId(int menuId);
       Task<List<CartEntity>> OrderedFoodItems(int userId);
       Task<bool> ProcessOrder(int userId);
       Task<List<OrdersEntity>> OrdersList(int userId);
       Task<OrdersEntity> SaveOrder(OrdersEntity request);
       Task<int> GetTotalCartItems(int userId);
       List<FoodItemModel> GetFoodItemsByRestaurantId(int restaurantId);
    }
}
