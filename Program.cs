using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Linq;
using lemmikkiAPI_esimerkki;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        DBManager dBManager = new DBManager("DataBase");

        app.Use(async (context, next) =>
        {
            Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");
            await next.Invoke();
        });

        app.MapGet("/", () => Results.Redirect("/index.html"));

        app.MapPost("/API/V1/Pets", (Pet pet) =>
        {
            bool success = dBManager.PostLemmikki(pet.name, pet.type, pet.omistaja_name);
            if (!success)
            {
                return Results.BadRequest("Owner not found. Pet could not be added.");
            }
            return Results.Ok("Pet added successfully.");
        });

        app.MapPost("/API/V1/Owners", (Owner owner) =>
        {
            dBManager.PostOmistaja(owner.name, owner.number);
        });

        app.MapGet("/API/V1/Pets/{petName}/Owner/Number", (string petName) =>
        {
            return dBManager.GetOmistajaNumberFromLemmikkiName(petName);
        });

        app.MapGet("/API/V1/Owners", () =>
        {
            return dBManager.GetAllOwners();
        });

        app.MapGet("/API/V1/Pets", () =>
        {
            return dBManager.GetAllPets();
        });

        app.MapPost("/API/V1/Owners/Update", (UpdateOwnerRequest request) =>
        {
            if (request.Id <= 0 || string.IsNullOrEmpty(request.NewNumber))
            {
                return Results.BadRequest("Id and NewNumber are required.");
            }

            dBManager.UpdateOmistajaNumber(request.Id, request.NewNumber);
            return Results.Ok();
        });

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

        app.UseStaticFiles(); // This should be after all route definitions

        app.Run();
    }
}