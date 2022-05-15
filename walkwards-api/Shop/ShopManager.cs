using System.Security.Cryptography.X509Certificates;
using walkwards_api.Sql;
using walkwards_api.UserManager;
using walkwards_api.Utilities;

namespace walkwards_api.Shop;

public static class ShopManager
{
    public static async Task<object> AddProduct(int days, int startPrice, string name, string producer, string image, string description)
    {
        Random rnd = new Random();
        int id = rnd.Next(100000, 999999);

        Product product = new Product(id, days, DateTime.Now.Hour, startPrice, startPrice, null, name, producer, image,
            description);

        product.EndDate = DateTime.Today.AddDays(product.Days).ToString();
        
        await SqlManager.ExecuteNonQuery(
            $"INSERT INTO base.walkshop VALUES({product.ProductId}, '{product.EndDate}', {product.StartPrice}, {product.ActualPrice}, {0}, '{product.Name}', '{product.Producer}', {product.EndHour}, '{product.Image}', '{product.Description}');");


        return product;
    }

    public static async Task<Product> GetProduct(int pid)
    {
        var data = await SqlManager.Reader($"SELECT * FROM base.walkshop WHERE productid = {pid};");

        if (data.Count == 0) throw new CustomError("ProductIsNotExist");

        if (data[0]["currentwinner"] == 0) data[0]["currentwinner"] = 154351;
        
        Product product = new Product(data[0]["productid"], 0, data[0]["hour"], data[0]["startprice"],
            data[0]["actualprice"], await UserMethod.GetUserData(data[0]["currentwinner"]), data[0]["name"], data[0]["producer"],
            data[0]["image"], data[0]["description"], data[0]["enddate"]);
        
        return product;
    }

    public static async Task<object> GetAllActiveAuction()
    {
        var data = await SqlManager.Reader($"SELECT * FROM base.walkshop;");

        if (data.Count == 0) throw new CustomError("ProductIsNotExist");

        List<Product> products = new List<Product>();

        foreach (var item in data)
        {
            Product product = new Product(data[0]["productid"], data[0]["enddate"], data[0]["hour"],
                item["startprice"], item["actualprice"], await UserMethod.GetUserData(item["currentwinner"], false, false), item["name"], item["producer"],
                item["image"], item["description"]);
            
            products.Add(product);
        }

        return products;
    }

    public static async Task<object> JoinAuction(int uid, int pid, int nextPrice)
    {
        User user = await UserMethod.GetUserData(uid, false, false);
        Product product = await GetProduct(pid);

        if (product.ActualPrice >
            nextPrice) throw new CustomError("NextPriceIsLow");
        
        if (uid == product.CurrentWinner.Id) throw new CustomError("UserNowIsWinThisAuction");
        
        if (user.WalkCoins <= product.ActualPrice) throw new CustomError("UserNotHaveEnoughMoney");

        await SqlManager.ExecuteFromList(new()
        {
            $"UPDATE base.walkshop SET actualprice = {nextPrice} WHERE productid = {pid};",
            $"UPDATE base.walkshop SET currentwinner = {user.Id} WHERE productid = {pid};",
            $"UPDATE user_{product.CurrentWinner.Id}.user SET walkcoins = {product.CurrentWinner.WalkCoins - nextPrice};"
        });
        

        return true;
    }
} 