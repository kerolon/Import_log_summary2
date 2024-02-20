using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using api.Attribute;
using Google.Apis.Auth;
using ils.Apps;
using ils.core.Domain.Entities;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;

namespace api
{
    public class Function : ServerlessHub
    {
        private const string NewMessageTarget = "newMessage";
        private const string NewConnectionTarget = "newConnection";


        private readonly IEventService _eventService;
        private readonly AppSettings _appSettings;
        public Function(IEventService eventService, AppSettings appSettings)
        {
            this._eventService = eventService;
            this._appSettings = appSettings;
        }

        [FunctionName("AddData")]
        [OpenApiOperation(operationId: "AddData", tags: new[] {"external" }, Summary ="EventDataの登録", Description ="外部からEventDataの登録を行うAPI",Deprecated =false, Visibility =Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums.OpenApiVisibilityType.Important)]
        [OpenApiParameter(name:"Authorization", In =Microsoft.OpenApi.Models.ParameterLocation.Header, Summary ="認証用ヘッダ",Description ="独自に発行したトークンをBearer (トークン)の形式で設定",Required =true)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(BatchEvent), Required = true, Description = "登録するEventData")]
        [OpenApiResponseWithoutBody(statusCode:HttpStatusCode.NoContent,Description ="リクエスト成功")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest,"text/plain",typeof(string),Description = "authorizationヘッダがない事によるエラー")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "text/plain", typeof(string), Description = "token不正によるエラー")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous,nameof(HttpMethods.Post))] HttpRequest req, IBinder binder, ILogger logger)
        {
            logger.LogInformation($"Post.");

            if(!req.Headers.ContainsKey("Authorization"))
            {
                return new BadRequestObjectResult("authorization header is required");
            }
            string token = req.Headers["Authorization"].ToString().Remove(0, 7); //remove Bearer 
            if (token != _appSettings.AddDataToken)
            {
                return new UnauthorizedObjectResult("Invalid token");
            }

            var signalRMessages = binder.Bind<IAsyncCollector<SignalRMessage>>(new SignalRAttribute
            {
                ConnectionStringSetting = _appSettings.SignalRConnectionStringSetting,
                HubName = _appSettings.SignalRHubName
            });

            var body = await new StreamReader(req.Body).ReadToEndAsync();
            
            var data = DeserializeFromJson<BatchEvent>(body);
            var logs = await _eventService.AddEventAsync(data);
            await signalRMessages.AddAsync(
                    new SignalRMessage
                    {
                        Target = NewMessageTarget,
                        Arguments = new[] { logs },
                    });
            return new NoContentResult();
        }


        [FunctionName("GetData")]
        [OpenApiOperation(operationId: "GetData", tags: new[] { "internal" }, Summary = "Dataの取得", Description = "SignalRクライアントからEventDataの取得を行うAPI", Deprecated = false, Visibility = Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums.OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = Microsoft.OpenApi.Models.ParameterLocation.Header, Summary = "認証用ヘッダ", Description = "SignalRのコネクションを張った際に発行されたトークンをBearer (トークン)の形式で設定", Required = true)]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Description = "リクエスト成功")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "authorizationヘッダがない事によるエラー")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "text/plain", typeof(string), Description = "token不正によるエラー")]
        public async Task<IActionResult> Run2([HttpTrigger(AuthorizationLevel.Anonymous, nameof(HttpMethods.Get))] HttpRequest req, IBinder binder, ILogger logger)
        {
            logger.LogInformation($"Get.");
            if (!req.Headers.ContainsKey("Authorization"))
            {
                return new BadRequestObjectResult("authorization header is required");
            }
            var authheader = req.Headers["Authorization"].ToString();
            if (!authheader.Contains("Bearer "))
            {
                return new BadRequestObjectResult("authorization header is invalid");
            }
            string token = authheader.Remove(0, 7); //remove Bearer 
            (bool isValid, string errMessage) = VerifyJWT(token);
            if (!isValid)
            {
                return new UnauthorizedObjectResult("Invalid token:" + errMessage);
            }

            var signalRMessages = binder.Bind<IAsyncCollector<SignalRMessage>>(new SignalRAttribute
            {
                ConnectionStringSetting = _appSettings.SignalRConnectionStringSetting,
                HubName = _appSettings.SignalRHubName
            });
            var logs = await _eventService.GetEventSnapshotsAsync();
            await signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = NewMessageTarget,
                    Arguments = new[] { logs },
                });
            return new NoContentResult();
        }


        [FunctionName(nameof(GetToken))]
        [OpenApiOperation(operationId: nameof(GetToken), tags: new[] {"signalR"}, Summary ="SignalR用のToken取得" ,Description ="GoogleLoginによって取得したCredentialの正当性を検査し、SignalR用のJWTトークンを生成して返却します")]
        [OpenApiParameter(name: "Authorization",In =Microsoft.OpenApi.Models.ParameterLocation.Header,Required =true,Summary = "GoogleLoginによって取得したCredential", Description = "「Bearer (GoogleLoginによって取得したCredential)」の形式で指定してください")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "authorizationヘッダがない事によるエラー")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "text/plain", typeof(string), Description = "token不正によるエラー")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "text/plain", typeof((string token, string uid)), Description = "生成したtokenとCredentialから取得したuserId")]
        public async Task<IActionResult> GetToken([HttpTrigger(AuthorizationLevel.Anonymous, nameof(HttpMethods.Get))] HttpRequest req, ILogger logger)
        {
            if (!req.Headers.ContainsKey("Authorization"))
            {
                return new BadRequestObjectResult("authorization header is required");
            }
            string token = req.Headers["Authorization"].ToString().Remove(0, 7); //remove Bearer 
            
            var (isValid,userId) = await IsValidGoogleTokenId(token, logger);

            if (!isValid)
            {
                return new UnauthorizedObjectResult("Invalid token");
            }
            var tokenString = GenerateJWT(userId);

            return new OkObjectResult((token:tokenString,uid:userId));
        }

        private (bool isValid,string message) VerifyJWT(string token)
        {
            var secret = _appSettings.TokenSecret;
            try
            {
                var json = JwtBuilder.Create()
                     .WithAlgorithm(new HMACSHA256Algorithm())
                     .WithSecret(secret)
                     .MustVerifySignature()
                     .Decode(token);
            }
            catch (TokenNotYetValidException)
            {
                return (false, "Token is not valid yet");
            }
            catch (TokenExpiredException)
            {
                return (false, "Token has expired");
            }
            catch (SignatureVerificationException)
            {
                return (false , "Token has invalid signature");
            }

            return (true,string.Empty);
        }
        /// <summary>
        /// SignalRで認証に使用するJWTを生成する
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        private string GenerateJWT(string userName)
        {
            var secret = _appSettings.TokenSecret;
            var jwtToken = new JwtBuilder()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(secret)
                .AddClaim("uid", userName)
                .AddClaim("admin", true)
                .Encode();
            return jwtToken;
        }

        /// <summary>
        /// クライアントから送られてきたGoogleLoginのTokenの正当性を確認する
        /// </summary>
        /// <param name="token"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        private async Task<(bool,string)> IsValidGoogleTokenId(string token, ILogger logger)
        {
            try
            {
                var validationSettings = new GoogleJsonWebSignature.ValidationSettings
                { 
                    Audience = new string[] { _appSettings.GoogleLoginClientId }
                };
                GoogleJsonWebSignature.Payload payload =  await GoogleJsonWebSignature.ValidateAsync(token, validationSettings);

                return ((payload != null && payload.HostedDomain == _appSettings.AllowedDomain), payload.Email);
            }
            catch (System.Exception)
            {
                logger.LogError($"invalid token:" + token);
                return (false,"") ;
            }
        }

        [FunctionName("negotiate")]
        [OpenApiOperation(operationId: "negotiate", tags: new[] { "signalR" }, Summary = "SignalR接続開始エンドポイント", Description = "クライアント側でSignalRの処理を開始するとSignalRライブラリがまずこの処理を呼び出します。このエンドポイントを明示的に呼び出す必要はありません。")]
        public SignalRConnectionInfo Negotiate([HttpTrigger(AuthorizationLevel.Anonymous, nameof(HttpMethods.Post))] HttpRequest req)
        {
            return Negotiate(req.Headers["x-ms-signalr-user-id"], GetClaims(req.Headers["Authorization"]));
        }


        [FunctionName(nameof(OnConnected))]
        public async Task OnConnected([SignalRTrigger] InvocationContext invocationContext, ILogger logger)
        {
            var logs = await _eventService.GetEventSnapshotsAsync();
            await Clients.All.SendAsync(NewConnectionTarget, new NewMessage(invocationContext.ConnectionId, logs));
            logger.LogInformation($"{invocationContext.ConnectionId} has connected");
        }
        [SignalRFunctionAuthorize]
        [FunctionName(nameof(Reload))]
        public async Task Reload([SignalRTrigger] InvocationContext invocationContext, ILogger logger)
        {
            var logs = await _eventService.GetEventSnapshotsAsync();
            await Clients.User(invocationContext.UserId).SendAsync(NewMessageTarget, logs);
        }

        [FunctionName(nameof(OnDisconnected))]
        public void OnDisconnected([SignalRTrigger] InvocationContext invocationContext)
        {
        }
        private class NewMessage
        {
            public string ConnectionId { get; }
            public IEnumerable<BatchEvent> Logs { get; }

            public NewMessage(string connectionId, IEnumerable<BatchEvent> logs)
            {
                ConnectionId = connectionId;
                Logs = logs;
            }
        }

        private string SerializeToJson<T>(T target)
        {
            return JsonSerializer.Serialize(target, new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
            });
        }
        private T DeserializeFromJson<T>(string target)
        {
            return JsonSerializer.Deserialize<T>(target, new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
            });
        }
    }
}