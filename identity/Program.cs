using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);


var users = new List<User>();
var orders = new List<Order>();

var md5 = MD5.Create();


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/registerC", (RegisterCustomer body) =>
{
    var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(body.Password));
    Customer user = new Customer()
    {
        Id = Guid.NewGuid(),
        Login = body.Login,
        Password = hash,
        AddressPost = body.AddressPost,
        Phone = body.Phone
        
    }; 
    users.Add(user);
    return "Ok";
});

app.MapPost("/registerS", (RegisterSeller body) =>
{
    var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(body.Password));
    Seller user = new Seller()
    {
        Id = Guid.NewGuid(),
        Login = body.Login,
        Password = hash,
        AddressGive = body.AddressGive,
        Info = body.Info

    }; 
    users.Add(user);
    return "Ok";
});

app.MapPost("/login", (LoginBody body) =>
{
    var user = users.Find(u => u.Login == body.Login);
    
    
    var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(body.Password));

    if (user == null) return "Пользователь не найден";

    if (user.Password.ToString() == hash.ToString()) return "Вход выполнен";
    return "Неверный пароль";
   
});

app.MapPost("/goods", (CreateGood body) =>
{
    var user = users.Find(u => u.Id == body.Id);
    if (user == null) return "Пользователь не найден";
    if (user is Customer) return "Пользователь - клиент";
    var seller = user as Seller;
    seller.goods.Add(new Goods(body.Name, body.Price));
    return "Ок";
});

app.MapPost("/newOrder", (NewOrder body) =>
{ 
    var customer = users.Find(u => u.Id == body.CustomerId);
    if (customer == null) return "Пользователь не найден";
    if (customer is Seller) return "Пользователь - продавец";
    
    
    var seller = users.Find(u => u.Id == body.SellerId);
    if (seller == null) return "Пользователь не найден";
    if (seller is Customer) return "Пользователь - клиент";
    var sellerOrder = seller as Seller;
    var good = sellerOrder.goods.Find(g => g.Name == body.Good.Name);
    if (good == null) return "товар не существует";
    
    
    orders.Add(new Order()
    {
        customerOrder = customer as Customer,
        sellerOrder = seller as Seller,
        State = Order.Status.Stock,
        Name = body.Good
    });
    return "Ок";
});

app.MapGet("/users", () =>
{
    return users.Select(u => new UserViewModel()
    {
        Id = u.Id,
        Login = u.Login,
        Type = u is Customer ? "Клиент" : "Продавец"
    });
});

app.MapGet("/getSeller", ([FromQuery] Guid id) =>
{
    var user = users.Find(u => u.Id == id);
    if (user == null) return null;
    if (user is Customer) return null;
    return user as Seller;
});

app.Run();

class User
{
    public Guid Id { get; set; }
    public string Login { get; set; }
    public byte[] Password { get; set; }

  
}

class RegisterCustomer
{
    public string Login { get; set; }
    public string Password { get; set; }
    public string Phone { get; set; }
    public string AddressPost { get; set; }
}

class RegisterSeller
{
    public string Login { get; set; }
    public string Password { get; set; }
    public string Info { get; set; }
    public string AddressGive { get; set; }
}

class LoginBody
{
    public string Login { get; set; }
    public string Password { get; set; }
}

class Customer : User {
    public string Phone { get; set; }
    public string AddressPost { get; set; }
}

class Seller : User {
    public string Info { get; set; }
    public string AddressGive { get; set; }
    public List<Goods> goods { get; set; } = new List<Goods>();
}

public record Goods(string Name, double Price);

class CreateGood
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public double Price { get; set; }
}

record UserViewModel
{
    public Guid Id { get; set; }
    public string Login { get; set; }
    public string Type { get; set; }
}

class NewOrder
{
    public Guid CustomerId { get; set; }
    public Guid SellerId { get; set; }
    public Goods Good { get; set; }
}

 class Order
{
    public Customer customerOrder { get; set; }
    public Seller sellerOrder { get; set; }
    public Status State { get; set; }
    public Goods Name { get; set; }
    
    public enum Status
    {
        Stock,
        Transit,
        Delivered
    }
}