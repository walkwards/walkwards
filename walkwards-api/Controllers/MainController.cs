using System;
using System.Diagnostics;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using walkwards_api.Utilities;
using Newtonsoft.Json.Linq;
using walkwards_api.structure;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using walkwards_api.UserManager;

namespace walkwards_api.Controllers
{
    [Route("apiv1/[controller]")]
    [ApiController]
    [EnableCors]
    public class MainController : ControllerBase
    {
        private readonly IWebHostEnvironment _config;
        IHostApplicationLifetime _lifeTime;

        public MainController(IWebHostEnvironment config, IHostApplicationLifetime lifeTime)
        {
            _config = config;
            _lifeTime = lifeTime;
        }

        //szymusiowe końcówki
        [HttpGet("/")]
        public IActionResult IsBackendOnline()
        {
            return Ok();
        }

        [HttpPost("/Action/test")]
        public async Task<ActionResult<object>> Test()
        {
            //for (int i = 0; i < 200; i++)
            //{
            //    var user = await UserMethod.CreateUser($"testuser{i}", $"testuser{i}@test.rzepa", "Trudnehaslo123!");
            //    await user.SetNewAvatar(
            //        "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");
            //}
            
            
            return Ok("Starting web host new");
        }


        [HttpGet("/Action/EndpointDevelopedDaily")]
        public async Task<ActionResult<object>> EndpointDevelopedDaily()
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                return await Utilities.EndpointDevelopedDaily.DevelopedDaily();
                ;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(99999));

            dynamic data = await task;

