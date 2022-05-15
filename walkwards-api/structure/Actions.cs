using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using walkwards_api.Challenge;
using walkwards_api.Competition;
using walkwards_api.Notifications;
using walkwards_api.Shop;
using walkwards_api.UserManager;
using walkwards_api.Utilities;

namespace walkwards_api.structure
{
    public enum Actions
    {
        Activate,
        CreateUser,
        Login,
        IsUserExist,
        GetRanking,
        ResetPassword,
        SetNewAvatar,
        SetNewEmail,
        SetNewLogin,
        GetUserData,
        AddActivity,
        GetAllActivity,
        GetLastWeeklyActivity,
        GetTodayActivity,
        InviteFriend,
        ReturnFriendship,
        AnswerFriendInvite,
        RemoveFriend,
        ForgotPassword,
        Logout,
        SetGoal,
        SetAccountPrivacyType,
        GetFriend,
        GetRankingAllSteps,
        SearchUser,
        SendDailyRaport,
        DropUser,
        AddColumnInUserData,
        AddTableToUserData,
        DropTableToUserData,
        SetValueInAllUser,
        EndpointDevelopedDaily,
        GetToken,
        AddActivityForDate,
        GetActivity,
        AddActivityFromArray,

        //Challenge

        SetChallenge,
        GetActiveChallenge,
        AcceptOrCancelChallengeRequest,
        GetFriendsRequest,
        GiveUpChallenge,
        GetActiveBetweenUsersChallenge,
        GetFinishedChallenges,

        //Competitions

        GetCompetitionUsers,
        CreateCompetition,
        JoinCompetition,
        DropFromCompetition,
        EditCompetition,
        GetCompetition,
        GetUserCompetitions,
        GetCompetitions,
        GetUserActiveCompetitions,
        DropCompetition,

        GetNotifications,
        EditValueType,
        SendAndroidObject,

        //Guilds

        GetGuild,
        CreateGuild,
        DropFromGuild,
        DeleteGuild,
        InviteToGuild,
        ResponseToInvite,
        GetGuildInvite,
        GetUserGuild,
        GetAllGuildsRankingSum,
        GetAllGuildsRankingAvg,
        GuildSearch,
        GuildJoinRequest,
        AnswerGuildJoinRequest,
        GetGuildJoinRequest,
        CancelGuildJoinRequest,
        GetUserRelation,
        
        //shop
        
        AddProduct,
        GetProduct,
        GetAllActiveAuction,
        JoinAuction,
        DropData,
        GetSentInvite,
        EditGuild,
        
        PayinForUkraine
    }

