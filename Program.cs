using apbd_OM_kolos.Services;

var builder = WebApplication.CreateBuilder(args);
 
builder.Services.AddControllers();
builder.Services.AddScoped<IDbService, DbService>();
 
var app = builder.Build();
 
// Configure the HTTP request pipeline
app.UseHttpsRedirection();
app.MapControllers();
 
app.Run();