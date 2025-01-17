namespace lemmikkiAPI_esimerkki
{
    // Class to represent a request to update an owner's phone number
    public class UpdateOwnerRequest
    {
        // The ID of the owner to update
        public int Id { get; set; }

        // The new phone number for the owner
        public string NewNumber { get; set; }
    }
}