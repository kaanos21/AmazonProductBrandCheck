using AmazonProductBrandCheck.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // Swagger için gerekli
builder.Services.AddSwaggerGen();            // Swagger için gerekli

// HttpClient ile AlProductBrandAnalysis servisini ekle
builder.Services.AddHttpClient<IAlProductBrandAnalysis, AlProductBrandAnalysis>();

// Excel okuma servisini ekle
builder.Services.AddScoped<IReadProductsFromExcel, ReadProductsFromExcel>();

var app = builder.Build();

// Swagger dev ortamýnda aktif
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
