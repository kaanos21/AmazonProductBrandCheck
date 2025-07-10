using AmazonProductBrandCheck.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // Swagger i�in gerekli
builder.Services.AddSwaggerGen();            // Swagger i�in gerekli

// HttpClient ile AlProductBrandAnalysis servisini ekle
builder.Services.AddHttpClient<IAlProductBrandAnalysis, AlProductBrandAnalysis>();

// Excel okuma servisini ekle
builder.Services.AddScoped<IReadProductsFromExcel, ReadProductsFromExcel>();

var app = builder.Build();

// Swagger dev ortam�nda aktif
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
