using Test.Entity;
using Test.Model;

namespace Test.BAL.Interfaces
{
    public interface IMasterMgmtService
    {
       Task<(IEnumerable<FoodItemEntity>, IEnumerable<RestaurantEntity>, IEnumerable<MenuItemEntity>)> SearchAsync(string query);
       Task<List<RestaurantEntity>> GetAllRestaurants();
       Task<GenricResponse> AddOrUpdateRestaurant(RestaurantEntity model);
       Task<GenricResponse> AddOrUpdateMenu(MenuItemEntity model);
       List<MenuItemModel> GetAllMenuItems();
       MenuItemEntity GetMenuById(int id);
       Task<GenricResponse> AddOrUpdateFoodItem(FoodItemEntity model);
       List<FoodItemModel> GetAllFoodItems();
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
