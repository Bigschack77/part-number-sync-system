using Microsoft.Data.SqlClient;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure middleware
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");

// Configuration
var primaryConnectionString = builder.Configuration.GetConnectionString("PrimaryServer");
var secondaryConnectionString = builder.Configuration.GetConnectionString("SecondaryServer");

// API ENDPOINTS

// Check if part number exists (checks BOTH servers)
app.MapGet("/api/partnumbers/check/{partNumber}", async (string partNumber) =>
{
    // Check primary server
    using (var conn = new SqlConnection(primaryConnectionString))
    {
        await conn.OpenAsync();
        using var cmd = new SqlCommand("sp_CheckPartNumberExists", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@PartNumber", partNumber);
        cmd.Parameters.Add("@Exists", SqlDbType.Bit).Direction = ParameterDirection.Output;
        cmd.Parameters.Add("@Location", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;
        
        await cmd.ExecuteNonQueryAsync();
        
        if ((bool)cmd.Parameters["@Exists"].Value)
        {
            return Results.Ok(new 
            { 
                Exists = true, 
                Location = cmd.Parameters["@Location"].Value.ToString(),
                Server = "Primary (Locations 1 & 2)"
            });
        }
    }
    
    // Check secondary server
    using (var conn = new SqlConnection(secondaryConnectionString))
    {
        await conn.OpenAsync();
        using var cmd = new SqlCommand("sp_CheckPartNumberExists", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@PartNumber", partNumber);
        cmd.Parameters.Add("@Exists", SqlDbType.Bit).Direction = ParameterDirection.Output;
        cmd.Parameters.Add("@Location", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;
        
        await cmd.ExecuteNonQueryAsync();
        
        if ((bool)cmd.Parameters["@Exists"].Value)
        {
            return Results.Ok(new 
            { 
                Exists = true, 
                Location = cmd.Parameters["@Location"].Value.ToString(),
                Server = "Secondary (Location 3)"
            });
        }
    }
    
    return Results.Ok(new { Exists = false, Location = "", Server = "" });
});

// Create new part number
app.MapPost("/api/partnumbers", async (PartNumberRequest request) =>
{
    // First check both servers for duplicates
    var checkResult = await CheckBothServers(request.PartNumber, primaryConnectionString, secondaryConnectionString);
    if (checkResult.Exists)
    {
        return Results.BadRequest(new 
        { 
            Success = false, 
            Message = $"Part number already exists at {checkResult.Location} on {checkResult.Server}" 
        });
    }
    
    // Determine which server to use based on location
    var connectionString = (request.Location == "Location 3") 
        ? secondaryConnectionString 
        : primaryConnectionString;
    
    using var conn = new SqlConnection(connectionString);
    await conn.OpenAsync();
    
    using var cmd = new SqlCommand("sp_InsertPartNumber", conn);
    cmd.CommandType = CommandType.StoredProcedure;
    cmd.Parameters.AddWithValue("@PartNumber", request.PartNumber);
    cmd.Parameters.AddWithValue("@Description", request.Description ?? "");
    cmd.Parameters.AddWithValue("@Location", request.Location);
    cmd.Parameters.AddWithValue("@CreatedBy", request.CreatedBy);
    cmd.Parameters.Add("@Result", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;
    cmd.Parameters.Add("@Message", SqlDbType.NVarChar, 500).Direction = ParameterDirection.Output;
    
    await cmd.ExecuteNonQueryAsync();
    
    var result = cmd.Parameters["@Result"].Value.ToString();
    var message = cmd.Parameters["@Message"].Value.ToString();
    
    if (result == "SUCCESS")
    {
        // Replicate to other server
        await ReplicateToOtherServer(request, connectionString == primaryConnectionString 
            ? secondaryConnectionString 
            : primaryConnectionString);
        
        return Results.Ok(new { Success = true, Message = message });
    }
    
    return Results.BadRequest(new { Success = false, Message = message });
});

// Search part numbers (searches both servers)
app.MapGet("/api/partnumbers/search", async (string? searchTerm, string? location) =>
{
    var allResults = new List<PartNumberDto>();
    
    // Search primary server
    using (var conn = new SqlConnection(primaryConnectionString))
    {
        await conn.OpenAsync();
        using var cmd = new SqlCommand("sp_SearchPartNumbers", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@SearchTerm", (object)searchTerm ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Location", (object)location ?? DBNull.Value);
        
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            allResults.Add(new PartNumberDto
            {
                PartNumber = reader.GetString(0),
                Description = reader.IsDBNull(1) ? "" : reader.GetString(1),
                Location = reader.GetString(2),
                CreatedBy = reader.GetString(3),
                CreatedDate = reader.GetDateTime(4),
                LastModified = reader.GetDateTime(5)
            });
        }
    }
    
    // Search secondary server
    using (var conn = new SqlConnection(secondaryConnectionString))
    {
        await conn.OpenAsync();
        using var cmd = new SqlCommand("sp_SearchPartNumbers", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@SearchTerm", (object)searchTerm ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Location", (object)location ?? DBNull.Value);
        
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            allResults.Add(new PartNumberDto
            {
                PartNumber = reader.GetString(0),
                Description = reader.IsDBNull(1) ? "" : reader.GetString(1),
                Location = reader.GetString(2),
                CreatedBy = reader.GetString(3),
                CreatedDate = reader.GetDateTime(4),
                LastModified = reader.GetDateTime(5)
            });
        }
    }
    
    return Results.Ok(allResults.OrderByDescending(x => x.CreatedDate));
});

// Import from Excel (CSV format)
app.MapPost("/api/partnumbers/import", async (ImportRequest request) =>
{
    var results = new List<ImportResult>();
    var successCount = 0;
    var errorCount = 0;
    
    foreach (var item in request.Data)
    {
        try
        {
            // Validate format
            if (item.PartNumber.Length != 8 || !long.TryParse(item.PartNumber, out _))
            {
                results.Add(new ImportResult 
                { 
                    PartNumber = item.PartNumber, 
                    Success = false, 
                    Message = "Invalid format - must be 8 digits" 
                });
                errorCount++;
                continue;
            }
            
            // Check both servers
            var checkResult = await CheckBothServers(item.PartNumber, primaryConnectionString, secondaryConnectionString);
            if (checkResult.Exists)
            {
                results.Add(new ImportResult 
                { 
                    PartNumber = item.PartNumber, 
                    Success = false, 
                    Message = $"Already exists at {checkResult.Location}" 
                });
                errorCount++;
                continue;
            }
            
            // Insert
            var connectionString = (item.Location == "Location 3") 
                ? secondaryConnectionString 
                : primaryConnectionString;
            
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            
            using var cmd = new SqlCommand("sp_InsertPartNumber", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@PartNumber", item.PartNumber);
            cmd.Parameters.AddWithValue("@Description", item.Description ?? "");
            cmd.Parameters.AddWithValue("@Location", item.Location);
            cmd.Parameters.AddWithValue("@CreatedBy", request.ImportedBy);
            cmd.Parameters.Add("@Result", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("@Message", SqlDbType.NVarChar, 500).Direction = ParameterDirection.Output;
            
            await cmd.ExecuteNonQueryAsync();
            
            var result = cmd.Parameters["@Result"].Value.ToString();
            var message = cmd.Parameters["@Message"].Value.ToString();
            
            if (result == "SUCCESS")
            {
                // Replicate to other server
                await ReplicateToOtherServer(new PartNumberRequest
                {
                    PartNumber = item.PartNumber,
                    Description = item.Description,
                    Location = item.Location,
                    CreatedBy = request.ImportedBy
                }, connectionString == primaryConnectionString 
                    ? secondaryConnectionString 
                    : primaryConnectionString);
                
                results.Add(new ImportResult 
                { 
                    PartNumber = item.PartNumber, 
                    Success = true, 
                    Message = "Imported successfully" 
                });
                successCount++;
            }
            else
            {
                results.Add(new ImportResult 
                { 
                    PartNumber = item.PartNumber, 
                    Success = false, 
                    Message = message 
                });
                errorCount++;
            }
        }
        catch (Exception ex)
        {
            results.Add(new ImportResult 
            { 
                PartNumber = item.PartNumber, 
                Success = false, 
                Message = ex.Message 
            });
            errorCount++;
        }
    }
    
    return Results.Ok(new 
    { 
        TotalProcessed = results.Count,
        SuccessCount = successCount,
        ErrorCount = errorCount,
        Details = results
    });
});

app.Run();

// Helper Methods
async Task<(bool Exists, string Location, string Server)> CheckBothServers(
    string partNumber, string primary, string secondary)
{
    // Check primary
    using (var conn = new SqlConnection(primary))
    {
        await conn.OpenAsync();
        using var cmd = new SqlCommand("sp_CheckPartNumberExists", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@PartNumber", partNumber);
        cmd.Parameters.Add("@Exists", SqlDbType.Bit).Direction = ParameterDirection.Output;
        cmd.Parameters.Add("@Location", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;
        
        await cmd.ExecuteNonQueryAsync();
        
        if ((bool)cmd.Parameters["@Exists"].Value)
        {
            return (true, cmd.Parameters["@Location"].Value.ToString(), "Primary");
        }
    }
    
    // Check secondary
    using (var conn = new SqlConnection(secondary))
    {
        await conn.OpenAsync();
        using var cmd = new SqlCommand("sp_CheckPartNumberExists", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@PartNumber", partNumber);
        cmd.Parameters.Add("@Exists", SqlDbType.Bit).Direction = ParameterDirection.Output;
        cmd.Parameters.Add("@Location", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;
        
        await cmd.ExecuteNonQueryAsync();
        
        if ((bool)cmd.Parameters["@Exists"].Value)
        {
            return (true, cmd.Parameters["@Location"].Value.ToString(), "Secondary");
        }
    }
    
    return (false, "", "");
}

async Task ReplicateToOtherServer(PartNumberRequest request, string targetConnectionString)
{
    try
    {
        using var conn = new SqlConnection(targetConnectionString);
        await conn.OpenAsync();
        
        using var cmd = new SqlCommand("sp_InsertPartNumber", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@PartNumber", request.PartNumber);
        cmd.Parameters.AddWithValue("@Description", request.Description ?? "");
        cmd.Parameters.AddWithValue("@Location", request.Location);
        cmd.Parameters.AddWithValue("@CreatedBy", request.CreatedBy + " (Synced)");
        cmd.Parameters.Add("@Result", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;
        cmd.Parameters.Add("@Message", SqlDbType.NVarChar, 500).Direction = ParameterDirection.Output;
        
        await cmd.ExecuteNonQueryAsync();
    }
    catch
    {
        // Log replication failure
    }
}

// Models
record PartNumberRequest(string PartNumber, string Description, string Location, string CreatedBy);
record PartNumberDto
{
    public string PartNumber { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastModified { get; set; }
}
record ImportRequest(List<PartNumberImportItem> Data, string ImportedBy);
record PartNumberImportItem(string PartNumber, string Description, string Location);
record ImportResult
{
    public string PartNumber { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; }
}
