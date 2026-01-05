using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

var builder = WebApplication.CreateBuilder(args);

// הגדרת מסד נתונים
var connectionString = builder.Configuration.GetConnectionString("ToDoDB"); 
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0))));

// הגדרת JWT
var secretKey = "ThisIsAStrongPasswordForMyJwtToken123456!";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCors(options => options.AddPolicy("AllowAll", p => 
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();



// התחברות
app.MapPost("/api/login", async (User loginUser, ToDoDbContext db) => {
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == loginUser.Username && u.Password == loginUser.Password);
    if (user == null) return Results.Unauthorized();

    var claims = new[] { 
        new Claim(ClaimTypes.Name, user.Username), 
        new Claim("Id", user.Id.ToString()) 
    };

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddDays(30),
        signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
    );

    return Results.Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
});

// הרשמה 
app.MapPost("/api/register", async (User newUser, ToDoDbContext db) => {
    var exists = await db.Users.AnyAsync(u => u.Username == newUser.Username);
    if (exists) 
    {
        return Results.BadRequest(new { message = "שם משתמש זה כבר תפוס" });
    }

    db.Users.Add(newUser);
    await db.SaveChangesAsync();
    return Results.Ok(new { message = "נרשמת בהצלחה" });
});
// שליפת משימות 
app.MapGet("/api/items", async (ToDoDbContext db, ClaimsPrincipal user) =>
{
    var userIdClaim = user.FindFirst("Id")?.Value;
    if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId)) 
        return Results.Unauthorized();
    
    var items = await db.Items.Where(i => i.UserId == userId).ToListAsync();
    return Results.Ok(items);
}).RequireAuthorization();

// הוספת משימה
app.MapPost("/api/items", async (Item inputItem, ToDoDbContext db, ClaimsPrincipal user) =>
{
    var userIdClaim = user.FindFirst("Id")?.Value;
    if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId)) 
        return Results.Unauthorized();

    var newTask = new Item
    {
        Name = inputItem.Name,
        IsComplete = inputItem.IsComplete ?? false,
        UserId = userId 
    };

    db.Items.Add(newTask);
    await db.SaveChangesAsync();
    
    return Results.Created($"/api/items/{newTask.Id}", newTask);
}).RequireAuthorization();

// עדכון משימה
app.MapPut("/api/items/{id}", async (int id, Item inputItem, ToDoDbContext db, ClaimsPrincipal user) => 
{
    var userIdClaim = user.FindFirst("Id")?.Value;
    if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId)) 
        return Results.Unauthorized();

    var todo = await db.Items.FindAsync(id);
    if (todo == null) return Results.NotFound();
    if (todo.UserId != userId) return Results.Forbid();

    todo.Name = inputItem.Name;
    todo.IsComplete = inputItem.IsComplete;
    
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

// מחיקת משימה
app.MapDelete("/api/items/{id}", async (int id, ToDoDbContext db, ClaimsPrincipal user) => 
{
    var userIdClaim = user.FindFirst("Id")?.Value;
    if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId)) 
        return Results.Unauthorized();

    var todo = await db.Items.FindAsync(id);
    if (todo == null) return Results.NotFound();
    if (todo.UserId != userId) return Results.Forbid();

    db.Items.Remove(todo);
    await db.SaveChangesAsync();
    return Results.Ok(new { Message = "Deleted successfully" });
}).RequireAuthorization();

app.Run();