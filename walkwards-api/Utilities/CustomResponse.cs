namespace walkwards_api.Utilities
{
    public class CustomResponse
    {
        public CustomResponse(string type, object content)
        {
            this.type = type;
            this.content = content;
        }

        public string type { get; set; }
        public object content { get; set; }
    }
}
