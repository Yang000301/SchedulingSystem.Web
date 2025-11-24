using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SchedulingSystem.API.Data;
using SchedulingSystem.API.Middlewares;
using SchedulingSystem.API.Models;
using SchedulingSystem.API.Repositories.ScheduleRepos;
using SchedulingSystem.API.Services.Calend;
using SchedulingSystem.API.Services.Calendar;
using SchedulingSystem.API.Services.External;
using SchedulingSystem.API.Services.HolidayInfoServices;
using SchedulingSystem.API.Services.ScheduleServices;
using System.Text;


namespace SchedulingSystem.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            //API Schedule Service & Repository DI
            builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();
            builder.Services.AddScoped<IScheduleService, ScheduleService>();

            // API Calendar Service  DI
            builder.Services.AddScoped<ICalendarService, CalendarService>();

            // Taiwan Calendar Client DI  外部API
            builder.Services.AddHttpClient<ITaiwanCalendarClient, TaiwanCalendarClient>();
            builder.Services.AddScoped<IHolidayService, HolidayService>();

            builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            // ✅✅✅【這一塊就是 Step 3 要加 JWT 的地方】✅✅✅
            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
                        )
                    };
                });
            var app = builder.Build();
            

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            //Exception Middleware
            app.UseMiddleware<GlobalExceptionMiddleware>();
            // ✅ 這兩個順序要長這樣：
            app.UseAuthentication();  // 先驗證 Token
            app.UseAuthorization();   // 再跑授權（[Authorize] 才會生效）
            

            
            app.MapControllers();

            app.Run();
        }
    }
}
