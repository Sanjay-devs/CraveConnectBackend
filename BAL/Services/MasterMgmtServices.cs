using Test.BAL.Interfaces;
using Test.DAL.Interfaces;
using Test.Entity;
using Test.Model;

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
        public Task<List<RestaurantEntity>> GetAllRestaurants()
        {
            return repo.GetAllRestaurants();
        }

        public Task<GenricResponse> AddOrUpdateRestaurant(RestaurantEntity model)
        {
            return repo.AddOrUpdateRestaurant(model);
        }

        public Task<GenricResponse> AddOrUpdateMenu(MenuItemEntity model)
        {
            return repo.AddOrUpdateMenu(model);
        }

        public List<MenuItemModel> GetAllMenuItems()
        {
            return repo.GetAllMenuItems();
        }

        public MenuItemEntity GetMenuById(int id)
        {
            return repo.GetMenuById(id);
        }
        public Task<GenricResponse> AddOrUpdateFoodItem(FoodItemEntity model)
        {
            return repo.AddOrUpdateFoodItem(model);
        }
        public List<FoodItemModel> GetAllFoodItems()
        {
            return repo.GetAllFoodItems();
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
