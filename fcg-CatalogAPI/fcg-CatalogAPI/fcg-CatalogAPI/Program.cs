using Domain.Enums;
using Infrastructure.Extensions.App;
using Infrastructure.Extensions.Builder;
using Domain.Data.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddInfrastructure(ProjectType.Api);


/*builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DbFcg")));*/

/*builder.Services
    .AddIdentity<User, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<UserDbContext>()
    .AddDefaultTokenProviders();*/

var app = builder.Build();

await app.ConfigurePipeline(ProjectType.Api);

await app.RunAsync();