using Test.Entity;
using Test.Model;
using static CraveConnect.Utilities.UserActions;

namespace Test.DAL.Interfaces
{
    public interface IMasterMgmtRepo
    {
        Task<(IEnumerable<FoodItemEntity>, IEnumerable<RestaurantEntity>, IEnumerable<MenuItemEntity>)> SearchAsync(string query);
        Task<List<DropDownList>> RestaurantsDD(string? q = "");
        Task<List<RestaurantEntity>> GetAllRestaurants();
        Task<GenricResponse> AddOrUpdateRestaurant(RestaurantEntity model);
        RestaurantspModel GetAllRestaurantsPagenation(string? q = "", int pageNumber = 1, int pageSize = 5);
        GenricResponse DeleteRestaurant(int id);
        Task<List<DropDownList>> MenuItemsDD(string? q = "");
        Task<GenricResponse> AddOrUpdateMenu(MenuItemEntity model);
        List<MenuItemModel> GetAllMenuItems();
        MenuItemspModel GetAllMenuItemsPagenation(string? q = "", int pageNumber = 1, int pageSize = 10);
        MenuItemEntity GetMenuById(int id);
        GenricResponse DeleteMenuItem(int id);
        Task<GenricResponse> AddOrUpdateFoodItem(FoodItemEntity model);
        List<FoodItemModel> GetAllFoodItems();
        FoodItemspModel GetAllFoodItemsPagenation(string? q = "", int pageNumber = 1, int pageSize = 10);
        GenricResponse DeleteFoodItem(int id);
        (MostOrderedFood, MostVisitedRestaurant) GetMostOrderedFoodAndRestaurant(int? userId = null);
        Task<bool> AddandRemoveCart(CartModel cartModel);
        List<FoodItemModel> GetFoodItemsByMenuId(int menuId);
        Task<List<CartEntity>> OrderedFoodItems(int userId);
        Task<bool> ProcessOrder(int userId);
        Task<OrdersEntity> SaveOrder(OrdersEntity request);
        Task<List<OrdersEntity>> OrdersList(int userId);
        Task<int> GetTotalCartItems(int userId);
        List<FoodItemModel> GetFoodItemsByRestaurantId(int restaurantId);
    }
}
