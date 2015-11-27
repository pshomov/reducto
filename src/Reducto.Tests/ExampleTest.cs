using System;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Reducto.Tests
{
    [TestFixture]
    public class ExampleTest
    {
        // Actions

        public struct LoginStarted { 
            public string Username; 
        }

        public struct LoginFailed {}

        public struct LoginSucceeded {
            public string Token;
        }

        // State

        public enum LoginStatus {
            LoginInProgress, LoggedIn, NotLoggedIn
        }

        public struct AppState
        {
            public LoginStatus Status;
            public String Username;
            public String Token;
        }
            
        [Test]
        public async void all_in_one_example(){
            var reducer = new SimpleReducer<AppState>()
                .When<LoginStarted>((status, action) => {
                    status.Username = action.Username;
                    status.Token = "";
                    status.Status = LoginStatus.LoginInProgress;
                    return status;
                })
                .When<LoginSucceeded>((status, action) => {
                    status.Token = action.Token;
                    status.Status = LoginStatus.LoggedIn;
                    return status;
                })
                .When<LoginFailed>((status, action) => {
                    status.Status = LoginStatus.NotLoggedIn;
                    return status;
                });

            var store = new Store<AppState>(reducer);

            var loginAsyncAction = store.asyncAction(async(dispatch, getState) => {
                dispatch(new LoginStarted{Username = "John Doe"});

                // faking authentication of user
                await Task.Delay(500);
                var authenticated = new Random().Next() % 2 == 0;

                if (authenticated) {
                    dispatch(new LoginSucceeded{Token = "1234"});
                } else {
                    dispatch(new LoginFailed());
                }
                return  authenticated;
            });

            var logged = await store.Dispatch(loginAsyncAction);

            if (logged){
                Assert.That(store.GetState().Status, Is.EqualTo(LoginStatus.LoggedIn));
            } else {
                Assert.That(store.GetState().Status, Is.EqualTo(LoginStatus.NotLoggedIn));
            }
        }
    }
}

