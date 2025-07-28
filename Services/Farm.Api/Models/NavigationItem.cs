using System.Net;

namespace Farm.Api.Models
{
    public class NavigationItem
    {
        public string DisplayName { get; set; }
        public string Route { get; set; }
        public string[] Roles { get; set; }

        public bool IsAuthorized { get; set; }

        public string Url { get; set; }

        public IList<NavigationItem> Children { get; set; }

        /// <summary>
        /// Get all navigation items
        /// </summary>
        /// <returns>List of NavigationItem</returns>
        public static IList<NavigationItem> GetAll()
        {
            var items = new List<NavigationItem>
            {
                new NavigationItem
                {
                  DisplayName = "Farms",
                  Route = "app/farms",
                  IsAuthorized = true,
                },
                new NavigationItem
                {
                    DisplayName = "Cages",
                    Route = "app/cages",
                    IsAuthorized = true,
                },
                new NavigationItem
                {
                    DisplayName = "Animals",
                    Route = "app/animals",
                    IsAuthorized = true,
                },
                new NavigationItem
                {
                    DisplayName = "Users",
                    Route = "app/users",
                    IsAuthorized = true,
                }
            };
            return items;
        }
    }
}
