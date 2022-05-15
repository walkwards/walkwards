using System.Collections.Generic;
using System.Threading.Tasks;
using walkwards_api.structure;
using Xunit;
using walkwards_api.UserManager;

namespace walkwards_api_tests
{
    public class Tests
    {
        private const int ExistingUserId = 643500;
        private const string ExistingUserName = "krzys";
        private const string ExistingUserPassword = "Trudnehaslo123!";
        private const string Token = "2ykrohna";

        // Example of static test
        [Fact]
        public async Task IsUserExist()
        {
            var goodError = await Helpers.CheckTaskError(UserMethod.IsUserExist(000000), "UserNotExist");

            var response = await UserMethod.IsUserExist(ExistingUserId);

            //Check if task return good exception and we have good response
            Assert.True(goodError);
            Assert.IsType<User>(response);

            //Verify some data from response
            Assert.Equal(ExistingUserId, response.Id);
            Assert.Equal(ExistingUserName, response.Username);
        }

        // Example of test with passed data
        [Fact]
        public async Task GetUserData()
        {
            var goodError = await Helpers.CheckTaskError(UserMethod.GetUserData(000000), "UserNotExist");

            var response = await UserMethod.IsUserExist(ExistingUserId);

            //Check if task return good exception and we have good response
            Assert.True(goodError);
            Assert.IsType<User>(response);

            //Verify some data from response
            Assert.Equal(ExistingUserId, response.Id);
            Assert.Equal(ExistingUserName, response.Username);
        }

        [Fact]
        public async Task ResetPassword()
        {
            var goodError = await Helpers.CheckTaskError(UserMethod.ResetPassword(ExistingUserId, "", "newPassword"),
                "PasswordIsNotSame");

            var response = await UserMethod.ResetPassword(ExistingUserId, "Trudnehaslo123!", "Trudnehaslo123!");

            Assert.True(goodError);
            Assert.True(response);
        }

        [Fact]
        public async Task ActivateUser()
        {
            var goodError = await Helpers.CheckTaskError(UserMethod.ActivateUser(000000), "InvalidLogin");

            var response = await UserMethod.ActivateUser(ExistingUserId);

            Assert.True(goodError);
            Assert.True(response);
        }

        [Fact]
        public async Task Login()
        {
            var goodError = await Helpers.CheckTaskError(UserMethod.Login(ExistingUserName, ""), "InvalidLogin");

            var response = await UserMethod.Login(ExistingUserName, ExistingUserPassword) as User;

            //Check if task return good exception and we have good response
            Assert.True(goodError);
            Assert.IsType<User>(response);

            //Verify some data from response
            Assert.Equal(ExistingUserId, response?.Id);
            Assert.Equal(ExistingUserName, response?.Username);
            Assert.True(response?.Token is not null);
        }

        [Theory]
        [InlineData("ja")]
        public async Task FriendSearch(string query)
        {
            //var goodError =
            //    await Helpers.CheckTaskError(UserMethod.FriendSearch("", ExistingUserId, query), "InvalidToken");
//
            //var goodResponse = await UserMethod.FriendSearch(Token, ExistingUserId, query);

            //Assert.True(goodError);
            //Assert.IsType<List<User>>(goodResponse);
//
            //foreach (var user in goodResponse)
            //{
            //    Assert.IsType<User>(user);
            //    Assert.True(user.IsActivated);
            //}
        }

        [Fact]
        public async Task GetRanking()
        {
            var goodError = await Helpers.CheckTaskError(UserMethod.GetRanking(0, "", ExistingUserId), "InvalidToken");

            var response = await UserMethod.GetRanking(0, Token, ExistingUserId);

            Assert.True(goodError);
            Assert.IsType<UserActivity[]>(response);

            var prevSteps = -1;

            foreach (var activity in response)
            {
                Assert.IsType<UserActivity>(activity);

                if (prevSteps != -1)
                    Assert.True(activity.Steps <= prevSteps);

                prevSteps = activity.Steps;

                Assert.True(activity.IsActivated);
            }

        }
    }
}