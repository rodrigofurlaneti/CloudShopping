namespace CloudShopping.Infrastructure.Services;

// Every unknown administrative controller is restricted to the general administrator.
public static class AccessRequirements
{
    public static string[] For(string controller, string action, bool read)
    {
        if (controller is "Session" or "AccountSecurity") return [];
        if (!read && controller == "Products" && action is "AddStock" or "AdjustInventory") return ["stock.write"];
        if (!read && controller == "Products" && action == "Create") return ["catalog.write", "stock.write"];
        if (!read && controller == "CatalogImports") return ["catalog.write", "stock.write"];
        if (!read && controller == "Operations" && action == "Decide") return ["orders.write", "stock.write"];
        if (controller == "Asaas" && action == "Refund") return ["finance.refund"];
        var module = controller switch
        {
            "Products" or "Departments" or "CatalogDetails" or "CatalogImports" => "catalog",
            "Orders" or "Operations" or "OrderStateHistories" => "orders",
            "Customers" or "Carts" => "customers",
            "Reports" => "reports",
            "Coupons" => "promotions",
            "EngagementAdmin" => action is "Reviews" or "Moderate" ? "moderation" : "support",
            "Asaas" => action is "Connection" or "Configure" ? "settings" : "finance",
            "Storefront" or "StoreBanners" or "OrderSectors" or "OrderStates" or "NotificationAdmin" => "settings",
            _ => null
        };
        return [module == null ? "*" : module + (read ? ".read" : ".write")];
    }
}
