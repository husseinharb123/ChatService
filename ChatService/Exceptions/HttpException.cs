namespace ChatService.Web.Exceptions
{
    public class HttpException : Exception
    {
        public int StatusCode { get; private set; }

        public HttpException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public void Rethrow()
        {
            throw new HttpException(Message, StatusCode);
        }
    }

}
