using System;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Reducto.Tests
{
    [TestFixture]
    public class ExampleTest
    {
        // Actions

        public struct LoginStarted 
        { 
            public string Username; 
        }

        public struct LoginFailed {}

        public struct LoginSucceeded 
        {
            public string Token;
        }

        // State

        public enum LoginStatus 
        {
            LoginInProgress, LoggedIn, NotLoggedIn
        }

        public struct AppState
        {
            public LoginStatus Status;
            public String Username;
            public String Token;
        }
            
        [Test]
        public async Task all_in_one_example(){
            var reducer = new SimpleReducer<AppState>()
                .When<LoginStarted>((state, action) => {
                    state.Username = action.Username;
                    state.Token = "";
                    state.Status = LoginStatus.LoginInProgress;
                    return state;
                })
                .When<LoginSucceeded>((state, action) => {
                    state.Token = action.Token;
                    state.Status = LoginStatus.LoggedIn;
                    return state;
                })
                .When<LoginFailed>((state, action) => {
                    state.Status = LoginStatus.NotLoggedIn;
                    return state;
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

