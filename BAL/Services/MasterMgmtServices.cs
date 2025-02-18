using Test.BAL.Interfaces;
using Test.DAL.Interfaces;
using Test.Entity;
using Test.Model;
using static CraveConnect.Utilities.UserActions;

namespace Test.BAL.Services
{
    public class MasterMgmtServices : IMasterMgmtService
    {
        private readonly IMasterMgmtRepo repo;

        public MasterMgmtServices(IMasterMgmtRepo _repo)
        {
            repo = _repo;
        }

        public Task<(IEnumerable<FoodItemEntity>, IEnumerable<RestaurantEntity>, IEnumerable<MenuItemEntity>)> SearchAsync(string query)
        {
            return repo.SearchAsync(query);
        }
        public Task<List<DropDownList>> RestaurantsDD(string? q = "")
        {
            return repo.RestaurantsDD(q);
        }
        public RestaurantspModel GetAllRestaurantsPagenation(string? q = "", int pageNumber = 1, int pageSize = 5)
        {
            return repo.GetAllRestaurantsPagenation(q, pageNumber, pageSize);
        }
        public Task<List<RestaurantEntity>> GetAllRestaurants()
        {
            return repo.GetAllRestaurants();
        }

        public Task<GenricResponse> AddOrUpdateRestaurant(RestaurantEntity model)
        {
            return repo.AddOrUpdateRestaurant(model);
        }

        public GenricResponse DeleteRestaurant(int id)
        {
            return repo.DeleteRestaurant(id);
        }
        public Task<List<DropDownList>> MenuItemsDD(string? q = "")
        {
            return repo.MenuItemsDD(q);
        }
        public Task<GenricResponse> AddOrUpdateMenu(MenuItemEntity model)
        {
            return repo.AddOrUpdateMenu(model);
        }

        public List<MenuItemModel> GetAllMenuItems()
        {
            return repo.GetAllMenuItems();
        }

        public MenuItemspModel GetAllMenuItemsPagenation(string? q = "", int pageNumber = 1, int pageSize = 10)
        {
            return repo.GetAllMenuItemsPagenation(q, pageNumber, pageSize);
        }
        public MenuItemEntity GetMenuById(int id)
        {
            return repo.GetMenuById(id);
        }
        public GenricResponse DeleteMenuItem(int id)
        {
            return repo.DeleteMenuItem(id);
        }
        public Task<GenricResponse> AddOrUpdateFoodItem(FoodItemEntity model)
        {
            return repo.AddOrUpdateFoodItem(model);
        }
        public List<FoodItemModel> GetAllFoodItems()
        {
            return repo.GetAllFoodItems();
        }
        public FoodItemspModel GetAllFoodItemsPagenation(string? q = "", int pageNumber = 1, int pageSize = 10)
        {
            return repo.GetAllFoodItemsPagenation(q, pageNumber, pageSize);
        }
        public GenricResponse DeleteFoodItem(int id)
        {
            return repo.DeleteFoodItem(id);
        }
        public (MostOrderedFood, MostVisitedRestaurant) GetMostOrderedFoodAndRestaurant(int? userId = null)
        {
            return repo.GetMostOrderedFoodAndRestaurant(userId);
        }
        public Task<bool> AddandRemoveCart(CartModel cartModel)
        {
            return repo.AddandRemoveCart(cartModel);
        }
        public List<FoodItemModel> GetFoodItemsByMenuId(int menuId)
        {
            return repo.GetFoodItemsByMenuId(menuId);
        }
        public Task<List<CartEntity>> OrderedFoodItems(int userId)
        {
            return repo.OrderedFoodItems(userId);
        }
        public Task<bool> ProcessOrder(int userId)
        {
            return repo.ProcessOrder(userId);
        }
        public Task<OrdersEntity> SaveOrder(OrdersEntity request)
        {
            return repo.SaveOrder(request);
        }
        public Task<List<OrdersEntity>> OrdersList(int userId)
        {
            return repo.OrdersList(userId);
        }
        public Task<int> GetTotalCartItems(int userId)
        {
            return repo.GetTotalCartItems(userId);
        }
        public List<FoodItemModel> GetFoodItemsByRestaurantId(int restaurantId)
        {
            return repo.GetFoodItemsByRestaurantId(restaurantId);
        }
    }
}
