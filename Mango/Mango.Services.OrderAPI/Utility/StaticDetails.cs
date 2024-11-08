namespace Mango.Services.OrderAPI.Utility
{
    public class StaticDetails
    {
        public enum OrderStatus
        {
            Pending,
            Approved,
            ReadyForPickup,
            Completed,
            Refunded,
            Cancelled
        }
        public static readonly Dictionary<OrderStatus, string> Statuses = new()
        {
            { OrderStatus.Pending, "Pending" },
            { OrderStatus.Approved, "Approved" },
            { OrderStatus.ReadyForPickup, "Ready For Pickup" },
            { OrderStatus.Completed, "Completed" },
            { OrderStatus.Refunded, "Refunded" },
            { OrderStatus.Cancelled, "Cancelled" }
        };

        public enum UserRoles
        {
            Admin,
            Customer
        }
        public static readonly Dictionary<UserRoles, string> Roles = new()
        {
            {UserRoles.Customer, "Customer" },
            {UserRoles.Admin, "Admin" }
        };

    }
}
