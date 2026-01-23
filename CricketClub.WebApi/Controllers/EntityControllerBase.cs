#nullable disable
using System.Collections.Specialized;
using System.Text.Json;
using System.Text.RegularExpressions;
using CricketClubDAL;
using Microsoft.AspNetCore.Mvc;

namespace CricketClub.WebApi.Controllers
{
    [ApiController]
    public abstract class EntityControllerBase<T> : ControllerBase
    {
        protected readonly IDao database;

        protected EntityControllerBase(IDao database)
        {
            this.database = database;
        }

        [NonAction]
        public override void ProcessRequest(IHandlerContext context)
        {
            switch (context.Request.HttpMethod)
            {
                case "GET":
                    DoGet(context);
                    break;
                case "POST":
                    DoPost(context);
                    break;
                case "DELETE":
                    DoDelete(context);
                    break;
                case "PUT":
                    DoPut(context);
                    break;
                default:
                    context.Response.StatusCode = 405;
                    break;
            }
        }

        private void DoPut(IHandlerContext context)
        {
            var tee = DeserializeRequestBody(context);
            var updatedEntity = UpdateEntity(tee);
            context.Response.ContentType = "application/json";
            context.Response.Write(JsonSerializer.Serialize(updatedEntity));
            context.Response.StatusCode = 200;
            context.Response.End();
        }

        protected abstract T UpdateEntity(T entity);

        private void DoDelete(IHandlerContext context)
        {
            ExtractSingleIdOr(context, idFound => DoDeleteSingle(idFound, context),
                () =>
                {
                    context.Response.ContentType = "text/plain";
                    context.Response.Write("ID not specified for delete");
                    context.Response.StatusCode = 400;
                    context.Response.End();
                });
        }

        private void DoDeleteSingle(int id, IHandlerContext context)
        {
            DeleteEntity(id);
            context.Response.StatusCode = 204;
            context.Response.End();
        }

        protected abstract void DeleteEntity(int id);

        private void DoPost(IHandlerContext context)
        {
            var entity = CreateEntity(DeserializeRequestBody(context));
            context.Response.ContentType = "application/json";
            context.Response.Write(JsonSerializer.Serialize(entity));
            context.Response.StatusCode = 201;
            context.Response.End();
        }

        protected abstract T CreateEntity(T deserializeRequestBody);

        private void DoGet(IHandlerContext context)
        {
            ExtractSingleIdOr(context,
                idFound => GetSingle(idFound, context),
                () => GetAll(context));
        }

        private void ExtractSingleIdOr(IHandlerContext context, Action<int> idFoundAction, Action orElse)
        {
            var matchCollection = Regex.Matches(context.Request.Url.ToString(), "/"+GetTypeName()+"/*([0-9]+)/*");
            if (matchCollection.Count == 1)
            {
                var matchId = int.Parse(matchCollection[0].Groups[1].Value);
                idFoundAction.Invoke(matchId);
            }
            else
            {
                orElse.Invoke();
            }
        }

        private void GetAll(IHandlerContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.Write(JsonSerializer.Serialize(GetAllEntities(context.Request.QueryString)));
            context.Response.StatusCode = 200;
            context.Response.End();
        }

        protected abstract List<T> GetAllEntities(NameValueCollection requestQueryString);

        private void GetSingle(int matchId, IHandlerContext context)
        {
            context.Response.ContentType = "application/json";
            var entity = GetEntity(matchId);
            if (entity == null)
            {
                context.Response.StatusCode = 404;
                context.Response.End();
                return;
            }
            context.Response.Write(JsonSerializer.Serialize(entity));
            context.Response.StatusCode = 200;
            context.Response.End();
        }

        private T DeserializeRequestBody(IHandlerContext context)
        {
            var stringReader = new StreamReader(context.Request.InputStream);
            string body = stringReader.ReadToEnd();
            return JsonSerializer.Deserialize<T>(body);
        }

        protected abstract T GetEntity(int id);

        [NonAction]
        public abstract string GetTypeName();
    }
}
