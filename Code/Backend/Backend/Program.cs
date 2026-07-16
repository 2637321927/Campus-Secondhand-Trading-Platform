using System.Text;
using Backend.Data;
using Backend.Repositories;
using Backend.Services;
using Backend.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

//数据库
// 将数据库上下文AppDbContext注册到DI容器，配置Oracle数据库连接
builder.Services.AddDbContext<AppDbContext>(options =>
   // 指定使用Oracle数据库驱动，从配置文件读取名为DefaultConnection的数据库连接字符串
    options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")));
//注册之后在收到HTTP请求时，ASP.NET Core会自动创建这些实例，并将其注入到需要它们的Service或Controller中，从而实现依赖注入和解耦。   
//AddScoped 一次HTTP请求共用一个实例
//AddTransient 每次获取都创建一个新的实例
//AddSingleton 整个应用程序生命周期共用一个实例
//Repository层注册
builder.Services.AddScoped<IBaseUserRepository, BaseUserRepository>();
builder.Services.AddScoped<INormUserRepository, NormUserRepository>();
builder.Services.AddScoped<IAdminUserRepository, AdminUserRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProdImageRepository, ProdImageRepository>();
builder.Services.AddScoped<ICollectionRepository, CollectionRepository>();
builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IRevImageRepository, RevImageRepository>();
builder.Services.AddScoped<IRefundRepository, RefundRepository>();
builder.Services.AddScoped<IRefundReviewRepository, RefundReviewRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
builder.Services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
builder.Services.AddScoped<ISysInfoRepository, SysInfoRepository>();
builder.Services.AddScoped<IUpdatedFileRepository, UpdatedFileRepository>();
builder.Services.AddScoped<IProductViewRepository, ProductViewRepository>();
//Service层注册
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IBaseUserService, BaseUserService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IUpdatedFileService, UpdatedFileService>();
builder.Services.AddScoped<IProdImageService, ProdImageService>();
builder.Services.AddScoped<ICollectionService, CollectionService>();

//JWT认证配置
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

//基础服务
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// CORS 跨域配置
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

builder.Services.AddSwaggerGen(options =>
{
    // 在 Swagger 右上角添加 JWT Authorize 按钮
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "请输入 JWT Token（不需要加 Bearer 前缀）"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("AllowFrontend");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