    public static class ActionHandler
    {
        public static async Task<object> GetAction(Actions actions, Dictionary<string, object> args)
        {
            switch (actions)
            {
                case Actions.Activate:
                    return UserMethod.ActivateUser((int) args["id"]);
                case Actions.CreateUser:
                    return await UserMethod.CreateUser(args["login"].ToString(), args["email"].ToString(),
                        args["password"].ToString());
                case Actions.ForgotPassword:
                    return await UserMethod.ForgotPassword(args["loginOrEmail"].ToString());
                case Actions.Login:
                    return await UserMethod.Login(args["loginOrEmail"].ToString(), args["password"].ToString());
                case Actions.IsUserExist:
                    return await UserMethod.IsUserExist((int) args["id"]);
                case Actions.GetRanking:
                    return await UserMethod.GetRanking((int) args["page"], (string) args["token"], (int) args["id"]);
                case Actions.ResetPassword:
                    return UserMethod.ResetPassword((int) args["id"], args["pass1"].ToString(),
                        args["pass2"].ToString());
                case Actions.SetNewAvatar:
                    return await (await UserMethod.GetUserData((int) args["id"], false)).SetNewAvatar(args["newAvatar"]
                        .ToString());
                case Actions.SetNewEmail:
                    return await (await UserMethod.GetUserData((int) args["id"], false)).SetNewEmail(args["newEmail"]
                        .ToString());
                case Actions.SetNewLogin:
                    return await (await UserMethod.GetUserData((int) args["id"], false)).SetNewLogin(args["newLogin"]
                        .ToString());
                case Actions.GetUserData:
                    return await UserMethod.GetUserData((int) args["id"], true, false);
                case Actions.AddActivity:
                    return await (await UserMethod.GetUserData((int) args["id"], false)).AddActivity((int) args["steps"]);
                case Actions.AddActivityForDate:
                    return await (await UserMethod.GetUserData((int) args["id"], false)).AddActivityForDate(
                        (int) args["steps"], (string) args["date"]);
                case Actions.GetAllActivity:
                    return await (await UserMethod.GetUserData((int) args["id"], false)).GetAllActivity();
                case Actions.GetLastWeeklyActivity:
                    return await (await UserMethod.GetUserData((int) args["id"], false)).GetLastWeekActivity(
                        (int) args["dayCount"]);
                case Actions.GetTodayActivity:
                    return await (await UserMethod.GetUserData((int) args["id"], false)).GetActivityCurrentDay();
                case Actions.InviteFriend:
                    return await (await UserMethod.GetUserData((int) args["id"], false)).AddFriend((int) args["friendId"]);
                case Actions.AddActivityFromArray:
                    return await (await UserMethod.GetUserData((int) args["id"], false)).AddActivityFromArray(
                        (JArray) args["content"]);
                case Actions.ReturnFriendship:
                    return await (await UserMethod.GetUserData((int) args["id"], false)).GetFriendShip((int) args["friendId"]);
                case Actions.AnswerFriendInvite:
                    return await (await UserMethod.GetUserData((int) args["id"], false)).AcceptOrCancelFriendRequest(
                        (int) args["friendId"], (bool) args["accepted"]);
                case Actions.RemoveFriend:
                    return await (await UserMethod.GetUserData((int) args["id"], false)).RemoveFriend((int) args["friendId"]);
                case Actions.Logout:
                    return await (await UserMethod.GetUserData((int) args["id"], false)).Logout((string) args["token"]);
                case Actions.SetGoal:
                    return await (await UserMethod.GetUserData((int) args["id"], false)).SetGoal((int) args["goal"]);
                case Actions.SetAccountPrivacyType:
                    return await (await UserMethod.GetUserData((int) args["id"], false)).SetAccountPrivacy(
                        (int) args["privacyType"]);
                case Actions.GetFriend:
                    return await (await UserMethod.GetUserData((int) args["id"], false)).GetAllFriend((int) args["page"]);
                case Actions.GetActivity:
                    return await (await UserMethod.GetUserData((int) args["id"], false)).GetActivity((bool) args["isGlobal"]);
                case Actions.SendDailyRaport:
                    return await (await UserMethod.GetUserData((int) args["id"], false)).SendDailyRaport();
                case Actions.GetFriendsRequest:
                    return await (await UserMethod.GetUserData((int) args["id"], false)).GetFriendsRequest();
                case Actions.PayinForUkraine:
                    return await (await UserMethod.GetUserData((int) args["id"], false)).PayinForUkraine();
                case Actions.GetRankingAllSteps:
                    return await UserMethod.GetRankingAllSteps((int) args["page"], (string) args["token"],
                        (int) args["id"]);
                case Actions.SearchUser:
                    return await UserMethod.FriendSearch((string) args["token"], (int) args["id"],
                        (string) args["query"], (int) args["page"]);
                case Actions.DropUser:
                    return await AdminMethods.DropUser((int) args["id"]);
                case Actions.DropData:
                    return await AdminMethods.DropData((string) args["tablename"]);
                case Actions.GetToken:
                    return await AdminMethods.GetToken((int) args["id"]);
                case Actions.AddColumnInUserData:
                    return await AdminMethods.AddColumnInUserData((string) args["fieldname"], (string) args["type"], (string) args["tablename"]);
                case Actions.AddTableToUserData:
                    return await AdminMethods.AddTableToUserData((string) args["tablename"], (JArray) args["fields"]);
                case Actions.DropTableToUserData:
                    return await AdminMethods.DropTableToUserData((string) args["tablename"]);
                case Actions.SetValueInAllUser:
                    return await AdminMethods.SetValueInAllUser((string) args["tablename"], (string) args["fieldname"],
                        (string) args["value"]);
                case Actions.EndpointDevelopedDaily:
                    return await EndpointDevelopedDaily.DevelopedDaily();
                case Actions.SetChallenge:
                    return await ChallengeManager.SetChallenge((int) args["id"], (int) args["recipient"],
                        (int) args["betValue"], (int) args["dayCount"]);
                case Actions.GetActiveChallenge:
                    return await ChallengeManager.GetActiveChallenge((int) args["id"]);
                case Actions.AcceptOrCancelChallengeRequest:
                    return await ChallengeManager.AcceptOrCancelChallengeRequest((int) args["challengeId"],
                        (bool) args["accepted"]);
                case Actions.GiveUpChallenge:
                    return await ChallengeManager.GiveUpChallenge((int) args["cid"], (int) args["id"]);
                case Actions.GetActiveBetweenUsersChallenge:
                    return await ChallengeManager.GetActiveBetweenUsersChallenge((int) args["id"],
                        (int) args["recipientId"]);
                case Actions.GetFinishedChallenges:
                    return await ChallengeManager.GetFinishedChallenges((int) args["id"]);

                case Actions.CreateCompetition:
                    return await CompetitionManager.CreateCompetition((string) args["name"], (string) args["startDate"],
                        (string) args["endDate"], (bool) args["isPublic"], (int) args["creator"],
                        (bool) args["isOfficial"], (string) args["avatar"], (string) args["description"],
                        (int) args["entranceFee"], (string)args["companyName"]);
                case Actions.GetCompetitionUsers:
                    return await CompetitionManager.GetCompetition((int) args["competitionId"], (int) args["id"]);
                case Actions.JoinCompetition:
                    return await CompetitionManager.JoinCompetition((int) args["id"], (string) args["ctoken"]);
                case Actions.DropFromCompetition:
                    return await CompetitionManager.DropFromCompetition((int) args["id"], (int) args["cid"]);
                case Actions.EditCompetition:
                    return await CompetitionManager.EditCompetition((int) args["competitionId"],
                        (string) args["fieldName"], (string) args["value"]);
                case Actions.GetCompetitions:
                    return await CompetitionManager.GetCompetitions((int) args["id"]);
                case Actions.GetUserActiveCompetitions:
                    return await CompetitionManager.GetUserActiveCompetitions((int) args["id"]);
                case Actions.DropCompetition:
                    return await CompetitionManager.DropCompetition((int) args["cid"]);

                case Actions.GetNotifications:
                    return await NotificationsManager.GetNotifications((int) args["id"], (int) args["page"],
                        (bool) args["getAll"]);
                case Actions.EditValueType:
                    return await AdminMethods.EditValueType((string) args["tablename"], (string) args["fieldname"],
                        (string) args["newtype"]);
                case Actions.SendAndroidObject:
                    return await AdminMethods.SendAndroidObject((int) args["id"], (string) args["obj"]);

                //guilds

                case Actions.GuildJoinRequest: return await GuildManager.GuildJoinRequest((int)args["id"], (int) args["guildId"]); //
                case Actions.AnswerGuildJoinRequest: return await GuildManager.AnswerGuildJoinRequest((int) args["guildId"], (int) args["userId"], (int) args["id"], (bool) args["accepted"]); //
                case Actions.GetGuildJoinRequest: return await GuildManager.GetGuildJoinRequest((int) args["guildId"], (int) args["id"]); //
                case Actions.CancelGuildJoinRequest: return await GuildManager.CancelGuildJoinRequest((int) args["guildId"], (int)args["id"]); //
                case Actions.GetSentInvite: return await GuildManager.GetSentInvite((int) args["guildId"]); //
                case Actions.EditGuild: return await GuildManager.EditGuild((string)args["field"],(string)args["value"], (int) args["guildId"]); //
                
                case Actions.GetGuild: return await GuildManager.GetGuild((int) args["guildId"]); //
                case Actions.DeleteGuild: return await GuildManager.DeleteGuild((int) args["guildId"]);
                case Actions.ResponseToInvite: return await GuildManager.ResponseToInvite((int) args["id"], (int) args["guildId"], (bool) args["accepted"]); //
                case Actions.GetGuildInvite: return await GuildManager.GetGuildInvite((int) args["id"]); //
                case Actions.DropFromGuild: return await GuildManager.DropFromGuild((int) args["userId"], (int) args["guildId"]); //
                case Actions.GetUserRelation: return await GuildManager.GetUserRelation((int) args["guildId"], (int) args["id"]); //
                case Actions.InviteToGuild: return await GuildManager.InviteToGuild((int) args["userId"], (int) args["guildId"], (int) args["id"]); //
                case Actions.CreateGuild: return await GuildManager.CreateGuild((string) args["name"], (int) args["id"], (string) args["avatar"], (bool)args["canMembersAddMembers"]); //
                case Actions.GetUserGuild: return await GuildManager.GetUserGuild((int) args["id"]); //
                case Actions.GetAllGuildsRankingSum: return await GuildManager.GetAllGuildsRankingSum((int)args["page"]); //
                case Actions.GetAllGuildsRankingAvg: return await GuildManager.GetAllGuildsRankingAvg((int)args["page"]); //
                case Actions.GuildSearch: return await GuildManager.GuildSearch((string)args["query"], (int)args["page"]); 
                case Actions.GetAllActiveAuction: return await ShopManager.GetAllActiveAuction(); 
                case Actions.GetProduct: return await ShopManager.GetProduct((int)args["productId"]); 
                case Actions.AddProduct: return await ShopManager.AddProduct((int)args["days"], (int)args["startPrice"], (string)args["name"], (string)args["producer"], (string)args["image"], (string)args["description"]); 
                case Actions.JoinAuction: return await ShopManager.JoinAuction((int)args["id"], (int)args["productId"], (int)args["nextPrice"]); 
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(actions), actions, null);
            }
        }
    }
}