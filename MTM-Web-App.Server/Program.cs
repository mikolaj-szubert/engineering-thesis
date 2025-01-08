using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

using MTM_Web_App.Server.Data;
using MTM_Web_App.Server.Helpers;
using MTM_Web_App.Server.Models;

using MTM_Web_App.Server.Services;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace MTM_Web_App.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddLocalization(o => o.ResourcesPath = "Resources");

            var supportedCultures = new[] { "en-US", "pl" };
            var localizationOptions = new RequestLocalizationOptions().SetDefaultCulture(supportedCultures[0])
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);

            builder.Services.AddControllers();
            builder.Services.AddControllers().AddJsonOptions(opts =>
            {
                var enumConverter = new JsonStringEnumConverter();
                opts.JsonSerializerOptions.Converters.Add(enumConverter);
            });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.AddTransient<IEmailSender, NotificationService>();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "MTM API",
                    Version = "v1",
                    Description = "API created to manage backend of MTM Project."
                });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                c.OperationFilter<AcceptLanguageHeaderFilter>();
                c.MapType<Restaurant>(() => new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>
                    {
                        ["name"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("Pizza Hut") },
                        ["prize"] = new OpenApiSchema { Type = "number", Format = "double", Example = new OpenApiDouble(20.99) },
                        ["currency"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("USD") },
                        ["cusines"] = new OpenApiSchema { Type = "string", Example = new OpenApiArray { new OpenApiString("Polish"), new OpenApiString("Italian") } },
                        ["openDays"] = new OpenApiSchema
                        {
                            Type = "array",
                            Items = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    ["openingTime"] = new OpenApiSchema { Type = "string" },
                                    ["closingTime"] = new OpenApiSchema { Type = "string" },
                                    ["dayOfWeek"] = new OpenApiSchema { Type = "string" }
                                }
                            },
                            Example = new OpenApiArray
                            {
                                new OpenApiObject
                                {
                                    ["openingTime"] = new OpenApiString("10:00"),
                                    ["closingTime"] = new OpenApiString("23:00"),
                                    ["dayOfWeek"] = new OpenApiString("Monday")
                                },
                                new OpenApiObject
                                {
                                    ["openingTime"] = new OpenApiString("10:00"),
                                    ["closingTime"] = new OpenApiString("23:00"),
                                    ["dayOfWeek"] = new OpenApiString("Tuesday")
                                },
                                new OpenApiObject
                                {
                                    ["openingTime"] = new OpenApiString("10:00"),
                                    ["closingTime"] = new OpenApiString("23:00"),
                                    ["dayOfWeek"] = new OpenApiString("Wednesday")
                                },
                                new OpenApiObject
                                {
                                    ["openingTime"] = new OpenApiString("10:00"),
                                    ["closingTime"] = new OpenApiString("23:00"),
                                    ["dayOfWeek"] = new OpenApiString("Thursday")
                                },
                                new OpenApiObject
                                {
                                    ["openingTime"] = new OpenApiString("10:00"),
                                    ["closingTime"] = new OpenApiString("24:00"),
                                    ["dayOfWeek"] = new OpenApiString("Friday")
                                },
                                new OpenApiObject
                                {
                                    ["openingTime"] = new OpenApiString("10:00"),
                                    ["closingTime"] = new OpenApiString("24:00"),
                                    ["dayOfWeek"] = new OpenApiString("Saturday")
                                },
                                new OpenApiObject
                                {
                                    ["openingTime"] = new OpenApiString("10:00"),
                                    ["closingTime"] = new OpenApiString("23:00"),
                                    ["dayOfWeek"] = new OpenApiString("Sunday")
                                }
                            }
                        },
                        ["addressRoad"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("South Pulaski Road") },
                        ["addressHouseNumber"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("4350") },
                        ["addressCity"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("Chicago") },
                        ["addressState"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("Illinois") },
                        ["addressStateCode"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("il") },
                        ["addressPostalCode"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("60632") },
                        ["addressCountry"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("United States of America") },
                        ["description"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("Located 160 metres’ walk from the beach, this Californian Haze hotel features an outdoor swimming pool and sun terrace. The hotel is 9 minutes’ drive from the shops and restaurants of Lincoln Road and offers free WiFi to guests.") },
                        ["lat"] = new OpenApiSchema { Type = "string", Example = new OpenApiDouble(41.8138261) },
                        ["lon"] = new OpenApiSchema { Type = "string", Example = new OpenApiDouble(-87.7242681) },
                        ["imagesSrc"] = new OpenApiSchema
                        {
                            Type = "array",
                            Example = new OpenApiArray
                            {
                                new OpenApiString("1b304de9-6c61-4e20-b63a-53d3e1600a3f.jpg"),
                                new OpenApiString("84f62ab4-6830-4502-a5c1-341b7556eaeb.png")
                            }
                        }
                    }
                });
                c.MapType<HReturnCoordinates>(() => new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>
                    {
                        ["displayName"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("Four Seasons Hotel Chicago, 120, East Delaware Place, Magnificent Mile, Chicago, Cook County, Illinois, 60611, USA") },
                        ["lat"] = new OpenApiSchema { Type = "double", Example = new OpenApiDouble(41.899607149999994) },
                        ["lon"] = new OpenApiSchema { Type = "double", Example = new OpenApiDouble(-87.6248798647693) },
                        ["houseNumber"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("120") },
                        ["road"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("East Delaware Place") },
                        ["city"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("Chicago") },
                        ["state"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("Illinois") },
                        ["stateCode"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("il") },
                        ["postalCode"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("60611") },
                        ["country"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("United States of America") }
                    }
                });
                c.MapType<RReturnCoordinates>(() => new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>
                    {
                        ["displayName"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("Pizza Hut Delivery, 4350, South Pulaski Road, Archer Heights, Chicago, Cook County, Illinois, 60632, USA") },
                        ["lat"] = new OpenApiSchema { Type = "double", Example = new OpenApiDouble(41.8138261) },
                        ["lon"] = new OpenApiSchema { Type = "double", Example = new OpenApiDouble(-87.7242681) },
                        ["houseNumber"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("4350") },
                        ["road"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("South Pulaski Road") },
                        ["city"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("Chicago") },
                        ["state"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("Illinois") },
                        ["stateCode"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("il") },
                        ["postalCode"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("60632") },
                        ["country"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("United States of America") }
                    }
                });
                c.MapType<Hotel>(() => new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>
                    {
                        ["name"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("California Haze") },
                        ["prize"] = new OpenApiSchema { Type = "number", Format = "double", Example = new OpenApiDouble(20.99) },
                        ["currency"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("USD") },
                        ["cusines"] = new OpenApiSchema { Type = "string", Example = new OpenApiArray { new OpenApiString("Polish"), new OpenApiString("Italian") } },
                        ["openDays"] = new OpenApiSchema
                        {
                            Type = "array",
                            Items = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    ["openingTime"] = new OpenApiSchema { Type = "string" },
                                    ["closingTime"] = new OpenApiSchema { Type = "string" },
                                    ["dayOfWeek"] = new OpenApiSchema { Type = "string" }
                                }
                            },
                            Example = new OpenApiArray
                            {
                                new OpenApiObject
                                {
                                    ["openingTime"] = new OpenApiString("10:00"),
                                    ["closingTime"] = new OpenApiString("23:00"),
                                    ["dayOfWeek"] = new OpenApiString("Monday")
                                },
                                new OpenApiObject
                                {
                                    ["openingTime"] = new OpenApiString("10:00"),
                                    ["closingTime"] = new OpenApiString("23:00"),
                                    ["dayOfWeek"] = new OpenApiString("Tuesday")
                                },
                                new OpenApiObject
                                {
                                    ["openingTime"] = new OpenApiString("10:00"),
                                    ["closingTime"] = new OpenApiString("23:00"),
                                    ["dayOfWeek"] = new OpenApiString("Wednesday")
                                },
                                new OpenApiObject
                                {
                                    ["openingTime"] = new OpenApiString("10:00"),
                                    ["closingTime"] = new OpenApiString("23:00"),
                                    ["dayOfWeek"] = new OpenApiString("Thursday")
                                },
                                new OpenApiObject
                                {
                                    ["openingTime"] = new OpenApiString("10:00"),
                                    ["closingTime"] = new OpenApiString("24:00"),
                                    ["dayOfWeek"] = new OpenApiString("Friday")
                                },
                                new OpenApiObject
                                {
                                    ["openingTime"] = new OpenApiString("10:00"),
                                    ["closingTime"] = new OpenApiString("24:00"),
                                    ["dayOfWeek"] = new OpenApiString("Saturday")
                                },
                                new OpenApiObject
                                {
                                    ["openingTime"] = new OpenApiString("10:00"),
                                    ["closingTime"] = new OpenApiString("23:00"),
                                    ["dayOfWeek"] = new OpenApiString("Sunday")
                                }
                            }
                        },
                        ["addressRoad"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("South Pulaski Road") },
                        ["addressHouseNumber"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("4350") },
                        ["addressCity"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("Chicago") },
                        ["addressState"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("Illinois") },
                        ["addressStateCode"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("il") },
                        ["addressPostalCode"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("60632") },
                        ["addressCountry"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("United States of America") },
                        ["description"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("Located 160 metres’ walk from the beach, this Californian Haze hotel features an outdoor swimming pool and sun terrace. The hotel is 9 minutes’ drive from the shops and restaurants of Lincoln Road and offers free WiFi to guests.") },
                        ["lat"] = new OpenApiSchema { Type = "double", Example = new OpenApiDouble(41.8138261) },
                        ["lon"] = new OpenApiSchema { Type = "double", Example = new OpenApiDouble(-87.7242681) },
                        ["imagesSrc"] = new OpenApiSchema
                        {
                            Type = "array",
                            Example = new OpenApiArray
                            {
                                new OpenApiString("1b304de9-6c61-4e20-b63a-53d3e1600a3f.jpg"),
                                new OpenApiString("84f62ab4-6830-4502-a5c1-341b7556eaeb.png")
                            }
                        }
                    }
                });
                c.MapType<String200>(() => new OpenApiSchema { Type = "string", Example = new OpenApiString("Success!") });
                c.MapType<String400>(() => new OpenApiSchema { Type = "string", Example = new OpenApiString("Invalid request.") });
                c.MapType<String401>(() => new OpenApiSchema { Type = "string", Example = new OpenApiString("Unauthorized.") });
                c.MapType<String404>(() => new OpenApiSchema { Type = "string", Example = new OpenApiString("Not found.") });
                c.MapType<String409>(() => new OpenApiSchema { Type = "string", Example = new OpenApiString("Conflict.") });
                c.MapType<String500>(() => new OpenApiSchema { Type = "string", Example = new OpenApiString("Internal server error.") });
                c.MapType<Token>(() => new OpenApiSchema { Type = "string", Example = new OpenApiString("eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWUsImlhdCI6MTUxNjIzOTAyMn0.VFb0qJ1LRg_4ujbZoRMXnVkUgiuKq5KxWqNdbKq_G9Vvz-S1zZa9LPxtHWKa64zDl2ofkT8F6jBt_K4riU-fPg") });
                c.MapType<RestaurantRes>(() => new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>
                    {
                        ["startTime"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("18:00") },
                        ["endTime"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("22:00") },
                        ["restaurantName"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("Pizza Hut") },
                        ["peopleCount"] = new OpenApiSchema { Type = "integer", Example = new OpenApiInteger(3) },
                        ["summaryCost"] = new OpenApiSchema { Type = "float", Example = new OpenApiFloat(321) },
                        ["currency"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("PLN") }
                    }
                });
                c.MapType<HotelRes>(() => new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>
                    {
                        ["startDate"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("18:00") },
                        ["endDate"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("22:00") },
                        ["hotelName"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("California Haze") },
                        ["peopleCount"] = new OpenApiSchema { Type = "integer", Example = new OpenApiInteger(3) },
                        ["summaryCost"] = new OpenApiSchema { Type = "float", Example = new OpenApiFloat(321) },
                        ["currency"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("PLN") }
                    }
                });
            });
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });
            builder.Services.AddHttpClient();
            builder.Services.AddDbContext<MTM_Web_AppServerContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddAuthentication();

            string? secretKey = builder.Configuration["JWT:SecretKey"];
            if (string.IsNullOrEmpty(secretKey) || secretKey == "your key here") throw new Exception("Secret key for JWT is not configured.");
            if (secretKey.Length < 65) throw new Exception("Secret key must be longer than 65 signs..");
            builder.Services.AddHostedService<NotificationService>();

            string? geocodingKey = builder.Configuration["Geocoding:Key"];
            if (string.IsNullOrEmpty(geocodingKey) || geocodingKey == "your key here") throw new Exception("Geocoding key is not configured.");

            string? senderEmail = builder.Configuration["EmailSettings:SenderEmail"];
            if (string.IsNullOrEmpty(senderEmail) || senderEmail == "your email here") throw new Exception("Sender email is not configured.");

            string? senderPassword = builder.Configuration["EmailSettings:SenderPassword"];
            if (string.IsNullOrEmpty(senderPassword) || senderPassword == "your password here") throw new Exception("Sender password is not configured.");

            byte[] key = Encoding.UTF8.GetBytes(secretKey);
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Audience = "MTM";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = "http://localhost:5105",
                        ValidAudience = "MTM",
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            context.Token = context.Request.Cookies["jwtToken"];
                            return Task.CompletedTask;
                        }
                    };
                });


            var app = builder.Build();

            app.UseRequestLocalization(localizationOptions);

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MTM API V1");
                    c.RoutePrefix = "api";
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
    internal class String200;
    internal class String400;
    internal class String401;
    internal class String404;
    internal class String409;
    internal class String500;
    internal class HReturnCoordinates;
    internal class RReturnCoordinates;
    internal class Token;
    public class Resource;
    public class AcceptLanguageHeaderFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= [];
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Accept-Language",
                In = ParameterLocation.Header,
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Default = new OpenApiString("en-US")
                },
                Description = "Język odpowiedzi (np. en-US, pl)"
            });
        }
    }
}