﻿using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Abp.App.WebAPI.Providers
{
    /// <summary>
    /// Client Credentials 授权
    /// </summary>
    public class ClientAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        /*
         private OAuth2ClientService _oauthClientService;
         public ClientAuthorizationServerProvider()
         {
             this.OAuth2ClientService = new OAuth2ClientService();
         }
        */

        /// <summary>
        /// 验证Client Credentials[client_id与client_secret]
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            //http://localhost:48339/token
            //grant_type=client_credentials&client_id=irving&client_secret=123456&scope=user order
            /*
            grant_type     授与方式（固定为 “client_credentials”）
            client_id 	   分配的调用oauth的应用端ID
            client_secret  分配的调用oaut的应用端Secret
            scope 	       授权权限。以空格分隔的权限列表，若不传递此参数，代表请求用户的默认权限
            */
            string client_id;
            string client_secret;
            context.TryGetFormCredentials(out client_id, out client_secret);
            //验证用户名密码
            if (client_id.Equals("irving", StringComparison.OrdinalIgnoreCase) && client_secret.Equals("123456", StringComparison.OrdinalIgnoreCase))
            {
                context.Validated(context.ClientId);
            }
            else
            {
                //Flurl 404 问题
                //context.Response.StatusCode = Convert.ToInt32(HttpStatusCode.OK);
                context.SetError("invalid_client", "client is not valid");
            }
            return base.ValidateClientAuthentication(context);
        }

        /// <summary>
        /// 客户端授权[生成access token]
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task GrantClientCredentials(OAuthGrantClientCredentialsContext context)
        {
            /*
               var client = _oauthClientService.GetClient(context.ClientId);
               claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, client.ClientName));
             */
            //验证权限
            int scopeCount = context.Scope.Count;
            if (scopeCount > 0)
            {
                string name = context.Scope[0].ToString();
            }
            //默认权限
            var claimsIdentity = new ClaimsIdentity(context.Options.AuthenticationType);
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, context.ClientId));
            var props = new AuthenticationProperties(new Dictionary<string, string> {
                            {
                                "name", "irving"
                            },
                            {
                                "age", "25"
                            },
                            {
                            "gender", "male"
                            }
                        });
            var ticket = new AuthenticationTicket(claimsIdentity, props);
            context.Validated(ticket);
            return base.GrantClientCredentials(context);
        }

        /// <summary>
        /// http://stackoverflow.com/questions/26357054/return-more-info-to-the-client-using-oauth-bearer-tokens-generation-and-owin-in
        /// My recommendation is not to add extra claims to the token if not needed, because will increase the size of the token and you will keep sending it with each request. As LeftyX advised add them as properties but make sure you override TokenEndPoint method to get those properties as a response when you obtain the toke successfully, without this end point the properties will not return in the response.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }
            return base.TokenEndpoint(context);
        }
    }
}