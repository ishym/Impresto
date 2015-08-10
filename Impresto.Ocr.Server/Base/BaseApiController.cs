using Impresto.Ocr.Server.Singletones;
using System;
using System.Web.Http;
using System.Web.Http.Results;

namespace Impresto.Ocr.Server.Base
{
    public class BaseApiController : ApiController
    {
        protected override ExceptionResult InternalServerError(Exception exception)
        {
            Logger.Instance.Fatal("InternalServerError", exception);

            return base.InternalServerError(exception);
        }
        protected override BadRequestErrorMessageResult BadRequest(string message)
        {
            Logger.Instance.InfoFormat("BadRequest: {0}", message);

            return base.BadRequest(message);
        }
    }
}