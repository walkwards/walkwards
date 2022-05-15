using System;

namespace walkwards_api.Utilities
{
    [Serializable]
    public class CustomError : Exception
    {
        public string Name = string.Empty;
        public new string Message = string.Empty;
        public int StatusCode = 409;

        public CustomError(string Name, int statusCode = 409, string Message = "")
        {
            this.StatusCode = statusCode;
            this.Name = Name;
            this.Message = Message;
        }
    }
}
