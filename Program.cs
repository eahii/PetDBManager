using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Linq;
using lemmikkiAPI_esimerkki;

public partial class Program
{
    public static void Main(string[] args)
    {
        // Create a new web application builder
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        // Initialize the database manager with the database name
        DBManager dBManager = new DBManager("DataBase");

        // Middleware to log each request
        app.Use(async (context, next) =>
        {
            Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");
            await next.Invoke();
        });

        // Redirect root URL to the index.html page
        app.MapGet("/", () => Results.Redirect("/index.html"));

        // Endpoint to add a new pet
        app.MapPost("/API/V1/Pets", (Pet pet) =>
        {
            bool success = dBManager.PostLemmikki(pet.name, pet.type, pet.omistaja_name);
            if (!success)
            {
                return Results.BadRequest("Owner not found. Pet could not be added.");
            }
            return Results.Ok("Pet added successfully.");
        });

        // Endpoint to add a new owner
        app.MapPost("/API/V1/Owners", (Owner owner) =>
        {
            dBManager.PostOmistaja(owner.name, owner.number);
        });

        // Endpoint to get the owner's number by pet name
        app.MapGet("/API/V1/Pets/{petName}/Owner/Number", (string petName) =>
        {
            try
            {
                return dBManager.GetOmistajaNumberFromLemmikkiName(petName);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message); // Return error message
            }
        });

        // Endpoint to get all owners
        app.MapGet("/API/V1/Owners", () =>
        {
            return dBManager.GetAllOwners();
        });

        // Endpoint to get all pets
        app.MapGet("/API/V1/Pets", () =>
        {
            return dBManager.GetAllPets();
        });

        // Endpoint to update an owner's number
        app.MapPost("/API/V1/Owners/Update", (UpdateOwnerRequest request) =>
        {
            if (request.Id <= 0 || string.IsNullOrEmpty(request.NewNumber))
            {
                return Results.BadRequest("Id and NewNumber are required.");
            }

            dBManager.UpdateOmistajaNumber(request.Id, request.NewNumber);
            return Results.Ok();
        });

        // Endpoint to update an owner's number using PUT
        app.MapPut("/API/V1/Owners/{id}", (int id, UpdateOwnerRequest request) =>
        {
            if (string.IsNullOrEmpty(request.NewNumber))
            {
                return Results.BadRequest("NewNumber is required.");
            }

            try
            {
                dBManager.UpdateOmistajaNumber(id, request.NewNumber);
                return Results.Ok("Owner number updated successfully.");
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message); // Return error message
            }
        });

        // Endpoint to delete an owner
        app.MapDelete("/API/V1/Owners/{id}", (int id) =>
        {
            try
            {
                dBManager.DeleteOwner(id);
                return Results.Ok("Owner deleted successfully.");
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message); // Return error message
            }
        });

        // Endpoint to delete a pet
        app.MapDelete("/API/V1/Pets/{id}", (int id) =>
        {
            try
            {
                dBManager.DeletePet(id);
                return Results.Ok("Pet deleted successfully.");
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message); // Return error message
            }
        });

        // Serve static files like index.html
        app.UseStaticFiles(); // This should be after all route definitions

        // Run the application
        app.Run();
    }
}