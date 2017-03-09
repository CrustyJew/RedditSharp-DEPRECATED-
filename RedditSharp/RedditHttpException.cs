using System;
using System.Net;

namespace RedditSharp {

  public class RedditHttpException : Exception {

    public HttpStatusCode StatusCode { get; }

    public RedditHttpException(HttpStatusCode statusCode)
      : base($"The server responded with error {(int)statusCode} ({statusCode})")
    {
      StatusCode = statusCode;
    }

  }

}
