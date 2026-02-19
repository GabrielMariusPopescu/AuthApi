var builder = WebApplication.CreateBuilder(args);

// DbContext + Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<TokenService>();

// JWT auth
var jwt = builder.Configuration.GetSection("Jwt");
var jwtKey = jwt["Key"]!;
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwt["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwt["Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30),
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.Name
    };
});

//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
//});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MyAuthApi", Version = "v1" });

    // Define the BearerAuth scheme
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Enter 'Bearer' [space] and then your valid JWT token.",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

    // Require the scheme globally (applies to all endpoints unless overridden)
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });

    // Optional: include XML comments if you want method/param descriptions in Swagger
    // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    // c.IncludeXmlComments(xmlPath);
});

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyAuthApi v1");
        c.RoutePrefix = string.Empty; // serve Swagger UI at app root
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed roles on startup
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    string[] roles = new[] { "Admin", "User" };
    foreach (var r in roles)
    {
        if (!await roleManager.RoleExistsAsync(r))
            await roleManager.CreateAsync(new IdentityRole(r));
    }

    // Optional: create a default admin if not exists
    var adminEmail = "admin@example.com";
    var admin = await userManager.FindByEmailAsync(adminEmail);
    if (admin == null)
    {
        admin = new User { FirstName = "Jon", LastName = "Doe", UserName = "admin", Email = adminEmail, EmailConfirmed = true };
        var res = await userManager.CreateAsync(admin, "Admin123!");
        if (res.Succeeded) 
            await userManager.AddToRoleAsync(admin, "Admin");
    }

    var userEmail = "user@example.com";
    var user = await userManager.FindByEmailAsync(userEmail);
    if (user == null)
    {
        user = new User { FirstName = "Jane", LastName = "Smith", UserName = "user", Email = userEmail, EmailConfirmed = true };
        var res = await userManager.CreateAsync(user, "User123!");
        if (res.Succeeded) 
            await userManager.AddToRoleAsync(user, "User");
    }
}

app.Run();