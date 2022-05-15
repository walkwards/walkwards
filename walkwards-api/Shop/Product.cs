using walkwards_api.UserManager;

namespace walkwards_api.Shop;

public class Product
{
    public int ProductId;
    public string EndDate;
    public int Days;
    public int StartPrice;
    public int ActualPrice;
    public User CurrentWinner;
    public int EndHour;
    public string Name;
    public string Producer;
    public string Image;
    public string Description;

    public Product(int productId, int days, int endHour, int startPrice, int actualPrice, User currentWinner, string name, string producer, string image, string description, string endDate = "")
    {
        ProductId = productId;
        EndHour = endHour;
        Days = days;
        CurrentWinner = currentWinner;
        Name = name;
        EndDate = endDate;
        Producer = producer;
        StartPrice = startPrice;
        ActualPrice = actualPrice;
        Image = image;
        Description = description;
    }
    public Product(int productId, string enddate, int endHour, int startPrice, int actualPrice, User currentWinner, string name, string producer, string image, string description, string endDate = "")
    {
        ProductId = productId;
        EndHour = endHour;
        EndDate = enddate;
        CurrentWinner = currentWinner;
        Name = name;
        EndDate = endDate;
        Producer = producer;
        StartPrice = startPrice;
        ActualPrice = actualPrice;
        Image = image;
        Description = description;
    }
}