            return data;
        }
        
        [HttpPost("/Admin/DropUser")]
        public async Task<ActionResult<object>> DropUser([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id", false, Actions.DropUser);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }        
        //[HttpPost("/Admin/DropData")]
        //public async Task<ActionResult<object>> DropData([FromBody] JObject json)
        //{
        //    Task<object> task = Task.Run(async () =>
        //    {
        //        HandleAction action = new(_config, this);
//
        //        await action.SetResponse(json, "tablename", false, Actions.DropData);
//
        //        return action.Response;
        //    });
//
        //    await task.WaitAsync(TimeSpan.FromSeconds(999));
//
        //    dynamic data = await task;
//
        //    return data;
        //}

        [HttpPost("/User/SendAndroidObject")]
        public async Task<ActionResult<object>> SendAndroidObject([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, obj", false, Actions.SendAndroidObject);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Admin/GetToken")]
        public async Task<ActionResult<object>> GetToken([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id", false, Actions.GetToken);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Admin/EditValueType")]
        public async Task<ActionResult<object>> EditValueType([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "tablename, fieldname, newtype", false, Actions.EditValueType);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Admin/SetValue")]
        public async Task<ActionResult<object>> SetValue([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "tablename, fieldname, value", false, Actions.SetValueInAllUser);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Admin/AddColumnInUserData")]
        public async Task<ActionResult<object>> AddColumnInUserData([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "fieldname, type, tablename", false, Actions.AddColumnInUserData);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Admin/AddTableToUser")]
        public async Task<ActionResult<object>> AddTableToUser([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "tablename, fields", false, Actions.AddTableToUserData);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Admin/DropTableToUser")]
        public async Task<ActionResult<object>> DropTableFromUser([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "tablename", false, Actions.DropTableToUserData);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Action/CreateUser")]
        public async Task<ActionResult<object>> CreateUser([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "login, email, password", false, Actions.CreateUser);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Action/Login")]
        public async Task<ActionResult<object>> Login([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "loginOrEmail, password", false, Actions.Login);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Action/IsUserExist")]
        public async Task<ActionResult<object>> IsUserExist([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token", true, Actions.IsUserExist);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Action/GetRanking")]
        public async Task<ActionResult<object>> GetRanking([FromBody] JObject json)
        {
            Console.WriteLine("test");

            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "page, token, id", true, Actions.GetRanking);

                string stop = "";
                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }


        [HttpPost("/Action/QueryUser")]
        public async Task<ActionResult<object>> Query([FromBody] JObject json)
        {
            Console.WriteLine("test");
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "query, token, id, page", true, Actions.SearchUser);

                string stop = "";
                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Action/GetRankingGlobal")]
        public async Task<ActionResult<object>> GetRankinsgGlobal([FromBody] JObject json)
        {
            Console.WriteLine("test");
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "page, token, id", true, Actions.GetRankingAllSteps);

                string stop = "";
                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpGet("/Action/Activate")]
        public async Task ActivateUser(int id)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(new[]
                {
                    new HandleAction.Arg("id", id),
                }, false, Actions.Activate);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            Response.Redirect("https://walkwards.pl/accountActivated");
        }

        //User Methods

        [HttpPost("/User/ResetPassword")]
        public async Task<ActionResult<object>> ResetPassword([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, pass1, pass2", false, Actions.ResetPassword);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/User/ForgotPassword")]
        public async Task<ActionResult<object>> ForgotPassword([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "loginOrEmail", false, Actions.ForgotPassword);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/User/SetNewAvatar")]
        public async Task<ActionResult<object>> SetNewAvatar([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, newAvatar, token", true, Actions.SetNewAvatar);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/User/SetGoal")]
        public async Task<ActionResult<object>> SetGoal([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, goal, token", true, Actions.SetGoal);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/User/SetAccountPrivacyType")]
        public async Task<ActionResult<object>> SetAccountPrivacy([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, privacyType, token", true, Actions.SetAccountPrivacyType);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/User/SetNewEmail")]
        public async Task<ActionResult<object>> SetNewEmail([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, newEmail, token", true, Actions.SetNewEmail);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/User/SetNewLogin")]
        public async Task<ActionResult<object>> SetNewLogin([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, newLogin, token", true, Actions.SetNewLogin);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/User/GetUserData")]
        public async Task<ActionResult<object>> GetUserData([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token", true, Actions.GetUserData);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/User/AddActivity")]
        public async Task<ActionResult<object>> AddActivity([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, steps", true, Actions.AddActivity);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/User/AddActivityOnDate")]
        public async Task<ActionResult<object>> AddActivityForDate([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, steps, date", true, Actions.AddActivityForDate);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/User/AddActivityFromArray")]
        public async Task<ActionResult<object>> AddActivityFromArray([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, content", true, Actions.AddActivityFromArray);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }


        [HttpPost("/User/GetAllActivity")]
        public async Task<ActionResult<object>> GetAllActivity([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id", false, Actions.GetAllActivity);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/User/GetActivity")]
        public async Task<ActionResult<object>> GetWeeklyActivity([FromBody] JObject json)
        {

            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, dayCount", false, Actions.GetLastWeeklyActivity);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/User/GetTodayActivity")]
        public async Task<ActionResult<object>> GetTodayActivity([FromBody] JObject json)
        {

            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id", false, Actions.GetTodayActivity);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        //Friends Methods

        [HttpPost("/User/Friends/InviteFriend")]
        public async Task<ActionResult<object>> InviteFriend([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, friendId", true, Actions.InviteFriend);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/User/Friends/ReturnFriendship")]
        public async Task<ActionResult<object>> ReturnFriendship([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, friendId", true, Actions.ReturnFriendship);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/User/Friends/AnswerFriendInvite")]
        public async Task<ActionResult<object>> AnswerFriendInvite([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, friendId, accepted", true, Actions.AnswerFriendInvite);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/User/Friends/RemoveFriend")]
        public async Task<ActionResult<object>> RemoveFriend([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, friendId", true, Actions.RemoveFriend);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/User/Friends/GetFriends")]
        public async Task<ActionResult<object>> GetFriend([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, page", true, Actions.GetFriend);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/User/GetSteps")]
        public async Task<ActionResult<object>> GetActivity([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, isGlobal", true, Actions.GetActivity);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/User/Friends/GetFriendRequests")]
        public async Task<ActionResult<object>> GetFriendRequests([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, challengeId, token, accepted", true, Actions.GetFriendsRequest);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        //Challenge -/ 

        [HttpPost("/Challenge/SetNewChallenge")]
        public async Task<ActionResult<object>> SetNewChallenge([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, recipient, betValue, dayCount", true,
                    Actions.SetChallenge);
                //{"sender":683022, "recipient":643500, "startDate":"01.07.2022", "endDate":"01.09.2022", "betValue":100, "token":"DSLEOL"}

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Challenge/GetActiveChallenge")]

        public async Task<ActionResult<object>> GetActiveChallenge([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token", true, Actions.GetActiveChallenge);
                //{"id":683022, "token":"DSLEOL"}

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Challenge/AcceptOrCancelChallengeRequest")]
        public async Task<ActionResult<object>> AcceptOrCancelChallengeRequest([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, challengeId, token, accepted", true,
                    Actions.AcceptOrCancelChallengeRequest);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Challenge/GiveUpChallenge")]
        public async Task<ActionResult<object>> GiveUpChallenge([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, cid, token", true, Actions.GiveUpChallenge);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Challenge/GetActiveBetweenUsersChallenge")]
        public async Task<ActionResult<object>> GetActiveBetweenUsersChallenge([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, recipientId", true, Actions.GetActiveBetweenUsersChallenge);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Challenge/GetFinishedChallenges")]
        public async Task<ActionResult<object>> GetFinishedChallenges([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token", true, Actions.GetFinishedChallenges);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        //competitions

        [HttpPost("/Competitions/GetCompetitionUsers")]
        public async Task<ActionResult<object>> GetCompetitionUsers([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, competitionId", true, Actions.GetCompetitionUsers);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Competitions/CreateCompetition")]
        public async Task<ActionResult<object>> CreateCompetition([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json,
                    "id, token, name, startDate, endDate, isPublic, creator, isOfficial, avatar, description, entranceFee, companyName",
                    true, Actions.CreateCompetition);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Competitions/JoinCompetition")]
        public async Task<ActionResult<object>> JoinCompetition([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, ctoken", true, Actions.JoinCompetition);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Competitions/DropFromCompetition")]
        public async Task<ActionResult<object>> DropFromCompetition([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, cid", true, Actions.DropFromCompetition);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Competitions/EditCompetition")]
        public async Task<ActionResult<object>> EditCompetition([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, competitionId, fieldName, value", true,
                    Actions.EditCompetition);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Competitions/GetCompetitions")]
        public async Task<ActionResult<object>> GetCompetitions([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token", true, Actions.GetCompetitions);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Competitions/GetUserActiveCompetitions")]
        public async Task<ActionResult<object>> GetUserActiveCompetitions([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token", true, Actions.GetUserActiveCompetitions);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Competitions/DropCompetition")]
        public async Task<ActionResult<object>> DropCompetition([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "cid", false, Actions.DropCompetition);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }


        [HttpPost("/Notification/GetNotifications")]
        public async Task<ActionResult<object>> GetNotifications([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, page, token, getAll", true, Actions.GetNotifications);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        //Guilds 

        [HttpPost("/Guilds/GetGuild")]
        public async Task<ActionResult<object>> GetGuild([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, guildId", true, Actions.GetGuild);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Guilds/DeleteGuild")]
        public async Task<ActionResult<object>> DeleteGuild([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, guildId", true, Actions.DeleteGuild);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Guilds/ResponseToInvite")]
        public async Task<ActionResult<object>> ResponseToInvite([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, guildId, accepted", true, Actions.ResponseToInvite);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Guilds/GetGuildInvite")]
        public async Task<ActionResult<object>> GetGuildInvite([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token", true, Actions.GetGuildInvite);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Guilds/DropFromGuild")]
        public async Task<ActionResult<object>> DropFromGuild([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, userId, guildId", true, Actions.DropFromGuild);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Guilds/InviteToGuild")]
        public async Task<ActionResult<object>> InviteToGuild([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, userId, guildId", true, Actions.InviteToGuild);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Guilds/CreateGuild")]
        public async Task<ActionResult<object>> CreateGuild([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, name, avatar, canMembersAddMembers", true, Actions.CreateGuild);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Guilds/GetUserGuild")]
        public async Task<ActionResult<object>> GetUserGuild([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token", true, Actions.GetUserGuild);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Guilds/GetAllGuildsRankingSum")]
        public async Task<ActionResult<object>> GetAllGuildsRankingSum([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, page", true, Actions.GetAllGuildsRankingSum);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Guilds/GetAllGuildsRankingAvg")]
        public async Task<ActionResult<object>> GetAllGuildsRankingAvg([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, page", true, Actions.GetAllGuildsRankingAvg);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }        
        
        [HttpPost("/Guilds/GuildSearch")] 
        public async Task<ActionResult<object>> GuildSearch([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, query, page", true, Actions.GuildSearch);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }
        //
        [HttpPost("/Guilds/GuildJoinRequest")] 
        public async Task<ActionResult<object>> GuildJoinRequest([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, guildId", true, Actions.GuildJoinRequest);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }                
        [HttpPost("/Guilds/EditGuild")] 
        public async Task<ActionResult<object>> EditGuild([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, guildId, field, value", true, Actions.EditGuild);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }        
        
        [HttpPost("/Guilds/GetSentInvite")] 
        public async Task<ActionResult<object>> GetSentInvite([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, guildId", true, Actions.GetSentInvite);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }
        
        [HttpPost("/Guilds/AnswerGuildJoinRequest")] 
        public async Task<ActionResult<object>> AnswerGuildJoinRequest([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, guildId, userId, accepted", true, Actions.AnswerGuildJoinRequest);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }
        
        [HttpPost("/Guilds/GetGuildJoinRequest")] 
        public async Task<ActionResult<object>> GetGuildJoinRequest([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, guildId", true, Actions.GetGuildJoinRequest);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }
        
        [HttpPost("/Guilds/CancelGuildJoinRequest")] 
        public async Task<ActionResult<object>> CancelGuildJoinRequest([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, guildId", true, Actions.CancelGuildJoinRequest);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }        
        [HttpPost("/Guilds/GetUserRelation")] 
        public async Task<ActionResult<object>> GetUserRelation([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, guildId", true, Actions.GetUserRelation);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }


        //shop
        
        [HttpPost("/Shop/GetAllActiveAuction")]
        public async Task<ActionResult<object>> GetAllActiveAuction([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token", true, Actions.GetAllActiveAuction);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Shop/GetProduct")]
        public async Task<ActionResult<object>> GetProduct([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, productId", true, Actions.GetProduct);

                return action.Response;
            });

            https: //backend.walkwards.pl/  await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Shop/AddProduct")]
        public async Task<ActionResult<object>> AddProduct([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, days, startPrice, name, producer, image, description", true,
                    Actions.AddProduct);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/Shop/JoinAuction")]
        public async Task<ActionResult<object>> JoinAuction([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token, productId, nextPrice", true, Actions.JoinAuction);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }        [HttpPost("/PayinForUkraine")]
        public async Task<ActionResult<object>> PayinForUkraine([FromBody] JObject json)
        {
            Task<object> task = Task.Run(async () =>
            {
                HandleAction action = new(_config, this);

                await action.SetResponse(json, "id, token", true, Actions.PayinForUkraine);

                return action.Response;
            });

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = await task;

            return data;
        }

        [HttpPost("/github/update")]
        public async Task<IActionResult> UpdateGithub([FromBody] JObject payload)
        {
            await LoggerManager.WriteLog(" github updater 1 ");
            
            
            await LoggerManager.WriteLog("Last commit: " + 
                                         payload?["commits"]?[0]?["author"]?["name"] + " : " +
                                         payload?["commits"]?[0]?["message"]);

            var startInfo = new ProcessStartInfo() { FileName = "/bin/bash", Arguments = "-c \"cd /home/ubuntu/api/repo/ && sudo git pull\"" }; 
            var proc = new Process() { StartInfo = startInfo, };
            proc.Start();

            await proc.WaitForExitAsync();
            
            await LoggerManager.WriteLog("Build update");
            
            var startInfo2 = new ProcessStartInfo()
            {
                FileName = "/bin/bash", 
                Arguments = "-c \"cd /home/ubuntu/api/repo/walkwards-api/ && sudo /home/ubuntu/dotnet/dotnet publish walkwards-api.csproj --output /home/ubuntu/api\""
            }; 
            var proc2 = new Process() { StartInfo = startInfo2, };
            proc2.Start();

            await proc2.WaitForExitAsync();

            await LoggerManager.WriteLog("github updater Done, now restarting");
            
            //own service restart backend on every exit 
            Environment.Exit(0);

            return Ok();
        }
    }
}
