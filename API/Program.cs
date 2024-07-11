using Library.Models;
using Library.Services;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
//using Library.Context;
using System;
using Library.Hub;
using Library.Interfaces;
using RMSAPI;
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<APIAppDBContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString
    ("DefaultConnection")), ServiceLifetime.Scoped);


builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

builder.Services.AddSingleton<MqttService>();
builder.Services.AddSingleton<ErrorViewModel>();
builder.Services.AddSingleton<ObservableDictionary>();


builder.Services.AddSingleton<ErrorLogHub>();
builder.Services.AddScoped<IAppDbContext, APIAppDBContext>();

builder.Services.AddSignalR();
var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();          
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();           //삭제
app.UseAuthorization();     //

app.MapControllers();

app.MapHub<ErrorLogHub>("/errorLogHub"); // SignalR Hub 매핑

var mqttService = app.Services.GetRequiredService<MqttService>();
await mqttService.ConnectAsync();
app.Run();