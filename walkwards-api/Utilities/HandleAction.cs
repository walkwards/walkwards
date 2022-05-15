using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using walkwards_api.UserManager;
using walkwards_api.structure;


namespace walkwards_api.Utilities
{
    public class HandleAction : ControllerBase
    {
        public class Arg
        {
            public Arg(string name, object value)
            {
                this.Name = name;

                this.Value = value is string ? value.ToString()!.Replace(" ", "") : value;
            }

            public string Name { get; }
            public object Value { get; }
        }

        private readonly IWebHostEnvironment _config;

        public HandleAction(IWebHostEnvironment config, dynamic response)
        {
            _config = config;
            Response = response;
        }

        public new dynamic Response;

        public ObjectResult ExceptionStringHandler(Exception ex)
        {
            return StatusCode(500, _config.IsDevelopment() ? ex.Message : "please contact support");
        }

        public async Task SetResponse(Arg[] args, bool reqToken, Actions action)
        {
            try
            {
                Dictionary<string, object> data = args.ToDictionary(item => item.Name, item => item.Value);

                if (reqToken)
                {
                    if (!data.ContainsKey("id") || !data.ContainsKey("token"))
                    {
                        Response = BadRequest();
                        return;
                    }
                    else
                    {
                        var userInstance = await UserMethod.GetUserData((int)data["id"], false);
                        var isTokenValid = await userInstance.CheckToken((string)data["token"]);

                        if (!isTokenValid)
                        {
                            Response = StatusCode(409, "InvalidToken");
                            return;
                        }
                    }
                }

                var task = Task.Run(async () => await ActionHandler.GetAction(action, data));

                //TimeoutException if too long (10 sec)
                await task.WaitAsync(TimeSpan.FromSeconds(999));
                Response = await task;
            }
            catch (TimeoutException)
            {
                Response = StatusCode(410, "TimeoutException");
            }
            catch (CustomError ce)
            {
                Response = StatusCode(ce.StatusCode, ce.Name);
            }
            catch (Exception ex)
            {
                if(ex.GetType() == typeof(AggregateException))
                {
                    if(ex.InnerException is CustomError cr)
                    {
                        Response = StatusCode(cr.StatusCode, cr.Name);
                        return;
                    }         
                }

                Response = ExceptionStringHandler(ex);
            }
        }
        
        public async Task SetResponse(JObject json, string reqArgs ,bool reqToken, Actions action)
        {
            Dictionary<string, object> formData = 
                json.ToObject<Dictionary<string, object>>() ?? 
                throw new InvalidOperationException("data form json was null");
            
            List <Arg> args = new();
            bool error = false;

            foreach (string arg in reqArgs.Replace(" ", "").Split(","))
            {
                if (!formData.ContainsKey(arg))
                {
                    Response = BadRequest();
                    error = true;
                    break;
                }

                try
                {
                    args.Add(new Arg(arg, (int) (long) formData[arg]));
                }
                catch
                {
                    args.Add(new Arg(arg, formData[arg]));   
                }
            }

            if (!error)
            {
                await SetResponse(args.ToArray(), reqToken, action);
            }
        }
    }
}

